using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SPRS.Active_Classes
{
    public class PasswordHelper
    {
        // Generate a hashed password
        public static string HashPassword(string plainPassword)
        {
            // Generate a salt
            byte[] salt = new byte[16];

            // Use RandomNumberGenerator to fill the salt
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(plainPassword, salt, 100_000, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(32); // 32-byte hash
                byte[] hashBytes = new byte[48];
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 32);

                // Convert the hash bytes to a Base64 string
                return Convert.ToBase64String(hashBytes);
            }
        }

        // Verify a hashed password
        public static bool VerifyPassword(string plainPassword, string storedHash)
        {
            byte[] hashBytes = Convert.FromBase64String(storedHash);

            // Extract the salt from the stored hash
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            // Rehash the password with the same salt
            using (var pbkdf2 = new Rfc2898DeriveBytes(plainPassword, salt, 100_000, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(32);

                // Compare the new hash with the stored hash
                for (int i = 0; i < 32; i++)
                {
                    if (hashBytes[i + 16] != hash[i])
                        return false;
                }
            }

            return true;
        }
    }
}
