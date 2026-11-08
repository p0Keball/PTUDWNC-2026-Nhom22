using System.Net;
using System.Net.Http.Json;
using FoodBlog.Tests.Infrastructure;
using Xunit;

namespace FoodBlog.Tests.Security;

/// <summary>
/// NFR-SEC-003 / 5.2: auth endpoint giới hạn 10 request/phút/IP và phải trả
/// đầy đủ header X-RateLimit-* + Retry-After khi vượt giới hạn.
///
/// Test lặp tới khi gặp 429 thay vì hard-code đúng request thứ 11: fixed window
/// bắt đầu từ request đầu tiên, nhưng nếu môi trường chạy chậm tới mức vượt
/// một cửa sổ thì vẫn nên xác nhận hành vi chặn thay vì flaky.
/// </summary>
public sealed class RateLimitHeadersTests : ApiTestBase
{
    private const int PermitLimit = 10;
    private const int MaxAttempts = 30;

    [Fact]
    public async Task AuthEndpoint_ReturnsRateLimitHeaders_AndRejectsAfterLimit()
    {
        HttpResponseMessage? first = null;
        HttpResponseMessage? rejected = null;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var response = await Client.PostAsJsonAsync(
                "/api/v1/auth/login",
                new { email = "nobody@example.com", password = "WrongPassword1!" });

            first ??= response;

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rejected = response;
                break;
            }

            // Trước khi chạm giới hạn, sai thông tin đăng nhập phải trả 401.
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        Assert.NotNull(first);
        Assert.Equal(
            PermitLimit.ToString(),
            first!.Headers.GetValues("X-RateLimit-Limit").Single());
        Assert.True(first.Headers.Contains("X-RateLimit-Remaining"));
        Assert.True(first.Headers.Contains("X-RateLimit-Reset"));

        Assert.NotNull(rejected);
        Assert.Equal(
            PermitLimit.ToString(),
            rejected!.Headers.GetValues("X-RateLimit-Limit").Single());
        Assert.Equal(
            "0",
            rejected.Headers.GetValues("X-RateLimit-Remaining").Single());
        Assert.NotNull(rejected.Headers.RetryAfter);
    }
}
