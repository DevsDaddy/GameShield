using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DevsDaddy.GameShield.Core.Constants;
using DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes;
using UnityEditor;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Editor
{
    /// <summary>
    /// Assemblies Whitelist Editor
    /// </summary>
    internal class AssembliesWhitelist : EditorWindow
    {
        private const string INITIAL_CUSTOM_NAME = "AssemblyName, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        private static List<AllowedAssembly> whiteList;
        private static string whitelistPath;
        
        private Vector2 scrollPosition;
        private bool manualAssemblyWhitelisting = false;
        private string manualAssemblyWhitelistingName = INITIAL_CUSTOM_NAME;
        
        [MenuItem("GameShield/Injection Scanner/Whitelist Editor", false, 42)]
        internal static void ShowWindow()
        {
            EditorWindow myself = GetWindow<AssembliesWhitelist>(false, "Game Libraries Whitelist Editor", true);
            myself.minSize = new Vector2(500, 200);
        }
        
        private void OnLostFocus()
        {
            manualAssemblyWhitelisting = false;
            manualAssemblyWhitelistingName = INITIAL_CUSTOM_NAME;
        }
        
        private void OnGUI()
		{
			if (whiteList == null)
			{
				whiteList = new List<AllowedAssembly>();
				LoadAndParseWhiteList();
			}

			GUIStyle tmpStyle = new GUIStyle(EditorStyles.largeLabel);
			tmpStyle.alignment = TextAnchor.MiddleCenter;
			tmpStyle.fontStyle = FontStyle.Bold;
			GUILayout.Label("User-defined Whitelist of Assemblies trusted by Injection Scanner", tmpStyle);

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
			bool whitelistUpdated = false;

			int count = whiteList.Count;

			if (count > 0)
			{
				for (int i = 0; i < count; i++)
				{
					AllowedAssembly assembly = whiteList[i];
					GUILayout.BeginHorizontal();
					GUILayout.Label(assembly.ToString());
					if (GUILayout.Button(new GUIContent("-", "Remove Assembly from Whitelist"), GUILayout.Width(30)))
					{
						whiteList.Remove(assembly);
						whitelistUpdated = true;
					}
					GUILayout.EndHorizontal();
				}
			}
			else
			{
				tmpStyle = new GUIStyle(EditorStyles.largeLabel);
				tmpStyle.alignment = TextAnchor.MiddleCenter;
				GUILayout.Label("- no Assemblies added so far (use buttons below to add) -", tmpStyle);
			}

			if (manualAssemblyWhitelisting)
			{
				manualAssemblyWhitelistingName = EditorGUILayout.TextField(manualAssemblyWhitelistingName);

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Save"))
				{
					try
					{
						AssemblyName assName = new AssemblyName(manualAssemblyWhitelistingName);
						WhitelisingResult res = TryWhitelistAssemblyName(assName, true);
						if (res != WhitelisingResult.Exists)
						{
							whitelistUpdated = true;
						}
						manualAssemblyWhitelisting = false;
						manualAssemblyWhitelistingName = INITIAL_CUSTOM_NAME;
					}
					catch (FileLoadException error)
					{
						ShowNotification(new GUIContent(error.Message));
					}

					GUI.FocusControl("");
				}

				if (GUILayout.Button("Cancel"))
				{
					manualAssemblyWhitelisting = false;
					manualAssemblyWhitelistingName = INITIAL_CUSTOM_NAME;
					GUI.FocusControl("");
				}
				GUILayout.EndHorizontal();
			}

			EditorGUILayout.EndScrollView();

			GUILayout.BeginHorizontal();
			GUILayout.Space(20);
			if (GUILayout.Button("Add Assembly"))
			{
				string assemblyPath = EditorUtility.OpenFilePanel("Choose an Assembly to add", "", "dll");
				if (!String.IsNullOrEmpty(assemblyPath))
				{
					whitelistUpdated |= TryWhitelistAssemblies(new[] { assemblyPath }, true);
				}
			}

			if (GUILayout.Button("Add Assemblies from folder"))
			{
				string selectedFolder = EditorUtility.OpenFolderPanel("Choose a folder with Assemblies", "", "");
				if (!String.IsNullOrEmpty(selectedFolder))
				{
					string[] libraries = InjectionDetectorGlobal.FindLibrariesAt(selectedFolder);
					whitelistUpdated |= TryWhitelistAssemblies(libraries);
				}
			}

			if (!manualAssemblyWhitelisting)
			{
				if (GUILayout.Button("Add Assembly manually"))
				{
					manualAssemblyWhitelisting = true;
				}
			}
			
			if (count > 0)
			{
				if (GUILayout.Button("Clear"))
				{
					if (EditorUtility.DisplayDialog("Please confirm", "Are you sure you wish to completely clear your Injection Scanner whitelist?", "Yes", "No"))
					{
						whiteList.Clear();
						whitelistUpdated = true;
					}
				}
			}
			GUILayout.Space(20);
			GUILayout.EndHorizontal();

			GUILayout.Space(20);

			if (whitelistUpdated)
			{
				WriteWhiteList();
			}
		}
        
        private bool TryWhitelistAssemblies(string[] libraries)
        {
	        return TryWhitelistAssemblies(libraries, false);
        }
        
        private bool TryWhitelistAssemblies(string[] libraries, bool singleFile)
        {
	        int added = 0;
	        int updated = 0;

	        int count = libraries.Length;

	        for (int i = 0; i < count; i++)
	        {
		        string libraryPath = libraries[i];
		        try
		        {
			        AssemblyName assName = AssemblyName.GetAssemblyName(libraryPath);
			        WhitelisingResult whitelistingResult = TryWhitelistAssemblyName(assName, singleFile);
			        if (whitelistingResult == WhitelisingResult.Added)
			        {
				        added++;
			        }
			        else if (whitelistingResult == WhitelisingResult.Updated)
			        {
				        updated++;
			        }

		        }
		        catch
		        {
			        if (singleFile) ShowNotification(new GUIContent("Selected file is not a valid .NET assembly!"));
		        }
	        }

	        if (!singleFile)
	        {
		        ShowNotification(new GUIContent("Assemblies added: " + added + ", updated: " + updated));
	        }

	        return added > 0 || updated > 0;
        }
        
        private WhitelisingResult TryWhitelistAssemblyName(AssemblyName assName, bool singleFile)
        {
	        WhitelisingResult result = WhitelisingResult.Exists;

	        string name = assName.Name;
	        int hash = InjectionDetectorGlobal.GetAssemblyHash(assName);

	        AllowedAssembly allowed = whiteList.FirstOrDefault(allowedAssembly => allowedAssembly.name == name);

	        if (allowed != null)
	        {
		        if (allowed.AddHash(hash))
		        {
			        if (singleFile) ShowNotification(new GUIContent("New hash added!"));
			        result = WhitelisingResult.Updated;
		        }
		        else
		        {
			        if (singleFile) ShowNotification(new GUIContent("Assembly already exists!"));
		        }
	        }
	        else
	        {
		        allowed = new AllowedAssembly(name, new[] { hash });
		        whiteList.Add(allowed);

		        if (singleFile) ShowNotification(new GUIContent("Assembly added!"));
		        result = WhitelisingResult.Added;
	        }

	        return result;
        }
        
        private void LoadAndParseWhiteList()
        {
	        whitelistPath = InjectionDetectorGlobal.ResolveInjectionUserWhitelistPath();
	        if (!String.IsNullOrEmpty(whitelistPath) && File.Exists(whitelistPath))
	        {
		        string[] separator = { GeneralConstants.INJECTION_DATA_SEPARATOR };

		        FileStream fs = new FileStream(whitelistPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		        BinaryReader br = new BinaryReader(fs);

		        int count = br.ReadInt32();

		        for (int i = 0; i < count; i++)
		        {
			        string line = br.ReadString();
			        line = SecuredString.EncryptDecrypt(line, "GAMESHIELD");
			        string[] strArr = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
			        int stringsCount = strArr.Length;
			        if (stringsCount > 1)
			        {
				        string assemblyName = strArr[0];

				        int[] hashes = new int[stringsCount - 1];
				        for (int j = 1; j < stringsCount; j++)
				        {
					        hashes[j - 1] = int.Parse(strArr[j]);
				        }

				        whiteList.Add(new AllowedAssembly(assemblyName, hashes));
			        }
			        else
			        {
				        Debug.LogWarning("Error parsing whitelist file line! Please report to " + GeneralStrings.REPORT_EMAIL);
			        }
		        }

		        br.Close();
		        fs.Close();
	        }
        }
        
        private void WriteWhiteList()
        {
	        if (whiteList.Count > 0)
	        {
		        bool fileExisted = File.Exists(whitelistPath);
		        InjectionDetectorGlobal.RemoveReadOnlyAttribute(whitelistPath);
		        FileStream fs = new FileStream(whitelistPath, FileMode.Create, FileAccess.Write, FileShare.Read);
		        BinaryWriter br = new BinaryWriter(fs);

		        br.Write(whiteList.Count);

		        foreach (AllowedAssembly assembly in whiteList)
		        {
			        string assemblyName = assembly.name;
			        string hashes = "";

			        for (int j = 0; j < assembly.hashes.Length; j++)
			        {
				        hashes += assembly.hashes[j];
				        if (j < assembly.hashes.Length - 1)
				        {
					        hashes += GeneralConstants.INJECTION_DATA_SEPARATOR;
				        }
			        }

			        string line = SecuredString.EncryptDecrypt(assemblyName + GeneralConstants.INJECTION_DATA_SEPARATOR + hashes, "GAMESHIELD");
			        br.Write(line);
		        }
		        br.Close();
		        fs.Close();

		        if (!fileExisted)
		        {
			        AssetDatabase.Refresh();
		        }
	        }
	        else
	        {
		        InjectionDetectorGlobal.RemoveReadOnlyAttribute(whitelistPath);
		        FileUtil.DeleteFileOrDirectory(whitelistPath);
		        AssetDatabase.Refresh();
	        }

	        Postprocessor.InjectionAssembliesScan();
        }
        
        private enum WhitelisingResult:byte
        {
	        Exists,
	        Added,
	        Updated
        }
    }
}