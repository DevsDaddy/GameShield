using System;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework.Core.Payloads;
using UnityEngine;

namespace DevsDaddy.GameShield.Demo.Payloads
{
    [System.Serializable]
    public class RequestWelcomeView : IPayload
    {
        public Action OnGameStarted;
        public Action OnCaptchaRequested;
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

    [System.Serializable]
    public class RequestDialogue : IPayload
    {
        public string Title;
        public string Message;
        public Color MessageColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        public Action OnComplete;
    }
}