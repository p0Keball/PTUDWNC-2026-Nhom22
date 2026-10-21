using FoodBlog.Application.Interfaces;

namespace FoodBlog.Infrastructure.Jobs;

public sealed class WelcomeEmailJob
{
    private readonly IEmailSender _emailSender;

    public WelcomeEmailJob(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public Task SendAsync(string email, string displayName, CancellationToken cancellationToken = default) =>
        _emailSender.SendWelcomeEmailAsync(email, displayName, cancellationToken);
}
