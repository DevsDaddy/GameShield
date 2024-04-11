using System;
using System.Collections.Generic;
using System.Linq;
using DevsDaddy.GameShield.Core.Constants;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.GameShield.Core.Utils;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;
using Random = System.Random;

namespace DevsDaddy.GameShield.Core.Modules.Captcha
{
    /// <summary>
    /// Rewarded Captcha Module
    /// </summary>
    public class RewardedCaptcha : IShieldModule
    {
        private Options _currentOptions;
        private bool _initialized = false;
        private bool _isPaused = false;

        private RewardedCaptchaData currentData;

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
        /// Disconnect Module
        /// </summary>
        public void Disconnect() {
            // Fire Disconnected Complete
            EventMessenger.Main.Publish(new SecurityModuleDisconnected {
                Module = this
            });
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
        /// Initialize Module
        /// </summary>
        private void Initialize() {
            // Fire Initialization Complete
            EventMessenger.Main.Publish(new SecurityModuleInitialized {
                Module = this
            });
        }

        /// <summary>
        /// Generate Captcha Data
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="availableSpritesCount"></param>
        /// <returns></returns>
        public RewardedCaptchaData GenerateCaptchaData(RequestCaptchaPayload payload, int availableSpritesCount) {
            // Check Sizes
            if (payload.NumOfImages > availableSpritesCount) {
                Debug.LogWarning($"{GeneralStrings.LOG_PREFIX} Number of Required images is larger than available sprites count in captcha.");
                payload.NumOfImages = availableSpritesCount;
            }
            
            // Current Data
            currentData = new RewardedCaptchaData {
                RequiredOrder = new int[payload.NumOfImages],
                CurrentFieldOrder = new int[payload.NumOfImages]
            };
            
            // Generate Required List
            currentData.RequiredOrder =
                Generator.GenerateUniqueRandomRange(0, availableSpritesCount - 1, payload.NumOfImages);
            
            // Shuffle Order for Clicks
            var rnd = new Random();
            currentData.CurrentFieldOrder = currentData.RequiredOrder.Select(x => (x, rnd.Next()))
                .OrderBy(tuple => tuple.Item2)
                .Select(tuple => tuple.Item1)
                .ToArray();
            return currentData;
        }

        /// <summary>
        /// Get Captcha Data
        /// </summary>
        /// <returns></returns>
        public RewardedCaptchaData GetData() {
            return currentData;
        }

        /// <summary>
        /// Check if is right clicking order
        /// </summary>
        /// <param name="providedOrder"></param>
        /// <returns></returns>
        public bool IsRightOrder(List<int> providedOrder) {
            // Check Length
            if (providedOrder.Count != currentData.RequiredOrder.Length) {
                Debug.LogError($"{GeneralStrings.LOG_PREFIX} Failed to check captcha order. Provided order length must be equal of current required order length");
                return false;
            }
            
            // Check Values
            bool isRight = true;
            for (int i = 0; i < providedOrder.Count; i++) {
                if (providedOrder[i] != currentData.RequiredOrder[i]) {
                    isRight = false;
                    break;
                }
            }

            return isRight;
        }

        /// <summary>
        /// Get Module Information
        /// </summary>
        /// <returns></returns>
        public ModuleInfo GetModuleInfo() {
            return new ModuleInfo {
                Name = "Rewarded Captcha",
                Description = "This module allows you to create client-side captchas when you suspect player cheating with a reward for completion.",
                DocumentationLink = "https://github.com/DevsDaddy/GameShield/wiki/Modules-Overview#rewarded-captcha"
            };
        }
        
        // Module Options Configuration
        [System.Serializable]
        public class Options : IShieldModuleConfig { }
    }
}