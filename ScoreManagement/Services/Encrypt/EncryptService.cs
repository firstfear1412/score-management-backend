using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace ScoreManagement.Services
{
    public class EncryptService : IEncryptService
    {
        private readonly IConfiguration _configuration;

        public EncryptService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            if (hashedPassword == null)
            {
                throw new ArgumentNullException(nameof(hashedPassword));
            }
            if (providedPassword == null)
            {
                throw new ArgumentNullException(nameof(providedPassword));
            }

            byte[] decodedHashedPassword = Convert.FromBase64String(hashedPassword);
            switch (decodedHashedPassword[0])
            {
                case 0x00:
                    return VerifyHashedPasswordV2(decodedHashedPassword, providedPassword);
                default:
                    return false; // unknown format marker
            }
        }

        private bool VerifyHashedPasswordV2(byte[] hashedPassword, string password)
        {
            const KeyDerivationPrf Pbkdf2Prf = KeyDerivationPrf.HMACSHA256; // default for Rfc2898DeriveBytes
            int pbkdf2IterCount = int.Parse(_configuration["Encryption:Pbkdf2IterCount"]!);
            int pbkdf2SubkeyLength = int.Parse(_configuration["Encryption:Pbkdf2SubkeyLength"]!);
            int saltSize = int.Parse(_configuration["Encryption:SaltSize"]!);

            if (hashedPassword.Length != 1 + saltSize + pbkdf2SubkeyLength)
            {
                return false; // bad size
            }

            byte[] salt = new byte[saltSize];
            Buffer.BlockCopy(hashedPassword, 1, salt, 0, salt.Length);

            byte[] expectedSubkey = new byte[pbkdf2SubkeyLength];
            Buffer.BlockCopy(hashedPassword, 1 + salt.Length, expectedSubkey, 0, expectedSubkey.Length);

            byte[] actualSubkey = KeyDerivation.Pbkdf2(password, salt, Pbkdf2Prf, pbkdf2IterCount, pbkdf2SubkeyLength);
            return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
        }

        public string EncryptPassword(string password)
        {
            int pbkdf2IterCount = int.Parse(_configuration["Encryption:Pbkdf2IterCount"]!);
            int pbkdf2SubkeyLength = int.Parse(_configuration["Encryption:Pbkdf2SubkeyLength"]!);
            int saltSize = int.Parse(_configuration["Encryption:SaltSize"]!);
            byte[] salt = new byte[saltSize];
            string hashedPasswordBase64 = "";
            if (!string.IsNullOrEmpty(password))
            {
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, pbkdf2IterCount, HashAlgorithmName.SHA256))
                {
                    byte[] subkey = pbkdf2.GetBytes(pbkdf2SubkeyLength);

                    byte[] decodedHashedPassword = new byte[1 + saltSize + pbkdf2SubkeyLength];
                    decodedHashedPassword[0] = 0x00;
                    Buffer.BlockCopy(salt, 0, decodedHashedPassword, 1, saltSize);
                    Buffer.BlockCopy(subkey, 0, decodedHashedPassword, 1 + saltSize, pbkdf2SubkeyLength);
                    hashedPasswordBase64 = Convert.ToBase64String(decodedHashedPassword);
                }
            }
            return hashedPasswordBase64;
        }
    }
}
