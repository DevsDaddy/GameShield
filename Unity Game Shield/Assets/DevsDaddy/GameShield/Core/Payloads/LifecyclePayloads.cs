using System;
using System.Collections;
using DevsDaddy.Shared.EventFramework.Core.Payloads;

namespace DevsDaddy.GameShield.Core.Payloads
{
    [System.Serializable]
    public class ApplicationClosePayload : IPayload
    {
        public bool IsQuitting = false;
        public DateTime Time;
    }

    [System.Serializable]
    public class ApplicationPausePayload : IPayload
    {
        public bool IsPaused = false;
        public DateTime Time;
    }

    [System.Serializable]
    public class ApplicationStartedPayload : IPayload
    {
        public DateTime Time;
    }

    [System.Serializable]
    public class ApplicationLoopUpdated : IPayload
    {
        public float DeltaTime;
    }
    
    [System.Serializable]
    public class ApplicationFixedLoopUpdated : IPayload
    {
        public float DeltaTime;
    }

    [System.Serializable]
    public class RequestCoroutine : IPayload
    {
        public string Id;
        public IEnumerator Coroutine;
    }

    [System.Serializable]
    public class StopCoroutine : IPayload
    {
        public string Id;
    }
}