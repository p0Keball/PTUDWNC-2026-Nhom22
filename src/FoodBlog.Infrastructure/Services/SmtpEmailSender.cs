using System.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using FoodBlog.Application.Interfaces;
using FoodBlog.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodBlog.Infrastructure.Services;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(
        IOptions<EmailOptions> options,
        ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(
        string recipientEmail,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Host) ||
            string.IsNullOrWhiteSpace(_options.FromAddress))
            throw new InvalidOperationException("Email SMTP chưa được cấu hình.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        message.To.Add(MailboxAddress.Parse(recipientEmail));
        message.Subject = "Chào mừng bạn đến với FoodBlog";
        message.Body = new BodyBuilder
        {
            HtmlBody = $"""
                <p>Xin chào {WebUtility.HtmlEncode(displayName)},</p>
                <p>Chào mừng bạn đã tham gia FoodBlog.</p>
                <p>Bạn có thể bắt đầu chia sẻ và khám phá các công thức ngay hôm nay.</p>
                <p>Trân trọng,<br/>Đội ngũ FoodBlog</p>
                """
        }.ToMessageBody();

        using var smtp = new SmtpClient();
        var socketOptions = _options.UseSsl
            ? _options.Port == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls
            : SecureSocketOptions.None;
        await smtp.ConnectAsync(
            _options.Host,
            _options.Port,
            socketOptions,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(_options.Username))
            await smtp.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);

        await smtp.SendAsync(message, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("Welcome email sent to {RecipientEmail}.", recipientEmail);
    }

    public async Task SendPasswordResetEmailAsync(
        string recipientEmail,
        string displayName,
        string resetUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Host) ||
            string.IsNullOrWhiteSpace(_options.FromAddress))
            throw new InvalidOperationException("Email SMTP chưa được cấu hình.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        message.To.Add(MailboxAddress.Parse(recipientEmail));
        message.Subject = "Đặt lại mật khẩu FoodBlog";
        message.Body = new BodyBuilder
        {
            HtmlBody = $"""
                <p>Xin chào {WebUtility.HtmlEncode(displayName)},</p>
                <p>Nhấn vào liên kết sau để đặt lại mật khẩu:</p>
                <p><a href="{WebUtility.HtmlEncode(resetUrl)}">Đặt lại mật khẩu</a></p>
                <p>Nếu bạn không yêu cầu, hãy bỏ qua email này.</p>
                """
        }.ToMessageBody();

        using var smtp = new SmtpClient();
        var socketOptions = _options.UseSsl
            ? _options.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls
            : SecureSocketOptions.None;
        await smtp.ConnectAsync(_options.Host, _options.Port, socketOptions, cancellationToken);
        if (!string.IsNullOrWhiteSpace(_options.Username))
            await smtp.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
        await smtp.SendAsync(message, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);
        _logger.LogInformation("Password reset email sent to {RecipientEmail}.", recipientEmail);
    }
}
