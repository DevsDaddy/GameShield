using System;
using System.Security.Cryptography;
using System.Text;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.SHA
{
    /// <summary>
    /// SHA1 Hashing Provider
    /// </summary>
    public class SHA1Provider : ICryptoProvider
    {
        private SHAEncryptionOptions _options;

        /// <summary>
        /// SHA1 Provider
        /// </summary>
        /// <param name="options"></param>
        public SHA1Provider(SHAEncryptionOptions options = null) {
            _options = options ?? new SHAEncryptionOptions();
        }
        
        /// <summary>
        /// Decrypt plain-text (Not Supported for Hashing)
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string DecryptString(string encryptedString) {
            throw new Exception($"Failed to decrypt text, because {this.GetType()} doesn't support decryption.");
        }

        /// <summary>
        /// Decrypt Binary Data (Not Supported for Hashing)
        /// </summary>
        /// <param name="encryptedBinary"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public byte[] DecryptBinary(byte[] encryptedBinary) {
            throw new Exception($"Failed to decrypt text, because {this.GetType()} doesn't support decryption.");
        }

        /// <summary>
        /// Get SHA1 Hash for Plain-text
        /// </summary>
        /// <param name="decryptedString"></param>
        /// <returns></returns>
        public string EncryptString(string decryptedString) {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(_options.encoding.GetBytes(decryptedString));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Get SHA1 Hash for Byte array
        /// </summary>
        /// <param name="decryptedBinary"></param>
        /// <returns></returns>
        public byte[] EncryptBinary(byte[] decryptedBinary) {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(decryptedBinary);
                return hash;
            }
        }
    }
}