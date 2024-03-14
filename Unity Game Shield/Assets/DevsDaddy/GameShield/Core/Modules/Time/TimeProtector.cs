using System;
using System.Collections;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;
using UnityEngine.Networking;

namespace DevsDaddy.GameShield.Core.Modules.Time
{
    /// <summary>
    /// Time Skip Protector Module
    /// </summary>
    public class TimeProtector : IShieldModule
    {
        private Options _currentOptions;
        private bool _initialized = false;
        private bool _isPaused = false;
        
        private float timeCheckInterval = 10f;
        private long availableTolerance = 60;
        private bool networkCompare = true;
        
        private float timeToCheck = 10f;
        private long lastTime = 0;
        private long lastLocalTime = 0;
        
        

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
            timeCheckInterval = _currentOptions.CheckingInterval;
            availableTolerance = _currentOptions.AvailableTolerance;
            networkCompare = _currentOptions.NetworkCompare;
            timeToCheck = timeCheckInterval;
            
            CompareTime();

            // Fire Initialization Complete
            EventMessenger.Main.Subscribe<ApplicationLoopUpdated>(OnLoopUpdated);
            EventMessenger.Main.Publish(new SecurityModuleInitialized {
                Module = this
            });
        }
        
        /// <summary>
        /// Disconnect Module
        /// </summary>
        public void Disconnect() {
            // Fire Disconnected Complete
            EventMessenger.Main.Unsubscribe<ApplicationLoopUpdated>(OnLoopUpdated);
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
        /// On Application Loop Updated
        /// </summary>
        /// <param name="payload"></param>
        private void OnLoopUpdated(ApplicationLoopUpdated payload) {
            if(_isPaused) return;
            if (timeToCheck <= 0f)
            {
                CompareTime();
            }
            else
            {
                timeToCheck -= payload.DeltaTime;
            }
        }

        /// <summary>
        /// Compare Timings
        /// </summary>
        private void CompareTime() {
            long currentNetworkTime = 0;
            long currentLocalTime = GetCurrentLocalTime();
            
            if (networkCompare)
            {
                GetCurrentNetworkTime(time =>
                {
                    currentNetworkTime = time;
                    if (lastTime == 0)
                    {
                        lastTime = currentNetworkTime;
                        lastLocalTime = currentLocalTime;
                    }
                    else
                    {
                        CompareTwoTimestamps(currentNetworkTime, currentLocalTime);
                    }
                }, () => {
                    EventMessenger.Main.Publish(new SecurityWarningPayload {
                        Code = 244,
                        IsCritical = true,
                        Message = TimeWarnings.NetworkTimeError,
                        Module = this
                    });
                    PauseDetector(true);
                });
            }
            else
            {
                currentNetworkTime = GetCurrentLocalTime();
                if (lastTime == 0)
                {
                    lastTime = currentNetworkTime;
                    lastLocalTime = currentLocalTime;
                }
                else
                {
                    CompareTwoTimestamps(currentNetworkTime, currentLocalTime);
                }
            }
        }
        
        /// <summary>
        /// Compare Two Timestamps
        /// </summary>
        /// <param name="currentNetworkTime"></param>
        /// <param name="currentLocalTime"></param>
        void CompareTwoTimestamps(long currentNetworkTime, long currentLocalTime)
        {
            long networkTimeDiff = 0;
            long localTimeDiff = 0;
            long avgTimeDiff = 0;
            
            networkTimeDiff = currentNetworkTime - lastTime;
            localTimeDiff = currentLocalTime - lastLocalTime;
            avgTimeDiff = (localTimeDiff > networkTimeDiff)
                ? localTimeDiff - networkTimeDiff
                : networkTimeDiff - localTimeDiff;
            if (avgTimeDiff > availableTolerance)
            {
                EventMessenger.Main.Publish(new SecurityWarningPayload {
                    Code = 245,
                    IsCritical = true,
                    Message = TimeWarnings.TimeCheating,
                    Module = this
                });
                PauseDetector(true);
                return;
            }
                    
            lastTime = currentNetworkTime;
            lastLocalTime = currentLocalTime;
        }
        
        /// <summary>
        /// Get Current Network Time
        /// </summary>
        /// <param name="onTimeRecieved"></param>
        /// <param name="onRequestError"></param>
        private void GetCurrentNetworkTime(Action<long> onTimeRecieved, Action onRequestError) {
            EventMessenger.Main.Publish(new RequestCoroutine {
                Coroutine = RequestNetworkTime(onTimeRecieved, onRequestError),
                Id = "NetworkTimeCompare"
            });
        }
        
        /// <summary>
        /// Request Network Time
        /// </summary>
        /// <param name="onTimeRecieved"></param>
        /// <param name="onRequestError"></param>
        /// <returns></returns>
        private IEnumerator RequestNetworkTime(Action<long> onTimeRecieved, Action onRequestError)
        {
            UnityWebRequest webRequest = new UnityWebRequest(_currentOptions.NetworkServer, "GET");
            DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
            webRequest.downloadHandler = dH;
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                NetworkTimeModel response = JsonUtility.FromJson<NetworkTimeModel>(webRequest.downloadHandler.text);
                onTimeRecieved?.Invoke(response.unixtime);
            }
            else
            {
                onRequestError?.Invoke();
            }
            
            webRequest.Dispose();
        }
        
        /// <summary>
        /// Get Current Local Time
        /// </summary>
        /// <returns></returns>
        private long GetCurrentLocalTime()
        {
            DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long currentEpochTime = (long)(DateTime.UtcNow - epochStart).TotalSeconds;
            return currentEpochTime;
        }
        
        /// <summary>
        /// Seconds Elapsed
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        private int SecondsElapsed(int t1, int t2)
        {
            int difference = t1 - t2;
            return Mathf.Abs(difference);
        }

        /// <summary>
        /// Get Module Information
        /// </summary>
        /// <returns></returns>
        public ModuleInfo GetModuleInfo() {
            return new ModuleInfo {
                Name = "Time Skip Protector",
                Description = "This module allows you to protect yourself against time rewinds on the user's device",
                DocumentationLink = "https://github.com/DevsDaddy/GameShield/wiki/Modules-Overview#timeskip-protector"
            };
        }
        
        // Module Options Configuration
        [System.Serializable]
        public class Options : IShieldModuleConfig
        {
            public float CheckingInterval = 10f;
            public int AvailableTolerance = 60;
            public bool NetworkCompare = true;
            public string NetworkServer = "https://worldtimeapi.org/api/timezone/utc";
        }
        
        [System.Serializable]
        public class NetworkTimeModel
        {
            public string abbreviation = "";
            public string client_ip = "";
            public string datetime = "";
            public uint day_of_week = 0;
            public uint day_of_year = 0;
            public bool dst = false;
            public int dst_offset = 0;
            public int raw_offset = 0;
            public string timezone = "";
            public long unixtime = 0;
            public string utc_datetime = "";
            public string utc_offset = "+00:00";
            public uint week_number = 0;
        }
    }
}