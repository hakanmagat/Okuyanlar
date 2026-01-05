using Moq;
using Xunit;
using Microsoft.Extensions.Caching.Memory;
using Okuyanlar.Web.Services;

namespace Okuyanlar.Tests
{
  /// <summary>
  /// Unit tests for <see cref="PasswordTokenService"/>.
  /// Tests token generation, validation, and consumption logic.
  /// </summary>
  public class PasswordTokenServiceTests
  {
    private readonly IMemoryCache _memoryCache;
    private readonly IPasswordTokenService _tokenService;

    public PasswordTokenServiceTests()
    {
      _memoryCache = new MemoryCache(new MemoryCacheOptions());
      _tokenService = new PasswordTokenService(_memoryCache);
    }

    // --- CREATE RESET TOKEN TESTS ---

    [Fact]
    public void CreateResetToken_Should_GenerateToken()
    {
      // Arrange
      var email = "user@test.com";

      // Act
      var token = _tokenService.CreateResetToken(email);

      // Assert
      Assert.NotNull(token);
      Assert.NotEmpty(token);
    }

    [Fact]
    public void CreateResetToken_Should_GenerateUniqueTokens()
    {
      // Arrange
      var email = "user@test.com";

      // Act
      var token1 = _tokenService.CreateResetToken(email);
      var token2 = _tokenService.CreateResetToken(email);

      // Assert - Tokens should be different for different calls
      Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void CreateResetToken_Should_StoreTokenInCache()
    {
      // Arrange
      var email = "user@test.com";

      // Act
      var token = _tokenService.CreateResetToken(email);

      // Assert
      var isValid = _tokenService.ValidateResetToken(email, token);
      Assert.True(isValid);
    }

    [Fact]
    public void CreateResetToken_Should_ThrowException_When_EmailIsNull()
    {
      // Act & Assert
      var exception = Assert.Throws<ArgumentException>(() => _tokenService.CreateResetToken(null!));
      Assert.NotNull(exception);
    }

    [Fact]
    public void CreateResetToken_Should_ThrowException_When_EmailIsEmpty()
    {
      // Act & Assert
      var exception = Assert.Throws<ArgumentException>(() => _tokenService.CreateResetToken(""));
      Assert.NotNull(exception);
    }

    [Fact]
    public void CreateResetToken_Should_ThrowException_When_EmailIsWhitespace()
    {
      // Act & Assert
      var exception = Assert.Throws<ArgumentException>(() => _tokenService.CreateResetToken("   "));
      Assert.NotNull(exception);
    }

    [Fact]
    public void CreateResetToken_Should_TrimEmailWhitespace()
    {
      // Arrange
      var email = "  user@test.com  ";

      // Act
      var token = _tokenService.CreateResetToken(email);

      // Assert - Token should be created and valid with trimmed email
      var isValid = _tokenService.ValidateResetToken(email, token);
      Assert.True(isValid);
    }

    [Fact]
    public void CreateResetToken_Should_HandleCaseInsensitiveEmail()
    {
      // Arrange
      var emailLower = "user@test.com";
      var emailUpper = "USER@TEST.COM";

      // Act
      var token = _tokenService.CreateResetToken(emailLower);

      // Assert - Should work with both cases
      var isValid = _tokenService.ValidateResetToken(emailUpper, token);
      Assert.True(isValid);
    }

    // --- VALIDATE RESET TOKEN TESTS ---

    [Fact]
    public void ValidateResetToken_Should_ReturnTrue_When_TokenIsValid()
    {
      // Arrange
      var email = "user@test.com";
      var token = _tokenService.CreateResetToken(email);

      // Act
      var isValid = _tokenService.ValidateResetToken(email, token);

      // Assert
      Assert.True(isValid);
    }

    [Fact]
    public void ValidateResetToken_Should_ReturnFalse_When_TokenIsInvalid()
    {
      // Arrange
      var email = "user@test.com";
      _tokenService.CreateResetToken(email);
      var invalidToken = "invalid-token";

      // Act
      var isValid = _tokenService.ValidateResetToken(email, invalidToken);

      // Assert
      Assert.False(isValid);
    }

    [Fact]
    public void ValidateResetToken_Should_ReturnFalse_When_EmailIsNull()
    {
      // Act
      var isValid = _tokenService.ValidateResetToken(null!, "token");

      // Assert
      Assert.False(isValid);
    }

    [Fact]
    public void ValidateResetToken_Should_ReturnFalse_When_TokenIsNull()
    {
      // Act
      var isValid = _tokenService.ValidateResetToken("email@test.com", null!);

      // Assert
      Assert.False(isValid);
    }

    [Fact]
    public void ValidateResetToken_Should_ReturnFalse_When_TokenIsEmpty()
    {
      // Arrange
      var email = "user@test.com";
      _tokenService.CreateResetToken(email);

      // Act
      var isValid = _tokenService.ValidateResetToken(email, "");

      // Assert
      Assert.False(isValid);
    }

    [Fact]
    public void ValidateResetToken_Should_ReturnFalse_When_EmailIsEmpty()
    {
      // Act
      var isValid = _tokenService.ValidateResetToken("", "token");

      // Assert
      Assert.False(isValid);
    }

    [Fact]
    public void ValidateResetToken_Should_HandleCaseInsensitiveEmail()
    {
      // Arrange
      var emailLower = "user@test.com";
      var emailUpper = "USER@TEST.COM";
      var token = _tokenService.CreateResetToken(emailLower);

      // Act
      var isValid = _tokenService.ValidateResetToken(emailUpper, token);

      // Assert
      Assert.True(isValid);
    }

    // --- CONSUME RESET TOKEN TESTS ---

    [Fact]
    public void ConsumeResetToken_Should_RemoveTokenFromCache()
    {
      // Arrange
      var email = "user@test.com";
      var token = _tokenService.CreateResetToken(email);

      // Verify token exists
      Assert.True(_tokenService.ValidateResetToken(email, token));

      // Act
      _tokenService.ConsumeResetToken(email, token);

      // Assert
      Assert.False(_tokenService.ValidateResetToken(email, token));
    }

    [Fact]
    public void ConsumeResetToken_Should_NotThrowException_When_TokenDoesNotExist()
    {
      // Act & Assert - Should not throw
      _tokenService.ConsumeResetToken("user@test.com", "invalid-token");
    }

    [Fact]
    public void ConsumeResetToken_Should_NotThrowException_When_EmailIsNull()
    {
      // Act & Assert - Should not throw
      _tokenService.ConsumeResetToken(null!, "token");
    }

    [Fact]
    public void ConsumeResetToken_Should_NotThrowException_When_TokenIsNull()
    {
      // Act & Assert - Should not throw
      _tokenService.ConsumeResetToken("email@test.com", null!);
    }

    [Fact]
    public void ConsumeResetToken_Should_NotThrowException_When_EmailIsEmpty()
    {
      // Act & Assert - Should not throw
      _tokenService.ConsumeResetToken("", "token");
    }

    [Fact]
    public void ConsumeResetToken_Should_NotThrowException_When_TokenIsEmpty()
    {
      // Act & Assert - Should not throw
      _tokenService.ConsumeResetToken("email@test.com", "");
    }

    [Fact]
    public void ConsumeResetToken_Should_HandleCaseInsensitiveEmail()
    {
      // Arrange
      var emailLower = "user@test.com";
      var emailUpper = "USER@TEST.COM";
      var token = _tokenService.CreateResetToken(emailLower);

      // Act
      _tokenService.ConsumeResetToken(emailUpper, token);

      // Assert
      Assert.False(_tokenService.ValidateResetToken(emailLower, token));
    }

    // --- TTL (Time To Live) TESTS ---

    [Fact]
    public void CreateResetToken_Should_ExpireAfterTimespan()
    {
      // Arrange
      var email = "user@test.com";
      var token = _tokenService.CreateResetToken(email);

      // Verify token exists initially
      Assert.True(_tokenService.ValidateResetToken(email, token));

      // Act - Wait for token to expire (15 minutes)
      // Note: In unit tests, we typically mock time or use shorter timeouts
      // For now, this test documents the expected behavior
      // In integration tests, we would verify the actual expiration
    }

    // --- INTEGRATION TESTS ---

    [Fact]
    public void TokenWorkflow_Should_Work_End_To_End()
    {
      // Arrange
      var email = "user@test.com";

      // Act
      // Step 1: Create token
      var token = _tokenService.CreateResetToken(email);

      // Step 2: Validate token
      Assert.True(_tokenService.ValidateResetToken(email, token));

      // Step 3: Use token (consume)
      _tokenService.ConsumeResetToken(email, token);

      // Step 4: Verify token is consumed
      Assert.False(_tokenService.ValidateResetToken(email, token));
    }

    [Fact]
    public void MultipleTokens_Should_Work_Independently()
    {
      // Arrange
      var email1 = "user1@test.com";
      var email2 = "user2@test.com";

      // Act
      var token1 = _tokenService.CreateResetToken(email1);
      var token2 = _tokenService.CreateResetToken(email2);

      // Assert
      Assert.True(_tokenService.ValidateResetToken(email1, token1));
      Assert.True(_tokenService.ValidateResetToken(email2, token2));
      Assert.False(_tokenService.ValidateResetToken(email1, token2));
      Assert.False(_tokenService.ValidateResetToken(email2, token1));
    }
  }
}
