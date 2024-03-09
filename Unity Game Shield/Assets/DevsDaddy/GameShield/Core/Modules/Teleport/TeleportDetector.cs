using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;

namespace DevsDaddy.GameShield.Core.Modules.Teleport
{
    /// <summary>
    /// Teleport Detector Module
    /// </summary>
    public class TeleportDetector : IShieldModule
    {
        private Options _currentOptions = new Options();
        private bool _initialized = false;

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
                Name = "Teleport Detector",
                Description = "This module tracks a player's maximum allowable movements based on their speed to check for teleportation hacks",
                DocumentationLink = "https://github.com/DevsDaddy/GameShield/wiki/Modules-Overview#teleport-detector"
            };
        }
        
        // Module Options Configuration
        [System.Serializable]
        public class Options : IShieldModuleConfig
        {
            
        }
    }
}