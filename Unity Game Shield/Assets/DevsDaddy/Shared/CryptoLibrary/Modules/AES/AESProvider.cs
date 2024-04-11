using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.AES
{
    /// <summary>
    /// AES Encryption Provider.
    /// Using for encrypt/decrypt plain text or byte arrays
    /// </summary>
    public class AESProvider : ICryptoProvider
    {
        private AESEncryptionOptions _options;
        
        /// <summary>
        /// AES Cryptography Provider
        /// </summary>
        /// <param name="options"></param>
        public AESProvider(AESEncryptionOptions options = null) {
            _options = options ?? new AESEncryptionOptions();
        }

        /// <summary>
        /// Decrypt AES-encrypted base-64 string to plain-text
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <returns></returns>
        public string DecryptString(string encryptedString) {
            byte[] decrypted = DecryptBinary(Convert.FromBase64String(encryptedString));
            return _options.encoding.GetString(decrypted);
        }

        /// <summary>
        /// Decrypt AES-encrypted binary
        /// </summary>
        /// <param name="encryptedBinary"></param>
        /// <returns></returns>
        public byte[] DecryptBinary(byte[] encryptedBinary) {
            RijndaelManaged rij = setupRijndaelManaged;

            List<byte> compile = new List<byte>(encryptedBinary);

            // First 32 bytes are salt.
            List<byte> salt = compile.GetRange(0, _options.bufferKeySize);
            // Second 32 bytes are IV.
            List<byte> iv = compile.GetRange(_options.bufferKeySize, _options.bufferKeySize);
            rij.IV = iv.ToArray();

            Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(_options.cryptoKey, salt.ToArray());
            byte[] bufferKey = deriveBytes.GetBytes(_options.bufferKeySize);    // Convert 32 bytes of salt to password
            rij.Key = bufferKey;

            byte[] plain = compile.GetRange(_options.bufferKeySize * 2, compile.Count - (_options.bufferKeySize * 2)).ToArray();

            using (ICryptoTransform decrypt = rij.CreateDecryptor(rij.Key, rij.IV))
            {
                byte[] dest = decrypt.TransformFinalBlock(plain, 0, plain.Length);
                return dest;
            }
        }

        /// <summary>
        /// Encrypt plain-text to AES-encrypted base-64 string
        /// </summary>
        /// <param name="decryptedString"></param>
        /// <returns></returns>
        public string EncryptString(string decryptedString) {
            byte[] encrypted = EncryptBinary(_options.encoding.GetBytes(decryptedString));
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Encrypt byte array to AES-encrypted byte array
        /// </summary>
        /// <param name="decryptedBinary"></param>
        /// <returns></returns>
        public byte[] EncryptBinary(byte[] decryptedBinary) {
            RijndaelManaged rij = setupRijndaelManaged;

            // A pseudorandom number is newly generated based on the inputted password
            Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(_options.cryptoKey, _options.bufferKeySize);
            // The missing parts are specified in advance to fill in 0 length
            byte[] salt = new byte[_options.bufferKeySize];
            // Rfc2898DeriveBytes gets an internally generated salt
            salt = deriveBytes.Salt;
            // The 32-byte data extracted from the generated pseudorandom number is used as a password
            byte[] bufferKey = deriveBytes.GetBytes(_options.bufferKeySize);

            rij.Key = bufferKey;
            rij.GenerateIV();

            using (ICryptoTransform encrypt = rij.CreateEncryptor(rij.Key, rij.IV))
            {
                byte[] dest = encrypt.TransformFinalBlock(decryptedBinary, 0, decryptedBinary.Length);
                // first 32 bytes of salt and second 32 bytes of IV for the first 64 bytes
                List<byte> compile = new List<byte>(salt);
                compile.AddRange(rij.IV);
                compile.AddRange(dest);
                return compile.ToArray();
            }
        }
        
        /// <summary>
        /// Setup Manager for AES
        /// </summary>
        private RijndaelManaged setupRijndaelManaged
        {
            get
            {
                RijndaelManaged rij = new RijndaelManaged();
                rij.BlockSize = _options.blockSize;
                rij.KeySize = _options.keySize;
                rij.Mode = _options.cipherMode;
                rij.Padding = _options.paddingMode;
                return rij;
            }
        }
    }
}