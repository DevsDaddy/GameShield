using DevsDaddy.Shared.EventFramework.Core.Payloads;

namespace DevsDaddy.Shared.EventFramework.Core.Messenger
{
    /// <summary>
    /// Event Messenger Publish Interface
    /// </summary>
    public interface IEventMessengerPublish
    {
        /// <summary>
        /// Publish Payload
        /// </summary>
        /// <param name="payload"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IEventMessengerPublish Publish<T>(T payload) where T : IPayload;

        /// <summary>
        /// Get Current Payload State or Null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetState<T>() where T : class, IPayload;
    }
}