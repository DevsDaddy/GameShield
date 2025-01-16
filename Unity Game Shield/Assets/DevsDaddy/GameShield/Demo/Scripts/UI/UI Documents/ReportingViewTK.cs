using System;
using DevsDaddy.GameShield.Core.Constants;
using DevsDaddy.GameShield.Core.Reporter;
using DevsDaddy.GameShield.Demo.Payloads;
using DevsDaddy.Shared.EventFramework;
using Mono.Cecil.Cil;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevsDaddy.GameShield.Demo.UI
{
    /// <summary>
    /// Reporting View
    /// </summary>
    internal class ReportingViewTK : MonoBehaviour
    {
        [Header("UI References")]
        public UIDocument reportingDocument;
        private VisualElement rootReporting;

        private Button applyButton;
        private Button cancelButton;
        private EventCallback<ClickEvent> applyCallback;
        private EventCallback<ClickEvent> cancelCallback;

        private TextField messageField;
        private EventCallback<NavigationSubmitEvent> navigationSubmitCallback;
        private EventCallback<ChangeEvent<string>> valueChangedCallback;

        /// <summary>
        /// On View Initialized
        /// </summary>
        private void Awake()
        {
            rootReporting = reportingDocument.rootVisualElement;
            messageField = rootReporting.Q<TextField>("InsertText");

            applyButton = rootReporting.Q<Button>("SendReportButton");
            cancelButton = rootReporting.Q<Button>("cancelButton");
            BindEvents();
        }

        /// <summary>
        /// On View Destroy
        /// </summary>
        private void OnDestroy()
        {
            applyButton?.UnregisterCallback(applyCallback);
            cancelButton?.UnregisterCallback(cancelCallback);

            messageField?.UnregisterCallback(navigationSubmitCallback, TrickleDown.TrickleDown);
            messageField?.UnregisterValueChangedCallback(valueChangedCallback);

            UnbindEvents();
        }

        /// <summary>
        /// Bind Events
        /// </summary>
        private void BindEvents()
        {
            EventMessenger.Main.Subscribe<RequestReportingView>(OnViewRequested);
        }

        /// <summary>
        /// Unbind Events
        /// </summary>
        private void UnbindEvents()
        {
            EventMessenger.Main.Unsubscribe<RequestReportingView>(OnViewRequested);
        }

        /// <summary>
        /// On View Requested
        /// </summary>
        /// <param name="payload"></param>
        private void OnViewRequested(RequestReportingView payload)
        {
            applyCallback = evt =>
            {
                SendReport(payload);
            };

            cancelCallback = evt =>
            {
                rootReporting.Q("ReportingScreen").style.display = DisplayStyle.None;
            };

            applyButton.RegisterCallback(applyCallback);
            cancelButton.RegisterCallback(cancelCallback);

            navigationSubmitCallback = evt =>
            {
                if (!string.IsNullOrEmpty(messageField.text))
                {
                    applyButton.style.opacity = 1f;
                    applyButton.SetEnabled(true);
                }
                else
                {
                    applyButton.style.opacity = 0.5f;
                    applyButton.SetEnabled(false);
                }
                evt.StopPropagation();
            };

            valueChangedCallback = text =>
            {
                bool isValid = !string.IsNullOrEmpty(text.newValue);
                applyButton.style.opacity = isValid ? 1f : 0.5f;
                applyButton.SetEnabled(isValid);
            };

            messageField.RegisterCallback(navigationSubmitCallback, TrickleDown.TrickleDown);
            messageField.RegisterValueChangedCallback(valueChangedCallback);

            bool isMessageValid = !string.IsNullOrEmpty(messageField.text);
            applyButton.style.opacity = isMessageValid ? 1f : 0.5f;
            applyButton.SetEnabled(isMessageValid);

            rootReporting.Q("ReportingScreen").style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Send Report Using GameShield API
        /// </summary>
        private void SendReport(RequestReportingView payload)
        {
            EventMessenger.Main.Publish(new ReportingPayload
            {
                Data = new ReportData
                {
                    Gateway = "/demo/",
                    Code = 999.ToString(),
                    IsUserReport = true,
                    LocalTime = DateTime.Now,
                    Message = messageField.text,
                    ModuleType = "UserReporting"
                },
                OnComplete = response => {
                    Debug.Log($"{GeneralStrings.LOG_PREFIX} Reporter Server Response: {response}");
                    payload.OnReportSended?.Invoke();
                },
                OnError = error => {
                    payload.OnError?.Invoke(error);
                }
            });
        }
    }
}