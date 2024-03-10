#define DEBUG
#undef DEBUG

#define DEBUG_VERBOSE
#undef DEBUG_VERBOSE

#define DEBUG_PARANIOD
#undef DEBUG_PARANIOD


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DevsDaddy.GameShield.Core.Constants;
using DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
#if (DEBUG || DEBUG_VERBOSE || DEBUG_PARANIOD)
    using System.Diagnostics;
#endif

namespace DevsDaddy.GameShield.Core.Editor
{
    /// <summary>
    /// Post-Processor for GameShield
    /// </summary>
    internal class Postprocessor : AssetPostprocessor
    {
        private static readonly List<AllowedAssembly> allowedAssemblies = new List<AllowedAssembly>();
        private static readonly List<string> allLibraries = new List<string>();
        
#if (DEBUG || DEBUG_VERBOSE || DEBUG_PARANIOD)
		[MenuItem("GameShield/Injection Scanner/Force Collect Data", false, 45)]
		private static void CallInjectionScan()
		{
			InjectionAssembliesScan(true); 
		}
#endif
        /// <summary>
        /// Post Process Assets
        /// </summary>
        /// <param name="mportedAssets"></param>
        /// <param name="deletedAssets"></param>
        /// <param name="movedAssets"></param>
        /// <param name="movedFromAssetPaths"></param>
        private static void OnPostprocessAllAssets(String[] mportedAssets, String[] deletedAssets, String[] movedAssets, String[] movedFromAssetPaths)
        {
	        if (deletedAssets.Length > 0)
            {
                foreach (string deletedAsset in deletedAssets)
                {
                    if (deletedAsset.IndexOf(GeneralConstants.INJECTION_DATA_FILE) > -1 && !EditorApplication.isCompiling)
                    {
#if (DEBUG || DEBUG_VERBOSE || DEBUG_PARANIOD)
						Debug.LogWarning($"{GeneralStrings.LOG_PREFIX} Looks like Injection Detector data file was accidentally removed! Re-creating...");
#endif
                        InjectionAssembliesScan();
                    }
                }
            }
        }
        
        [DidReloadScripts]
        private static void ScriptsWereReloaded()
        {
            EditorUserBuildSettings.activeBuildTargetChanged += OnBuildTargetChanged;
            InjectionAssembliesScan();
        }
        
        private static void OnBuildTargetChanged()
        {
            InjectionDetectorTargetCompatibleCheck();
        }

        internal static void InjectionAssembliesScan()
        {
            InjectionAssembliesScan(false);
        }
        
