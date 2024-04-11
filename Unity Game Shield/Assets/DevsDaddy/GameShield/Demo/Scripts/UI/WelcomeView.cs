using DevsDaddy.GameShield.Demo.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;
using UnityEngine.UI;

namespace DevsDaddy.GameShield.Demo.UI
{
    /// <summary>
    /// Welcome UI
    /// </summary>
    internal class WelcomeView : BaseView
    {
        [Header("UI References")] 
        [SerializeField] private Button startButton;
        [SerializeField] private Button showCaptchaButton;
        
        /// <summary>
        /// On View Initialized
        /// </summary>
        public override void OnInitialized() {
            BindEvents();
        }

        /// <summary>
        /// On View Destroy
        /// </summary>
        private void OnDestroy() {
            UnbindEvents();
        }
        
        /// <summary>
        /// Bind Events
        /// </summary>
        private void BindEvents() {
            EventMessenger.Main.Subscribe<RequestWelcomeView>(OnViewRequested);
        }

        /// <summary>
        /// Unbind Events
        /// </summary>
        private void UnbindEvents() {
            EventMessenger.Main.Unsubscribe<RequestWelcomeView>(OnViewRequested);
        }

        /// <summary>
        /// On View Requested
        /// </summary>
        /// <param name="payload"></param>
        private void OnViewRequested(RequestWelcomeView payload) {
            // Add Button Listeners
            Show();
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(() => {
                Hide();
                payload.OnGameStarted?.Invoke();
            });
            
            showCaptchaButton.onClick.RemoveAllListeners();
            showCaptchaButton.onClick.AddListener(() => {
                payload.OnCaptchaRequested?.Invoke();
            });
        }
    }
}