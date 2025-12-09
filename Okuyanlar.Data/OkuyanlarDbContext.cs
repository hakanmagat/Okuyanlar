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
  }
}