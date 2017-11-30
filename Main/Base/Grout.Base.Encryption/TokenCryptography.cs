using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Grout.Base.Encryption
{
    public class TokenCryptography : ITokenCryptography
    {
        private static readonly byte[] InitVectorBytes =
        {
            83, 41, 145, 78, 53, 109, 228, 73, 173, 142, 47, 11, 90, 58,
            62, 218
        };

        private static readonly byte[] KeyBytes =
        {
            54, 227, 16, 11, 121, 32, 118, 118, 35, 101, 56, 146, 100, 220, 240,
            141, 172, 243, 93, 231, 110, 77, 156, 104, 26, 50, 231, 96, 101, 218, 107, 112
        };

        public string Encrypt(string plainText, string ip)
        {
            if (String.IsNullOrWhiteSpace(plainText))
            {
                return String.Empty;
            }

            var concatPlainext = String.Format("plainText={0};IP={1}", plainText, ip);
            return DoEncryption(concatPlainext);
        }

        /// <summary>
        /// Encrypt the string using RijndaelManaged algorithm
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <returns></returns>
        public string DoEncryption(string encryptedString)
        {
            if (String.IsNullOrWhiteSpace(encryptedString))
            {
                return String.Empty;
            }

            var plainTextBytes = Encoding.UTF8.GetBytes(encryptedString);
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC };
            var encryptor = symmetricKey.CreateEncryptor(KeyBytes, InitVectorBytes);
            var memoryStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();

            var cipherTextBytes = memoryStream.ToArray();

            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(cipherTextBytes);
        }

        public string Decrypt(string encryptToken)
        {
            if (String.IsNullOrWhiteSpace(encryptToken))
            {
                return String.Empty;
            }

            var cipherTextBytes = Convert.FromBase64String(encryptToken);
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC };
            var decryptor = symmetricKey.CreateDecryptor(KeyBytes, InitVectorBytes);
            var memoryStream = new MemoryStream(cipherTextBytes);
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            var plainTextBytes = new byte[cipherTextBytes.Length];
            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

            memoryStream.Close();
            cryptoStream.Close();

            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }
    }
}