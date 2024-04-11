using System.Text;
using DevsDaddy.Shared.CryptoLibrary.Core;

namespace DevsDaddy.Shared.CryptoLibrary.Modules.RSA
{
    [System.Serializable]
    public class RSAEncryptionOptions : IProviderOptions
    {
        public string publicKey = "RSAPublicKey";
        public string privateKey = "RSAPrivateKey";
        
        public Encoding encoding = Encoding.UTF8;
    }
}