using System;
using DevsDaddy.Shared.EventFramework.Core.Payloads;

namespace DevsDaddy.Shared.EventFramework.Core.Messenger
{
    /// <summary>
    /// Event Messenger Unsubscribe Interface
    /// </summary>
    public interface IEventMessengerUnsubscribe
    {
        /// <summary>
        /// Unsubscribe Listener from Payload
        /// </summary>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IEventMessengerUnsubscribe Unsubscribe<T>(Action<T> callback) where T : IPayload;
    }
}