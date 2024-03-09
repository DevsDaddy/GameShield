using DevsDaddy.GameShield.Core.Modules;
using DevsDaddy.Shared.EventFramework.Core.Payloads;

namespace DevsDaddy.GameShield.Core.Payloads
{
    [System.Serializable]
    public class SecurityWarningPayload : IPayload
    {
        public uint Code;
        public IShieldModule Module;
        public string Message = "";
        public bool IsCritical = false;
    }

    [System.Serializable]
    public class SecurityModuleConfigChanged : IPayload
    {
        public IShieldModule Module;
        public IShieldModuleConfig Config;
    }

    [System.Serializable]
    public class SecurityModuleInitialized : IPayload
    {
        public IShieldModule Module;
    }
    
    [System.Serializable]
    public class SecurityModuleDisconnected : IPayload
    {
        public IShieldModule Module;
    }
}