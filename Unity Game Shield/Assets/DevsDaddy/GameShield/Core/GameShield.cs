using System;
using System.Collections;
using System.Collections.Generic;
using DevsDaddy.GameShield.Core.Constants;
using DevsDaddy.GameShield.Core.Modules;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.GameShield.Core.Utils;
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
        public static GameShield Main { get; private set; }
        private static GameShield _main = null;
        
        // States Flag
        private bool isQuitting = false;
        private bool isPaused = false;
        private GameShieldConfig currentConfig = null;
        
        // Loaded Modules
        private List<IShieldModule> loadedModules = new List<IShieldModule>();
        private readonly Dictionary<string, IEnumerator> currentCoroutines = new Dictionary<string, IEnumerator>();

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
            Debug.Log($"{GeneralStrings.LOG_PREFIX} Worker Awake");
        }

        /// <summary>
        /// On GameShield Worker Start
        /// </summary>
        private void Start() {
            // Get Configs
            LoadConfig();

            // Application Started Event
            EventMessenger.Main.Subscribe<RequestCoroutine>(OnRequestCoroutine);
            EventMessenger.Main.Subscribe<StopCoroutine>(OnStopCoroutine);
            EventMessenger.Main.Publish(new ApplicationStartedPayload {
                Time = DateTime.Now
            });
            Debug.Log($"{GeneralStrings.LOG_PREFIX} Worker Started");
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
            EventMessenger.Main.Unsubscribe<RequestCoroutine>(OnRequestCoroutine);
            EventMessenger.Main.Unsubscribe<StopCoroutine>(OnStopCoroutine);
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
            Debug.Log($"{GeneralStrings.LOG_PREFIX} Application Quit Process.");
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
            loadedModules.Add(module);
            Debug.Log($"{GeneralStrings.LOG_PREFIX} Manual Module added: {module.GetType().Name}");
            return module;
        }

        /// <summary>
        /// Add Game Shield Module by Name
        /// </summary>
        /// <param name="moduleType"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public IShieldModule AddModule(string moduleType, IShieldModuleConfig config = null) {
            IShieldModule found = loadedModules.Find(mod => mod.GetType().Name == moduleType);
            if (found != null) return found;
            
            // Create Module by Name
            Type mType = ModuleManager.ByName(moduleType);
            if (mType == null) {
                Debug.LogError($"{GeneralStrings.LOG_PREFIX} Failed to Initialize Module {moduleType}. Type is not found");
                return null;
            }
            IShieldModule newModule = (IShieldModule)Activator.CreateInstance(mType);
            newModule.SetupModule();
            loadedModules.Add(newModule);
            return null;
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
            Debug.Log($"{GeneralStrings.LOG_PREFIX} Module Removed: {module.GetType().Name}");
            return true;
        }

        /// <summary>
        /// Get Developer Key
        /// </summary>
        /// <returns></returns>
        public string GetDeveloperKey() {
            return currentConfig.DeveloperKey;
        }

        /// <summary>
        /// On Coroutine Requested
        /// </summary>
        /// <param name="payload"></param>
        private void OnRequestCoroutine(RequestCoroutine payload) {
            if (currentCoroutines.ContainsKey(payload.Id)) {
                StopCoroutine(currentCoroutines[payload.Id]);
                currentCoroutines[payload.Id] = payload.Coroutine;
            }
            else {
                currentCoroutines.Add(payload.Id, payload.Coroutine);
            }

            StartCoroutine(currentCoroutines[payload.Id]);
        }

        /// <summary>
        /// On Stop Coroutine Requested
        /// </summary>
        /// <param name="payload"></param>
        private void OnStopCoroutine(StopCoroutine payload) {
            if (!currentCoroutines.ContainsKey(payload.Id))
                return;
            
            StopCoroutine(currentCoroutines[payload.Id]);
        }

        /// <summary>
        /// Remove All Coroutines
        /// </summary>
        private void RemoveAllCoroutines() {
            currentCoroutines.Clear();
            StopAllCoroutines();
        }

        /// <summary>
        /// Load Configuration
        /// </summary>
        private void LoadConfig() {
            // Load Configuration
            GameShieldConfig config = Resources.Load<GameShieldConfig>(GeneralConstants.CONFIG_PATH);
            currentConfig = config ? config : ScriptableObject.CreateInstance<GameShieldConfig>();
            Debug.Log($"{GeneralStrings.LOG_PREFIX} Configuration Loaded. Available Modules: {currentConfig.AvailableModules.Count}");
            
            // Initialize Modules
            if (currentConfig.AvailableModules.Count > 0) {
                foreach (var module in currentConfig.AvailableModules) {
                    Debug.Log($"{GeneralStrings.LOG_PREFIX} Trying to Auto-Initialize Module: {module}");
                    AddModule(module);
                }
            }
            
            // Auto-Pause for Application Terminated
            if (currentConfig.PauseOnApplicationTerminated && loadedModules.Count > 0) {
                EventMessenger.Main.Subscribe<ApplicationPausePayload>(payload => {
                    foreach (var module in loadedModules) {
                        module.PauseDetector(payload.IsPaused);
                        Debug.Log($"{GeneralStrings.LOG_PREFIX} Toggle Pause for Module: {module}, In-State: {payload.IsPaused}");
                    }
                });
            }
        }
    }
}