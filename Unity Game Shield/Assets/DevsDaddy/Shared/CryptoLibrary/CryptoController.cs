using DevsDaddy.Shared.CryptoLibrary.Core;
using DevsDaddy.Shared.CryptoLibrary.Modules.AES;

namespace DevsDaddy.Shared.CryptoLibrary
{
    public static class CryptoController
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
        /// Decrypt Encrypted Plain-Text using provided Crypto-provider or Default Crypto-provider
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static string Decrypt(string encryptedString, ICryptoProvider provider = null) {
            if (provider == null) provider = defaultProvider;
            return provider.DecryptString(encryptedString);
        }

        /// <summary>
        /// Decrypt encrypted byte array using provided Crypto-provider or Default Crypto-provider
        /// </summary>
        /// <param name="encryptedArray"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] encryptedArray, ICryptoProvider provider) {
            if (provider == null) provider = defaultProvider;
            return provider.DecryptBinary(encryptedArray);
        }

        /// <summary>
        /// Encrypt plain-text using provided Crypto-provider or Default Crypto-provider
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static string Encrypt(string plainText, ICryptoProvider provider = null) {
            if (provider == null) provider = defaultProvider;
            return provider.EncryptString(plainText);
        }
        
        /// <summary>
        /// Encrypt binary array using provided Crypto-provider or Default Crypto-provider
        /// </summary>
        /// <param name="decryptedArray"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] decryptedArray, ICryptoProvider provider = null) {
            if (provider == null) provider = defaultProvider;
            return provider.EncryptBinary(decryptedArray);
        }

        public static void Test() {
            CryptoFile.SetDefaultProvider(new AESProvider(new AESEncryptionOptions {
                cryptoKey = "key"
            }));

            string decryptedText = CryptoFile.ReadText("path_to_encrypted_file");
            bool writtenFile = CryptoFile.WriteText("path_to_encrypted_file", decryptedText);
        }
    }
}