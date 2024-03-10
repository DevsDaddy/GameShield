using System;
using DevsDaddy.GameShield.Core.Constants;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.SpeedHack
{
    /// <summary>
    /// SpeedHack Detector Module
    /// </summary>
    public class SpeedHackDetector : IShieldModule
    {
        private Options _currentOptions;
        private bool _initialized = false;
        private bool _isPaused = false;
        
        private const long TICKS_PER_SECOND = TimeSpan.TicksPerMillisecond * 1000;
        private const int THRESHOLD = 5000000;
        
        private float m_Interval = 1f;
        private byte m_MaxFalsePositives = 3;
        private int m_CoolDown = 30;
        
        private byte m_CurrentFalsePositives;
        private int m_CurrentCooldownShots;
        private long m_TicksOnStart;
        private long m_VulnerableTicksOnStart;
        private long m_PrevTicks;
        private long m_PrevIntervalTicks;

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
            // Setup from Config
            m_Interval = _currentOptions.Interval;
            m_MaxFalsePositives = _currentOptions.MaxFalsePositives;
            m_CoolDown = _currentOptions.CoolDown;
            
            ResetStartTicks();
            m_CurrentFalsePositives = 0;
            m_CurrentCooldownShots = 0;
            _isPaused = false;

            // Fire Initialization Complete
            EventMessenger.Main.Subscribe<ApplicationLoopUpdated>(OnGameLoopUpdate);
            EventMessenger.Main.Publish(new SecurityModuleInitialized {
                Module = this
            });
        }
        
        /// <summary>
        /// Reset Startup Ticks
        /// </summary>
        private void ResetStartTicks()
        {
            m_TicksOnStart = DateTime.UtcNow.Ticks;
            m_VulnerableTicksOnStart = System.Environment.TickCount * TimeSpan.TicksPerMillisecond;
            m_PrevTicks = m_TicksOnStart;
            m_PrevIntervalTicks = m_TicksOnStart;
        }

        /// <summary>
        /// Disconnect Module
        /// </summary>
        public void Disconnect() {
            // Fire Disconnected Complete
            EventMessenger.Main.Unsubscribe<ApplicationLoopUpdated>(OnGameLoopUpdate);
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
            if(!_isPaused) ResetStartTicks();
            EventMessenger.Main.Publish(new SecurityModulePause {
                Module = this,
                IsPaused = _isPaused
            });
        }

        /// <summary>
        /// On Game Loop Updated
        /// </summary>
        /// <param name="payload"></param>
        private void OnGameLoopUpdate(ApplicationLoopUpdated payload) {
            if(_isPaused) return;
            
            long ticks = DateTime.UtcNow.Ticks;
            long ticksSpentSinceLastUpdate = ticks - m_PrevTicks;
            
            if (ticksSpentSinceLastUpdate < 0 || ticksSpentSinceLastUpdate > TICKS_PER_SECOND)
            {
                if (Debug.isDebugBuild) Debug.LogWarning($"{GeneralStrings.LOG_PREFIX} SpeedHack Detector: System DateTime change or > 1 second game freeze detected!");
                ResetStartTicks();
                return;
            }
            m_PrevTicks = ticks;
            
            long intervalTicks = (long)(m_Interval * TICKS_PER_SECOND);
            if (ticks - m_PrevIntervalTicks >= intervalTicks)
            {
                long vulnerableTicks = System.Environment.TickCount * TimeSpan.TicksPerMillisecond;
                if (Mathf.Abs((vulnerableTicks - m_VulnerableTicksOnStart) - (ticks - m_TicksOnStart)) > THRESHOLD)
                {
                    m_CurrentFalsePositives++;
                    if (m_CurrentFalsePositives > m_MaxFalsePositives)
                    {
                        if (Debug.isDebugBuild) Debug.LogWarning($"{GeneralStrings.LOG_PREFIX} SpeedHackDetector: final detection!");
                        EventMessenger.Main.Publish(new SecurityWarningPayload {
                            Code = 321,
                            Message = SpeedHackWarnings.SpeedHackDetected,
                            IsCritical = true,
                            Module = this
                        });
                        PauseDetector(true);
                    }
                    else
                    {
                        if (Debug.isDebugBuild) Debug.LogWarning($"{GeneralStrings.LOG_PREFIX} SpeedHackDetector: detection! Allowed false positives left: " + (m_MaxFalsePositives - m_CurrentFalsePositives));
                        m_CurrentCooldownShots = 0;
                        ResetStartTicks();
                    }
                }
                else if (m_CurrentFalsePositives > 0 && m_CoolDown > 0)
                {
                    if (Debug.isDebugBuild) Debug.LogWarning($"{GeneralStrings.LOG_PREFIX} SpeedHackDetector: success shot! Shots till Cooldown: " + (m_CoolDown - m_CurrentCooldownShots));
                    m_CurrentCooldownShots++;
                    if (m_CurrentCooldownShots >= m_CoolDown)
                    {
                        if (Debug.isDebugBuild) Debug.LogWarning($"{GeneralStrings.LOG_PREFIX} SpeedHackDetector: Cooldown!");
                        m_CurrentFalsePositives = 0;
                    }
                }

                m_PrevIntervalTicks = ticks;
            }
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
                Name = "SpeedHack Detector",
                Description = "The module allows you to track time acceleration attempts to change character speeds in-game, by monitoring the real time flow and frame time.",
                DocumentationLink = "https://github.com/DevsDaddy/GameShield/wiki/Modules-Overview#speedhack-detector"
            };
        }
        
        // Module Options Configuration
        [System.Serializable]
        public class Options : IShieldModuleConfig
        {
            public float Interval = 1f;
            public byte MaxFalsePositives = 3;
            public int CoolDown = 30;
        }
    }
}