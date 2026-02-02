using DigitalLibrary.Data;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

public interface IEmailService
{
    Task SendAsync(string toEmail, string subject, string bodyHtml);
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendAsync(string toEmail, string subject, string bodyHtml)
    {
        var message = new MailMessage
        {
            From = new MailAddress(
                _settings.SenderEmail,
                _settings.SenderName),
            Subject = subject,
            Body = bodyHtml,
            IsBodyHtml = true
        };

        message.To.Add(toEmail);

        using var client = new SmtpClient(_settings.SmtpServer, _settings.Port)
        {
            Credentials = new NetworkCredential(
                _settings.Username,
                _settings.Password),
            EnableSsl = true
        };

        await client.SendMailAsync(message);
    }
}
