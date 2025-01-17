using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using DevsDaddy.Shared.EventFramework.Core.Payloads;

namespace DevsDaddy.GameShield.Core.Payloads
{
    [Serializable]
    public class ApplicationClosePayload : IPayload
    {
        public bool IsQuitting = false;
        public DateTime Time;
    }

    [Serializable]
    public class ApplicationPausePayload : IPayload
    {
        public bool IsPaused = false;
        public DateTime Time;
    }

    [Serializable]
    public class ApplicationStartedPayload : IPayload
    {
        public DateTime Time;
    }

    [Serializable]
    public class ApplicationLoopUpdated : IPayload
    {
        public float DeltaTime;
    }
    
    [Serializable]
    public class ApplicationFixedLoopUpdated : IPayload
    {
        public float DeltaTime;
    }

    [Serializable]
    public class RequestCoroutine : IPayload
    {
        public string Id;
        public IEnumerator Coroutine;
    }

    [Serializable]
    public class RequestTask : IPayload
    {
        public string Id;
        public Func<CancellationToken, Task> TaskToRun;
    }

    [Serializable]
    public class StopTask : IPayload
    {
        public string Id { get; set; }
    }

    [Serializable]
    public class StopCoroutine : IPayload
    {
        public string Id;
    }
}