using DevsDaddy.GameShield.Demo.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Demo.UI
{
    /// <summary>
    /// Reporting View
    /// </summary>
    internal class ReportingView : BaseView
    {
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
            
            Show();
        }
    }
}