        internal static void InjectionAssembliesScan(bool forced)
		{
			if (!InjectionDetectorTargetCompatibleCheck() && !forced)
			{
				return;
			}
			
			#if (DEBUG || DEBUG_VERBOSE || DEBUG_PARANIOD)
			Stopwatch sw = Stopwatch.StartNew();
	#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			Debug.Log($"{GeneralStrings.LOG_PREFIX} Injection Detector Assemblies Scan\n");
			Debug.Log($"{GeneralStrings.LOG_PREFIX} Paths:\n" +

			          "Assets: " + GeneralConstants.ASSETS_PATH + "\n" +
			          "Assemblies: " + GeneralConstants.ASSEMBLIES_PATH + "\n" +
			          "Injection Detector Data: " + GeneralConstants.INJECTION_DATA_PATH);
			sw.Start();
	#endif
			#endif

#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			Debug.Log($"{GeneralStrings.LOG_PREFIX} Looking for all assemblies in current project...");
			sw.Start();
#endif
			allLibraries.Clear();
			allowedAssemblies.Clear();

			allLibraries.AddRange(InjectionDetectorGlobal.FindLibrariesAt(GeneralConstants.ASSETS_PATH));
			allLibraries.AddRange(InjectionDetectorGlobal.FindLibrariesAt(GeneralConstants.ASSEMBLIES_PATH));
#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			Debug.Log($"{GeneralStrings.LOG_PREFIX} Total libraries found: " + allLibraries.Count);
			sw.Start();
#endif
			const string editorSubdir = "/editor/";
			string assembliesPathLowerCase = GeneralConstants.ASSEMBLIES_PATH_RELATIVE.ToLower();
			foreach (string libraryPath in allLibraries)
			{
				string libraryPathLowerCase = libraryPath.ToLower();
#if (DEBUG_PARANIOD)
				sw.Stop();
				Debug.Log($"{GeneralStrings.LOG_PREFIX} Checking library at the path: " + libraryPathLowerCase);
				sw.Start();
#endif
				if (libraryPathLowerCase.Contains(editorSubdir)) continue;
				if (libraryPathLowerCase.Contains("-editor.dll") && libraryPathLowerCase.Contains(assembliesPathLowerCase)) continue;

				try
				{
					AssemblyName assName = AssemblyName.GetAssemblyName(libraryPath);
					string name = assName.Name;
					int hash = InjectionDetectorGlobal.GetAssemblyHash(assName);

					AllowedAssembly allowed = allowedAssemblies.FirstOrDefault(allowedAssembly => allowedAssembly.name == name);

					if (allowed != null)
					{
						allowed.AddHash(hash);
					}
					else
					{
						allowed = new AllowedAssembly(name, new[] {hash});
						allowedAssemblies.Add(allowed);
					}
				}
				catch
				{
					// not a valid IL assembly, skipping
				}
			}

#if (DEBUG || DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			string trace = "Found assemblies (" + allowedAssemblies.Count + "):\n";

			foreach (AllowedAssembly allowedAssembly in allowedAssemblies)
			{
				trace += "  Name: " + allowedAssembly.name + "\n";
				trace = allowedAssembly.hashes.Aggregate(trace, (current, hash) => current + ("    Hash: " + hash + "\n"));
			}

			Debug.Log(trace);
			sw.Start();
#endif
			if (!Directory.Exists(GeneralConstants.RESOURCES_PATH))
			{
#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
				sw.Stop();
				Debug.Log($"{GeneralStrings.LOG_PREFIX} Creating resources folder: " + GeneralConstants.RESOURCES_PATH);
				sw.Start();
#endif
				Directory.CreateDirectory(GeneralConstants.RESOURCES_PATH);
			}

			InjectionDetectorGlobal.RemoveReadOnlyAttribute(GeneralConstants.INJECTION_DATA_PATH);
			BinaryWriter bw = new BinaryWriter(new FileStream(GeneralConstants.INJECTION_DATA_PATH, FileMode.Create, FileAccess.Write, FileShare.Read));
			int allowedAssembliesCount = allowedAssemblies.Count;

			int totalWhitelistedAssemblies = 0;

			#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			Debug.Log($"{GeneralStrings.LOG_PREFIX} Processing default whitelist");
			sw.Start();
			#endif

			string defaultWhitelistPath = InjectionDetectorGlobal.ResolveInjectionDefaultWhitelistPath();
			if (File.Exists(defaultWhitelistPath))
			{
				BinaryReader br = new BinaryReader(new FileStream(defaultWhitelistPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
				int assembliesCount = br.ReadInt32();
				totalWhitelistedAssemblies = assembliesCount + allowedAssembliesCount;

				bw.Write(totalWhitelistedAssemblies);

				for (int i = 0; i < assembliesCount; i++)
				{
					bw.Write(br.ReadString());
				}
				br.Close();
			}
			else
			{
				#if (DEBUG || DEBUG_VERBOSE || DEBUG_PARANIOD)
				sw.Stop();
				#endif
				bw.Close();
				Debug.LogError($"{GeneralStrings.LOG_PREFIX} Can't find " + GeneralConstants.INJECTION_DEFAULT_WHITELIST_FILE + " file!\nPlease, report to " + GeneralStrings.REPORT_EMAIL);
				return;
			}

			#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			Debug.Log($"{GeneralStrings.LOG_PREFIX} Processing user whitelist");
			sw.Start();
			#endif

			string userWhitelistPath = InjectionDetectorGlobal.ResolveInjectionUserWhitelistPath();
			if (File.Exists(userWhitelistPath))
			{
				BinaryReader br = new BinaryReader(new FileStream(userWhitelistPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
				int assembliesCount = br.ReadInt32();

				bw.Seek(0, SeekOrigin.Begin);
				bw.Write(totalWhitelistedAssemblies + assembliesCount);
				bw.Seek(0, SeekOrigin.End);
				for (int i = 0; i < assembliesCount; i++)
				{
					bw.Write(br.ReadString());
				}
				br.Close();
			}

			#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			Debug.Log($"{GeneralStrings.LOG_PREFIX} Processing project assemblies");
			sw.Start();
			#endif

			for (int i = 0; i < allowedAssembliesCount; i++)
			{
				AllowedAssembly assembly = allowedAssemblies[i];
				string name = assembly.name;
				string hashes = "";

				for (int j = 0; j < assembly.hashes.Length; j++)
				{
					hashes += assembly.hashes[j];
					if (j < assembly.hashes.Length - 1)
					{
						hashes += GeneralConstants.INJECTION_DATA_SEPARATOR;
					}
				}

				string line = SecuredString.EncryptDecrypt(name + GeneralConstants.INJECTION_DATA_SEPARATOR + hashes, "GAMESHIELD");
				
				#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
				Debug.Log($"{GeneralStrings.LOG_PREFIX} Writing assembly:\n" + name + GeneralConstants.INJECTION_DATA_SEPARATOR + hashes);
				#endif
				bw.Write(line);
			}

			bw.Close();			 
			#if (DEBUG || DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			Debug.Log($"{GeneralStrings.LOG_PREFIX} Assemblies scan duration: " + sw.ElapsedMilliseconds + " ms.");
			#endif

			if (allowedAssembliesCount == 0)
			{
				Debug.LogError($"{GeneralStrings.LOG_PREFIX} Can't find any assemblies!\nPlease, report to " + GeneralStrings.REPORT_EMAIL);
			}

			AssetDatabase.Refresh();
		}

		private static bool InjectionDetectorTargetCompatibleCheck()
		{
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_IPHONE || UNITY_ANDROID
			return true;
#else
			return false;
#endif
		}
    }
}