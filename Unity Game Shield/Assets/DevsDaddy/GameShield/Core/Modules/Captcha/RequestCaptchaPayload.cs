using System;
using DevsDaddy.Shared.EventFramework.Core.Payloads;

namespace DevsDaddy.GameShield.Core.Modules.Captcha
{
    /// <summary>
    /// Request Captcha Payload
    /// </summary>
    [System.Serializable]
    public class RequestCaptchaPayload : IPayload
    {
        public int NumOfImages;
        
        public Action OnComplete;
        public Action<string> OnError;
        public Action OnCanceled;
    }
}