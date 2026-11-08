using FoodBlog.Application.Interfaces;
using Hangfire;

namespace FoodBlog.Infrastructure.Jobs;

/// <summary>
/// FR-JOB-001: gửi email chào mừng sau khi đăng ký thành công.
/// Retry 3 lần với backoff 1 phút, 5 phút, 30 phút (NFR-REL-002).
/// </summary>
public sealed class WelcomeEmailJob
{
    private readonly IEmailSender _emailSender;

    public WelcomeEmailJob(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    [AutomaticRetry(
        Attempts = 3,
        DelaysInSeconds = new[] { 60, 300, 1800 },
        OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public Task SendAsync(string email, string displayName, CancellationToken cancellationToken = default) =>
        _emailSender.SendWelcomeEmailAsync(email, displayName, cancellationToken);
}
