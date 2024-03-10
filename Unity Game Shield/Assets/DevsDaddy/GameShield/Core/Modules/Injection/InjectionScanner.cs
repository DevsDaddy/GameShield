#define DEBUG
#undef DEBUG

#define DEBUG_VERBOSE
#undef DEBUG_VERBOSE

#define DEBUG_PARANOID
#undef DEBUG_PARANOID

using System;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;
using System.IO;
using System.Reflection;
using DevsDaddy.GameShield.Core.Constants;
using DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes;
using UnityEngine;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
using System.Diagnostics;
#endif

namespace DevsDaddy.GameShield.Core.Modules.Injection
{
    /// <summary>
    /// Injection Scanner Module
    /// </summary>
    public class InjectionScanner : IShieldModule
    {
        private Options _currentOptions;
        private bool _initialized = false;
        private bool _isPaused = false;
        
        // Private Params
        private bool m_SignaturesAreNotGenuine;
        private AllowedAssembly[] m_AllowedAssemblies;
        private string[] m_HexTable;
        
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
            
            #if !UNITY_STANDALONE && !UNITY_WEBPLAYER && !UNITY_IPHONE && !UNITY_ANDROID
            Debug.LogError($"{GeneralStrings.LOG_PREFIX}Injection Scanner is not supported on selected platform!");
            return;
            #endif
            
            #if UNITY_EDITOR
            #if !DEBUG && !DEBUG_VERBOSE && !DEBUG_PARANOID
            if (Application.isEditor)
            {
                Debug.LogWarning($"{GeneralStrings.LOG_PREFIX} Injection Detection does not work in editor (check readme for details).");
                return;
            }
            #else
                Debug.LogWarning($"{GeneralStrings.LOG_PREFIX} Injection Scanner works in debug mode. There WILL BE false positives in editor, it's fine!");
            #endif
            #endif
            
            // Initialize Module
            if (!_initialized && !reinitialize)
                Initialize();
        }

        /// <summary>
        /// Disconnect Module
        /// </summary>
        public void Disconnect() {
#if !UNITY_STANDALONE && !UNITY_WEBPLAYER && !UNITY_IPHONE && !UNITY_ANDROID
            Debug.LogError($"{GeneralStrings.LOG_PREFIX}Injection Scanner is not supported on selected platform!");
            return;
#endif
            
            AppDomain.CurrentDomain.AssemblyLoad -= OnNewAssemblyLoaded;
            _isPaused = true;
            
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
#if !UNITY_STANDALONE && !UNITY_WEBPLAYER && !UNITY_IPHONE && !UNITY_ANDROID
            Debug.LogError($"{GeneralStrings.LOG_PREFIX}Injection Scanner is not supported on selected platform!");
            return;
#endif
            
            if(isPaused == _isPaused) return;
            _isPaused = isPaused;

            if (_isPaused) {
                AppDomain.CurrentDomain.AssemblyLoad -= OnNewAssemblyLoaded;
            }
            else {
                AppDomain.CurrentDomain.AssemblyLoad += OnNewAssemblyLoaded;
            }
            
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
            // Load Allowed Assemblies
            if (m_AllowedAssemblies == null)
            {
                LoadAndParseAllowedAssemblies();
            }
            
            // Check Signatures
            if (m_SignaturesAreNotGenuine)
            {
                OnInjectionDetected();
                return;
            }
            
            // Find Injections
            if (!FindInjectionInCurrentAssemblies())
            {
                // listening for new assemblies
                AppDomain.CurrentDomain.AssemblyLoad += OnNewAssemblyLoaded;
                _isPaused = false;
            }
            else
            {
                OnInjectionDetected();
            }

            // Fire Initialization Complete
            EventMessenger.Main.Publish(new SecurityModuleInitialized {
                Module = this
            });
        }
        
        /// <summary>
        /// On Injection Detected
        /// </summary>
        private void OnInjectionDetected()
        {
            EventMessenger.Main.Publish(new SecurityWarningPayload {
                Code = 101,
                Message = InjectionWarnings.InjectionDetected,
                IsCritical = true,
                Module = this
            });
            PauseDetector(true);
        }
        
