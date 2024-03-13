using System.Collections.Generic;
using System.Linq;
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
        
        private float timeToCheck = 1f;
        private float interval = 1f;

        private List<TeleportTargetChecker> _targets = new List<TeleportTargetChecker>();

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
            _targets = _currentOptions.targets;
            interval = _currentOptions.Interval;
            timeToCheck = interval;

            if(_targets.Count < 1)
                Debug.LogWarning($"{GeneralStrings.LOG_PREFIX} No Target setup to Teleport Detector. Please, use AddTarget() to initialize detection.");

            _isPaused = false;

            // Fire Initialization Complete
            EventMessenger.Main.Subscribe<ApplicationLoopUpdated>(OnGameLoopUpdate);
            EventMessenger.Main.Publish(new SecurityModuleInitialized {
                Module = this
            });
        }

        /// <summary>
        /// Add Teleport Target
        /// </summary>
        /// <param name="target"></param>
        public void AddTarget(TeleportTargetChecker target) {
            if(_targets.Contains(target)) return;
            _targets.Add(target);
            interval = _currentOptions.Interval;
            timeToCheck = _currentOptions.Interval;
        }

        /// <summary>
        /// Remove Target
        /// </summary>
        /// <param name="target"></param>
        public void RemoveTarget(TeleportTargetChecker target) {
            if (_targets.Contains(target))
                _targets.Remove(target);
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
            if(_isPaused || _targets.Count < 1) return;
            
            if (timeToCheck <= 0f)
            {
                foreach (var target in _targets) {
                    if(target == null || target.Target == null) continue;
                    if (target.LastPosition != Vector3.zero) {
                        float distance = Vector3.Distance(target.LastPosition, target.Target.position);
                        if (distance > target.MaxSpeed)
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

                    target.LastPosition = target.Target.position;
                }
                
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
            public List<TeleportTargetChecker> targets = new List<TeleportTargetChecker>();
            public float Interval = 1f;
        }
    }
}