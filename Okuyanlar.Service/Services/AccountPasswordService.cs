using Microsoft.AspNetCore.Identity;
using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Interfaces;

namespace Okuyanlar.Service.Services
{
    public class AccountPasswordService : IAccountPasswordService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountPasswordService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public (bool exists, string username) FindUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, string.Empty);

            var user = _userRepository.GetByEmail(email.Trim());
            if (user == null)
                return (false, string.Empty);

            return (true, user.Username);
        }

        public void UpdatePasswordByEmail(string email, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email boþ olamaz.");

            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Þifre boþ olamaz.");

            var user = _userRepository.GetByEmail(email.Trim());
            if (user == null)
                throw new InvalidOperationException("Bu e-posta ile kullanýcý bulunamadý.");

            // UserService ile ayný hashing yaklaþýmý:
            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            user.IsActive = true;

            // UserRepository.Update zaten SaveChanges yapýyor -> Save() yok!
            _userRepository.Update(user);
        }
    }
}
