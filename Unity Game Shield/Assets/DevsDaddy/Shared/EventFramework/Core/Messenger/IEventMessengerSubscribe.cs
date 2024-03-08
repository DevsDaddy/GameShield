using System;
using DevsDaddy.Shared.EventFramework.Core.Payloads;

namespace DevsDaddy.Shared.EventFramework.Core.Messenger
{
    /// <summary>
    /// Event Messenger Subscribe Interface
    /// </summary>
    public interface IEventMessengerSubscribe
    {
        /// <summary>
        /// Subscribe Listener to Payload
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="predicate"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IEventMessengerSubscribe Subscribe<T>(Action<T> callback, Predicate<T> predicate = null) where T : IPayload;
    }
}