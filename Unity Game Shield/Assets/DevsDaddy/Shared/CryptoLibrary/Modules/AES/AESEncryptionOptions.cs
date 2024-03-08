using System.Security.Cryptography;
using System.Text;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.AES
{
    [System.Serializable]
    public class AESEncryptionOptions : IProviderOptions
    {
        public int bufferKeySize = 32;
        public int blockSize = 256;
        public int keySize = 256;

        public string cryptoKey = "AESPassword";
        public Encoding encoding = Encoding.UTF8;
            
        public CipherMode cipherMode = CipherMode.CBC;
        public PaddingMode paddingMode = PaddingMode.PKCS7;
    }
}