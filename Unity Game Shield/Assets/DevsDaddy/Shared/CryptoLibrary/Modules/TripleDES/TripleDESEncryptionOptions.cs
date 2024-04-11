using System.Security.Cryptography;
using System.Text;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.TripleDES
{
    [System.Serializable]
    public class TripleDESEncryptionOptions : IProviderOptions
    {
        public string cryptoKey = "ABCDEFGH";
        public Encoding encoding = Encoding.UTF8;

        public CipherMode mode = CipherMode.ECB;
        public PaddingMode padding = PaddingMode.PKCS7;
    }
}