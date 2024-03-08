using System;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.Crc32
{
    /// <summary>
    /// CRC32 Hash Provider
    /// </summary>
    public class CRC32Provider : ICryptoProvider
    {
        // Provider Options
        private CRC32EncryptionOptions _options;

        /// <summary>
        /// CRC32 Hash Provider
        /// </summary>
        /// <param name="options"></param>
        public CRC32Provider(CRC32EncryptionOptions options = null) {
            _options = options ?? new CRC32EncryptionOptions();
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
        /// Get CRC32 of Plain Text
        /// </summary>
        /// <param name="decryptedString"></param>
        /// <returns></returns>
        public string EncryptString(string decryptedString) {
            byte[] encrypted = EncryptBinary(_options.encoding.GetBytes(decryptedString));
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Encrypt Binary
        /// </summary>
        /// <param name="decryptedBinary"></param>
        /// <returns></returns>
        public byte[] EncryptBinary(byte[] decryptedBinary) {
            using (Crc32 crc = new Crc32())
            {
                byte[] hashBytes = crc.ComputeHash(decryptedBinary);
                return hashBytes;
            }
        }
    }
}