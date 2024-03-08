using System;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.RIPEMD_160
{
    /// <summary>
    /// Old RIPEMD-160 Hashing Provider
    /// </summary>
    public class RipemdProvider : ICryptoProvider
    {
        // Provider Options
        private RipemdEncryptionOptions _options;

        /// <summary>
        /// RIPEMD-160 Provider
        /// </summary>
        /// <param name="options"></param>
        public RipemdProvider(RipemdEncryptionOptions options = null) {
            _options = options ?? new RipemdEncryptionOptions();
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
        /// Get RIPEMD-160 Hash from String
        /// </summary>
        /// <param name="decryptedString"></param>
        /// <returns></returns>
        public string EncryptString(string decryptedString) {
            byte[] encrypted = EncryptBinary(_options.encoding.GetBytes(decryptedString));
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Get RIPEMD-160 Hash from Byte Array
        /// </summary>
        /// <param name="decryptedBinary"></param>
        /// <returns></returns>
        public byte[] EncryptBinary(byte[] decryptedBinary) {
            using (RIPEMD160 r160 = RIPEMD160.Create())
            {
                byte[] hashBytes = r160.ComputeHash(decryptedBinary);
                return hashBytes;
            }
        }
    }
}