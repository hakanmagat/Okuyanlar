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
    }
  }
}