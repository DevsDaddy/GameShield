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
            // Request Welcome View
            EventMessenger.Main.Publish(new RequestWelcomeView {
                
            });
            StartGameplay(true);
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
            
        }

        /// <summary>
        /// Unbind Events
        /// </summary>
        private void UnbindEvents() {
            
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