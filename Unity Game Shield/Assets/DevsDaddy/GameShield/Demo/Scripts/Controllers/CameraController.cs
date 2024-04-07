using System;
using DevsDaddy.GameShield.Demo.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Demo.Controllers
{
    /// <summary>
    /// Basic Camera Controller
    /// </summary>
    internal class CameraController : MonoBehaviour
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
            EventMessenger.Main.Subscribe<OnCameraStateChanged>(OnStateChanged);
        }

        /// <summary>
        /// Unbind Events
        /// </summary>
        private void UnbindEvents() {
            EventMessenger.Main.Unsubscribe<OnCameraStateChanged>(OnStateChanged);
        }

        private void OnStateChanged(OnCameraStateChanged payload) {
            
        }
    }
}