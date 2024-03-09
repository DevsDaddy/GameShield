using DevsDaddy.GameShield.Core.Modules;

namespace DevsDaddy.GameShield.Core.Payloads
{
    [System.Serializable]
    public class SecurityWarningPayload
    {
        public uint Code;
        public IShieldModule Module;
        public string Message = "";
        public bool IsCritical = false;
    }
}