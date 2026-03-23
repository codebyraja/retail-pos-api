
using Microsoft.Extensions.Options;
using QSRAPIServices.Models;
using MimeKit;
using MailKit.Security;

namespace QSREmailServices.Services.Email
{
    public class EmailHelper
    {
        private readonly EmailSettings _emailSettings;

        public EmailHelper(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailSettings.CompanyHead, _emailSettings.FromEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = body
            };
            email.Body = builder.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient();

            SecureSocketOptions option = _emailSettings.EnableTls
            ? SecureSocketOptions.StartTls
            : _emailSettings.EnableSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.None;

            await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, option);
            //await smtp.ConnectAsync(_emailSettings.Host, 465, _emailSettings.EnableSsl);
            await smtp.AuthenticateAsync(_emailSettings.FromEmail, _emailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
