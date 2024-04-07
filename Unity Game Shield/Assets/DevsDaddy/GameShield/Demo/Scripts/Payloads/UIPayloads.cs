using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework.Core.Payloads;

namespace DevsDaddy.GameShield.Demo.Payloads
{
    [System.Serializable]
    public class RequestWelcomeView : IPayload
    {
        
    }
    
    [System.Serializable]
    public class RequestInGameView : IPayload
    {
        
    }
    
    [System.Serializable]
    public class RequestDetectionView : IPayload
    {
        public SecurityWarningPayload DetectionData;
    }
    
    [System.Serializable]
    public class RequestReportingView : IPayload
    {
        
    }
}