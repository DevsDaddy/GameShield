using System;
using System.Text;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.MD5
{
    /// <summary>
    /// MD5 Hash Provider (No Decryption, Only Encryption)
    /// </summary>
    public class MD5Provider : ICryptoProvider
    {
        // Provider Options
        private MD5EncryptionOptions _options;

        /// <summary>
        /// MD5 Hashing Provider
        /// </summary>
        /// <param name="options"></param>
        public MD5Provider(MD5EncryptionOptions options = null) {
            _options = options ?? new MD5EncryptionOptions();
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
        /// Encrypt plain-text and returns MD5 hash
        /// </summary>
        /// <param name="decryptedString"></param>
        /// <returns></returns>
        public string EncryptString(string decryptedString) {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = _options.encoding.GetBytes(decryptedString);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                
                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Encrypt byte array and return MD5 hashed byte array
        /// </summary>
        /// <param name="decryptedBinary"></param>
        /// <returns></returns>
        public byte[] EncryptBinary(byte[] decryptedBinary) {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(decryptedBinary);
                return hashBytes;
            }
        }
    }
}