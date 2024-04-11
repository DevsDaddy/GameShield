using System.Text;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.SHA
{
    [System.Serializable]
    public class SHAEncryptionOptions : IProviderOptions
    {
        public Encoding encoding = Encoding.UTF8;
    }
}