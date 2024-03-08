using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.RSA
{
    /// <summary>
    /// RSA Encryption Provider
    /// Using for encrypt/decrypt plain text or byte arrays
    /// </summary>
    public class RSAProvider : ICryptoProvider
    {
        // Provider Options
        private RSAEncryptionOptions _options;

        /// <summary>
        /// RSA Encryption Provider
        /// </summary>
        /// <param name="options"></param>
        public RSAProvider(RSAEncryptionOptions options = null) {
            _options = options ?? new RSAEncryptionOptions();
        }
        
        /// <summary>
        /// Decrypt RSA-encrypted string
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <returns></returns>
        public string DecryptString(string encryptedString) {
            byte[] decrypted = DecryptBinary(Convert.FromBase64String(encryptedString));
            return _options.encoding.GetString(decrypted);
        }

        /// <summary>
        /// Decrypt RSA-encrypted byte array
        /// </summary>
        /// <param name="encryptedBinary"></param>
        /// <returns></returns>
        public byte[] DecryptBinary(byte[] encryptedBinary) {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(_options.privateKey);
                byte[] decrypted = rsa.Decrypt(encryptedBinary, false);
                return decrypted;
            }
        }

        /// <summary>
        /// Encrypt plain-text to RSA-encrypted string
        /// </summary>
        /// <param name="decryptedString"></param>
        /// <returns></returns>
        public string EncryptString(string decryptedString) {
            byte[] encrypted = EncryptBinary(_options.encoding.GetBytes(decryptedString));
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Encrypt byte array to RSA-encrypted byte array
        /// </summary>
        /// <param name="decryptedBinary"></param>
        /// <returns></returns>
        public byte[] EncryptBinary(byte[] decryptedBinary) {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(_options.publicKey);
                byte[] encrypted = rsa.Encrypt(decryptedBinary, false);
                return encrypted;
            }
        }
        
        /// <summary>
        /// Generate Key Pair for RSA Provider
        /// </summary>
        /// <param name="keySize"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> GenerateKeyPair(int keySize)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(keySize);
            string publicKey = rsa.ToXmlString(false);
            string privateKey = rsa.ToXmlString(true);
            return new KeyValuePair<string, string>(publicKey, privateKey);
        }
    }
}