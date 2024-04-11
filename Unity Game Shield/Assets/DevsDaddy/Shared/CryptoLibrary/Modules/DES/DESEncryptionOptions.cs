using System.Text;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.DES
{
    [System.Serializable]
    public class DESEncryptionOptions : IProviderOptions
    {
        public string cryptoKey = "ABCDEFGH";
        public Encoding encoding = Encoding.UTF8;
    }
}