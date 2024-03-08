using System;
using System.Reflection;

namespace DevsDaddy.Shared.EventFramework.Core.Threading
{
    public class WeakRefDelegate : WeakRefWrapper, IEquatable<Delegate>, IComparable
    {
        private int Id { get; }
        private MethodInfo Method { get; set; }

        public override int GetHashCode()
        {
            return Id;
        }
        
        public WeakRefDelegate(Delegate method) : base(method.Target)
        {
            Method = method.Method;
            Id = method.GetHashCode();
        }

        public object Invoke(object[] args)
        {
            if (IsDisposed || !IsAlive)
            {
                return null;
            }
            
            var result = Method.Invoke(Target, args);
            return result;
        }

        public bool Contains(Delegate method)
        {
            if(method == null || !IsAlive)
            {
                return false;
            }
            
            var contains = Equals(Target, method.Target) && Equals(Method, method.Method);
            return contains;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                Method = null;
            }
        }
        
        public bool Equals(Delegate other)
        {
            if(other == null) return false;
            var otherId = other.GetHashCode();
            var equals = Id == otherId;
            return equals;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is Delegate @delegate))
            {
                return -1;
            }

            if (Equals(@delegate))
            {
                return 0;
            }
            return -1;
        }

        public override string ToString()
        {
            return $"{GetType().Name}: {Id}, {Method}, {IsAlive}";
        }
    }
}