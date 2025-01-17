using DevsDaddy.GameShield.Demo.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevsDaddy.GameShield.Demo.UI
{
    /// <summary>
    /// Welcome UI
    /// </summary>
    internal class WelcomeViewTK : MonoBehaviour
    {
        [Header("UI References")]
        public UIDocument welcomeDocument;
        private VisualElement rootWelcome;

        private Button startDemo;
        private Button rewardCaptcha;

        private EventCallback<ClickEvent> startDemoCallback;
        private EventCallback<ClickEvent> rewardCaptchaCallback;

        private void Awake()
        {
            rootWelcome = welcomeDocument.rootVisualElement;
            BindEvents();
        }

        /// <summary>
        /// On View Destroy
        /// </summary>
        private void OnDestroy()
        {
            startDemo?.UnregisterCallback(startDemoCallback);
            rewardCaptcha?.UnregisterCallback(rewardCaptchaCallback);
            UnbindEvents();
        }

        /// <summary>
        /// Bind Events
        /// </summary>
        private void BindEvents()
        {
            EventMessenger.Main.Subscribe<RequestWelcomeView>(OnViewRequested);
        }

        /// <summary>
        /// Unbind Events
        /// </summary>
        private void UnbindEvents()
        {
            EventMessenger.Main.Unsubscribe<RequestWelcomeView>(OnViewRequested);
        }

        private void OnViewRequested(RequestWelcomeView payload)
        {
            rootWelcome.Q("WelcomeScreen").style.display = DisplayStyle.Flex;

            startDemo = rootWelcome.Q<Button>("StartDemoButton");
            rewardCaptcha = rootWelcome.Q<Button>("rewardCaptchaButton");

            startDemoCallback = evt =>
            {
                rootWelcome.Q("WelcomeScreen").style.display = DisplayStyle.None;
                payload.OnGameStarted?.Invoke();
            };

            rewardCaptchaCallback = evt =>
            {
                payload.OnCaptchaRequested?.Invoke();
            };

            startDemo.RegisterCallback(startDemoCallback);
            rewardCaptcha.RegisterCallback(rewardCaptchaCallback);
        }
    }
}