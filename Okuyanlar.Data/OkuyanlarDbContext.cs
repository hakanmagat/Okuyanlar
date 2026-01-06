using Microsoft.EntityFrameworkCore;
using Okuyanlar.Core.Entities;

namespace Okuyanlar.Data
{
  /// <summary>
  /// Represents the session with the database and maps entities to database tables.
  /// Configured for SQLite in this project.
  /// </summary>
  public class OkuyanlarDbContext : DbContext
  {
    public OkuyanlarDbContext(DbContextOptions<OkuyanlarDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Represents the 'Users' table in the database.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Represents the 'Book' table in the database.
    /// </summary>
    public DbSet<Book> Books { get; set; }

    /// <summary>
    /// Represents the per-user ratings for books.
    /// </summary>
    public DbSet<BookRating> BookRatings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      // Unique constraint: one rating per user per book.
      modelBuilder.Entity<BookRating>()
        .HasIndex(br => new { br.BookId, br.UserId })
        .IsUnique();
    }
  }
}