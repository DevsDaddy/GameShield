using System;
using DevsDaddy.GameShield.Demo.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevsDaddy.GameShield.Demo.UI
{
    /// <summary>
    /// Just an Dialogue View
    /// </summary>
    internal class DialogViewTK : MonoBehaviour
    {
        [Header("UI References")]
        public UIDocument dialogDocument;
        private VisualElement rootDialog;

        private Button applyButton;
        private EventCallback<ClickEvent> applyCallback;

        private Label header;
        private Label message;

        /// <summary>
        /// On Initialized
        /// </summary>
        private void Awake()
        {
            rootDialog = dialogDocument.rootVisualElement;
            header = rootDialog.Q<Label>("headerText");
            message = rootDialog.Q<Label>("messageText");
            applyButton = rootDialog.Q<Button>("ContinueButton");

            BindEvents();
        }

        /// <summary>
        /// On View Destroy
        /// </summary>
        private void OnDestroy()
        {
            applyButton?.UnregisterCallback(applyCallback, TrickleDown.TrickleDown);
            UnbindEvents();
        }

        /// <summary>
        /// Bind View Events
        /// </summary>
        private void BindEvents()
        {
            EventMessenger.Main.Subscribe<RequestDialogue>(OnViewRequested);
        }

        /// <summary>
        /// Unbind View Events
        /// </summary>
        private void UnbindEvents()
        {
            EventMessenger.Main.Unsubscribe<RequestDialogue>(OnViewRequested);
        }

        /// <summary>
        /// On View Requested
        /// </summary>
        /// <param name="payload"></param>
        private void OnViewRequested(RequestDialogue payload)
        {
            header.text = payload.Title;
            message.text = payload.Message;
            message.style.color = new StyleColor(payload.MessageColor);

            applyCallback = evt =>
            {
                rootDialog.Q("DialogueScreen").style.display = DisplayStyle.None;
                payload.OnComplete?.Invoke();
            };

            applyButton.RegisterCallback(applyCallback, TrickleDown.TrickleDown);
            rootDialog.Q("DialogueScreen").style.display = DisplayStyle.Flex;
        }
    }
}