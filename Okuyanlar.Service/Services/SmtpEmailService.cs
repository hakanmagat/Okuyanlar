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

            var subject = "Okuyanlar - Şifre Oluşturma";
            var body = $@"
                <h3>Merhaba {username},</h3>
                <p>Okuyanlar sistemine kaydınız yapıldı.</p>
                <p>Şifrenizi belirlemek için:</p>
                <a href='{link}'>Şifremi Oluştur</a>
                <br/><br/>
                <small>Bu linkin süresi sınırlı olabilir.</small>";

            SendEmail(toEmail, subject, body);
        }

        public void SendPasswordResetLink(string toEmail, string username, string token)
        {
            var link = $"https://localhost:7214/Account/ResetPassword?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(toEmail)}";

            var subject = "Okuyanlar - Şifre Sıfırlama";
            var body = $@"
                <h3>Merhaba {username},</h3>
                <p>Şifre sıfırlama isteği alındı.</p>
                <p>Yeni şifre belirlemek için aşağıdaki linke tıklayın:</p>
                <a href='{link}'>Şifremi Sıfırla</a>
                <br/><br/>
                <small>Eğer bu isteği siz yapmadıysanız bu e-postayı yok sayabilirsiniz.</small>";

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
                throw new InvalidOperationException($"E-posta gönderilirken hata oluştu: {ex.Message}");
            }
        }
    }
}
