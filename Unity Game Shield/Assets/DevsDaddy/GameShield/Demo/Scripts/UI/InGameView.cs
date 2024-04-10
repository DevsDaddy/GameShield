using DevsDaddy.GameShield.Demo.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;
using UnityEngine.UI;

namespace DevsDaddy.GameShield.Demo.UI
{
    /// <summary>
    /// In-Game HUD
    /// </summary>
    internal class InGameView : BaseView
    {
        [Header("UI References")] 
        [SerializeField] private Button restartButton;
        [SerializeField] private Button reportButton;
        [SerializeField] private Button startDetector;
        [SerializeField] private Button stopDetector;
        [SerializeField] private Text detectorState;
        
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
            EventMessenger.Main.Subscribe<RequestInGameView>(OnViewRequested);
        }

        /// <summary>
        /// Unbind Events
        /// </summary>
        private void UnbindEvents() {
            EventMessenger.Main.Unsubscribe<RequestInGameView>(OnViewRequested);
        }

        /// <summary>
        /// On View Requested
        /// </summary>
        /// <param name="payload"></param>
        private void OnViewRequested(RequestInGameView payload) {
            UpdateDetectorState();
            
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(() => {
                payload.OnGameRestarted?.Invoke();
                Hide();
            });
            
            reportButton.onClick.RemoveAllListeners();
            reportButton.onClick.AddListener(() => {
                payload.OnReportRequested?.Invoke();
            });
            
            startDetector.onClick.RemoveAllListeners();
            startDetector.onClick.AddListener(() => {
                payload.OnDetectionStarted?.Invoke();
                UpdateDetectorState();
            });
            
            stopDetector.onClick.RemoveAllListeners();
            stopDetector.onClick.AddListener(() => {
                payload.OnDetectionPaused?.Invoke();
                UpdateDetectorState();
            });
            
            Show();
        }

        /// <summary>
        /// Update Detector State
        /// </summary>
        private void UpdateDetectorState() {
            detectorState.text = $"Paused Detectors: <color=white>{Core.GameShield.Main.GetPausedDetectorsCount():N0}</color>";
        }
    }
}