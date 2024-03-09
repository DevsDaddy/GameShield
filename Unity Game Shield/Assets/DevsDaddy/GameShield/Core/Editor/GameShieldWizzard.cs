using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DevsDaddy.GameShield.Core.Constants;
using DevsDaddy.GameShield.Core.Modules;
using DevsDaddy.GameShield.Core.Utils;
using UnityEditor;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Editor
{
    /// <summary>
    /// Game Shield Wizzard Window
    /// </summary>
    public class GameShieldWizzard : EditorWindow
    {
        // Current Wizzard Tab
        private static GameShieldConfig currentConfig = null;
        private static WizzardTab currentWizzardTab = WizzardTab.Welcome;

        // Window Sizes
        private static readonly Vector2 minWindowSize = new Vector2(450, 600);
        private static readonly Vector2 maxWindowSize = new Vector2(550, 1024);
        
        // Styles and Images
        private GameShieldStyles styles = new GameShieldStyles();
        private Texture wizzardHeaderImage;

        // Modules
        private static string rootPath = "";
        private List<IShieldModule> availableModules = new List<IShieldModule>();

        // Scroll Positions
        Vector2 scrollPos;

        /// <summary>
        /// Show GameShield Wizzard
        /// </summary>
        [MenuItem("GameShield/Setup Wizzard", false, 0)]
        private static void Init()
        {
            // Get existing open window or if none, make a new one:
            EditorPrefs.SetBool(GeneralConstants.EDITOR_WIZZARD_KEY, true);
            GameShieldWizzard window = (GameShieldWizzard)EditorWindow.GetWindow(typeof(GameShieldWizzard));
            window.titleContent = new GUIContent(GeneralStrings.SETUP_WIZZARD_TITLE, GeneralStrings.SETUP_WIZZARD_HINT);
            window.maxSize = maxWindowSize;
            window.minSize = minWindowSize;
            window.position = new Rect(Screen.width / 2, Screen.height / 2, minWindowSize.x, minWindowSize.y);
            window.Show();
            window.CenterOnMainWin();
            window.SwitchTab(WizzardTab.Welcome);
        }

        /// <summary>
        /// GUI Updates
        /// </summary>
        private void OnGUI(){
            DrawHeader();
            DrawBody();
            DrawFooter();
        }

        /// <summary>
        /// Draw Wizzard Header
        /// </summary>
        private void DrawHeader() {
            // Load Image
            if (wizzardHeaderImage == null)
                wizzardHeaderImage = Resources.Load<Texture>(GeneralConstants.EDITOR_WIZZARD_HEADER);
            
            // Draw Window BG
            GUI.DrawTexture(new Rect(0, 0, position.width, position.height), styles.GetBGTexture(), ScaleMode.StretchToFill, true);
            
            // Draw Texture
            float ratio = wizzardHeaderImage.height / wizzardHeaderImage.width;
            float w = position.width;
            float h = position.width / wizzardHeaderImage.width * wizzardHeaderImage.height;
            GUILayout.BeginHorizontal();
            GUI.DrawTexture(new Rect(0, 0, w, h), wizzardHeaderImage, ScaleMode.StretchToFill, true, ratio);
            GUILayout.EndHorizontal();
            GUILayout.Space(h);
            
            // Header
            GUILayout.BeginHorizontal();
            GUILayout.Label(GetTabText(), styles.GetHeaderStyle());
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw Wizzard Body
        /// </summary>
        private void DrawBody() {
            if (currentWizzardTab == WizzardTab.Welcome)
                DrawWelcomeScreen();
            if (currentWizzardTab == WizzardTab.GeneralSetup)
                DrawConfigEditor();
            if (currentWizzardTab == WizzardTab.Complete)
                DrawCompleteSetup();
        }

        /// <summary>
        /// Draw Welcome Screen
        /// </summary>
        private void DrawWelcomeScreen() {
            GUILayout.BeginVertical(styles.GetBodyAreaStyle(), GUILayout.ExpandHeight(true));
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            GUILayout.Label($"{GeneralStrings.VERSION_TITLE} {GeneralConstants.GAME_SHIELD_VERSION}", styles.GetRegularTextStyle(TextAnchor.MiddleCenter));
            GUILayout.Space(20);
            GUILayout.Label(GeneralStrings.SETUP_WIZZARD_THANKS, styles.GetRegularTextStyle(TextAnchor.MiddleCenter));
            GUILayout.Space(10);
            if(GUILayout.Button("Open Documentation", styles.GetBasicButtonSyle())) {
                Application.OpenURL("https://github.com/DevsDaddy/GameShield/wiki");
            }
            if(GUILayout.Button("Check New Versions", styles.GetBasicButtonSyle())) {
                Application.OpenURL("https://github.com/DevsDaddy/GameShield/releases");
            }
            if(GUILayout.Button("Report a Bug", styles.GetBasicButtonSyle())) {
                Application.OpenURL("https://github.com/DevsDaddy/GameShield/issues");
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draw Config Editor
        /// </summary>
        private void DrawConfigEditor() {
            GUILayout.BeginVertical(styles.GetBodyAreaStyle(), GUILayout.ExpandHeight(true));
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            GUILayout.Label("MODULES", styles.GetSubHeaderStyle());
            DrawModules();
            
            GUILayout.Space(20);
            GUILayout.Label("MODULES", styles.GetSubHeaderStyle());
            
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draw Complete Setup
        /// </summary>
        private void DrawCompleteSetup() {
            
        }

        /// <summary>
        /// Draw Wizzard Footer
        /// </summary>
        private void DrawFooter() {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal(styles.GetFooterAreaStyle());
            if (currentWizzardTab == WizzardTab.GeneralSetup || currentWizzardTab == WizzardTab.Complete) {
                if(GUILayout.Button("Go Back", styles.GetFooterButtonStyle(true))) {
                    SwitchTab((currentWizzardTab == WizzardTab.GeneralSetup) ? WizzardTab.Welcome : WizzardTab.GeneralSetup);
                }
            }
            if (currentWizzardTab == WizzardTab.Welcome || currentWizzardTab == WizzardTab.GeneralSetup) {
                if(GUILayout.Button("Continue", styles.GetFooterButtonStyle(false))) {
                    SwitchTab(currentWizzardTab = (currentWizzardTab == WizzardTab.Welcome) ? WizzardTab.GeneralSetup : WizzardTab.Complete);
                }
            }
            if (currentWizzardTab == WizzardTab.Complete) {
                if (GUILayout.Button("Complete Setup", styles.GetFooterButtonStyle(false)))
                    CompleteSetup();
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw Modules
        /// </summary>
        private void DrawModules() {
            // Get Available Modules
            if (availableModules.Count < 1) {
                availableModules = ModuleManager.GetAllModules();
                Debug.Log($"Loading All Available Modules. Found Modules: {availableModules.Count}");
            }

            if (availableModules.Count > 0)
                availableModules.ForEach(DrawModule);
            else {
                GUILayout.Label("No modules found. Please, re-install GameShield.", styles.GetWarningTextStyle(TextAnchor.MiddleCenter));
            }
        }

        /// <summary>
        /// Draw Single Module
        /// </summary>
        /// <param name="module"></param>
        private void DrawModule(IShieldModule module) {
            ModuleInfo moduleInfo = module.GetModuleInfo();
            bool isModuleEnabled = IsModuleEnabled(module);
            GUILayout.BeginHorizontal(styles.GetListElementStyle());
            GUILayout.BeginVertical();
            if (GUILayout.Button("", styles.GetSwitchButtonStyle(isModuleEnabled), GUILayout.ExpandWidth(false))) {
                ToggleModule(module);
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Label($"<b>{moduleInfo.Name}</b>", styles.GetRegularTextStyle(TextAnchor.UpperLeft));
            GUILayout.Label(moduleInfo.Description, styles.GetRegularTextStyle(TextAnchor.UpperLeft));
            if (GUILayout.Button("Show Documentation", styles.GetBasicButtonSyle(true), GUILayout.ExpandWidth(false))) {
                Application.OpenURL(moduleInfo.DocumentationLink);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Is Module Enabled
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        private bool IsModuleEnabled(IShieldModule module) {
            string found = currentConfig.AvailableModules.Find(mod => mod == module.GetType().ToString());
            return (found != null);
        }

        /// <summary>
        /// Toggle Module
        /// </summary>
        /// <param name="module"></param>
        private bool ToggleModule(IShieldModule module) {
            string found = currentConfig.AvailableModules.Find(mod => mod == module.GetType().ToString());
            if (found == null) {
                currentConfig.AvailableModules.Add(module.GetType().ToString());
                Debug.Log($"GameShield Module {nameof(module)} is Enabled");
                return true;
            }
            else {
                currentConfig.AvailableModules.Remove(found);
                Debug.Log($"GameShield Module {nameof(module)} is Disabled");
                return false;
            }
        }

        /// <summary>
        /// Complete Setup
        /// </summary>
        private void CompleteSetup() {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Close();
        }

        /// <summary>
        /// Initialize Wizzard
        /// </summary>
        [InitializeOnLoadMethod]
        private static void InitializeWizzard() {
            // Get Config
            if (string.IsNullOrEmpty(rootPath)) rootPath = GetRoot();
            bool isWizzardShown = EditorPrefs.GetBool(GeneralConstants.EDITOR_WIZZARD_KEY, false);
            if (!isWizzardShown) {
                currentWizzardTab = WizzardTab.Welcome;
                Init();
            }
        }

        [MenuItem("GameShield/Utils/Reset Setup Wizzard", false, 99)]
        private static void ResetWizzard() {
            EditorPrefs.SetBool(GeneralConstants.EDITOR_WIZZARD_KEY, false);
        }

        /// <summary>
        /// Switch Wizzard to Tab 
        /// </summary>
        /// <param name="tab"></param>
        private void SwitchTab(WizzardTab tab) {
            currentWizzardTab = tab;
            scrollPos = new Vector2(0, 0);

            if (currentWizzardTab == WizzardTab.GeneralSetup) {
                GetConfig();
                currentConfig.ConfigRevision += 1;
                EditorUtility.SetDirty(currentConfig);
            }
        }

        /// <summary>
        /// Get Tab Text
        /// </summary>
        /// <returns></returns>
        private string GetTabText() {
            switch (currentWizzardTab) {
                case WizzardTab.Welcome:
                    return "Welcome to GameShield!";
                case WizzardTab.GeneralSetup:
                    return "Configure GameShield";
                case WizzardTab.Complete:
                    return "Complete Setup";
            }
            
            return "";
        }
        
        /// <summary>
        /// Get Script Path for
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string GetRoot() {
            var asset = "";
            var guids = AssetDatabase.FindAssets( string.Format( "{0} t:script", nameof(GameShieldWizzard)));
            
            if ( guids.Length > 1 ) {
                foreach ( var guid in guids ) {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var filename = Path.GetFileNameWithoutExtension( assetPath );
                    if ( filename == nameof(GameShieldWizzard) ) {
                        asset = guid;
                        break;
                    }
                }
            } else if ( guids.Length == 1 ) {
                asset = guids [0];
            } else {
                Debug.LogErrorFormat("Unable to locate {0}", nameof(GameShieldWizzard));
                return null;
            }
            
            string path = AssetDatabase.GUIDToAssetPath (asset);
            string relative = path.Replace($"Core/Editor/{nameof(GameShieldWizzard)}.cs", "");
            return relative;
        }
        
        /// <summary>
        /// Get Current Config
        /// </summary>
        private static void GetConfig() {
            if (string.IsNullOrEmpty(rootPath)) rootPath = GetRoot();
            
            // Load Configurations
            GameShieldConfig config = Resources.Load<GameShieldConfig>(GeneralConstants.CONFIG_PATH);
            if (config == null) {
                currentConfig = CreateInstance<GameShieldConfig>();
                string pathToConfig = $"{rootPath}Resources/{GeneralConstants.CONFIG_PATH}.asset";
                AssetDatabase.CreateAsset(currentConfig, pathToConfig);
                AssetDatabase.Refresh();
                Debug.Log($"GameShield Config Created At: {pathToConfig}");
                //currentConfig = Resources.Load<GameShieldConfig>(GeneralConstants.CONFIG_PATH);
                return;
            }
            
            currentConfig = config;
            Debug.Log($"GameShield Loaded Config: {currentConfig}");
            Debug.Log($"GameShield Enabled Modules: {currentConfig.AvailableModules.Count}");
        }
    }
}