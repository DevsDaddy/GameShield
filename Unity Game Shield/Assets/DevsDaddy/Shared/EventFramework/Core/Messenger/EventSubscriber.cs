using System;
using System.Reflection;
using UnityEngine;

namespace DevsDaddy.Shared.EventFramework.Core.Messenger
{
    /// <summary>
    /// Event Subscriber Class
    /// </summary>
    internal class EventSubscriber : IDisposable
    {
        private WeakReference _callbackTarget;
        private MethodInfo _callbackMethod;
        
        private WeakReference _predicateTarget;
        private MethodInfo _predicateMethod;
        
        public bool IsAlive
        {
            get
            {
                if(_callbackMethod == null)
                {
                    return false;
                }
                if(_callbackMethod.IsStatic)
                {
                    return true;
                }
                if(_callbackTarget == null ||
                   !_callbackTarget.IsAlive ||
                   _callbackTarget.Target == null)
                {
                    return false;
                }
                return true;
            }
        }
        
        public Type PayloadType
        {
            get;
        }
        
        public int Id 
        {
            get;
        }

        public override int GetHashCode()
        {
            return Id;
        }
        
        public EventSubscriber(Type payloadType, Delegate callback, Delegate predicate = null)
        {
            // validate params
            if(payloadType == null)
            {
                throw new ArgumentNullException(nameof(payloadType));
            }
            if(callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }
            
            // assign values to vars
            PayloadType = payloadType;
            Id = callback.GetHashCode();
            _callbackMethod = callback.Method;

            // check if callback method is not a static method
            if(!_callbackMethod.IsStatic && 
               callback.Target != null)
            {
                // init weak reference to callback owner
                _callbackTarget = new WeakReference(callback.Target);
            }
            
            // --- init predicate ---
            if(predicate == null)
            {
                return;
            }            
            _predicateMethod = predicate.Method;

            if(!_predicateMethod.IsStatic && 
               !Equals(predicate.Target, callback.Target))
            {
                _predicateTarget = new WeakReference(predicate.Target);
            }                     
        }
        
        public void Invoke<T>(T payload)
        {
            // validate callback method info
            if(_callbackMethod == null)
            {
                Debug.LogError($"{nameof(_callbackMethod)} is null.");
                return;
            }
            if(!_callbackMethod.IsStatic && 
               (_callbackTarget == null || 
                !_callbackTarget.IsAlive))
            {
                Debug.LogWarning($"{nameof(_callbackMethod)} is not alive.");
                return;
            }

            // get reference to the predicate function owner
            if(_predicateMethod != null)
            {
                object predicateTarget = null;
                if(!_predicateMethod.IsStatic)
                {
                    if(_predicateTarget != null && 
                       _predicateTarget.IsAlive)
                    {
                        predicateTarget = _predicateTarget.Target;
                    }
                    else if(_callbackTarget != null && 
                            _callbackTarget.IsAlive)
                    {
                        predicateTarget = _callbackTarget.Target;
                    }
                }

                // check if predicate returned 'true'
                var isAccepted = (bool)_predicateMethod.Invoke(predicateTarget, new object[] {payload});
                if(!isAccepted)
                {
                    // TODO log ?
                    return;
                }
            }

            // invoke callback method
            object callbackTarget = null;
            if(!_callbackMethod.IsStatic && 
               _callbackTarget != null && _callbackTarget.IsAlive)
            {
                callbackTarget = _callbackTarget.Target;
            }
            _callbackMethod.Invoke(callbackTarget, new object[] {payload});
        }
        
        public void Dispose()
        {
            _callbackMethod = null;
            if(_callbackTarget != null)
            {             
                _callbackTarget.Target = null;
                _callbackTarget = null;
            }
            
            _predicateMethod = null;
            if(_predicateTarget != null)
            {
                _predicateTarget.Target = null;
                _predicateTarget = null;
            }
        }
    }
}