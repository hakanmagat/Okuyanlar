using Moq;
using Microsoft.AspNetCore.Identity;
using Xunit;
using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Enums;
using Okuyanlar.Core.Interfaces;
using Okuyanlar.Service.Services;

namespace Okuyanlar.Tests
{
  /// <summary>
  /// Unit tests for <see cref="UserService"/>.
  /// Covers User Creation, Password Setting (Hashing), and Validation (Login) logic.
  /// </summary>
  public class UserServiceTests
  {
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IPasswordHasher<User>> _mockHasher;
    private readonly UserService _userService;

    public UserServiceTests()
    {
      // Initialize Mocks
      _mockUserRepository = new Mock<IUserRepository>();
      _mockEmailService = new Mock<IEmailService>();
      _mockHasher = new Mock<IPasswordHasher<User>>();

      // Default Mock Behavior for Hasher:
      // 1. HashPassword will return a predictable string "hashed_password_string".
      _mockHasher.Setup(x => x.HashPassword(It.IsAny<User>(), It.IsAny<string>()))
                 .Returns("hashed_password_string");

      // 2. VerifyHashedPassword will default to Success (can be overridden in specific tests).
      _mockHasher.Setup(x => x.VerifyHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
                 .Returns(PasswordVerificationResult.Success);

      // Initialize Service with Mocks (Injecting the hasher now)
      _userService = new UserService(_mockUserRepository.Object, _mockEmailService.Object, _mockHasher.Object);
    }

    // ==========================================
    // SECTION 1: USER CREATION TESTS
    // ==========================================

    /// <summary>
    /// Verifies that an Admin can successfully create a Librarian user.
    /// </summary>
    [Fact]
    public void CreateUser_Should_Succeed_When_AdminCreatesLibrarian()
    {
      // Arrange
      var admin = new User { Role = UserRole.Admin };
      var librarian = new User { Role = UserRole.Librarian, Email = "lib@test.com", Username = "lib" };

      // Act
      _userService.CreateUser(admin, librarian);

      // Assert
      _mockUserRepository.Verify(x => x.Add(librarian), Times.Once);
      _mockEmailService.Verify(x => x.SendPasswordCreationLink(librarian.Email, librarian.Username, It.IsAny<string>()), Times.Once);
    }

    /// <summary>
    /// Verifies that a Librarian can successfully create an EndUser.
    /// </summary>
    [Fact]
    public void CreateUser_Should_Succeed_When_LibrarianCreatesEndUser()
    {
      // Arrange
      var librarian = new User { Role = UserRole.Librarian };
      var endUser = new User { Role = UserRole.EndUser, Email = "user@test.com", Username = "user" };

      // Act
      _userService.CreateUser(librarian, endUser);

      // Assert
      _mockUserRepository.Verify(x => x.Add(endUser), Times.Once);
    }

    /// <summary>
    /// Verifies that a Librarian is forbidden from creating an Admin user.
    /// </summary>
    [Fact]
    public void CreateUser_Should_ThrowException_When_LibrarianTriesToCreateAdmin()
    {
      // Arrange
      var librarian = new User { Role = UserRole.Librarian };
      var newAdmin = new User { Role = UserRole.Admin };

      // Act & Assert
      Assert.Throws<UnauthorizedAccessException>(() => _userService.CreateUser(librarian, newAdmin));

      // Ensure DB was not touched
      _mockUserRepository.Verify(x => x.Add(It.IsAny<User>()), Times.Never);
    }

    /// <summary>
    /// Verifies that an EndUser is forbidden from creating any user.
    /// </summary>
    [Fact]
    public void CreateUser_Should_ThrowException_When_EndUserTriesToCreateAnyone()
    {
      // Arrange
      var endUser = new User { Role = UserRole.EndUser };
      var targetUser = new User { Role = UserRole.EndUser };

      // Act & Assert
      Assert.Throws<UnauthorizedAccessException>(() => _userService.CreateUser(endUser, targetUser));
      _mockUserRepository.Verify(x => x.Add(It.IsAny<User>()), Times.Never);
    }

    // ==========================================
    // SECTION 2: SET PASSWORD TESTS (With Hashing)
    // ==========================================

    /// <summary>
    /// Verifies that setting a password hashes the input, updates the user, and activates the account.
    /// </summary>
    [Fact]
    public void SetPassword_Should_HashPasswordAndActivateUser_When_UserExists()
    {
      // Arrange
      var email = "test@mail.com";
      var plainPassword = "securePassword123";
      var existingUser = new User { Email = email, IsActive = false, PasswordHash = "" };

      _mockUserRepository.Setup(x => x.GetByEmail(email)).Returns(existingUser);

      // Act
      _userService.SetPassword(email, plainPassword);

      // Assert
      Assert.True(existingUser.IsActive); // Should be active
      Assert.Equal("hashed_password_string", existingUser.PasswordHash); // Should contain the HASH, not plain text

      // Verify Hasher was called
      _mockHasher.Verify(x => x.HashPassword(existingUser, plainPassword), Times.Once);

      // Verify Repository Update was called
      _mockUserRepository.Verify(x => x.Update(existingUser), Times.Once);
    }

    /// <summary>
    /// Verifies that an exception is thrown if the user email is not found.
    /// </summary>
    [Fact]
    public void SetPassword_Should_ThrowException_When_UserNotFound()
    {
      // Arrange
      var email = "ghost@mail.com";
      _mockUserRepository.Setup(x => x.GetByEmail(email)).Returns((User?)null);

      // Act & Assert
      var exception = Assert.Throws<Exception>(() => _userService.SetPassword(email, "pass"));
      Assert.Equal("User not found.", exception.Message);
    }

    // ==========================================
    // SECTION 3: LOGIN / VALIDATION TESTS
    // ==========================================

    /// <summary>
    /// Verifies that a valid user is returned when credentials are correct and account is active.
    /// </summary>
    [Fact]
    public void ValidateUser_Should_ReturnUser_When_CredentialsAreCorrect_And_UserIsActive()
    {
      // Arrange
      var email = "valid@mail.com";
      var password = "correctpass";
      var user = new User { Email = email, PasswordHash = "hashed_db_string", IsActive = true };

      _mockUserRepository.Setup(x => x.GetByEmail(email)).Returns(user);

      // Mock Hasher to return Success
      _mockHasher.Setup(x => x.VerifyHashedPassword(user, user.PasswordHash, password))
                 .Returns(PasswordVerificationResult.Success);

      // Act
      var result = _userService.ValidateUser(email, password);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(email, result.Email);
    }

    /// <summary>
    /// Verifies that null is returned when the password verification fails.
    /// </summary>
    [Fact]
    public void ValidateUser_Should_ReturnNull_When_PasswordIsWrong()
    {
      // Arrange
      var email = "valid@mail.com";
      var user = new User { Email = email, PasswordHash = "hashed_db_string", IsActive = true };

      _mockUserRepository.Setup(x => x.GetByEmail(email)).Returns(user);

      // Mock Hasher to return Failed (This simulates wrong password)
      _mockHasher.Setup(x => x.VerifyHashedPassword(user, user.PasswordHash, "WRONG_PASS"))
                 .Returns(PasswordVerificationResult.Failed);

      // Act
      var result = _userService.ValidateUser(email, "WRONG_PASS");

      // Assert
      Assert.Null(result);
    }

    /// <summary>
    /// Verifies that null is returned when the user does not exist.
    /// </summary>
    [Fact]
    public void ValidateUser_Should_ReturnNull_When_UserNotFound()
    {
      // Arrange
      _mockUserRepository.Setup(x => x.GetByEmail(It.IsAny<string>())).Returns((User?)null);

      // Act
      var result = _userService.ValidateUser("ghost@mail.com", "pass");

      // Assert
      Assert.Null(result);
    }

    /// <summary>
    /// Verifies that an exception is thrown when credentials are correct but the account is inactive.
    /// </summary>
    [Fact]
    public void ValidateUser_Should_ThrowException_When_UserIsInactive()
    {
      // Arrange
      var email = "inactive@mail.com";
      var password = "correctpass";
      var user = new User { Email = email, PasswordHash = "hashed_db_string", IsActive = false };

      _mockUserRepository.Setup(x => x.GetByEmail(email)).Returns(user);

      // Mock Hasher to return Success (Password is correct, but status is wrong)
      _mockHasher.Setup(x => x.VerifyHashedPassword(user, user.PasswordHash, password))
                 .Returns(PasswordVerificationResult.Success);

      // Act & Assert
      var exception = Assert.Throws<Exception>(() => _userService.ValidateUser(email, password));
      Assert.Equal("Your account is not active yet.", exception.Message);
    }

    [Fact]
    public void ValidateUser_Should_ReturnNull_When_UserExists_But_HasNoPasswordHash()
    {
      // Arrange
      var email = "newuser@mail.com";
      // User exists but password not yet assigned (PasswordHash is empty or null)      
      var user = new User { Email = email, PasswordHash = null, IsActive = true };

      _mockUserRepository.Setup(x => x.GetByEmail(email)).Returns(user);

      // Act
      var result = _userService.ValidateUser(email, "anyPassword");

      // Assert
      Assert.Null(result); // It should return null, not throw an exception.
    }

    [Fact]
    public void CreateUser_Should_Succeed_When_SystemAdminCreatesAdmin()
    {
      // Arrange
      var sysAdmin = new User { Role = UserRole.SystemAdmin };
      var newAdmin = new User { Role = UserRole.Admin, Email = "admin@test.com", Username = "admin" };

      // Act
      _userService.CreateUser(sysAdmin, newAdmin);

      // Assert
      _mockUserRepository.Verify(x => x.Add(newAdmin), Times.Once);
    }
  }
}