using DevsDaddy.GameShield.Core.Constants;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.Teleport
{
    /// <summary>
    /// Teleport Detector Module
    /// </summary>
    public class TeleportDetector : IShieldModule
    {
        private Options _currentOptions;
        private bool _initialized = false;
        private bool _isPaused = false;
        
        private Transform _target;
        private float availableSpeed = 3f;
        private float timeToCheck = 1f;
        private Vector3 lastPosition;

        private float interval = 1f;

        /// <summary>
        /// Setup Module
        /// </summary>
        /// <param name="config"></param>
        /// <param name="reinitialize"></param>
        public void SetupModule(IShieldModuleConfig config = null, bool reinitialize = false) {
            if (!Application.isPlaying) return;
            
            // Change Configuration
            _currentOptions = (Options)config ?? new Options();
            EventMessenger.Main.Publish(new SecurityModuleConfigChanged {
                Module = this,
                Config = _currentOptions
            });
            
            // Initialize Module
            if (!_initialized && !reinitialize)
                Initialize();
        }
        
        /// <summary>
        /// Initialize Module
        /// </summary>
        private void Initialize() {
            _target = _currentOptions.Target;
            availableSpeed = _currentOptions.AvailableSpeed;
            interval = _currentOptions.Interval;
            timeToCheck = interval;

            if (_target != null)
                lastPosition = _target.transform.position;
            
            if(_target == null)
                Debug.LogWarning($"{GeneralStrings.LOG_PREFIX} No Target setup to Teleport Detector. Please, use SetupTarget() to initialize detection.");

            _isPaused = false;

            // Fire Initialization Complete
            EventMessenger.Main.Subscribe<ApplicationLoopUpdated>(OnGameLoopUpdate);
            EventMessenger.Main.Publish(new SecurityModuleInitialized {
                Module = this
            });
        }

        /// <summary>
        /// Setup Target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="maxSpeed"></param>
        /// <param name="maxInterval"></param>
        public void SetupTarget(Transform target, float maxSpeed = 30f, float maxInterval = 1f) {
            _target = target;
            availableSpeed = maxSpeed;
            interval = maxInterval;
            timeToCheck = maxInterval;
        }

        /// <summary>
        /// Disconnect Module
        /// </summary>
        public void Disconnect() {
            EventMessenger.Main.Unsubscribe<ApplicationLoopUpdated>(OnGameLoopUpdate);
            EventMessenger.Main.Publish(new SecurityModuleDisconnected {
                Module = this
            });
        }

        /// <summary>
        /// On Game Loop Updated
        /// </summary>
        /// <param name="payload"></param>
        private void OnGameLoopUpdate(ApplicationLoopUpdated payload) {
            if(_isPaused || _target == null) return;
            
            if (timeToCheck <= 0f)
            {
                if (lastPosition != Vector3.zero) {
                    float distance = Vector3.Distance(lastPosition, _target.position);
                    if (distance > availableSpeed)
                    {
                        EventMessenger.Main.Publish(new SecurityWarningPayload {
                            Code = 422,
                            Message = TeleportWarnings.TeleportCheating,
                            IsCritical = true,
                            Module = this
                        });
                        PauseDetector(true);
                    }
                }

                lastPosition = _target.position;
                timeToCheck = interval;
            }
            else
            {
                timeToCheck -= payload.DeltaTime;
            }
        }
        
        /// <summary>
        /// Toggle Pause for Current Detector
        /// </summary>
        /// <param name="isPaused"></param>
        public void PauseDetector(bool isPaused) {
            if(isPaused == _isPaused) return;
            _isPaused = isPaused;
            EventMessenger.Main.Publish(new SecurityModulePause {
                Module = this,
                IsPaused = _isPaused
            });
        }

        /// <summary>
        /// Check if Detector Paused
        /// </summary>
        /// <returns></returns>
        public bool IsPaused() {
            return _isPaused;
        }

        /// <summary>
        /// Get Module Information
        /// </summary>
        /// <returns></returns>
        public ModuleInfo GetModuleInfo() {
            return new ModuleInfo {
                Name = "Teleport Detector",
                Description = "This module tracks a player's maximum allowable movements based on their speed to check for teleportation hacks",
                DocumentationLink = "https://github.com/DevsDaddy/GameShield/wiki/Modules-Overview#teleport-detector"
            };
        }
        
        // Module Options Configuration
        [System.Serializable]
        public class Options : IShieldModuleConfig
        {
            public Transform Target;
            public float AvailableSpeed = 30f;
            public float Interval = 1f;
        }
    }
}