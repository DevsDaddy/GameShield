using System;
using DevsDaddy.Shared.EventFramework.Core.Payloads;

namespace DevsDaddy.GameShield.Core.Reporter
{
    [System.Serializable]
    public class ReportingPayload : IPayload
    {
        public ReportData Data;
        public Action<string> OnComplete;
        public Action<string> OnError;
    }
}