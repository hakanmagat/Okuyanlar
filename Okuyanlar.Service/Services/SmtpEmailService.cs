using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Okuyanlar.Core.Interfaces;
using Okuyanlar.Core.Settings;

namespace Okuyanlar.Service.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public SmtpEmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public void SendPasswordCreationLink(string toEmail, string username, string token)
        {
            var link = $"https://localhost:7214/Account/CreatePassword?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(toEmail)}";

            var subject = "Okuyanlar - Password Creation";
            var body = $@"
                <h3>Hello {username},</h3>
                <p>Your Okuyanlar account has been created.</p>
                <p>To set your password:</p>
                <a href='{link}'>Set My Password</a>
                <br/><br/>
                <small>This link may expire after a short period.</small>";

            SendEmail(toEmail, subject, body);
        }

        public void SendPasswordResetLink(string toEmail, string username, string token)
        {
            var link = $"https://localhost:7214/Account/ResetPassword?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(toEmail)}";

            var subject = "Okuyanlar - Password Reset";
            var body = $@"
                <h3>Hello {username},</h3>
                <p>We received a password reset request.</p>
                <p>Click the link below to set a new password:</p>
                <a href='{link}'>Reset My Password</a>
                <br/><br/>
                <small>If you did not request this, you can safely ignore this email.</small>";

            SendEmail(toEmail, subject, body);
        }

        private void SendEmail(string toEmail, string subject, string htmlBody)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port);
                client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);
                client.Send(mailMessage);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while sending email: {ex.Message}");
            }
        }
    }
}