        /// <summary>
        /// New Assembly Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnNewAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
        {
#if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
			Debug.Log($"{GeneralStrings.LOG_PREFIX} New assembly loaded: {args.LoadedAssembly.FullName}");
#endif
            
            if (!AssemblyAllowed(args.LoadedAssembly))
            {
#if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
				Debug.Log($"{GeneralStrings.LOG_PREFIX} Injected Assembly found:\n {args.LoadedAssembly.FullName}" );
#endif
                OnInjectionDetected();
            }
        }
        
        /// <summary>
        /// Find Injection in Current Assemblies
        /// </summary>
        /// <returns></returns>
        private bool FindInjectionInCurrentAssemblies()
        {
            bool result = false;
            
#if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
			Stopwatch stopwatch = Stopwatch.StartNew();
#endif
            
            Assembly[] assembliesInCurrentDomain = AppDomain.CurrentDomain.GetAssemblies();
            if (assembliesInCurrentDomain.Length == 0)
            {
#if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
				stopwatch.Stop();
				Debug.Log($"{GeneralStrings.LOG_PREFIX} 0 assemblies in current domain! Not genuine behavior.");
				stopwatch.Start();
#endif
                result = true;
            }else
            {
                foreach (Assembly ass in assembliesInCurrentDomain)
                {
#if DEBUG_VERBOSE	
				    stopwatch.Stop();
				    Debug.Log($"{GeneralStrings.LOG_PREFIX} Currenly loaded assembly:\n {ass.FullName}");
				    stopwatch.Start();
#endif
                    
                    if (!AssemblyAllowed(ass))
                    {
#if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
						stopwatch.Stop();
						Debug.Log($"{GeneralStrings.LOG_PREFIX} Injected Assembly found:\n {ass.FullName} \n {GetAssemblyHash(ass)}");
						stopwatch.Start();
#endif
                        
                        result = true;
                        break;
                    }
                }
            }
            
#if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
			stopwatch.Stop();
			Debug.Log($"{GeneralStrings.LOG_PREFIX} Loaded assemblies scan duration: {stopwatch.ElapsedMilliseconds} ms.");
#endif
            
            return result;
        }
        
