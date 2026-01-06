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

      // 2. Ensure a SystemAdmin exists with updated defaults
      var existingOldSysAdmin = context.Users.FirstOrDefault(u => u.Role == UserRole.SystemAdmin && u.Email == "admin@okuyanlar.com");
      var existingNewSysAdmin = context.Users.FirstOrDefault(u => u.Role == UserRole.SystemAdmin && u.Email == "sys@okuyanlar.oku");

      if (existingOldSysAdmin != null)
      {
        // Update old seeded admin to new email and password
        if (existingNewSysAdmin == null)
        {
          existingOldSysAdmin.Email = "sys@okuyanlar.oku";
        }
        existingOldSysAdmin.Username = string.IsNullOrWhiteSpace(existingOldSysAdmin.Username) ? "SystemAdmin" : existingOldSysAdmin.Username;
        existingOldSysAdmin.IsActive = true;
        existingOldSysAdmin.PasswordHash = passwordHasher.HashPassword(existingOldSysAdmin, "pass1234");
        context.Users.Update(existingOldSysAdmin);
        context.SaveChanges();
      }
      else if (existingNewSysAdmin == null)
      {
        // Create default SystemAdmin if none exists
        var sysAdmin = new User
        {
          Username = "SystemAdmin",
          Email = "sys@okuyanlar.oku",
          Role = UserRole.SystemAdmin,
          IsActive = true,
          CreatedAt = DateTime.UtcNow
        };
        sysAdmin.PasswordHash = passwordHasher.HashPassword(sysAdmin, "pass1234");
        context.Users.Add(sysAdmin);
        context.SaveChanges();
      }

      // 6. SEED BOOKS: Add demo books if none exist
      if (!context.Books.Any())
      {
        var demoBooks = new List<Book>
        {
          new Book
          {
            Title = "Clean Code",
            Author = "Robert C. Martin",
            ISBN = "978-0132350884",
            Stock = 3,
            IsActive = true,
            Category = "Software Engineering",
            CoverUrl = "/images/book-placeholder.jpg",
            CreatedAt = DateTime.UtcNow.AddDays(-100)
          },
          new Book
          {
            Title = "Design Patterns",
            Author = "Gang of Four",
            ISBN = "978-0201633610",
            Stock = 2,
            IsActive = true,
            Category = "Software Engineering",
            CoverUrl = "/images/book-placeholder.jpg",
            CreatedAt = DateTime.UtcNow.AddDays(-90)
          },
          new Book
          {
            Title = "The Pragmatic Programmer",
            Author = "Andrew Hunt",
            ISBN = "978-0201616224",
            Stock = 1,
            IsActive = true,
            Category = "Software Engineering",
            CoverUrl = "/images/book-placeholder.jpg",
            CreatedAt = DateTime.UtcNow.AddDays(-80)
          },
          new Book
          {
            Title = "Introduction to Algorithms",
            Author = "Thomas H. Cormen",
            ISBN = "978-0262033848",
            Stock = 4,
            IsActive = true,
            Category = "Computer Science",
            CoverUrl = "/images/book-placeholder.jpg",
            CreatedAt = DateTime.UtcNow.AddDays(-70)
          },
          new Book
          {
            Title = "The Art of Computer Programming",
            Author = "Donald Knuth",
            ISBN = "978-0201896831",
            Stock = 0,
            IsActive = true,
            Category = "Computer Science",
            CoverUrl = "/images/book-placeholder.jpg",
            CreatedAt = DateTime.UtcNow.AddDays(-60)
          },
          new Book
          {
            Title = "Refactoring",
            Author = "Martin Fowler",
            ISBN = "978-0134757599",
            Stock = 2,
            IsActive = true,
            Category = "Software Engineering",
            CoverUrl = "/images/book-placeholder.jpg",
            CreatedAt = DateTime.UtcNow.AddDays(-50)
          },
          new Book
          {
            Title = "Code Complete",
            Author = "Steve McConnell",
            ISBN = "978-0735619678",
            Stock = 3,
            IsActive = true,
            Category = "Software Engineering",
            CoverUrl = "/images/book-placeholder.jpg",
            CreatedAt = DateTime.UtcNow.AddDays(-40)
          },
          new Book
          {
            Title = "Working Effectively with Legacy Code",
            Author = "Michael Feathers",
            ISBN = "978-0131177055",
            Stock = 1,
            IsActive = true,
            Category = "Software Engineering",
            CoverUrl = "/images/book-placeholder.jpg",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
          }
        };

        context.Books.AddRange(demoBooks);
        context.SaveChanges();
      }

      // 7. Seed one default user for each role if missing, and normalize usernames
      // Admin
      var adminExisting = context.Users.FirstOrDefault(u => u.Role == UserRole.Admin);
      if (adminExisting == null)
      {
        var admin = new User
        {
          Username = "Admin",
          Email = "admin@okuyanlar.oku",
          Role = UserRole.Admin,
          IsActive = true,
          CreatedAt = DateTime.UtcNow
        };
        admin.PasswordHash = passwordHasher.HashPassword(admin, "pass1234");
        context.Users.Add(admin);
        context.SaveChanges();
      }
      else if (adminExisting.Username.StartsWith("Default", StringComparison.OrdinalIgnoreCase))
      {
        adminExisting.Username = "Admin";
        context.Users.Update(adminExisting);
        context.SaveChanges();
      }

      // Librarian
      var librarianExisting = context.Users.FirstOrDefault(u => u.Role == UserRole.Librarian);
      if (librarianExisting == null)
      {
        var librarian = new User
        {
          Username = "Librarian",
          Email = "librarian@okuyanlar.oku",
          Role = UserRole.Librarian,
          IsActive = true,
          CreatedAt = DateTime.UtcNow
        };
        librarian.PasswordHash = passwordHasher.HashPassword(librarian, "pass1234");
        context.Users.Add(librarian);
        context.SaveChanges();
      }
      else if (librarianExisting.Username.StartsWith("Default", StringComparison.OrdinalIgnoreCase))
      {
        librarianExisting.Username = "Librarian";
        context.Users.Update(librarianExisting);
        context.SaveChanges();
      }

      // EndUser
      var endUserExisting = context.Users.FirstOrDefault(u => u.Role == UserRole.EndUser);
      if (endUserExisting == null)
      {
        var endUser = new User
        {
          Username = "User",
          Email = "user@okuyanlar.oku",
          Role = UserRole.EndUser,
          IsActive = true,
          CreatedAt = DateTime.UtcNow
        };
        endUser.PasswordHash = passwordHasher.HashPassword(endUser, "pass1234");
        context.Users.Add(endUser);
        context.SaveChanges();
      }
      else if (endUserExisting.Username.StartsWith("Default", StringComparison.OrdinalIgnoreCase))
      {
        endUserExisting.Username = "User";
        context.Users.Update(endUserExisting);
        context.SaveChanges();
      }
    }
  }
}