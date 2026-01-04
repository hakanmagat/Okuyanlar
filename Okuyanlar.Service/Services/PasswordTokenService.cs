using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace Okuyanlar.Web.Services
{
    public class PasswordTokenService : IPasswordTokenService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _ttl = TimeSpan.FromMinutes(15);

        public PasswordTokenService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public string CreateResetToken(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("email boþ olamaz.");

            var token = GenerateToken();
            var key = CacheKey(email, token);

            _cache.Set(key, true, _ttl);
            return token;
        }

        public bool ValidateResetToken(string email, string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
                return false;

            return _cache.TryGetValue(CacheKey(email, token), out _);
        }

        public void ConsumeResetToken(string email, string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
                return;

            _cache.Remove(CacheKey(email, token));
        }

        private static string CacheKey(string email, string token)
            => $"pwdreset:{email.Trim().ToLowerInvariant()}:{token}";

        private static string GenerateToken()
        {
            // URL-safe token
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }
}
