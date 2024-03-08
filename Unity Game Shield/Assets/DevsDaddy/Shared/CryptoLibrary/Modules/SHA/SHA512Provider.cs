using System;
using System.Security.Cryptography;
using System.Text;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.SHA
{
    /// <summary>
    /// SHA512 Hashing Provider
    /// </summary>
    public class SHA512Provider : ICryptoProvider
    {
        private SHAEncryptionOptions _options;

        /// <summary>
        /// SHA512 Provider
        /// </summary>
        /// <param name="options"></param>
        public SHA512Provider(SHAEncryptionOptions options = null) {
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
        /// Get SHA512 Hash for Plain-text
        /// </summary>
        /// <param name="decryptedString"></param>
        /// <returns></returns>
        public string EncryptString(string decryptedString) {
            using (SHA512Managed sha256 = new SHA512Managed())
            {
                var hash = sha256.ComputeHash(_options.encoding.GetBytes(decryptedString));
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
        /// Get SHA512 Hash for Byte array
        /// </summary>
        /// <param name="decryptedBinary"></param>
        /// <returns></returns>
        public byte[] EncryptBinary(byte[] decryptedBinary) {
            using (SHA512Managed sha512 = new SHA512Managed())
            {
                var hash = sha512.ComputeHash(decryptedBinary);
                return hash;
            }
        }
    }
}