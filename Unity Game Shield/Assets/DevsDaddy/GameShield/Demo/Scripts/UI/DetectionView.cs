using System;
using DevsDaddy.GameShield.Demo.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;
using UnityEngine.UI;

namespace DevsDaddy.GameShield.Demo.UI
{
    /// <summary>
    /// Cheat Detection UI
    /// </summary>
    internal class DetectionView : BaseView
    {
        [Header("UI References")] 
        [SerializeField] private Button applyButton;
        [SerializeField] private Button cancelButton;

        [SerializeField] private Text message;
        [SerializeField] private Text code;
        [SerializeField] private Text critical;
        [SerializeField] private Text module;
        
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
            EventMessenger.Main.Subscribe<RequestDetectionView>(OnViewRequested);
        }

        /// <summary>
        /// Unbind Events
        /// </summary>
        private void UnbindEvents() {
            EventMessenger.Main.Unsubscribe<RequestDetectionView>(OnViewRequested);
        }

        /// <summary>
        /// On View Requested
        /// </summary>
        /// <param name="payload"></param>
        private void OnViewRequested(RequestDetectionView payload) {
            // Add Warning Data
            message.text = payload.DetectionData.Message;
            code.text = $"<color=red>Code:</color> {payload.DetectionData.Code}";
            critical.text = $"<color=red>Type:</color> {(payload.DetectionData.IsCritical ? "Is Critical" : "Non Critical")}";
            module.text = $"<color=red>Module:</color> {payload.DetectionData.Module.GetType()}";
            
            // Add Buttons Listener
            applyButton.onClick.RemoveAllListeners();
            applyButton.onClick.AddListener(() => {
                payload.OnApply?.Invoke();
                Hide();
                if (payload.DetectionData.IsCritical) {
                    Application.Quit();
                }
            });
            
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() => {
                payload.OnCancel?.Invoke();
                Hide();
            });
            Show();
        }
    }
}