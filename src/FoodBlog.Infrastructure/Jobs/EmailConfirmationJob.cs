using FoodBlog.Application.Interfaces;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FoodBlog.Infrastructure.Jobs;

/// <summary>
/// FR-AUTH-008 (bổ sung theo SRS 2.3 - policy VerifiedAuthor):
/// gửi email chứa liên kết xác nhận địa chỉ email sau khi đăng ký.
/// </summary>
public sealed class EmailConfirmationJob
{
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailConfirmationJob> _logger;

    public EmailConfirmationJob(
        IEmailSender emailSender,
        IConfiguration configuration,
        ILogger<EmailConfirmationJob> logger)
    {
        _emailSender = emailSender;
        _configuration = configuration;
        _logger = logger;
    }

    [AutomaticRetry(
        Attempts = 3,
        DelaysInSeconds = new[] { 60, 300, 1800 },
        OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task SendAsync(
        string email,
        string displayName,
        string token,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = (_configuration["App:FrontendBaseUrl"] ?? "http://localhost:3000")
            .TrimEnd('/');
        var link =
            $"{baseUrl}/auth/confirm-email?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

        // Dev/demo thường không cấu hình SMTP: ghi liên kết vào log thay vì để job fail.
        if (string.IsNullOrWhiteSpace(_configuration["Email:Host"]))
        {
            _logger.LogWarning(
                "SMTP chưa được cấu hình. Liên kết xác nhận email cho {Email}: {Link}",
                email,
                link);
            return;
        }

        await _emailSender.SendEmailConfirmationAsync(
            email,
            displayName,
            link,
            cancellationToken);
    }
}
