using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.WallHack
{
    /// <summary>
    /// NoClip Detector Module
    /// </summary>
    public class WallHackProtector : IShieldModule
    {
        private Options _currentOptions;
        private bool _initialized = false;
        private bool _isPaused = false;
        
        private const string SERVICE_CONTAINER_NAME = "__WALLHACK_SERVICE__";
        private readonly Vector3 rigidPlayerVelocity = new Vector3(0, 0, 1f);
        
        private Vector3 spawnPosition;
        
        // Private Params
        private int whLayer = -1;
        private GameObject serviceContainer;
        private Rigidbody rigidPlayer;
        private CharacterController charControllerPlayer;
        private float charControllerVelocity = 0;

        private float timeToCheck = 4f;
        private float checkInterval = 4f;
        private bool checkRigid = false;
        private bool checkController = false;
        
#if DEBUG
        private bool rigidDetected = false;
        private bool controllerDetected = false;
#endif

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
            spawnPosition = _currentOptions.SpawnPosition;
            InitDetector();
            
            // Fire Initialization Complete
            EventMessenger.Main.Publish(new SecurityModuleInitialized {
                Module = this
            });
        }

        /// <summary>
        /// Disconnect Module
        /// </summary>
        public void Disconnect() {
            UninitDetector();
            
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
            if (isPaused) {
                StopRigidModule();
                StopControllerModule();
            }
            else {
                StartRigidModule();
                StartControllerModule();
            }
            EventMessenger.Main.Publish(new SecurityModulePause {
                Module = this,
                IsPaused = _isPaused
            });
        }
        
        /// <summary>
        /// Initialize Detector
        /// </summary>
        private void InitDetector()
        {
            InitCommon();
            InitRigidModule();
            InitControllerModule();

            StartRigidModule();
            StartControllerModule();
        }
        
        /// <summary>
        /// Uninitialize Detector
        /// </summary>
        private void UninitDetector()
        {
            _isPaused = true;
            StopRigidModule();
            StopControllerModule();
            GameObject.Destroy(serviceContainer);
        }
        
        /// <summary>
        /// Initialize Common Modules
        /// </summary>
        private void InitCommon()
        {
            if (whLayer == -1) whLayer = LayerMask.NameToLayer("Ignore Raycast");

            serviceContainer = new GameObject(SERVICE_CONTAINER_NAME);
            serviceContainer.layer = whLayer;
            serviceContainer.transform.position = spawnPosition;
            serviceContainer.transform.parent = null;
            GameObject.DontDestroyOnLoad(serviceContainer);

            GameObject wall = new GameObject("Wall");
            wall.AddComponent<BoxCollider>();
            wall.layer = whLayer;
            wall.transform.parent = serviceContainer.transform;
            wall.transform.localPosition = Vector3.zero;

            wall.transform.localScale = new Vector3(3, 3, 0.5f);
        }
        
        /// <summary>
        /// Initialize Rigid Modules
        /// </summary>
        private void InitRigidModule()
        {
            GameObject player = new GameObject("RigidPlayer");
            player.AddComponent<CapsuleCollider>().height = 2;
            player.layer = whLayer;
            player.transform.parent = serviceContainer.transform;
            player.transform.localPosition = new Vector3(0.75f, 0, -1f);
            rigidPlayer = player.AddComponent<Rigidbody>();
            rigidPlayer.useGravity = false;
        }
        
        /// <summary>
        /// Initialize Controller Module
        /// </summary>
        private void InitControllerModule()
        {
            GameObject player = new GameObject("ControlledPlayer");
            player.AddComponent<CapsuleCollider>().height = 2;
            player.layer = whLayer;
            player.transform.parent = serviceContainer.transform;
            player.transform.localPosition = new Vector3(-0.75f, 0, -1f);
            charControllerPlayer = player.AddComponent<CharacterController>();
        }
        
        /// <summary>
        /// Start Rigid Module
        /// </summary>
        private void StartRigidModule()
        {
            rigidPlayer.rotation = Quaternion.identity;
            rigidPlayer.angularVelocity = Vector3.zero;
            rigidPlayer.transform.localPosition = new Vector3(0.75f, 0, -1f);
            rigidPlayer.velocity = rigidPlayerVelocity;
            checkRigid = true;
        }
        
        /// <summary>
        /// Stop Rigid Module
        /// </summary>
        private void StopRigidModule()
        {
            rigidPlayer.velocity = Vector3.zero;
            checkRigid = false;
        }
        
        /// <summary>
        /// Start Controller Module
        /// </summary>
        private void StartControllerModule()
        {
            charControllerPlayer.transform.localPosition = new Vector3(-0.75f, 0, -1f);
            charControllerVelocity = 0.01f;
            checkController = true;
        }
        
        /// <summary>
        /// Stop Controller Module
        /// </summary>
        private void StopControllerModule()
        {
            charControllerVelocity = 0;
            checkController = false;
        }

        /// <summary>
        /// On Game Loop Updated
        /// </summary>
        /// <param name="payload"></param>
        private void OnGameLoopUpdate(ApplicationLoopUpdated payload) {
            if(_isPaused) return;
            
            // Restart checks
            if (timeToCheck <= 0f) {
                timeToCheck = checkInterval;
                if(checkRigid) StartRigidModule();
                if(checkController) StartControllerModule();
            }
            else {
                timeToCheck -= payload.DeltaTime;
            }
            
            if (charControllerVelocity > 0)
            {
                charControllerPlayer.Move(new Vector3(Random.Range(-0.002f, 0.002f), 0, charControllerVelocity));
                if (charControllerPlayer.transform.localPosition.z > 1f)
                {
#if DEBUG
                    controllerDetected = true;
#endif
                    StopControllerModule();
                    Detect();
                }
            }
        }

        /// <summary>
        /// On Fixed Loop Updated
        /// </summary>
        /// <param name="payload"></param>
        private void OnFixedLoopUpdate(ApplicationFixedLoopUpdated payload) {
            if(_isPaused) return;
            
            if (rigidPlayer.transform.localPosition.z > 1f)
            {
#if DEBUG
                rigidDetected = true;
#endif
                StopRigidModule();

                Detect();
            }
        }
        
        private void Detect() {
            EventMessenger.Main.Publish(new SecurityWarningPayload {
                Module = this,
                Message = WallhackMessages.Detected,
                Code = 562,
                IsCritical = true
            });
            PauseDetector(true);
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
                Name = "Wallhack Protector",
                Description = "This module allows you to cheat physics-based WallHack software cheats",
                DocumentationLink = "https://github.com/DevsDaddy/GameShield/wiki/Modules-Overview#wallhack-protector"
            };
        }
        
        // Module Options Configuration
        [System.Serializable]
        public class Options : IShieldModuleConfig
        {
            public Vector3 SpawnPosition = new Vector3(-1000,-1000,-1000);
        }
    }
}