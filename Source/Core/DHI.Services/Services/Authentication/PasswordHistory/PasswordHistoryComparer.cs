namespace DHI.Services.Authentication.PasswordHistory
{
    using DHI.Services.Accounts;
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;

    public static class PasswordHistoryComparer
    {
        /// <summary>
        /// Compare password
        /// </summary>
        /// <param name="bytePassword">The password as a byte array</param>
        /// <param name="stringPassword">The password as a string</param>
        /// <returns></returns>
        public static bool Comparer(byte[] bytePassword, string stringPassword)
        {
            // This is kept for backward compatibility so that existing account repositories using the old hashing mechanism can still be validated
            if (bytePassword.Length == 20)
            {
                return Account.HashPassword(stringPassword).SequenceEqual(bytePassword);
            }

            // Extract salt and hash from the stored encrypted password
            var salt = new byte[16];
            Array.Copy(bytePassword, 0, salt, 0, 16);

            // Hash the provided password using PBKDF2 with the extracted salt
            var pbkdfs2 = new Rfc2898DeriveBytes(stringPassword, salt, 10000);
            var hash = new byte[20];
            Array.Copy(bytePassword, 16, hash, 0, 20);

            // Compare the resulting hash with the stored hash
            if (pbkdfs2.GetBytes(20).SequenceEqual(hash))
            {
                return true;
            }

            return false;
        }
    }
}
