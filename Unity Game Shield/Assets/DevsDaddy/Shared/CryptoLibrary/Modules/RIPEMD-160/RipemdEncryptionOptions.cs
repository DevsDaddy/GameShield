using System.Text;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.RIPEMD_160
{
    [System.Serializable]
    public class RipemdEncryptionOptions : IProviderOptions
    {
        public Encoding encoding = Encoding.UTF8;
    }
}