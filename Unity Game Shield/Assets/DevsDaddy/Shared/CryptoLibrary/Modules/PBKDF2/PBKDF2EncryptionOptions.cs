using System.Text;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.PBKDF2
{
    [System.Serializable]
    public class PBKDF2EncryptionOptions : IProviderOptions
    {
        public bool autoRegenerateSalt = true;
        public Encoding encoding = Encoding.UTF8;
        public int hashIterations = 100000;
        public int saltSize = 34;
    }
}