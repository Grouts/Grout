using System;
using System.Security.Cryptography;
using System.Text;

namespace Grout.Base
{
    /// <summary>
    ///     Class that handles Encryption and Decryption of data
    /// </summary>
    public class Cryptography : IDisposable
    {
        private bool _disposed;
        public static string Key = "9hn8J64dJXvZ9";


        #region Public Methods

        /// <summary>
        ///     Encrypts the given text using the given Key
        /// </summary>
        /// <param name="plainText">Text to be Encrypted</param>
        /// <param name="key">Encryption Key</param>
        /// <returns>Byte array</returns>
        public byte[] Encryption(string plainText)
        {
            if (String.IsNullOrWhiteSpace(plainText))
                throw new ArgumentNullException("plainText");
            if (String.IsNullOrWhiteSpace(Key))
                throw new ArgumentNullException("key");

            var des = CreateDES(Key);
            var ct = des.CreateEncryptor();
            var input = Encoding.Unicode.GetBytes(plainText);
            return ct.TransformFinalBlock(input, 0, input.Length);
        }

        /// <summary>
        ///     Decrypts the given string using the given Key
        /// </summary>
        /// <param name="cypherText">Encrypted string</param>
        /// <param name="key">Decryption Key</param>
        /// <returns>String</returns>
        public string Decryption(string cypherText)
        {
            if (String.IsNullOrWhiteSpace(cypherText))
                throw new ArgumentNullException("cypherText");
            if (String.IsNullOrWhiteSpace(Key))
                throw new ArgumentNullException("key");

            try
            {
                var encryptedBytes = Convert.FromBase64String(cypherText);
                var des = CreateDES(Key);
                var ct = des.CreateDecryptor();
                var output = ct.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                return Encoding.Unicode.GetString(output);
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }

        #endregion Public Methods

        #region Private Methods

        private TripleDES CreateDES(string key)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            TripleDES des = new TripleDESCryptoServiceProvider();
            des.Key = md5.ComputeHash(Encoding.Unicode.GetBytes(key));
            des.IV = new byte[des.BlockSize / 8];
            return des;
        }

        #endregion Private Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }
                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
}