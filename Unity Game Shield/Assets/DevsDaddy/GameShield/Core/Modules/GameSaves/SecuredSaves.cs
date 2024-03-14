using System;
using System.IO;
using DevsDaddy.GameShield.Core.Constants;
using DevsDaddy.GameShield.Core.Modules.GameSaves.Payloads;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.CryptoLibrary;
using DevsDaddy.Shared.CryptoLibrary.Core;
using DevsDaddy.Shared.CryptoLibrary.Modules.AES;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.GameSaves
{
    /// <summary>
    /// Secured Game Saves Module
    /// </summary>
    public class SecuredSaves : IShieldModule
    {
        private Options _currentOptions;
        private bool _initialized = false;
        private bool _isPaused = false;

        private ICryptoProvider _currentProvider = null;
        private SaveGameData _currentSave = null;

        /// <summary>
        /// Setup Module
        /// </summary>
        /// <param name="config"></param>
        /// <param name="reinitialize"></param>
        public void SetupModule(IShieldModuleConfig config = null, bool reinitialize = false) {
            if (!Application.isPlaying) return;
            
            // Change Configuration
            _currentOptions = (Options)config ?? new Options();
            _currentProvider = _currentOptions.Provider;
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
            // Unsubscribe from payloads
            EventMessenger.Main.Unsubscribe<SaveGamePayload>(OnSaveRequested);
            EventMessenger.Main.Unsubscribe<LoadGamePayload>(OnLoadGameRequested);
            
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
            // Subscribe to Payloads
            EventMessenger.Main.Subscribe<SaveGamePayload>(OnSaveRequested);
            EventMessenger.Main.Subscribe<LoadGamePayload>(OnLoadGameRequested);
            
            // Fire Initialization Complete
            EventMessenger.Main.Publish(new SecurityModuleInitialized {
                Module = this
            });
        }

        /// <summary>
        /// Save Game Requested
        /// </summary>
        /// <param name="payload"></param>
        private void OnSaveRequested(SaveGamePayload payload) {
            SaveGame(payload.Path, payload.Object, payload.OnComplete);
        }

        /// <summary>
        /// Load Game Requested
        /// </summary>
        /// <param name="payload"></param>
        private void OnLoadGameRequested(LoadGamePayload payload) {
            LoadGame<ISaveObject>(payload.Path, (saveData, saveObject) => {
                payload.OnComplete?.Invoke(saveData, saveObject);
            });
        }

        /// <summary>
        /// Save Game
        /// </summary>
        /// <param name="path"></param>
        /// <param name="saveObject"></param>
        /// <param name="onComplete"></param>
        /// <param name="onError"></param>
        public void SaveGame<T>(string path, T saveObject, Action onComplete = null, Action<string> onError = null) {
            // Create Save Data
            SaveGameData data = new SaveGameData {
                Date = DateTime.Now,
                Object = (ISaveObject)saveObject
            };
            
            // Convert to JSON and Save
            string jsonData = JsonUtility.ToJson(data);
            bool encryptedFile = CryptoFile.WriteText(path, jsonData, _currentProvider);
            if (!encryptedFile) {
                Debug.LogError($"{GeneralStrings.LOG_PREFIX} Error thrown at game saving process");
                onError?.Invoke(SavesWarnings.NO_DATA_FOUND_IN_SAVES);
                return;
            }
            
            // Complete Save
            onComplete?.Invoke();
        }

        /// <summary>
        /// Load Game
        /// </summary>
        /// <param name="path"></param>
        /// <param name="onComplete"></param>
        /// <param name="onError"></param>
        /// <typeparam name="T"></typeparam>
        public void LoadGame<T>(string path, Action<SaveGameData, T> onComplete = null, Action<string> onError = null) where T : ISaveObject{
            if(!HasSave(path)) return;
            
            // Load and Decrypt Save File
            string decryptedData = CryptoFile.ReadText(path, _currentProvider);
            if (string.IsNullOrEmpty(decryptedData)) {
                Debug.LogError($"{GeneralStrings.LOG_PREFIX} Load Game Error: {SavesWarnings.NO_DATA_FOUND_IN_SAVES}");
                onError?.Invoke(SavesWarnings.NO_DATA_FOUND_IN_SAVES);
                return;
            }

            // Parse Save Data
            SaveGameData data = JsonUtility.FromJson<SaveGameData>(decryptedData);
            if (data == null) {
                Debug.LogError($"{GeneralStrings.LOG_PREFIX} Load Game Error: {SavesWarnings.NO_DATA_FOUND_IN_SAVES}");
                onError?.Invoke(SavesWarnings.NO_DATA_FOUND_IN_SAVES);
                return;
            }

            _currentSave = data;
            onComplete?.Invoke(data, (T)data.Object);
            Debug.Log($"{GeneralStrings.LOG_PREFIX} Load Game Complete: {path}");
        }

        /// <summary>
        /// Get Latest Save
        /// </summary>
        /// <returns></returns>
        public SaveGameData GetLatestSave() {
            return _currentSave;
        }

        /// <summary>
        /// Check Game Save Exists
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool HasSave(string path) {
            return File.Exists(path);
        }

        /// <summary>
        /// Set Crypto Provider
        /// </summary>
        /// <param name="provider"></param>
        public void SetCryptoProvider(ICryptoProvider provider) {
            if(_currentProvider == null || provider != _currentProvider) 
                _currentProvider = provider;
        }

        /// <summary>
        /// Get Crypto Provider
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public ICryptoProvider GetCryptoProvider() {
            return _currentProvider;
        }

        /// <summary>
        /// Get Module Information
        /// </summary>
        /// <returns></returns>
        public ModuleInfo GetModuleInfo() {
            return new ModuleInfo {
                Name = "Secured Saves",
                Description = "This module allows you to save game states simply and quickly by encrypted serialization/deserialization of save files.",
                DocumentationLink = "https://github.com/DevsDaddy/GameShield/wiki/Modules-Overview#secured-saves"
            };
        }
        
        // Module Options Configuration
        [System.Serializable]
        public class Options : IShieldModuleConfig
        {
            public ICryptoProvider Provider = new AESProvider(new AESEncryptionOptions {
                cryptoKey = GameShield.Main.GetDeveloperKey()
            });
        }
    }
}