namespace Okuyanlar.Core.Interfaces
{
  /// <summary>
  /// Defines the contract for sending email notifications.
  /// </summary>
  public interface IEmailService
  {
    /// <summary>
    /// Sends an email containing a secure link for password creation.
    /// </summary>
    /// <param name="toEmail">Recipient's email address.</param>
    /// <param name="username">Recipient's username for personalization.</param>
    /// <param name="token">The unique security token to include in the link.</param>
    void SendPasswordCreationLink(string toEmail, string username, string token);
  }
}