using System;
using System.Security.Cryptography;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.PBKDF2
{
    /// <summary>
    /// PBKDF2 Hashing Provider
    /// </summary>
    public class PBKDF2Provider : ICryptoProvider
    {
        // Provider Options
        private PBKDF2EncryptionOptions _options;
        private string currentSalt;

        /// <summary>
        /// PBKDF2 Provider
        /// </summary>
        /// <param name="options"></param>
        public PBKDF2Provider(PBKDF2EncryptionOptions options = null) {
            _options = options ?? new PBKDF2EncryptionOptions();
        }
        
        /// <summary>
        /// Decrypt String (Not Supported for hashing)
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string DecryptString(string encryptedString) {
            throw new Exception($"Failed to decrypt text, because {this.GetType()} doesn't support decryption.");
        }

        /// <summary>
        /// Decrypt Binary (Not Supported for hashing)
        /// </summary>
        /// <param name="encryptedBinary"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public byte[] DecryptBinary(byte[] encryptedBinary) {
            throw new Exception($"Failed to decrypt text, because {this.GetType()} doesn't support decryption.");
        }

        /// <summary>
        /// Get PBKDF2 Hash from plain-text
        /// </summary>
        /// <param name="decryptedString"></param>
        /// <returns></returns>
        public string EncryptString(string decryptedString) {
            if (string.IsNullOrEmpty(currentSalt) || _options.autoRegenerateSalt) GenerateSalt();
            return calculateHash(_options.hashIterations, decryptedString);
        }

        /// <summary>
        /// Get PBKDF2 Hash from byte array
        /// </summary>
        /// <param name="decryptedBinary"></param>
        /// <returns></returns>
        public byte[] EncryptBinary(byte[] decryptedBinary) {
            if (string.IsNullOrEmpty(currentSalt) || _options.autoRegenerateSalt) GenerateSalt();
            return calculateHash(_options.hashIterations, decryptedBinary);
        }
        
        /// <summary>
        /// Generate Hash Salt
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public string GenerateSalt()
        {
            if (_options.saltSize < 1) 
                throw new InvalidOperationException(string.Format("Cannot generate a salt of size {0}, use a value greater than 1, recommended: 16", _options.saltSize));

            var rand = RandomNumberGenerator.Create();
            var ret = new byte[_options.saltSize];
            rand.GetBytes(ret);
            
            currentSalt = string.Format("{0}.{1}", _options.hashIterations, Convert.ToBase64String(ret));
            return currentSalt;
        }
        
        /// <summary>
        /// Calculate Text Hash
        /// </summary>
        /// <param name="iteration"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private string calculateHash(int iteration, string text)
        {
            byte[] saltBytes = _options.encoding.GetBytes(currentSalt);
            var pbkdf2 = new Rfc2898DeriveBytes(text, saltBytes, iteration);
            var key = pbkdf2.GetBytes(64);
            return Convert.ToBase64String(key);
        }

        /// <summary>
        /// Calculate Binary Hash
        /// </summary>
        /// <param name="iteration"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private byte[] calculateHash(int iteration, byte[] data) {
            byte[] saltBytes = _options.encoding.GetBytes(currentSalt);
            var pbkdf2 = new Rfc2898DeriveBytes(data, saltBytes, iteration);
            var key = pbkdf2.GetBytes(64);
            return key;
        }
        
        /// <summary>
        /// Expand Salt
        /// </summary>
        /// <exception cref="FormatException"></exception>
        private void expandSalt()
        {
            try
            {
                var i = currentSalt.IndexOf('.');
                _options.hashIterations = int.Parse(currentSalt.Substring(0, i), System.Globalization.NumberStyles.Number);
            }
            catch (Exception)
            {
                throw new FormatException("The salt was not in an expected format of {int}.{string}");
            }
        }
    }
}