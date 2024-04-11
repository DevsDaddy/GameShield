using System.Text;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.XOR
{
    [System.Serializable]
    public class XOREncryptionOptions : IProviderOptions
    {
        public string cryptoKey = "123456";
        public Encoding encoding = Encoding.UTF8;
    }
}