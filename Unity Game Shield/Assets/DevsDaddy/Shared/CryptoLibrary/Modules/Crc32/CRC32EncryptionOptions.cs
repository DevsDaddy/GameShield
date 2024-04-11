using System.Text;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.Crc32
{
    [System.Serializable]
    public class CRC32EncryptionOptions : IProviderOptions
    {
        public Encoding encoding = Encoding.UTF8;
    }
}