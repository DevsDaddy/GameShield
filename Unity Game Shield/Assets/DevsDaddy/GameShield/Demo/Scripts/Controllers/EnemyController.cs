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

        private void OnStateChanged(OnEnemyStateChanged payload) {
            
        }
    }
}