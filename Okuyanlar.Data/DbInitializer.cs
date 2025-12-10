using Microsoft.AspNetCore.Identity;
using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Enums;

namespace Okuyanlar.Data
{
  /// <summary>
  /// Handles database seeding operations during application startup.
  /// Responsible for ensuring essential initial data exists.
  /// </summary>
  public static class DbInitializer
  {
    /// <summary>
    /// Checks the database for the existence of a System Admin.
    /// If no System Admin is found, it creates a default one securely.
    /// </summary>
    /// <param name="context">The database context to interact with the data store.</param>
    /// <param name="passwordHasher">The service used to securely hash the default password.</param>
    public static void Initialize(OkuyanlarDbContext context, IPasswordHasher<User> passwordHasher)
    {
      // 1. Ensure the database file exists
      context.Database.EnsureCreated();

      // 2. CHECK: Is there any user with the 'SystemAdmin' role?
      bool systemAdminExists = context.Users.Any(u => u.Role == UserRole.SystemAdmin);

      if (systemAdminExists)
      {
        return; // Seed data already exists, no action needed.
      }

      // 3. CREATE: Prepare the default SystemAdmin entity
      var sysAdmin = new User
      {
        Username = "SystemAdmin",
        Email = "admin@okuyanlar.com",
        Role = UserRole.SystemAdmin,
        IsActive = true, // Auto-activate since we are setting the password manually
        CreatedAt = DateTime.UtcNow,
        PasswordHash = "" // Will be set below
      };

      // 4. HASH: Securely hash the default password "Admin123!"
      sysAdmin.PasswordHash = passwordHasher.HashPassword(sysAdmin, "Admin123!");

      // 5. SAVE: Persist to database
      context.Users.Add(sysAdmin);
      context.SaveChanges();
    }
  }
}