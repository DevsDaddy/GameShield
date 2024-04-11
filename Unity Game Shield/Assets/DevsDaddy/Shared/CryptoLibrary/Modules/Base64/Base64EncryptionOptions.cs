using System.Text;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.Base64
{
    [System.Serializable]
    public class Base64EncryptionOptions : IProviderOptions
    {
        public Encoding encoding = Encoding.UTF8;
    }
}