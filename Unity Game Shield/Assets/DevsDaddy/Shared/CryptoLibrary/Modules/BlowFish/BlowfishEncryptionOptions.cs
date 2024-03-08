using System.Text;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.BlowFish
{
    [System.Serializable]
    public class BlowfishEncryptionOptions : IProviderOptions
    {
        public Encoding encoding = Encoding.UTF8;
        public string cryptoKey = "MyCryptoKey";
    }
}