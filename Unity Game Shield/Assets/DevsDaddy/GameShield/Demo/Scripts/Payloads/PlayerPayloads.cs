using DevsDaddy.GameShield.Demo.Enums;
using DevsDaddy.Shared.EventFramework.Core.Payloads;

namespace DevsDaddy.GameShield.Demo.Payloads
{
    [System.Serializable]
    public class OnPlayerStateChanged : IPayload
    {
        public BaseState State;
    }
}