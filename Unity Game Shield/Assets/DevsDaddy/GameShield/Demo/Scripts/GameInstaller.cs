using DevsDaddy.GameShield.Core.Constants;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.GameShield.Demo.Enums;
using DevsDaddy.GameShield.Demo.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Demo
{
    /// <summary>
    /// Game Installer Demo Class
    /// </summary>
    internal class GameInstaller : MonoBehaviour
    {
        /// <summary>
        /// On Scene Installer Awake
        /// </summary>
        private void Awake() {
            BindEvents();
        }

        /// <summary>
        /// On Scene Installer Started
        /// </summary>
        private void Start() {
            ShowWelcomeView();
        }

        /// <summary>
        /// Show Welcome View
        /// </summary>
        private void ShowWelcomeView() {
            StopGameplay();
            EventMessenger.Main.Publish(new RequestWelcomeView {
                OnGameStarted = () => {
                    ShowGameplayUI();
                    StartGameplay(false);
                }
            });
        }

        /// <summary>
        /// Show Gameplay UI
        /// </summary>
        private void ShowGameplayUI() {
            EventMessenger.Main.Publish(new RequestInGameView {
                OnGameRestarted = ShowWelcomeView,
                OnDetectionPaused = () => Core.GameShield.Main.ToggleAllModulesDetection(true),
                OnDetectionStarted = () => Core.GameShield.Main.ToggleAllModulesDetection(false),
                OnReportRequested = ShowReportWindow
            });
        }

        /// <summary>
        /// Show Report Window
        /// </summary>
        private void ShowReportWindow() {
            EventMessenger.Main.Publish(new RequestReportingView {
                OnReportSended = () => {
                    Debug.Log($"{GeneralStrings.LOG_PREFIX} Report Completely Sended");
                },
                OnError = error => {
                    Debug.LogError($"{GeneralStrings.LOG_PREFIX} Report Sending Error: {error}");
                }
            });
        }

        /// <summary>
        /// On Scene Installer Destroy
        /// </summary>
        private void OnDestroy() {
            UnbindEvents();
        }

        /// <summary>
        /// Bind Events
        /// </summary>
        private void BindEvents() {
            EventMessenger.Main.Subscribe<SecurityWarningPayload>(OnCheatDetected);
        }

        /// <summary>
        /// Unbind Events
        /// </summary>
        private void UnbindEvents() {
            EventMessenger.Main.Unsubscribe<SecurityWarningPayload>(OnCheatDetected);
        }

        /// <summary>
        /// On Cheat Detected
        /// </summary>
        /// <param name="payload"></param>
        private void OnCheatDetected(SecurityWarningPayload payload) {
            EventMessenger.Main.Publish(new RequestDetectionView {
                DetectionData = payload
            });
        }

        /// <summary>
        /// Start Gameplay
        /// </summary>
        /// <param name="restart"></param>
        private void StartGameplay(bool restart) {
            EventMessenger.Main.Publish(new OnPlayerStateChanged {
                State = (restart) ? BaseState.Restart : BaseState.Start
            });
            EventMessenger.Main.Publish(new OnCameraStateChanged {
                State = (restart) ? BaseState.Restart : BaseState.Start
            });
            EventMessenger.Main.Publish(new OnEnemyStateChanged {
                State = (restart) ? BaseState.Restart : BaseState.Start
            });
        }

        /// <summary>
        /// Stop Gameplay
        /// </summary>
        private void StopGameplay() {
            EventMessenger.Main.Publish(new OnPlayerStateChanged {
                State = BaseState.Stop
            });
            EventMessenger.Main.Publish(new OnCameraStateChanged {
                State = BaseState.Stop
            });
            EventMessenger.Main.Publish(new OnEnemyStateChanged {
                State = BaseState.Stop
            });
        }
    }
}