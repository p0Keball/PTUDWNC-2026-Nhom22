namespace FoodBlog.Application.Interfaces;

public interface IEmailSender
{
    Task SendWelcomeEmailAsync(
        string recipientEmail,
        string displayName,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetEmailAsync(
        string recipientEmail,
        string resetLink,
        CancellationToken cancellationToken = default);

    // FR-AUTH-008 (bổ sung theo SRS 2.3 - policy VerifiedAuthor):
    // gửi liên kết xác nhận địa chỉ email sau khi đăng ký.
    Task SendEmailConfirmationAsync(
        string recipientEmail,
        string displayName,
        string confirmationLink,
        CancellationToken cancellationToken = default);
}
