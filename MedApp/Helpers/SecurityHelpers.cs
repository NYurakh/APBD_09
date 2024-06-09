using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace MedApp.Helpers
{
    public static class SecurityHelpers
    {
        public static (string hashedPassword, string salt) GetHashedPasswordAndSalt(string password)
        {
            byte[] salt = new byte[128 / 8];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);

            string hashedPassword = HashPassword(password, salt);
            string saltBase64 = Convert.ToBase64String(salt);

            return new ValueTuple<string, string>(hashedPassword, saltBase64);
        }

        public static string GetHashedPasswordWithSalt(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);

            var currentHashedPassword = HashPassword(password, saltBytes);

            return currentHashedPassword;
        }

        private static string HashPassword(string password, byte[] saltBytes)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
        }
    }
}