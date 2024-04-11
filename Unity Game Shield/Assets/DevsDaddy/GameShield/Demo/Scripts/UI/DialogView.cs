using System;
using DevsDaddy.GameShield.Demo.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;
using UnityEngine.UI;

namespace DevsDaddy.GameShield.Demo.UI
{
    /// <summary>
    /// Just an Dialogue View
    /// </summary>
    internal class DialogView : BaseView
    {
        [Header("UI References")] 
        [SerializeField] private Button applyButton;
        [SerializeField] private Text header;
        [SerializeField] private Text message;

        /// <summary>
        /// On Initialized
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
        /// Bind View Events
        /// </summary>
        private void BindEvents() {
            EventMessenger.Main.Subscribe<RequestDialogue>(OnViewRequested);
        }

        /// <summary>
        /// Unbind View Events
        /// </summary>
        private void UnbindEvents() {
            EventMessenger.Main.Unsubscribe<RequestDialogue>(OnViewRequested);
        }

        /// <summary>
        /// On View Requested
        /// </summary>
        /// <param name="payload"></param>
        private void OnViewRequested(RequestDialogue payload) {
            header.text = payload.Title;
            message.text = payload.Message;
            message.color = payload.MessageColor;
            
            applyButton.onClick.RemoveAllListeners();
            applyButton.onClick.AddListener(() => {
                Hide();
                payload.OnComplete?.Invoke();
            });
            Show();
        }
    }
}