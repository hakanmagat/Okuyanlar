using Moq;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Interfaces;
using Okuyanlar.Service.Services;

namespace Okuyanlar.Tests
{
  /// <summary>
  /// Unit tests for <see cref="AccountPasswordService"/>.
  /// Covers user lookup by email and password update functionality.
  /// </summary>
  public class AccountPasswordServiceTests
  {
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordHasher<User>> _mockPasswordHasher;
    private readonly AccountPasswordService _accountPasswordService;

    public AccountPasswordServiceTests()
    {
      _mockUserRepository = new Mock<IUserRepository>();
      _mockPasswordHasher = new Mock<IPasswordHasher<User>>();

      // Default mock behavior for hasher
      _mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<User>(), It.IsAny<string>()))
                         .Returns("hashed_password");

      _accountPasswordService = new AccountPasswordService(_mockUserRepository.Object, _mockPasswordHasher.Object);
    }

    // --- FIND USER BY EMAIL TESTS ---

    [Fact]
    public void FindUserByEmail_Should_ReturnUserData_When_UserExists()
    {
      // Arrange
      var email = "user@test.com";
      var user = new User { Email = email, Username = "testuser" };

      _mockUserRepository.Setup(x => x.GetByEmail(email)).Returns(user);

      // Act
      var (exists, username) = _accountPasswordService.FindUserByEmail(email);

      // Assert
      Assert.True(exists);
      Assert.Equal("testuser", username);
      _mockUserRepository.Verify(x => x.GetByEmail(email), Times.Once);
    }

    [Fact]
    public void FindUserByEmail_Should_ReturnFalse_When_UserNotFound()
    {
      // Arrange
      var email = "ghost@test.com";
      _mockUserRepository.Setup(x => x.GetByEmail(email)).Returns((User?)null);

      // Act
      var (exists, username) = _accountPasswordService.FindUserByEmail(email);

      // Assert
      Assert.False(exists);
      Assert.Equal(string.Empty, username);
    }

    [Fact]
    public void FindUserByEmail_Should_ReturnFalse_When_EmailIsNull()
    {
      // Act
      var (exists, username) = _accountPasswordService.FindUserByEmail(null!);

      // Assert
      Assert.False(exists);
      Assert.Equal(string.Empty, username);
    }

    [Fact]
    public void FindUserByEmail_Should_ReturnFalse_When_EmailIsEmpty()
    {
      // Act
      var (exists, username) = _accountPasswordService.FindUserByEmail("");

      // Assert
      Assert.False(exists);
      Assert.Equal(string.Empty, username);
    }

    [Fact]
    public void FindUserByEmail_Should_TrimWhitespace()
    {
      // Arrange
      var email = "  user@test.com  ";
      var user = new User { Email = "user@test.com", Username = "testuser" };

      _mockUserRepository.Setup(x => x.GetByEmail("user@test.com")).Returns(user);

      // Act
      var (exists, username) = _accountPasswordService.FindUserByEmail(email);

      // Assert
      Assert.True(exists);
      Assert.Equal("testuser", username);
      _mockUserRepository.Verify(x => x.GetByEmail("user@test.com"), Times.Once);
    }

    // --- UPDATE PASSWORD BY EMAIL TESTS ---

    [Fact]
    public void UpdatePasswordByEmail_Should_UpdatePasswordAndActivateUser_When_UserExists()
    {
      // Arrange
      var email = "user@test.com";
      var newPassword = "newPassword123";
      var user = new User { Email = email, IsActive = false, PasswordHash = "" };

      _mockUserRepository.Setup(x => x.GetByEmail(email)).Returns(user);

      // Act
      _accountPasswordService.UpdatePasswordByEmail(email, newPassword);

      // Assert
      Assert.True(user.IsActive);
      Assert.Equal("hashed_password", user.PasswordHash);
      _mockPasswordHasher.Verify(x => x.HashPassword(user, newPassword), Times.Once);
      _mockUserRepository.Verify(x => x.Update(user), Times.Once);
    }

    [Fact]
    public void UpdatePasswordByEmail_Should_ThrowException_When_UserNotFound()
    {
      // Arrange
      var email = "ghost@test.com";
      _mockUserRepository.Setup(x => x.GetByEmail(email)).Returns((User?)null);

      // Act & Assert
      var exception = Assert.Throws<InvalidOperationException>(() => 
        _accountPasswordService.UpdatePasswordByEmail(email, "newPassword"));

      Assert.NotNull(exception);
      _mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public void UpdatePasswordByEmail_Should_ThrowException_When_EmailIsNull()
    {
      // Act & Assert
      var exception = Assert.Throws<ArgumentException>(() => 
        _accountPasswordService.UpdatePasswordByEmail(null!, "password"));

      Assert.NotNull(exception);
    }

    [Fact]
    public void UpdatePasswordByEmail_Should_ThrowException_When_EmailIsEmpty()
    {
      // Act & Assert
      var exception = Assert.Throws<ArgumentException>(() => 
        _accountPasswordService.UpdatePasswordByEmail("", "password"));

      Assert.NotNull(exception);
    }

    [Fact]
    public void UpdatePasswordByEmail_Should_ThrowException_When_PasswordIsNull()
    {
      // Act & Assert
      var exception = Assert.Throws<ArgumentException>(() => 
        _accountPasswordService.UpdatePasswordByEmail("user@test.com", null!));

      Assert.NotNull(exception);
    }

    [Fact]
    public void UpdatePasswordByEmail_Should_ThrowException_When_PasswordIsEmpty()
    {
      // Act & Assert
      var exception = Assert.Throws<ArgumentException>(() => 
        _accountPasswordService.UpdatePasswordByEmail("user@test.com", ""));

      Assert.NotNull(exception);
    }

    [Fact]
    public void UpdatePasswordByEmail_Should_TrimWhitespace()
    {
      // Arrange
      var email = "  user@test.com  ";
      var newPassword = "newPassword123";
      var user = new User { Email = "user@test.com", IsActive = false, PasswordHash = "" };

      _mockUserRepository.Setup(x => x.GetByEmail("user@test.com")).Returns(user);

      // Act
      _accountPasswordService.UpdatePasswordByEmail(email, newPassword);

      // Assert
      Assert.True(user.IsActive);
      _mockUserRepository.Verify(x => x.GetByEmail("user@test.com"), Times.Once);
    }
  }
}
