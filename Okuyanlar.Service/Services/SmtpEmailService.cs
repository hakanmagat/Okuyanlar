using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Okuyanlar.Core.Interfaces;
using Okuyanlar.Core.Settings;

namespace Okuyanlar.Service.Services
{
  /// <summary>
  /// Implementation of <see cref="IEmailService"/> using the SMTP protocol.
  /// Reads configuration from appsettings via <see cref="EmailSettings"/>.
  /// </summary>
  public class SmtpEmailService : IEmailService
  {
    private readonly EmailSettings _emailSettings;

    public SmtpEmailService(IOptions<EmailSettings> emailSettings)
    {
      _emailSettings = emailSettings.Value;
    }

    /// <inheritdoc />
    public void SendPasswordCreationLink(string toEmail, string username, string token)
    {
      var link = $"http://localhost:5014/Account/CreatePassword?token={token}&email={toEmail}";

      var subject = "Okuyanlar Kütüphanesi - Şifre Oluşturma";
      var body = $@"
                <h3>Merhaba {username},</h3>
                <p>Okuyanlar sistemine kaydınız yapıldı.</p>
                <p>Lütfen aşağıdaki linke tıklayarak şifrenizi belirleyiniz:</p>
                <a href='{link}'>Şifremi Oluştur</a>
                <br/><br/>
                <small>Bu linkin süresi 24 saattir.</small>";

      SendEmail(toEmail, subject, body);
    }

    private void SendEmail(string toEmail, string subject, string htmlBody)
    {
      try
      {
        using (var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port))
        {
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
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"E-posta gönderilirken hata oluştu: {ex.Message}");
      }
    }
  }
}