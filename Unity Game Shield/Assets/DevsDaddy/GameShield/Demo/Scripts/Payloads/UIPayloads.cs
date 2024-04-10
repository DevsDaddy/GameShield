using System;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework.Core.Payloads;

namespace DevsDaddy.GameShield.Demo.Payloads
{
    [System.Serializable]
    public class RequestWelcomeView : IPayload
    {
        public Action OnGameStarted;
    }
    
    [System.Serializable]
    public class RequestInGameView : IPayload
    {
        public Action OnGameRestarted;
        public Action OnDetectionPaused;
        public Action OnDetectionStarted;
        public Action OnReportRequested;
    }
    
    [System.Serializable]
    public class RequestDetectionView : IPayload
    {
        public SecurityWarningPayload DetectionData;
        public Action OnApply;
        public Action OnCancel;
    }
    
    [System.Serializable]
    public class RequestReportingView : IPayload
    {
        public Action OnReportSended;
        public Action<string> OnError;
    }
}