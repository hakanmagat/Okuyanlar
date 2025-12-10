using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Enums;
using Okuyanlar.Core.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Okuyanlar.Service.Services
{
  /// <summary>
  /// Encapsulates all business logic and rules for User Management.
  /// Handles password hashing, role validations, and email notifications.
  /// Acts as the mediator between the Web (Controller) and Data (Repository) layers.
  /// </summary>
  public class UserService
  {
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(IUserRepository userRepository, IEmailService emailService, IPasswordHasher<User> passwordHasher)
    {
      _userRepository = userRepository;
      _emailService = emailService;
      _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Creates a new user if the requester has the appropriate role permissions.
    /// Also sends an activation email to the new user.
    /// </summary>
    /// <param name="creatorUser">The user initiating the request.</param>
    /// <param name="newUser">The user to be created.</param>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when role hierarchy rules are violated (e.g., Librarian trying to create Admin).
    /// </exception>
    public void CreateUser(User creatorUser, User newUser)
    {
      if (!CanCreate(creatorUser.Role, newUser.Role))
      {
        throw new UnauthorizedAccessException("You are not authorized to create this type of user.");
      }

      newUser.CreatedAt = DateTime.UtcNow;
      newUser.IsActive = true;

      _userRepository.Add(newUser);

      string token = Guid.NewGuid().ToString();
      _emailService.SendPasswordCreationLink(newUser.Email, newUser.Username, token);
    }

    // Helper method to centralize permission logic
    private bool CanCreate(UserRole creatorRole, UserRole targetRole)
    {
      if (creatorRole == UserRole.Admin || creatorRole == UserRole.SystemAdmin)
      {
        return targetRole == UserRole.Admin || targetRole == UserRole.Librarian;
      }

      if (creatorRole == UserRole.Librarian)
      {
        return targetRole == UserRole.EndUser;
      }

      return false;
    }

    /// <summary>
    /// Hashes the plain password and activates the user account.
    /// </summary>
    /// <param name="email">User's email.</param>
    /// <param name="password">Plain text password to be hashed.</param>
    public void SetPassword(string email, string password)
    {
      var user = _userRepository.GetByEmail(email);
      if (user == null)
      {
        throw new Exception("User not found.");
      }

      user.PasswordHash = _passwordHasher.HashPassword(user, password);

      user.IsActive = true;

      _userRepository.Update(user);
    }

    /// <summary>
    /// Validates login credentials by verifying the password hash.
    /// </summary>
    /// <param name="email">User's email address.</param>
    /// <param name="password">User's plain text password.</param>
    /// <returns>The User entity if validation succeeds; otherwise, null.</returns>
    /// <exception cref="Exception">Thrown if the credentials are correct but the account is not active yet.</exception>
    public User? ValidateUser(string email, string password)
    {
      var user = _userRepository.GetByEmail(email);

      if (user == null) return null;

      var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

      if (verificationResult == PasswordVerificationResult.Failed)
      {
        return null;
      }

      if (!user.IsActive)
      {
        throw new Exception("Your account is not active yet.");
      }

      return user;
    }
  }
}