using System;
using System.Collections;
using DevsDaddy.GameShield.Core.Constants;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core
{
    /// <summary>
    /// Game Shield Worker Class
    /// Used for Modules initialization and provide
    /// MonoBased Events
    /// </summary>
    [DisallowMultipleComponent]
    public class GameShield : MonoBehaviour
    {
        // GameShield General Instance
        public static GameShield Main {
            get => (_main != null) ? _main : CreateWorker();
            private set => _main = value;
        }
        private static GameShield _main = null;
        
        // States Flag
        private bool isQuitting = false;
        private bool isPaused = false;

        /// <summary>
        /// On GameShield Worker Awake
        /// </summary>
        private void Awake() {
            if (Main != null && Main != this) {
                Destroy(this);
                return;
            }
            
            Main = this;
            transform.SetParent(null);
            DontDestroyOnLoad(this);
            gameObject.name = GeneralConstants.WORKER_OBJECT_NAME;
        }

        /// <summary>
        /// On GameShield Worker Start
        /// </summary>
        private void Start() {
            
            // Application Started Event
            EventMessenger.Main.Publish(new ApplicationStartedPayload {
                Time = DateTime.Now
            });
        }

        /// <summary>
        /// On Update
        /// </summary>
        private void Update() {
            if(isPaused || isQuitting) return;
            EventMessenger.Main.Publish(new ApplicationLoopUpdated {
                DeltaTime = Time.deltaTime
            });
        }

        /// <summary>
        /// On Fixed Update
        /// </summary>
        private void FixedUpdate() {
            if(isPaused || isQuitting) return;
            EventMessenger.Main.Publish(new ApplicationFixedLoopUpdated {
                DeltaTime = Time.fixedDeltaTime
            });
        }

        /// <summary>
        /// Application is lost/has focused
        /// </summary>
        /// <param name="hasFocus"></param>
        private void OnApplicationFocus(bool hasFocus) {
            isPaused = !hasFocus;
            EventMessenger.Main.Publish(new ApplicationPausePayload {
                IsPaused = isPaused,
                Time = DateTime.Now
            });
        }

        /// <summary>
        /// On Instance Destroying
        /// </summary>
        private void OnDestroy() {
            EventMessenger.Main.Publish(new ApplicationClosePayload {
                IsQuitting = isQuitting,
                Time = DateTime.Now
            });
        }

        /// <summary>
        /// On Application Quit
        /// </summary>
        private void OnApplicationQuit() {
            RemoveAllCoroutines();
            isQuitting = true;
        }

        /// <summary>
        /// Add Coroutine
        /// </summary>
        /// <param name="coroutine"></param>
        public void AddCoroutine(IEnumerator coroutine) {
            StartCoroutine(coroutine);
        }

        /// <summary>
        /// Remove Coroutine
        /// </summary>
        /// <param name="coroutine"></param>
        public void RemoveCoroutine(IEnumerator coroutine) {
            StopCoroutine(coroutine);
        }

        /// <summary>
        /// Remove All Coroutines
        /// </summary>
        public void RemoveAllCoroutines() {
            StopAllCoroutines();
        }

        /// <summary>
        /// Create Worker
        /// </summary>
        /// <returns></returns>
        private static GameShield CreateWorker() {
            if (_main != null) return _main;
            GameObject workerObject = new GameObject(GeneralConstants.WORKER_OBJECT_NAME);
            GameShield shield = workerObject.AddComponent<GameShield>();
            _main = shield;
            return _main;
        }
    }
}