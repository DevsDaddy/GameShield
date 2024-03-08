using System.Text;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.xxHash
{
    [System.Serializable]
    public class xxHashEncryptionOptions : IProviderOptions
    {
        public uint PRIME32_1 = 2654435761U;
        public uint PRIME32_2 = 2246822519U;
        public uint PRIME32_3 = 3266489917U;
        public uint PRIME32_4 = 668265263U;
        public uint PRIME32_5 = 374761393U;

        public int length = 16;
        public uint seed = 0;
        
        public Encoding encoding = Encoding.UTF8;
    }
}