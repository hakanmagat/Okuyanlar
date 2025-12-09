using Okuyanlar.Core.Enums;

namespace Okuyanlar.Core.Entities
{
  /// <summary>
  /// Represents a registered user within the library system.
  /// This entity maps directly to the 'Users' table in the database.
  /// </summary>
  public class User
  {
    /// <summary>
    /// Unique identifier for the user (Primary Key).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The unique username used for system identification.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The user's email address, used for login and notifications.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The hashed version of the user's password.
    /// <remarks>Plain text passwords should never be stored here.</remarks>
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Defines the user's authorization level (Admin, Librarian, EndUser).
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Indicates if the user has completed the registration (set password).
    /// Users cannot log in if this is false.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// The UTC timestamp when the user record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  }
}