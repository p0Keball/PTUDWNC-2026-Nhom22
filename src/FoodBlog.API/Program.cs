using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using FoodBlog.API;
using FoodBlog.API.Authorization;
using FoodBlog.API.Endpoints;
using FoodBlog.API.Middlewares;
using FoodBlog.API.Services;
using FoodBlog.Application;
using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Application.Contracts.Persistence;
using FoodBlog.Application.Interfaces;
using FoodBlog.Application.Options;
using FoodBlog.Domain.Entities;
using FoodBlog.Infrastructure.Persistence;
using FoodBlog.Infrastructure.Seed;
using FoodBlog.Infrastructure.Jobs;
using FoodBlog.Infrastructure.Services;
using FoodBlog.Application.Features.Auth.Commands.GoogleLogin;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// FR-OBS-002: Structured logging bằng Serilog (Console + File + Seq).
// Cấu hình chi tiết nằm trong appsettings.json -> Serilog.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "FoodBlog.API"));

// FR-OBS-003: distributed tracing + metrics qua OpenTelemetry.
// OTLP exporter chỉ bật khi cấu hình endpoint (Collector/Jaeger/Grafana);
// dev không có Collector vẫn có trace in-process để debug.
var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("FoodBlog.API"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation();
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
            tracing.AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(otlpEndpoint);
                // Seq nhận OTLP qua HTTP/protobuf.
                options.Protocol = OtlpExportProtocol.HttpProtobuf;
            });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddMeter(FoodBlogMetrics.MeterName);
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
            metrics.AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(otlpEndpoint);
                options.Protocol = OtlpExportProtocol.HttpProtobuf;
            });
    });

builder.Services.AddOpenApi();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = false;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddApplication();
builder.Services.AddHangfire(configuration => configuration
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options =>
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("Default"))));
builder.Services.AddHangfireServer();
// NFR-PERF-003 / 6.2: Redis là cache phân tán cho dữ liệu ít thay đổi
// (danh mục). Khi chưa cấu hình Redis (dev/test) fallback về in-memory
// distributed cache để ứng dụng vẫn chạy độc lập.
var redisConnection = builder.Configuration["Redis:Connection"];
if (!string.IsNullOrWhiteSpace(redisConnection) &&
    !builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "FoodBlog:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}
builder.Services.AddMemoryCache();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>();
        if (origins is null || origins.Length == 0 || origins.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException("Cors:Origins must contain at least one trusted origin.");

        // Frontend gửi credentials: "include" (cookie auth của Identity),
        // nên bắt buộc phải AllowCredentials với origin cụ thể.
        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            // NFR-SEC-003 / 5.2: client cần đọc được các header rate limit.
            .WithExposedHeaders(
                "Retry-After",
                "X-RateLimit-Limit",
                "X-RateLimit-Remaining",
                "X-RateLimit-Reset");
    });
});
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("RecipeList", policy => policy
        .Tag("recipes")
        .Expire(TimeSpan.FromMinutes(15))
        .SetVaryByQuery("page", "pageSize", "categoryId", "difficulty", "maxCookTime", "minServings", "sort")
        // Response thay đổi theo user (Author thấy Draft của mình, Admin thấy tất cả),
        // nên cache phải tách theo token/cookie để không rò rỉ dữ liệu.
        .SetVaryByHeader("Authorization")
        .SetVaryByHeader("Cookie"));
});
builder.Services.AddHttpClient<GoogleLoginCommandHandler>();
builder.Services.AddFileStorage(builder.Configuration);
builder.Services.Configure<GoogleOAuthOptions>(
    builder.Configuration.GetSection(GoogleOAuthOptions.SectionName));
builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection(EmailOptions.SectionName));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IAuditLogger, AuditLogger>();
builder.Services.Configure<SitemapOptions>(
    builder.Configuration.GetSection(SitemapOptions.SectionName));
