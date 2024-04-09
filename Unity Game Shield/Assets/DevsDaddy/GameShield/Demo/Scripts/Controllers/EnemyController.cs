using DevsDaddy.GameShield.Demo.Enums;
using DevsDaddy.GameShield.Demo.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Demo.Controllers
{
    /// <summary>
    /// Basic Enemy Controller
    /// </summary>
    internal class EnemyController : MonoBehaviour
    {
        /// <summary>
        /// On Awake
        /// </summary>
        private void Awake() {
            BindEvents();
        }

        /// <summary>
        /// On Start
        /// </summary>
        private void Start() {
            
        }

        /// <summary>
        /// On Destroy
        /// </summary>
        private void OnDestroy() {
            UnbindEvents();
        }

        /// <summary>
        /// Reset Enemy
        /// </summary>
        private void ResetEnemey() {
            
        }

        /// <summary>
        /// Bind Events
        /// </summary>
        private void BindEvents() {
            EventMessenger.Main.Subscribe<OnEnemyStateChanged>(OnStateChanged);
        }

        /// <summary>
        /// Unbind Events
        /// </summary>
        private void UnbindEvents() {
            EventMessenger.Main.Unsubscribe<OnEnemyStateChanged>(OnStateChanged);
        }

        /// <summary>
        /// On Game State Changed
        /// </summary>
        /// <param name="payload"></param>
        private void OnStateChanged(OnEnemyStateChanged payload) {
            if (payload.State == BaseState.Start || payload.State == BaseState.Restart) {
                
            }
            else {
                
            }
        }
    }
}