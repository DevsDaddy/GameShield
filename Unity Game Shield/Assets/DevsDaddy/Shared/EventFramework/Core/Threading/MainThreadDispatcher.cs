using System;
using System.Collections.Concurrent;
using System.Threading;
using DevsDaddy.Shared.EventFramework.Core.Objects;
using UnityEngine;

namespace DevsDaddy.Shared.EventFramework.Core.Threading
{
    public class MainThreadDispatcher : MonoSingleton<IThreadDispatcher, MainThreadDispatcher>
        , IThreadDispatcher
    {
        private readonly ConcurrentQueue<DispatcherTask> _tasks = new ConcurrentQueue<DispatcherTask>();
        
        public int ThreadId
        {
            get;
            private set;
        }

        public int TasksCount => _tasks.Count;

        private void Awake()
        {
            ThreadId = Thread.CurrentThread.ManagedThreadId;                
        }

        public void Dispatch(Delegate action, object[] payload)
        {
            _tasks.Enqueue(new DispatcherTask(action, payload));
        }

        private void Update()
        {
            while(_tasks.Count > 0)
            {
                if (!_tasks.TryDequeue(out var task))
                {
                    continue;
                }
                Debug.Log($"(Queue.Count: {_tasks.Count}) Dispatching task {task.Action}");

                task.Invoke();
                task.Dispose();
            }
        }
    }
}