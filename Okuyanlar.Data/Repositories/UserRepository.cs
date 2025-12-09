using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Interfaces;
using System.Linq; // LINQ sorguları için gerekli

namespace Okuyanlar.Data.Repositories
{
  /// <summary>
  /// The concrete implementation of <see cref="IUserRepository"/> using Entity Framework Core.
  /// Handles the actual SQL operations.
  /// </summary>
  public class UserRepository : IUserRepository
  {
    private readonly OkuyanlarDbContext _context;

    public UserRepository(OkuyanlarDbContext context)
    {
      _context = context;
    }

    /// <inheritdoc />
    public void Add(User user)
    {
      _context.Users.Add(user);
      _context.SaveChanges();
    }

    /// <inheritdoc />
    public User? GetByEmail(string email)
    {
      return _context.Users.FirstOrDefault(u => u.Email == email);
    }

    /// <inheritdoc />
    public User? GetByUsername(string username)
    {
      return _context.Users.FirstOrDefault(u => u.Username == username);
    }

    /// <inheritdoc />
    public void Update(User user)
    {
      _context.Users.Update(user);
      _context.SaveChanges();
    }

  }
}