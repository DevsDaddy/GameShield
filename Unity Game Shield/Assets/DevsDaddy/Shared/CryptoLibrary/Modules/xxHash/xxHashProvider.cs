using System;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.xxHash
{
    /// <summary>
    /// xxHash Provider
    /// Can Hash for plain text or byte array
    /// </summary>
    public class xxHashProvider : ICryptoProvider
    {
        // Provider Options
        private xxHashEncryptionOptions _options;

        /// <summary>
        /// xxHash Provider
        /// </summary>
        /// <param name="options"></param>
        public xxHashProvider(xxHashEncryptionOptions options = null) {
            _options = options ?? new xxHashEncryptionOptions();
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
        /// Get xxHash for plain-text
        /// </summary>
        /// <param name="decryptedString"></param>
        /// <returns></returns>
        public string EncryptString(string decryptedString) {
            return Convert.ToString(EncryptBinary(_options.encoding.GetBytes(decryptedString)));
        }

        /// <summary>
        /// Get xxHash for byte array
        /// </summary>
        /// <param name="decryptedBinary"></param>
        /// <returns></returns>
        public byte[] EncryptBinary(byte[] decryptedBinary) {
            uint h32;
            int index = 0;

            if (_options.length >= 16)
            {
                int limit = _options.length - 16;
                uint v1 = _options.seed + _options.PRIME32_1 + _options.PRIME32_2;
                uint v2 = _options.seed + _options.PRIME32_2;
                uint v3 = _options.seed;
                uint v4 = _options.seed - _options.PRIME32_1;

                do
                {
                    uint read_value = (uint)(decryptedBinary[index++] | decryptedBinary[index++] << 8 | decryptedBinary[index++] << 16 | decryptedBinary[index++] << 24);
                    v1 += read_value * _options.PRIME32_2;
                    v1 = (v1 << 13) | (v1 >> 19);
                    v1 *= _options.PRIME32_1;

                    read_value = (uint)(decryptedBinary[index++] | decryptedBinary[index++] << 8 | decryptedBinary[index++] << 16 | decryptedBinary[index++] << 24);
                    v2 += read_value * _options.PRIME32_2;
                    v2 = (v2 << 13) | (v2 >> 19);
                    v2 *= _options.PRIME32_1;

                    read_value = (uint)(decryptedBinary[index++] | decryptedBinary[index++] << 8 | decryptedBinary[index++] << 16 | decryptedBinary[index++] << 24);
                    v3 += read_value * _options.PRIME32_2;
                    v3 = (v3 << 13) | (v3 >> 19);
                    v3 *= _options.PRIME32_1;

                    read_value = (uint)(decryptedBinary[index++] | decryptedBinary[index++] << 8 | decryptedBinary[index++] << 16 | decryptedBinary[index++] << 24);
                    v4 += read_value * _options.PRIME32_2;
                    v4 = (v4 << 13) | (v4 >> 19);
                    v4 *= _options.PRIME32_1;

                } while (index <= limit);

                h32 = ((v1 << 1) | (v1 >> 31)) + ((v2 << 7) | (v2 >> 25)) + ((v3 << 12) | (v3 >> 20)) + ((v4 << 18) | (v4 >> 14));
            }
            else
            {
                h32 = _options.seed + _options.PRIME32_5;
            }

            h32 += (uint)_options.length;

            while (index <= _options.length - 4)
            {
                h32 += (uint)(decryptedBinary[index++] | decryptedBinary[index++] << 8 | decryptedBinary[index++] << 16 | decryptedBinary[index++] << 24) * _options.PRIME32_3;
                h32 = ((h32 << 17) | (h32 >> 15)) * _options.PRIME32_4;
            }

            while (index < _options.length)
            {
                h32 += decryptedBinary[index] * _options.PRIME32_5;
                h32 = ((h32 << 11) | (h32 >> 21)) * _options.PRIME32_1;
                index++;
            }

            h32 ^= h32 >> 15;
            h32 *= _options.PRIME32_2;
            h32 ^= h32 >> 13;
            h32 *= _options.PRIME32_3;
            h32 ^= h32 >> 16;

            return BitConverter.GetBytes(h32);
        }
    }
}