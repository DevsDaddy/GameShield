using DevsDaddy.GameShield.Core.Modules.Captcha;
using DevsDaddy.GameShield.Demo.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevsDaddy.GameShield.Demo.UI
{
    /// <summary>
    /// Cheat Detection UI
    /// </summary>
    internal class DetectionViewTK : MonoBehaviour
    {
        [Header("UI References")]
        public UIDocument detectionDocument;
        private VisualElement rootDetection;

        private Button applyButton;
        private Button cancelButton;

        private EventCallback<ClickEvent> applyDetectionCallback;
        private EventCallback<ClickEvent> cancelCallback;

        private Label message;
        private Label code;
        private Label critical;
        private Label module;

        /// <summary>
        /// On View Initialized
        /// </summary>
        private void Awake()
        {
            rootDetection = detectionDocument.rootVisualElement;
            message = rootDetection.Q<Label>("messageText");
            code = rootDetection.Q<Label>("codeText");
            critical = rootDetection.Q<Label>("criticalText");
            module = rootDetection.Q<Label>("moduleText");

            applyButton = rootDetection.Q<Button>("SendReportButtons");
            cancelButton = rootDetection.Q<Button>("ContinueButton");
            BindEvents();
        }

        /// <summary>
        /// On View Destroy 
        /// </summary>
        private void OnDestroy()
        {
            applyButton?.UnregisterCallback(applyDetectionCallback);
            cancelButton?.UnregisterCallback(cancelCallback);

            UnbindEvents();
        }

        /// <summary>
        /// Bind Events
        /// </summary>
        private void BindEvents()
        {
            EventMessenger.Main.Subscribe<RequestDetectionView>(OnViewRequested);
        }

        /// <summary>
        /// Unbind Events
        /// </summary>
        private void UnbindEvents()
        {
            EventMessenger.Main.Unsubscribe<RequestDetectionView>(OnViewRequested);
        }

        /// <summary>
        /// On View Requested
        /// </summary>
        /// <param name="payload"></param>
        private void OnViewRequested(RequestDetectionView payload)
        {
            // Add Warning Data
            message.text = payload.DetectionData.Message;
            SetText(code, "Code", payload.DetectionData.Code.ToString());
            SetText(critical, "Type", payload.DetectionData.IsCritical ? "Is Critical" : "Non Critical");
            SetText(module, "Module", payload.DetectionData.Module.GetType().ToString());

            applyDetectionCallback = evt =>
            {
                payload.OnApply?.Invoke();
                rootDetection.Q("DetectionScreen").style.display = DisplayStyle.None;
                if (payload.DetectionData.IsCritical)
                {
                    Application.Quit();
                }
            };

            cancelCallback = evt =>
            {
                payload.OnCancel?.Invoke();
                rootDetection.Q("DetectionScreen").style.display = DisplayStyle.None;
            };

            applyButton.RegisterCallback(applyDetectionCallback);
            cancelButton.RegisterCallback(cancelCallback);

            rootDetection.Q("DetectionScreen").style.display = DisplayStyle.Flex;
        }

        private void SetText(Label label, string prefix, string value)
        {
            label.text = $"<color=red>{prefix}:</color> {value}";
        }
    }
}