using System;
using System.IO;
using System.Security.Cryptography;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.DES
{
    /// <summary>
    /// DES Encryption Provider.
    /// Using for encrypt/decrypt plain text or byte arrays
    /// </summary>
    public class DESProvider : ICryptoProvider
    {
        // Encryption Options
        private DESEncryptionOptions _options;

        /// <summary>
        /// DES Encryption Provider
        /// </summary>
        /// <param name="options"></param>
        public DESProvider(DESEncryptionOptions options = null) {
            _options = options ?? new DESEncryptionOptions();
        }
        
        /// <summary>
        /// Decrypt encrypted string to plain text using DES
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <returns></returns>
        public string DecryptString(string encryptedString) {
            byte[] txtByteData = Convert.FromBase64String(encryptedString);
            byte[] decoded = DecryptBinary(txtByteData);
            return _options.encoding.GetString(decoded);
        }

        /// <summary>
        /// Decrypt encrypted byte array to decrypted byte array using DES
        /// </summary>
        /// <param name="encryptedBinary"></param>
        /// <returns></returns>
        public byte[] DecryptBinary(byte[] encryptedBinary) {
            byte[] keyByteData = _options.encoding.GetBytes(_options.cryptoKey);
 
            DESCryptoServiceProvider DEScryptoProvider = new DESCryptoServiceProvider();
            ICryptoTransform trnsfrm = DEScryptoProvider.CreateDecryptor(keyByteData, keyByteData);
            CryptoStreamMode mode = CryptoStreamMode.Write;
 
            //Set up Stream & Write Encript data
            MemoryStream mStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(mStream, trnsfrm, mode);
            cStream.Write(encryptedBinary, 0, encryptedBinary.Length);
            cStream.FlushFinalBlock();
 
            //Read Ecncrypted Data From Memory Stream
            byte[] result = new byte[mStream.Length];
            mStream.Position = 0;
            mStream.Read(result, 0, result.Length);
 
            return result;
        }

        /// <summary>
        /// Encrypt plain-text using DES
        /// </summary>
        /// <param name="decryptedString"></param>
        /// <returns></returns>
        public string EncryptString(string decryptedString) {
            byte[] txtByteData = _options.encoding.GetBytes(decryptedString);
            byte[] encoded = EncryptBinary(txtByteData);
            return Convert.ToBase64String(encoded);
        }

        /// <summary>
        /// Encrypt byte-array using DES
        /// </summary>
        /// <param name="decryptedBinary"></param>
        /// <returns></returns>
        public byte[] EncryptBinary(byte[] decryptedBinary) {
            byte[] keyByteData = _options.encoding.GetBytes(_options.cryptoKey);
            DESCryptoServiceProvider DEScryptoProvider = new DESCryptoServiceProvider();
            ICryptoTransform trnsfrm = DEScryptoProvider.CreateEncryptor(keyByteData, keyByteData);
            CryptoStreamMode mode = CryptoStreamMode.Write;
 
            //Set up Stream & Write Encript data
            MemoryStream mStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(mStream, trnsfrm, mode);
            cStream.Write(decryptedBinary, 0, decryptedBinary.Length);
            cStream.FlushFinalBlock();
 
            //Read Ecncrypted Data From Memory Stream
            byte[] result = new byte[mStream.Length];
            mStream.Position = 0;
            mStream.Read(result, 0, result.Length);

            return result;
        }
    }
}