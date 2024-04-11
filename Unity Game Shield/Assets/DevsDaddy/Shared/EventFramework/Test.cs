using DevsDaddy.Shared.EventFramework.Core.Payloads;

namespace DevsDaddy.Shared.EventFramework
{
    public class Test
    {
        public class TestPayload : IPayload
        {
            
        }
        
        public void TestMe() {
            EventMessenger.Main.Subscribe<TestPayload>(payload => {
                
            });
            EventMessenger.Main.Publish(new TestPayload());
            EventMessenger.Main.Unsubscribe<TestPayload>(OnPayloadTested);
            TestPayload currentPayload = EventMessenger.Main.GetState<TestPayload>();
        }

        private void OnPayloadTested(TestPayload payload) {
            
        }
    }
}