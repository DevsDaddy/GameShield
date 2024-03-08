using System;
using System.IO;
using System.Security.Cryptography;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.Twofish
{
    /// <summary>
    /// Twofish Crypto Provider
    /// </summary>
    public class TwofishProvider : ICryptoProvider
    {
        // Provider Options and Worker
        private TwofishEncryptionOptions _options;
        private Twofish _worker;

        /// <summary>
        /// Twofish Provider
        /// </summary>
        /// <param name="options"></param>
        public TwofishProvider(TwofishEncryptionOptions options) {
            _options = options ?? new TwofishEncryptionOptions();
            _worker = new Twofish() {
                KeySize = _options.keySize,
                Mode = _options.mode,
                Padding = _options.padding
            };

            if (_options.IV == null)
                _worker.GenerateIV();
            else
                _worker.IV = _options.IV;
            
            if (_options.cryptoKey == null)
                _worker.GenerateKey();
            else
                _worker.Key = _options.cryptoKey;
        }
        
        /// <summary>
        /// Decrypt text encrypted by Twofish Provider
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <returns></returns>
        public string DecryptString(string encryptedString) {
            byte[] decrypted = DecryptBinary(Convert.FromBase64String(encryptedString));
            return _options.encoding.GetString(decrypted);
        }

        /// <summary>
        /// Decrypt binary data using Twofish Provider to byte array
        /// </summary>
        /// <param name="encryptedBinary"></param>
        /// <returns></returns>
        public byte[] DecryptBinary(byte[] encryptedBinary) {
            var inputData = new MemoryStream(encryptedBinary);
            var outputData = new MemoryStream();
            using var decryptor = _worker.CreateDecryptor();
            using (var csRead = new CryptoStream(inputData, decryptor, CryptoStreamMode.Read)) {
                csRead.CopyTo(outputData);
            }
            
            var bytes = outputData.ToArray();
            return bytes;
        }

        /// <summary>
        /// Encrypt Plain-Text using Twofish Provider
        /// </summary>
        /// <param name="decryptedString"></param>
        /// <returns></returns>
        public string EncryptString(string decryptedString) {
            byte[] encrypted = EncryptBinary(_options.encoding.GetBytes(decryptedString));
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Encrypt byte array using Twofish Provider
        /// </summary>
        /// <param name="decryptedBinary"></param>
        /// <returns></returns>
        public byte[] EncryptBinary(byte[] decryptedBinary) {
            var dataStream = new MemoryStream(decryptedBinary);
            using var encryptor = _worker.CreateEncryptor();
            var outDataStream = new MemoryStream();
            using (var csWrite = new CryptoStream(outDataStream, encryptor, CryptoStreamMode.Write)) {
                dataStream.CopyTo(csWrite);
            }
            
            var bytes = outDataStream.ToArray();
            return bytes;
        }
    }
}