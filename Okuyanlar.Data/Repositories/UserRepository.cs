using Microsoft.EntityFrameworkCore;
using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Interfaces;
using Okuyanlar.Data;

namespace Okuyanlar.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly OkuyanlarDbContext _context;

        public UserRepository(OkuyanlarDbContext context)
        {
            _context = context;
        }

        public User? GetByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public User? GetById(int id)
        {
            return _context.Users.FirstOrDefault(u => u.Id == id);
        }

        public User? GetByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username);
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void Update(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        // ✅ List Users
        public IEnumerable<User> GetAll()
        {
            return _context.Users
                .AsNoTracking()
                .OrderBy(u => u.Role)
                .ThenBy(u => u.Username)
                .ToList();
        }
    }
}
