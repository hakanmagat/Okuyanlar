using Okuyanlar.Core.Enums;

namespace Okuyanlar.Core.Extensions
{
    /// <summary>
    /// Extension methods for UserRole enum to provide user-friendly display names.
    /// </summary>
    public static class UserRoleExtensions
    {
        /// <summary>
        /// Gets the display name for a user role.
        /// </summary>
        /// <param name="role">The user role.</param>
        /// <returns>The display name for the role.</returns>
        public static string GetDisplayName(this UserRole role)
        {
            return role switch
            {
                UserRole.SystemAdmin => "System Admin",
                UserRole.Admin => "Admin",
                UserRole.Librarian => "Librarian",
                UserRole.EndUser => "Reader",
                _ => role.ToString()
            };
        }
    }
}
