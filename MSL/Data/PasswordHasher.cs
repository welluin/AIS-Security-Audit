using System.Security.Cryptography;

namespace MSL.Data
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public static string CreateHash(string password, out string salt)
        {
            byte[] saltBytes = RandomNumberGenerator.GetBytes(SaltSize);

            byte[] hashBytes = Rfc2898DeriveBytes.Pbkdf2(
                password,
                saltBytes,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize
            );

            salt = Convert.ToBase64String(saltBytes);

            return Convert.ToBase64String(hashBytes);
        }

        public static bool Verify(string password, string storedHash, string storedSalt)
        {
            try
            {
                byte[] saltBytes = Convert.FromBase64String(storedSalt);
                byte[] storedHashBytes = Convert.FromBase64String(storedHash);

                byte[] enteredHashBytes = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    saltBytes,
                    Iterations,
                    HashAlgorithmName.SHA256,
                    HashSize
                );

                return CryptographicOperations.FixedTimeEquals(
                    enteredHashBytes,
                    storedHashBytes
                );
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}