using System;
using System.Collections.Generic;
using System.Threading;
using DevsDaddy.Shared.EventFramework.Core.Extensions;
using DevsDaddy.Shared.EventFramework.Core.Messenger;
using DevsDaddy.Shared.EventFramework.Core.Objects;
using DevsDaddy.Shared.EventFramework.Core.Payloads;
using DevsDaddy.Shared.EventFramework.Core.Threading;
using UnityEngine;

namespace DevsDaddy.Shared.EventFramework
{
    /// <summary>
    /// Event Messenger
    /// </summary>
    public class EventMessenger : Singleton<IEventMessenger, EventMessenger>, IEventMessenger
    {
        // Subscribers Collection
        private readonly Dictionary<Type, Dictionary<int, EventSubscriber>> _subscribersSet = 
            new Dictionary<Type, Dictionary<int, EventSubscriber>>();
        
        // List of subscribers to optimize iteration during subscribers processing  
        private readonly List<EventSubscriber> _subscribers = new List<EventSubscriber>();
        private readonly Dictionary<Type, IPayload> _payloadStates = new Dictionary<Type, IPayload>();

        // List of subscribers to optimize add (subscribe) operation 
        private readonly List<EventSubscriber> _add = new List<EventSubscriber>();
        private bool _isPublishing;
        
        /// <summary>
        /// Event Messenger Controller
        /// </summary>
        static EventMessenger()
        {
            Debug.Log($"Main Thread ID: {MainThreadDispatcher.Main.ThreadId}");
        }
        
        /// <summary>
        /// Publish given payload to relevant subscribers
        /// </summary>
        /// <param name="payload">Instance of payload to publish</param>
        /// <typeparam name="T">The type of payload to publish</typeparam>
        /// <returns>Instance of the Messenger</returns>
        public IEventMessengerPublish Publish<T>(T payload) where T : IPayload
        {
            // if calling thread is same as main thread, call "PublishInternal" directly
            if(Thread.CurrentThread.ManagedThreadId == MainThreadDispatcher.Main.ThreadId)
            {
                PublishInternal(payload);
                return this;
            }

            // capture "PublishInternal" in local action var.
            Action<T> act = PublishInternal;
            // add "act" into "MainThreadDispatcher" queue
            MainThreadDispatcher.Main.Dispatch(act, new object[] { payload });
            return this;
        }
        
        /// <summary>
        /// Publish Payload
        /// </summary>
        /// <remarks>
        /// Internal function that is used with "MainThreadDispatcher"
        /// </remarks>
        /// <param name="payload">The payload</param>
        /// <typeparam name="T">The type of the payload</typeparam>
        private void PublishInternal<T>(T payload) where T : IPayload
        {
            try
            {
                // turn on the flag
                _isPublishing = true;

                var key = typeof(T); // capture the type of the payload in local var.

                if (!_payloadStates.ContainsKey(key))
                    _payloadStates.Add(key, payload);
                else
                    _payloadStates[key] = payload;
                
                if (!_subscribersSet.ContainsKey(key))
                {
                    return;
                }

                // get subscriber's dic. for the payload type
                _subscribersSet.TryGetValue(key, out var callbacks);
                // check if "callbacks" dic. is null or empty 
                if (callbacks.IsNullOrEmpty())
                {                
                    // remove payload type key is "callbacks" dic is empty
                    _subscribersSet.Remove(key);
                    return;
                }

                // iterate thru dic and invoke callbacks
                foreach (var callback in callbacks.Values)
                {
                    callback?.Invoke(payload);
                }
            }
            finally
            {
                // turn off the flag
                _isPublishing = false;
                // process pending tasks
                Process();
            }
        }
        
        /// <summary>
        /// Subscribe the callback to specified payload type <see cref="T"/>
        /// </summary>
        /// <param name="callback">Callback delegate</param>
        /// <param name="predicate">Callback's predicate</param>
        /// <typeparam name="T">The type of the payload</typeparam>
        /// <returns>Messenger instance</returns>
        public IEventMessengerSubscribe Subscribe<T>(Action<T> callback, Predicate<T> predicate = null) where T : IPayload
        {
            // check if current thread ID == main thread ID
            if(Thread.CurrentThread.ManagedThreadId == MainThreadDispatcher.Main.ThreadId)
            {
                // execute subscribe method on main thread
                SubscribeInternal(callback, predicate);
                return this;
            }

            // capture delegate reference
            Action<Action<T>, Predicate<T>> act = SubscribeInternal;
            // add delegate and payload into main thread dispatcher queue
            MainThreadDispatcher.Main.Dispatch(act, new object[] { callback, predicate });
            return this;
        }
        
        /// <summary>
        /// Subscribe the callback to specified payload type <see cref="T"/>
        /// </summary>
        /// <remarks>
        /// Used internally by messenger to sync threads
        /// </remarks>
        /// <param name="callback">Callback delegate</param>
        /// <param name="predicate">Predicate delegate (optional)</param>
        /// <typeparam name="T">The type of the payload</typeparam>
        private void SubscribeInternal<T>(Action<T> callback, Predicate<T> predicate = null)
        {
            // capture the type of the payload
            var key = typeof(T);
            // init new subscriber instance
            var sub = new EventSubscriber(key, callback, predicate);

            // check if messenger is busy with publishing payloads
            if(_isPublishing)
            {
                // add subscriber into "Add" queue if messenger is busy with publishing
                _add.Add(sub);
                return;
            }
            // if messenger is not busy with publishing, add into subscribers list
            SubscribeInternal(sub);
        }
        
