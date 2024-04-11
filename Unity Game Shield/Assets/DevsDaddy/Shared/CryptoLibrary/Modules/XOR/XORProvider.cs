using System;
using System.Text;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.XOR
{
    /// <summary>
    /// XOR Crypto Provider
    /// Works only with strings
    /// </summary>
    public class XORProvider : ICryptoProvider
    {
        // Provider Options
        private XOREncryptionOptions _options;

        /// <summary>
        /// XOR-Provicer
        /// </summary>
        /// <param name="options"></param>
        public XORProvider(XOREncryptionOptions options = null) {
            _options = options ?? new XOREncryptionOptions();
        }
        
        /// <summary>
        /// Decrypt String (Warning! For XOR Encrypt/Decrypt methods are similar, because it is XOR)
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <returns></returns>
        public string DecryptString(string encryptedString) {
            return EncryptOrDecrypt(encryptedString, _options.cryptoKey);
        }

        /// <summary>
        /// Decrypt Binary (Not Supported for XOR)
        /// </summary>
        /// <param name="encryptedBinary"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public byte[] DecryptBinary(byte[] encryptedBinary) {
            throw new Exception($"Failed to decrypt byte array, because {this.GetType()} support only plain text.");
        }

        /// <summary>
        /// Encrypt String (Warning! For XOR Encrypt/Decrypt methods are similar, because it is XOR)
        /// </summary>
        /// <param name="decryptedString"></param>
        /// <returns></returns>
        public string EncryptString(string decryptedString) {
            return EncryptOrDecrypt(decryptedString, _options.cryptoKey);
        }

        /// <summary>
        /// Encrypt Binary (Not Supported for XOR)
        /// </summary>
        /// <param name="decryptedBinary"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public byte[] EncryptBinary(byte[] decryptedBinary) {
            throw new Exception($"Failed to decrypt byte array, because {this.GetType()} support only plain text.");
        }
        
        
        /// <summary>
        /// Encrypt / Decrypt via XOR-mechanism
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private string EncryptOrDecrypt(string text, string key)
        {
            var result = new StringBuilder();
            for (int c = 0; c < text.Length; c++)
            {
                char character = text[c];
                uint charCode = (uint)character;
                int keyPosition = c % key.Length;
                char keyChar = key[keyPosition];
                uint keyCode = (uint)keyChar;
                uint combinedCode = charCode ^ keyCode;
                char combinedChar = (char)combinedCode;
                result.Append(combinedChar);
            }
            return result.ToString();
        }
    }
}