using System.Collections;
using System.Collections.Generic;
using DevsDaddy.GameShield.Demo.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Demo.Controllers
{
    /// <summary>
    /// Basic Player Controller
    /// </summary>
    internal class PlayerController : MonoBehaviour
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
            EventMessenger.Main.Subscribe<OnPlayerStateChanged>(OnStateChanged);
        }

        /// <summary>
        /// Unbind Events
        /// </summary>
        private void UnbindEvents() {
            EventMessenger.Main.Unsubscribe<OnPlayerStateChanged>(OnStateChanged);
        }

        private void OnStateChanged(OnPlayerStateChanged payload) {
            
        }
    }
}
