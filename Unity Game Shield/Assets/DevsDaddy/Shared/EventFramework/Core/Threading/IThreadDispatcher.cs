using System;

namespace DevsDaddy.Shared.EventFramework.Core.Threading
{
    /// <summary>
    /// Thread Dispatcher Interface
    /// </summary>
    public interface IThreadDispatcher
    {
        int ThreadId { get; }
        void Dispatch(Delegate action, object[] payload);
    }
}