namespace Okuyanlar.Core.Enums
{
  /// <summary>
  /// Defines the hierarchy and permission levels within the system.
  /// </summary>
  public enum UserRole
  {
    /// <summary>
    /// Full system access. Can create Admins and Librarians.
    /// </summary>
    Admin = 1,

    /// <summary>
    /// Library management access. Can create EndUsers.
    /// </summary>
    Librarian = 2,

    /// <summary>
    /// Standard member access (Reader). Cannot create users.
    /// </summary>
    EndUser = 3
  }
}