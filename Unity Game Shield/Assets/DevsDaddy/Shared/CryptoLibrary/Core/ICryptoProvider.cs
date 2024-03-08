namespace DevsDaddy.Shared.CryptoLibrary.Core
{
    /// <summary>
    /// Base Crypto-Provider Interface
    /// </summary>
    public interface ICryptoProvider
    {
        /// <summary>
        /// Decrypt String Data
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <returns></returns>
        string DecryptString(string encryptedString);
        
        /// <summary>
        /// Decrypt Binary Data
        /// </summary>
        /// <param name="encryptedBinary"></param>
        /// <returns></returns>
        byte[] DecryptBinary(byte[] encryptedBinary);
        
        /// <summary>
        /// Encrypt String
        /// </summary>
        /// <param name="decryptedString"></param>
        /// <returns></returns>
        string EncryptString(string decryptedString);
        
        /// <summary>
        /// Encrypt Binary
        /// </summary>
        /// <param name="decryptedBinary"></param>
        /// <returns></returns>
        byte[] EncryptBinary(byte[] decryptedBinary);
    }
}