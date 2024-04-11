using System;

namespace DevsDaddy.Shared.EventFramework.Core.Threading
{
    public abstract class WeakRefWrapper : IDisposable
    {
        private WeakReference _ref;

        protected object Target
        {
            get
            {
                var target = IsAlive ? _ref.Target : null;
                return target;
            }
        }

        public bool IsAlive
        {
            get
            {
                var isAlive = _ref == null || (_ref.IsAlive && _ref.Target != null);
                return isAlive;
            }
        }
        
        protected WeakRefWrapper(object target)
        {
            _ref = new WeakReference(target, false);
        }

        protected bool IsDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            if (disposing)
            {
                if(_ref != null)
                {
                    _ref.Target = null;
                    _ref = null;
                }
            }
            IsDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}