using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Security.Cryptography;

namespace AintBnB.BusinessLogic.Helpers
{
    internal static class PasswordHashing
    {

        /// <summary>Hashes a cleartext password with salt.</summary>
        /// <param name="password">The cleartext password.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="needsOnlyHash">If only a hash is needed.</param>
        /// <returns>The string with the hashed password</returns>
        public static string HashThePassword(string password, byte[] salt, bool needsOnlyHash)
        {
            if (salt == null || salt.Length != 16)
            {
                salt = new byte[128 / 8];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }
            }

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            if (needsOnlyHash) return hashed;

            return $"{hashed}:{Convert.ToBase64String(salt)}";
        }


        /// <summary>Verifies the unhashed password with the hashed password.</summary>
        /// <param name="passwordToCheck">The cleartext password to check.</param>
        /// <param name="hashedPasswordWithSalt">The hashed password with salt.</param>
        /// <returns>True if the cleartext password matches the hashed password, false otherwise</returns>
        public static bool VerifyThePassword(string passwordToCheck, string hashedPasswordWithSalt)
        {

            var passwordAndHash = hashedPasswordWithSalt.Split(':');
            if (passwordAndHash == null || passwordAndHash.Length != 2)
                return false;
            var salt = Convert.FromBase64String(passwordAndHash[1]);
            if (salt == null)
                return false;

            var hashOfpasswordToCheck = HashThePassword(passwordToCheck, salt, true);

            if (String.Compare(passwordAndHash[0], hashOfpasswordToCheck) == 0)
                return true;

            return false;
        }
    }
}
