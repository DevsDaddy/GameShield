using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;

namespace DevsDaddy.GameShield.Core.Modules.Injection
{
    /// <summary>
    /// Injection Scanner Module
    /// </summary>
    public class InjectionScanner : IShieldModule
    {
        private Options _currentOptions = new Options();
        private bool _initialized = false;
        private bool _isPaused = false;

        /// <summary>
        /// Setup Module
        /// </summary>
        /// <param name="config"></param>
        /// <param name="reinitialize"></param>
        public void SetupModule(IShieldModuleConfig config = null, bool reinitialize = false) {
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
        /// Get Module Information
        /// </summary>
        /// <returns></returns>
        public ModuleInfo GetModuleInfo() {
            return new ModuleInfo {
                Name = "Injection Scanner",
                Description = "This module monitors the introduction of external assemblies into the application executable code and checks it against the list of trusted ones.",
                DocumentationLink = "https://github.com/DevsDaddy/GameShield/wiki/Modules-Overview#injection-scanner"
            };
        }
        
        // Module Options Configuration
        [System.Serializable]
        public class Options : IShieldModuleConfig
        {
            
        }
    }
}