using System.Text;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.MD5
{
    [System.Serializable]
    public class MD5EncryptionOptions : IProviderOptions
    {
        public Encoding encoding = Encoding.UTF8;
    }
}