using System;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.Base64
{
    /// <summary>
    /// Base64 Encryption Provider.
    /// Using for encrypt/decrypt plain text or byte arrays
    /// </summary>
    public class Base64Provider : ICryptoProvider
    {
        private Base64EncryptionOptions _options;
        
        /// <summary>
        /// Base64 Encryption Provider
        /// </summary>
        /// <param name="options"></param>
        public Base64Provider(Base64EncryptionOptions options = null) {
            _options = options ?? new Base64EncryptionOptions();
        }
        
        /// <summary>
        /// Decrypt Base64 string to plain text
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <returns></returns>
        public string DecryptString(string encryptedString) {
            byte[] decodedBytes = Convert.FromBase64String (encryptedString);
            string decodedText = _options.encoding.GetString (decodedBytes);
            return decodedText;
        }

        /// <summary>
        /// Decrypt Base64 byte array to decrypted byte array
        /// </summary>
        /// <param name="encryptedBinary"></param>
        /// <returns></returns>
        public byte[] DecryptBinary(byte[] encryptedBinary) {
            string encoded = _options.encoding.GetString(encryptedBinary);
            byte[] decodedBytes = Convert.FromBase64String(encoded);
            return decodedBytes;
        }

        /// <summary>
        /// Encrypt plain-text to base64 string
        /// </summary>
        /// <param name="decryptedString"></param>
        /// <returns></returns>
        public string EncryptString(string decryptedString) {
            byte[] bytesToEncode = _options.encoding.GetBytes (decryptedString);
            string encodedText = Convert.ToBase64String (bytesToEncode);
            return encodedText;
        }

        /// <summary>
        /// Encrypt byte array to Base64 binary
        /// </summary>
        /// <param name="decryptedBinary"></param>
        /// <returns></returns>
        public byte[] EncryptBinary(byte[] decryptedBinary) {
            string encodedText = Convert.ToBase64String(decryptedBinary);
            byte[] encodedBytes = _options.encoding.GetBytes (encodedText);
            return encodedBytes;
        }
    }
}