using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DevsDaddy.Shared.CryptoLibrary.Core;
using DevsDaddy.Shared.CryptoLibrary.Modules.AES;
using UnityEngine;

namespace DevsDaddy.Shared.CryptoLibrary
{
    public static class CryptoFile
    {
        // Default Provider
        private static ICryptoProvider defaultProvider = new AESProvider();
        
        /// <summary>
        /// Set Default Crypto Provider
        /// </summary>
        /// <param name="provider"></param>
        public static void SetDefaultProvider(ICryptoProvider provider) {
            defaultProvider = provider;
        }
        
        /// <summary>
        /// Write Text file to path with encrypted plain-text using provided / default crypto-provider
        /// </summary>
        /// <param name="path"></param>
        /// <param name="plainText"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static bool WriteText(string path, string plainText, ICryptoProvider provider = null) {
            try {
                if (provider == null) provider = defaultProvider;
                string encryptedText = provider.EncryptString(plainText);
                File.WriteAllText(path, encryptedText);
                return true;
            }
            catch (Exception ex) {
                Debug.LogError(ex);
                return false;
            }
        }

        /// <summary>
        /// Write Binary File to path encrypted byte array using provided / default crypto-provider
        /// </summary>
        /// <param name="path"></param>
        /// <param name="byteArray"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static bool WriteBinary(string path, byte[] byteArray, ICryptoProvider provider = null) {
            try {
                if (provider == null) provider = defaultProvider;
                byte[] encryptedText = provider.EncryptBinary(byteArray);
                File.WriteAllBytes(path, encryptedText);
                return true;
            }
            catch (Exception ex) {
                Debug.LogError(ex);
                return false;
            }
        }

        /// <summary>
        /// Read encrypted file and returns decrypted plain-text using provided / default crypto-provider
        /// </summary>
        /// <param name="path"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static async Task<string> ReadTextAsync(string path, ICryptoProvider provider = null) {
            try {
                if (provider == null) provider = defaultProvider;
                string encryptedText = await File.ReadAllTextAsync(path);
                string decryptedText = provider.DecryptString(encryptedText);
                return decryptedText;
            }
            catch (Exception ex) {
                Debug.LogError(ex);
                return null;
            }
        }

        /// <summary>
        /// Read encrypted binary file and returns decrypted byte array using provided / default crypto-provider
        /// </summary>
        /// <param name="path"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static async Task<byte[]> ReadBinaryAsync(string path, ICryptoProvider provider = null) {
            try {
                if (provider == null) provider = defaultProvider;
                byte[] encryptedData = await File.ReadAllBytesAsync(path);
                byte[] decryptedData = provider.DecryptBinary(encryptedData);
                return decryptedData;
            }
            catch (Exception ex) {
                Debug.LogError(ex);
                return null;
            }
        }
        
        /// <summary>
        /// Write Text file to path with encrypted plain-text using provided / default crypto-provider
        /// </summary>
        /// <param name="path"></param>
        /// <param name="plainText"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static async Task<bool> WriteTextAsync(string path, string plainText, ICryptoProvider provider = null) {
            try {
                if (provider == null) provider = defaultProvider;
                string encryptedText = provider.EncryptString(plainText);
                await File.WriteAllTextAsync(path, encryptedText);
                return true;
            }
            catch (Exception ex) {
                Debug.LogError(ex);
                return false;
            }
        }

        /// <summary>
        /// Write Binary File to path encrypted byte array using provided / default crypto-provider
        /// </summary>
        /// <param name="path"></param>
        /// <param name="byteArray"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static async Task<bool> WriteBinaryAsync(string path, byte[] byteArray, ICryptoProvider provider = null) {
            try {
                if (provider == null) provider = defaultProvider;
                byte[] encryptedText = provider.EncryptBinary(byteArray);
                await File.WriteAllBytesAsync(path, encryptedText);
                return true;
            }
            catch (Exception ex) {
                Debug.LogError(ex);
                return false;
            }
        }

        /// <summary>
        /// Read encrypted file and returns decrypted plain-text using provided / default crypto-provider
        /// </summary>
        /// <param name="path"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static string ReadText(string path, ICryptoProvider provider = null) {
            try {
                if (provider == null) provider = defaultProvider;
                string encryptedText = File.ReadAllText(path);
                string decryptedText = provider.DecryptString(encryptedText);
                return decryptedText;
            }
            catch (Exception ex) {
                Debug.LogError(ex);
                return null;
            }
        }

        /// <summary>
        /// Read encrypted binary file and returns decrypted byte array using provided / default crypto-provider
        /// </summary>
        /// <param name="path"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static byte[] ReadBinary(string path, ICryptoProvider provider = null) {
            try {
                if (provider == null) provider = defaultProvider;
                byte[] encryptedData = File.ReadAllBytes(path);
                byte[] decryptedData = provider.DecryptBinary(encryptedData);
                return decryptedData;
            }
            catch (Exception ex) {
                Debug.LogError(ex);
                return null;
            }
        }
    }
}