// FR-SEO-001: typed client cho SitemapJob (gọi ping search engine).
builder.Services.AddHttpClient<SitemapJob>();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();

});
builder.Services.AddDbContext<FoodBlogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IFoodBlogDbContext>(sp => sp.GetRequiredService<FoodBlogDbContext>());
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddAuthorization(options =>
{
    // SRS 2.3 - tầng 3: Author phải xác nhận email mới được ghi dữ liệu.
    options.AddPolicy("VerifiedAuthor", policy =>
        policy.Requirements.Add(new EmailConfirmedRequirement()));
});
builder.Services.AddSingleton<
    Microsoft.AspNetCore.Authorization.IAuthorizationHandler,
    EmailConfirmedAuthorizationHandler>();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.User.RequireUniqueEmail = true;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<FoodBlogDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

// FR-AUTH-002/004 + NFR-SEC-002: JWT HS256 cho REST API.
// Giữ Identity cookie như scheme dự phòng để không phá vỡ luồng cũ;
// request có header Authorization sẽ được xác thực bằng Bearer JWT.
var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwtOptions.Secret) || jwtOptions.Secret.Length < 32)
    throw new InvalidOperationException(
        "Jwt:Secret phải được cấu hình (tối thiểu 32 ký tự) qua appsettings hoặc biến môi trường Jwt__Secret.");

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "Smart";
        options.DefaultAuthenticateScheme = "Smart";
        options.DefaultChallengeScheme = "Smart";
    })
    .AddPolicyScheme("Smart", "JWT Bearer hoặc Identity cookie", options =>
    {
        options.ForwardDefaultSelector = context =>
            context.Request.Headers.ContainsKey("Authorization")
                ? JwtBearerDefaults.AuthenticationScheme
                : IdentityConstants.ApplicationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.Configure<PasswordHasherOptions>(options =>
{
    options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
    options.IterationCount = 100_000;
});
var app = builder.Build();

app.UseForwardedHeaders();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseCors("Frontend");
// NFR-SEC-003: rate limit theo IP + header X-RateLimit-* (thay cho UseRateLimiter
// vì middleware built-in không trả số request còn lại cho response thành công).
app.UseMiddleware<RateLimitHeadersMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthentication();
// UserId chỉ có sau khi Authentication chạy -> push vào log context ở đây.
app.UseMiddleware<UserLogContextMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("CorrelationId", httpContext.Items[CorrelationIdMiddleware.ItemKey]);
        diagnosticContext.Set("UserId",
            httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? httpContext.User.FindFirstValue("sub"));
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
    };
});
app.UseAuthorization();
app.UseOutputCache();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAdminAuthorizationFilter()]
});

// Integration tests (tests/FoodBlog.Tests) dùng InMemory provider và tự seed dữ liệu,
// nên bỏ qua migrate + seed thật khi Environment = "Testing".
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<FoodBlogDbContext>();
        await db.Database.MigrateAsync();
        await FoodBlogSeeder.SeedAsync(app.Services);
    }

    // FR-SEO-001: sinh sitemap hằng ngày lúc 02:00 UTC.
    RecurringJob.AddOrUpdate<SitemapJob>(
        "sitemap-generation",
        job => job.GenerateAsync(CancellationToken.None),
        "0 2 * * *");
}

// Tài liệu API (OpenAPI + Scalar) cần khả dụng cả ở Production để
// phục vụ tích hợp; chỉ bỏ qua trong môi trường test.
if (!app.Environment.IsEnvironment("Testing"))
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapHealthEndpoints();
var apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .ReportApiVersions()
    .Build();

app.MapAuthEndpoints(apiVersionSet);
app.MapCategoryEndpoints(apiVersionSet);
app.MapRecipeEndpoints(apiVersionSet);
app.MapRecipeChildEndpoints(apiVersionSet);
app.MapStorageEndpoints();
app.MapAdminEndpoints(apiVersionSet);

app.Run();

Log.CloseAndFlush();

// Cần thiết để WebApplicationFactory<Program> trong test project truy cập được entry point.
public partial class Program;
