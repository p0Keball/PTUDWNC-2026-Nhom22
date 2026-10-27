namespace FoodBlog.Application.Interfaces;

public interface IEmailSender
{
    Task SendWelcomeEmailAsync(
        string recipientEmail,
        string displayName,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetEmailAsync(
        string recipientEmail,
        string displayName,
        string resetUrl,
        CancellationToken cancellationToken = default);
}
