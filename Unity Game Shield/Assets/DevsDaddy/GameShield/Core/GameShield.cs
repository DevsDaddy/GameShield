using System;
using System.Collections;
using System.Collections.Generic;
using DevsDaddy.GameShield.Core.Constants;
using DevsDaddy.GameShield.Core.Modules;
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
        private GameShieldConfig currentConfig = null;
        
        // Loaded Modules
        private List<IShieldModule> loadedModules = new List<IShieldModule>();

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
            // Get Configs
            LoadConfig();
            
            // Initialize Modules
            
            
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
        /// Add Game Shield Module
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddModule<T>(IShieldModuleConfig config = null) where T : class, IShieldModule, new() {
            T module = (T)loadedModules.Find(mod => mod.GetType() == typeof(T));
            if (module != null) return module;
            
            // Create Module
            module = new T();
            module.SetupModule(config);
            return module;
        }

        /// <summary>
        /// Get Game Shield Module
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetModule<T>() where T : class, IShieldModule {
            return (T)loadedModules.Find(mod => mod.GetType() == typeof(T));
        }

        /// <summary>
        /// Remove Game Shield Module
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public bool RemoveModule<T>() where T : class, IShieldModule  {
            T module = (T)loadedModules.Find(mod => mod.GetType() == typeof(T));
            if (module == null) return false;
            module.Disconnect();
            loadedModules.Remove(module);
            return true;
        }

        /// <summary>
        /// Add Coroutine
        /// </summary>
        /// <param name="coroutine"></param>
        private void AddCoroutine(IEnumerator coroutine) {
            StartCoroutine(coroutine);
        }

        /// <summary>
        /// Remove Coroutine
        /// </summary>
        /// <param name="coroutine"></param>
        private void RemoveCoroutine(IEnumerator coroutine) {
            StopCoroutine(coroutine);
        }

        /// <summary>
        /// Remove All Coroutines
        /// </summary>
        private void RemoveAllCoroutines() {
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

        /// <summary>
        /// Load Configuration
        /// </summary>
        private void LoadConfig() {
            
        }
    }
}