        /// <summary>
        /// Allowed Assembly
        /// </summary>
        /// <param name="ass"></param>
        /// <returns></returns>
        private bool AssemblyAllowed(Assembly ass)
        {
#if !UNITY_WEBPLAYER
            string assemblyName = ass.GetName().Name;
#else
			string fullname = ass.FullName;
			string assemblyName = fullname.Substring(0, fullname.IndexOf(", ", StringComparison.Ordinal));
#endif
            
            int hash = GetAssemblyHash(ass);
            
            bool result = false;
            for (int i = 0; i < m_AllowedAssemblies.Length; i++)
            {
                AllowedAssembly allowedAssembly = m_AllowedAssemblies[i];

                if (allowedAssembly.name == assemblyName)
                {
                    if (Array.IndexOf(allowedAssembly.hashes, hash) != -1)
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }
        
        /// <summary>
        /// Load and Parse Allowed Assemblies
        /// </summary>
        private void LoadAndParseAllowedAssemblies()
        {
            #if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
			Debug.Log($"{GeneralStrings.LOG_PREFIX} Starting LoadAndParseAllowedAssemblies()");
			Stopwatch sw = Stopwatch.StartNew();
            #endif
            
            TextAsset assembliesSignatures = (TextAsset)Resources.Load(GeneralConstants.ASSMDB_PATH, typeof(TextAsset));
            if (assembliesSignatures == null)
            {
                m_SignaturesAreNotGenuine = true;
                return;
            }
            
            #if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
			sw.Stop();
			Debug.Log($"{GeneralStrings.LOG_PREFIX} Creating separator array and opening MemoryStream");
			sw.Start();
            #endif
            
            string[] separator = {":"};

            MemoryStream ms = new MemoryStream(assembliesSignatures.bytes);
            BinaryReader br = new BinaryReader(ms);
			
            int count = br.ReadInt32();
            
            #if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
			sw.Stop();
			Debug.Log($"{GeneralStrings.LOG_PREFIX} Allowed assemblies count from MS: {count}");
			sw.Start();
            #endif
            
            m_AllowedAssemblies = new AllowedAssembly[count];
            
            for (int i = 0; i < count; i++)
            {
                string line = br.ReadString();
                #if (DEBUG_PARANOID)
				sw.Stop();
				Debug.Log($"{GeneralStrings.LOG_PREFIX} Line: {line}");
				sw.Start();
                #endif
                line = SecuredString.EncryptDecrypt(line, "GAMESHIELD");
                #if (DEBUG_PARANOID)
				sw.Stop();
				Debug.Log($"{GeneralStrings.LOG_PREFIX} Line decrypted : {line}");
				sw.Start();
                #endif
                
                string[] strArr = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                int stringsCount = strArr.Length;
                #if (DEBUG_PARANOID)
				sw.Stop();
				Debug.Log($"{GeneralStrings.LOG_PREFIX} stringsCount : {stringsCount}");
				sw.Start();
                #endif
                
                if (stringsCount > 1)
                {
                    string assemblyName = strArr[0];

                    int[] hashes = new int[stringsCount - 1];
                    for (int j = 1; j < stringsCount; j++)
                    {
                        hashes[j - 1] = int.Parse(strArr[j]);
                    }

                    m_AllowedAssemblies[i] = (new AllowedAssembly(assemblyName, hashes));
                }
                else
                {
                    m_SignaturesAreNotGenuine = true;
                    br.Close();
                    ms.Close();
                    #if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
					sw.Stop();
                    #endif
                    return;
                }
            }
            br.Close();
            ms.Close();
            Resources.UnloadAsset(assembliesSignatures);

            #if DEBUG || DEBUG_VERBOSE || DEBUG_PARANOID
			sw.Stop();
			Debug.Log($"{GeneralStrings.LOG_PREFIX} Allowed Assemblies parsing duration: {sw.ElapsedMilliseconds} ms.");
            #endif

            m_HexTable = new string[256];
            for (int i = 0; i < 256; i++)
            {
                m_HexTable[i] = i.ToString("x2");
            }
        }
        
        /// <summary>
        /// Get Assembly Hash
        /// </summary>
        /// <param name="ass"></param>
        /// <returns></returns>
        private int GetAssemblyHash(Assembly ass)
        {
            string hashInfo;
#if !UNITY_WEBPLAYER
            AssemblyName assName = ass.GetName();
            byte[] bytes = assName.GetPublicKeyToken();
            if (bytes.Length == 8)
            {
                hashInfo = assName.Name + PublicKeyTokenToString(bytes);
            }
            else
            {
                hashInfo = assName.Name;
            }
#else
			string fullName = ass.FullName;

			string assemblyName = fullName.Substring(0, fullName.IndexOf(", ", StringComparison.Ordinal));
			int tokenIndex = fullName.IndexOf("PublicKeyToken=", StringComparison.Ordinal) + 15;
			string token = fullName.Substring(tokenIndex, fullName.Length - tokenIndex);
			if (token == "null") token = "";
			hashInfo = assemblyName + token;
#endif
            
            int result = 0;
            int len = hashInfo.Length;

            for (int i = 0; i < len; ++i)
            {
                result += hashInfo[i];
                result += (result << 10);
                result ^= (result >> 6);
            }
            result += (result << 3);
            result ^= (result >> 11);
            result += (result << 15);

            return result;
        }
        
#if !UNITY_WEBPLAYER
        /// <summary>
        /// Public Token to String
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private string PublicKeyTokenToString(byte[] bytes)
        {
            string result = "";
            for (int i = 0; i < 8; i++)
            {
                result += m_HexTable[bytes[i]];
            }

            return result;
        }
#endif

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