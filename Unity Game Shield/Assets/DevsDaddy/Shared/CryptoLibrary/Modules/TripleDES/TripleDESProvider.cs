using System;
using System.Security.Cryptography;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.TripleDES
{
    /// <summary>
    /// Triple DES Modern Crypto Provider
    /// </summary>
    public class TripleDESProvider : ICryptoProvider
    {
        private TripleDESEncryptionOptions _options;

        /// <summary>
        /// Triple DES Provider
        /// </summary>
        /// <param name="options"></param>
        public TripleDESProvider(TripleDESEncryptionOptions options = null) {
            _options = options ?? new TripleDESEncryptionOptions();
        }
        
        /// <summary>
        /// Decrypt plain-text using Triple DES
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <returns></returns>
        public string DecryptString(string encryptedString) {
            byte[] txtByteData = Convert.FromBase64String(encryptedString);
            byte[] decoded = DecryptBinary(txtByteData);
            return _options.encoding.GetString(decoded);
        }

        /// <summary>
        /// Decrypt byte array using Triple DES
        /// </summary>
        /// <param name="encryptedBinary"></param>
        /// <returns></returns>
        public byte[] DecryptBinary(byte[] encryptedBinary) {
            MD5CryptoServiceProvider MD5CryptoService = new MD5CryptoServiceProvider();
            byte[] securityKeyArray = MD5CryptoService.ComputeHash(_options.encoding.GetBytes(_options.cryptoKey));
            MD5CryptoService.Clear();

            var tripleDESCryptoService = new TripleDESCryptoServiceProvider();
            tripleDESCryptoService.Key = securityKeyArray;
            tripleDESCryptoService.Mode = _options.mode;
            tripleDESCryptoService.Padding = _options.padding;

            var crytpoTransform = tripleDESCryptoService.CreateDecryptor();
            byte[] resultArray = crytpoTransform.TransformFinalBlock(encryptedBinary, 0, encryptedBinary.Length);
            tripleDESCryptoService.Clear();
            return resultArray;
        }
        
        /// <summary>
        /// Encrypt Plain-Text using Triple DES
        /// </summary>
        /// <param name="decryptedString"></param>
        /// <returns></returns>
        public string EncryptString(string decryptedString) {
            byte[] txtByteData = _options.encoding.GetBytes(decryptedString);
            byte[] encoded = EncryptBinary(txtByteData);
            return Convert.ToBase64String(encoded);
        }

        /// <summary>
        /// Encrypt Byte Array using Triple DES
        /// </summary>
        /// <param name="decryptedBinary"></param>
        /// <returns></returns>
        public byte[] EncryptBinary(byte[] decryptedBinary) {
            MD5CryptoServiceProvider MD5CryptoService = new MD5CryptoServiceProvider();
            byte[] securityKeyArray = MD5CryptoService.ComputeHash(_options.encoding.GetBytes(_options.cryptoKey));
            MD5CryptoService.Clear();
            
            var tripleDESCryptoService = new TripleDESCryptoServiceProvider();
            tripleDESCryptoService.Key = securityKeyArray;
            tripleDESCryptoService.Mode = _options.mode;
            tripleDESCryptoService.Padding = _options.padding;
            
            var crytpoTransform = tripleDESCryptoService.CreateEncryptor();
            byte[] resultArray = crytpoTransform.TransformFinalBlock(decryptedBinary, 0, decryptedBinary.Length);

            tripleDESCryptoService.Clear();
            return resultArray;
        }
    }
}