        /// <summary>
        /// Adds subscriber into subscribers list 
        /// </summary>
        /// <param name="subscriber"></param>
        private void SubscribeInternal(EventSubscriber subscriber)
        {
            // check is subscriber is valid
            if(!(subscriber is {IsAlive: true}))
            {
                Debug.LogError($"The {nameof(subscriber)} is null or not alive.");
                return;
            }

            // capture payload type into local var 'key' 
            var key = subscriber.PayloadType;
            // capture subscribers dic into local var 'dic'
            Dictionary<int, EventSubscriber> callbacks;
            if (_subscribersSet.ContainsKey(key))
            {
                // fetch list of callbacks for this payload type
                _subscribersSet.TryGetValue(key, out callbacks);
            }
            else
            {
                // init list of callbacks/subscribers
                callbacks = new Dictionary<int, EventSubscriber>();
                _subscribersSet.Add(key, callbacks);
            }

            if (callbacks == null)
            {
                Debug.LogError("Callbacks container is null!");
                return;
            }

            // check if subscriber is already registered
            if(callbacks.ContainsKey(subscriber.Id))
            {
                return;
            }
            // register new subscriber 
            callbacks.Add(subscriber.Id, subscriber);

            // add new list of callbacks/subscribers into flat list for fast access
            if (!_subscribers.Contains(subscriber))
            {
                _subscribers.Add(subscriber);
            }
        }
        
        /// <summary>
        /// Unsubscribe given callback by payload type <see cref="T"/>
        /// </summary>
        /// <param name="callback">The callback to unsubscribe</param>
        /// <typeparam name="T">The type of the payload</typeparam>
        /// <returns>Instance of <see cref="Messenger"/></returns>
        public IEventMessengerUnsubscribe Unsubscribe<T>(Action<T> callback) where T : IPayload
        {
            // check if method called on main thread
            if(Thread.CurrentThread.ManagedThreadId == MainThreadDispatcher.Main.ThreadId)
            {
                // call internal method
                UnsubscribeInternal(callback);
                return this;
            }

            // capture delegate in 'act' var
            Action<Action<T>> act = UnsubscribeInternal;
            // add 'act' delegate into main thread dispatcher queue
            MainThreadDispatcher.Main.Dispatch(act, new object[] { callback });
            return this;
        }

        /// <summary>
        /// Unsubscribe given callback by payload type <see cref="T"/>
        /// </summary>
        /// <remarks>Internal method</remarks>
        /// <param name="callback">The callback delegate</param>
        /// <typeparam name="T">The type of the payload</typeparam>
        private void UnsubscribeInternal<T>(Action<T> callback)
        {
            // capture payload type into 'key' var
            var key = typeof(T);          
            // capture subscribers dic into 'dic' var
            var dic = _subscribersSet;
            // check if payload is registered 
            if (!dic.ContainsKey(key))
            {
                return;
            }

            // get list of callbacks for the payload
            dic.TryGetValue(key, out var callbacks);
            // check if callbacks list is null or empty and if messenger is publishing payloads
            if(!_isPublishing && callbacks.IsNullOrEmpty())
            {
                // remove payload from subscribers dic
                dic.Remove(key);
                return;
            }

            // get callback ID
            var id = callback.GetHashCode();
            // check if callback is registered
            if(callbacks.ContainsKey(id))
            {
                // get subscriber instance and dispose it
                var subscriber = callbacks[id];
                subscriber.Dispose();         
                
                // check if messenger is busy with publishing
                if(!_isPublishing)
                {
                    // remove the subscriber from the callbacks dic
                    callbacks.Remove(id);
                    // remove the subscriber from the subscribers dic
                    if (_subscribers.Contains(subscriber))
                    {
                        _subscribers.Remove(subscriber);
                    }
                }
            }
            
            // check is messenger is busy with publishing or if callbacks are NOT empty
            if(_isPublishing || !callbacks.IsNullOrEmpty())
            {
                return;
            }
            // remove callbacks from the _subscribersSet
            dic.Remove(key);
        }

        /// <summary>
        /// Get Current Payload State
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetState<T>() where T : class, IPayload  {
            var key = typeof(T);
            if (!_payloadStates.ContainsKey(key)) {
                return null;
            }

            return (T)_payloadStates[key];
        }

        /// <summary>
        /// Process collections.
        /// <para>Cleanup "dead" subscribers and add from waiting list.</para>
        /// </summary>
        private void Process()
        {
            // cleanup "dead" subscribers
            for(var i = 0; i < _subscribers.Count; i++)
            {
                var subscriber = _subscribers[i];
                if(subscriber == null)
                {
                    _subscribers.RemoveAt(i);
                    i--;
                    continue;
                }
                if(subscriber.IsAlive)
                {
                    continue;
                }

                _subscribers.Remove(subscriber);
                i--;

                if(!_subscribersSet.ContainsKey(subscriber.PayloadType))
                {
                    continue;
                }

                var callbacks = _subscribersSet[subscriber.PayloadType];
                callbacks.Remove(subscriber.Id);

                if(callbacks.Count > 0)
                {
                    continue;
                }            
                _subscribersSet.Remove(subscriber.PayloadType);     
            }

            // add waiting subscribers
            foreach (var subscriber in _add)
            {
                SubscribeInternal(subscriber);
            }
            _add.Clear();
        }
    }
}