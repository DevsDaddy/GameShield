using DevsDaddy.GameShield.Demo.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;
using UnityEngine.UIElements;


namespace DevsDaddy.GameShield.Demo.UI
{
    /// <summary>
    /// In-Game HUD
    /// </summary>
    internal class InGameViewTK : MonoBehaviour
    {
        [Header("UI References")]
        public UIDocument InGameDocument;
        private VisualElement rootInGame;

        private Button restartButton;
        private Button reportButton;
        private Button startDetector;
        private Button stopDetector;

        private EventCallback<ClickEvent> restartCallback;
        private EventCallback<ClickEvent> reportCallback;
        private EventCallback<ClickEvent> startCallback;
        private EventCallback<ClickEvent> stopCallback;

        private Label detectorState;

        /// <summary>
        /// On View Initialized
        /// </summary>
        private void Awake()
        {
            rootInGame = InGameDocument.rootVisualElement;
            detectorState = rootInGame.Q<Label>("detectorsText");

            restartButton = rootInGame.Q<Button>("RestartButton");
            reportButton = rootInGame.Q<Button>("SendRepButton");
            startDetector = rootInGame.Q<Button>("UnpauseButton");
            stopDetector = rootInGame.Q<Button>("PauseButton");

            BindEvents();
        }

        /// <summary>
        /// On View Destroy
        /// </summary>
        private void OnDestroy()
        {
            restartButton?.UnregisterCallback(restartCallback);
            reportButton?.UnregisterCallback(reportCallback);
            startDetector?.UnregisterCallback(startCallback);
            stopDetector?.UnregisterCallback(stopCallback);

            UnbindEvents();
        }

        /// <summary>
        /// Bind Events
        /// </summary>
        private void BindEvents()
        {
            EventMessenger.Main.Subscribe<RequestInGameView>(OnViewRequested);
        }

        /// <summary>
        /// Unbind Events
        /// </summary>
        private void UnbindEvents()
        {
            EventMessenger.Main.Unsubscribe<RequestInGameView>(OnViewRequested);
        }

        /// <summary>
        /// On View Requested
        /// </summary>
        /// <param name="payload"></param>
        private void OnViewRequested(RequestInGameView payload)
        {
            UpdateDetectorState();

            restartCallback = evt =>
            {
                payload.OnGameRestarted?.Invoke();
                rootInGame.Q("InGameScreen").style.display = DisplayStyle.None;
            };

            reportCallback = evt =>
            {
                payload.OnReportRequested?.Invoke();
            };

            startCallback = evt =>
            {
                payload.OnDetectionStarted?.Invoke();
                UpdateDetectorState();
            };

            stopCallback = evt =>
            {
                payload.OnDetectionPaused?.Invoke();
                UpdateDetectorState();
            };

            restartButton.RegisterCallback(restartCallback);
            reportButton.RegisterCallback(reportCallback);
            startDetector.RegisterCallback(startCallback);
            stopDetector.RegisterCallback(stopCallback);

            rootInGame.Q("InGameScreen").style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Update Detector State
        /// </summary>
        private void UpdateDetectorState()
        {
            detectorState.text = $"Paused Detectors: <color=white>{Core.GameShield.Main.GetPausedDetectorsCount():N0}</color>";
        }
    }
}