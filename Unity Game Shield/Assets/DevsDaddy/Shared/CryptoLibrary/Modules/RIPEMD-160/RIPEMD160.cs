namespace DevsDaddy.Shared.CryptoLibrary.Modules.RIPEMD_160
{
    public abstract class RIPEMD160 : System.Security.Cryptography.HashAlgorithm
    {
        public RIPEMD160() { }

        public new static RIPEMD160 Create()
        {
            return new RIPEMD160Managed();
        }

        public new static RIPEMD160 Create(string hashname)
        {
            return new RIPEMD160Managed();            
        }
    }
}