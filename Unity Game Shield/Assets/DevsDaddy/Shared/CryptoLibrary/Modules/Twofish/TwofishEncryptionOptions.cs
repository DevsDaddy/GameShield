using System.Security.Cryptography;
using System.Text;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.Twofish
{
    [System.Serializable]
    public class TwofishEncryptionOptions : IProviderOptions
    {
        public Encoding encoding = Encoding.UTF8;
        public int keySize = 256;
        public CipherMode mode = CipherMode.CBC;
        public PaddingMode padding = PaddingMode.PKCS7;

        public byte[] IV;
        public byte[] cryptoKey;
    }
}