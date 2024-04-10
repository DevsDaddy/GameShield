using System;
using DevsDaddy.GameShield.Core.Constants;
using DevsDaddy.GameShield.Core.Reporter;
using DevsDaddy.GameShield.Demo.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;
using UnityEngine.UI;

namespace DevsDaddy.GameShield.Demo.UI
{
    /// <summary>
    /// Reporting View
    /// </summary>
    internal class ReportingView : BaseView
    {
        [Header("UI References")] 
        [SerializeField] private Button applyButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private InputField messageField;
        
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
            EventMessenger.Main.Subscribe<RequestReportingView>(OnViewRequested);
        }

        /// <summary>
        /// Unbind Events
        /// </summary>
        private void UnbindEvents() {
            EventMessenger.Main.Unsubscribe<RequestReportingView>(OnViewRequested);
        }

        /// <summary>
        /// On View Requested
        /// </summary>
        /// <param name="payload"></param>
        private void OnViewRequested(RequestReportingView payload) {
            applyButton.onClick.RemoveAllListeners();
            applyButton.onClick.AddListener(() => {
                SendReport(payload);
            });
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(Hide);
            
            // Work with Field
            messageField.onValueChanged.RemoveAllListeners();
            messageField.onValueChanged.AddListener(text => {
                applyButton.interactable = !string.IsNullOrEmpty(text);
            });
            applyButton.interactable = !string.IsNullOrEmpty(messageField.text);
            Show();
        }

        /// <summary>
        /// Send Report Using GameShield API
        /// </summary>
        private void SendReport(RequestReportingView payload) {
            EventMessenger.Main.Publish(new ReportingPayload {
                Data = new ReportData {
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