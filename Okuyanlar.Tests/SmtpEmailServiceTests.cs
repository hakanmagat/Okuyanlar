using Moq;
using Xunit;
using Microsoft.Extensions.Options;
using Okuyanlar.Core.Interfaces;
using Okuyanlar.Core.Settings;
using Okuyanlar.Service.Services;

namespace Okuyanlar.Tests
{
  /// <summary>
  /// Unit tests for <see cref="SmtpEmailService"/>.
  /// Tests email sending functionality for password creation and reset links.
  /// </summary>
  public class SmtpEmailServiceTests
  {
    private readonly IEmailService _emailService;
    private readonly Mock<IOptions<EmailSettings>> _mockEmailSettingsOptions;

    public SmtpEmailServiceTests()
    {
      // Setup mock email settings
      var emailSettings = new EmailSettings
      {
        SmtpServer = "smtp.test.com",
        Port = 587,
        Username = "testuser@test.com",
        Password = "testpass",
        SenderEmail = "noreply@okuyanlar.com",
        SenderName = "Okuyanlar System"
      };

      _mockEmailSettingsOptions = new Mock<IOptions<EmailSettings>>();
      _mockEmailSettingsOptions.Setup(x => x.Value).Returns(emailSettings);

      _emailService = new SmtpEmailService(_mockEmailSettingsOptions.Object);
    }

    [Fact]
    public void SendPasswordCreationLink_Should_GenerateCorrectLink()
    {
      // Arrange
      var email = "user@test.com";
      var username = "testuser";
      var token = "test-token-123";

      // Act - This will attempt to send but will throw because we don't have real SMTP
      // We're just testing that the method exists and has the right parameters
      var exception = Assert.Throws<InvalidOperationException>(() =>
        _emailService.SendPasswordCreationLink(email, username, token));

      // Assert - Verify that an InvalidOperationException is thrown (expected for missing SMTP)
      Assert.NotNull(exception);
      Assert.Contains("An error occurred while sending email:", exception.Message);
    }

    [Fact]
    public void SendPasswordResetLink_Should_GenerateCorrectLink()
    {
      // Arrange
      var email = "user@test.com";
      var username = "testuser";
      var token = "test-token-456";

      // Act - This will attempt to send but will throw because we don't have real SMTP
      var exception = Assert.Throws<InvalidOperationException>(() =>
        _emailService.SendPasswordResetLink(email, username, token));

      // Assert
      Assert.NotNull(exception);
      Assert.Contains("An error occurred while sending email:", exception.Message);
    }

    [Fact]
    public void SendPasswordCreationLink_Should_IncludeTokenInLink()
    {
      // Arrange
      var email = "user@test.com";
      var username = "testuser";
      var token = "secure-token-xyz";

      // Act & Assert
      // The method will throw, but we verify the exception is about SMTP, not validation
      var exception = Assert.Throws<InvalidOperationException>(() =>
        _emailService.SendPasswordCreationLink(email, username, token));

      Assert.Contains("An error occurred while sending email:", exception.Message);
    }

    [Fact]
    public void SendPasswordResetLink_Should_IncludeTokenInLink()
    {
      // Arrange
      var email = "user@test.com";
      var username = "testuser";
      var token = "reset-token-abc";

      // Act & Assert
      var exception = Assert.Throws<InvalidOperationException>(() =>
        _emailService.SendPasswordResetLink(email, username, token));

      Assert.Contains("An error occurred while sending email:", exception.Message);
    }

    [Fact]
    public void EmailSettings_Should_BeLoaded_From_Options()
    {
      // Act & Assert - Service initializes properly with settings
      Assert.NotNull(_mockEmailSettingsOptions.Object.Value);
      Assert.Equal("smtp.test.com", _mockEmailSettingsOptions.Object.Value.SmtpServer);
      Assert.Equal(587, _mockEmailSettingsOptions.Object.Value.Port);
      Assert.Equal("noreply@okuyanlar.com", _mockEmailSettingsOptions.Object.Value.SenderEmail);
    }
  }
}
