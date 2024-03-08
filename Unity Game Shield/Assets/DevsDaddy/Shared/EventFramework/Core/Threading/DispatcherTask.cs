using System;

namespace DevsDaddy.Shared.EventFramework.Core.Threading
{
    public class DispatcherTask : IDisposable
    {
        public WeakRefDelegate Action 
        {
            get; private set;
        }

        private object[] Payload
        {
            get;
            set;
        }
        
        public DispatcherTask(Delegate action, object[] payload)
        {
            Action = new WeakRefDelegate(action);
            Payload = payload;
        }

        public void Invoke()
        {
            if(Action == null || !Action.IsAlive)
            {
                return;
            }
            Action.Invoke(Payload);            
        }

        public void Dispose()
        {
            if(Action != null)
            {
                Action.Dispose();
                Action = null;
            }
            Payload = null;            
        }
    }
}