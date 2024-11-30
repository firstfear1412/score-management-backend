using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace ScoreManagement.Services.Encrypt
{
    public class EncryptService : IEncryptService
    {
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
            const KeyDerivationPrf Pbkdf2Prf = KeyDerivationPrf.HMACSHA1; // default for Rfc2898DeriveBytes
            const int Pbkdf2IterCount = 1000; // default for Rfc2898DeriveBytes
            const int Pbkdf2SubkeyLength = 256 / 8; // 256 bits
            const int SaltSize = 128 / 8; // 128 bits

            if (hashedPassword.Length != 1 + SaltSize + Pbkdf2SubkeyLength)
            {
                return false; // bad size
            }

            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(hashedPassword, 1, salt, 0, salt.Length);

            byte[] expectedSubkey = new byte[Pbkdf2SubkeyLength];
            Buffer.BlockCopy(hashedPassword, 1 + salt.Length, expectedSubkey, 0, expectedSubkey.Length);

            byte[] actualSubkey = KeyDerivation.Pbkdf2(password, salt, Pbkdf2Prf, Pbkdf2IterCount, Pbkdf2SubkeyLength);
            return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
        }

        public string Hash(string input, string algorithm)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            return Hash(Encoding.UTF8.GetBytes(input), algorithm);
        }

        public string Hash(byte[] input, string algorithm)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            using (HashAlgorithm alg = HashAlgorithm.Create(algorithm))
            {
                if (alg != null)
                {
                    byte[] hashData = alg.ComputeHash(input);
                    return BinaryToHex(hashData);
                }
                else
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Algorithm not supported: {0}", algorithm));
                }
            }
        }

        private static string BinaryToHex(byte[] data)
        {
            StringBuilder hex = new StringBuilder(data.Length * 2);
            foreach (byte b in data)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }
        public string EncryptPassword(string password)
        {
            const int Pbkdf2SubkeyLength = 256 / 8; // 256 bits
            const int SaltSize = 128 / 8; // 128 bits
            string passHash = Hash(password, "sha256");
            byte[] salt = new byte[SaltSize];
            string hashedPasswordBase64 = "";
            bool isError = false;
            string massage = "";
            try
            {
                if (!string.IsNullOrEmpty(password))
                {
                    using (var rng = RandomNumberGenerator.Create())
                    {
                        rng.GetBytes(salt);
                    }

                    using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 1000, HashAlgorithmName.SHA1))
                    {
                        byte[] subkey = pbkdf2.GetBytes(Pbkdf2SubkeyLength);

                        byte[] decodedHashedPassword = new byte[1 + SaltSize + Pbkdf2SubkeyLength];
                        decodedHashedPassword[0] = 0x00;
                        Buffer.BlockCopy(salt, 0, decodedHashedPassword, 1, SaltSize);
                        Buffer.BlockCopy(subkey, 0, decodedHashedPassword, 1 + SaltSize, Pbkdf2SubkeyLength);
                        hashedPasswordBase64 = Convert.ToBase64String(decodedHashedPassword);
                        isError = true;
                    }
                }
                else
                {
                    isError = false;
                    massage = "Password is empty";
                }
            }
            catch (Exception ex)
            {
                isError = false;
                massage = ex.Message.ToString();
            }
            return hashedPasswordBase64;
        }
    }
}
