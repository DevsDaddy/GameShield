using System;
using System.Collections.Generic;
using System.IO;
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
            if(GUILayout.Button("Join Discord", styles.GetBasicButtonSyle())) {
                Application.OpenURL("https://discord.gg/xuNTKRDebx");
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
            GUILayout.Label("GENERAL", styles.GetSubHeaderStyle());
            DrawGeneralConfigs();
            
            GUILayout.Space(20);
            GUILayout.Label("MODULES", styles.GetSubHeaderStyle());
            DrawModules();
            
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draw Complete Setup
        /// </summary>
        private void DrawCompleteSetup() {
            GUILayout.BeginVertical(styles.GetBodyAreaStyle(), GUILayout.ExpandHeight(true));
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            GUILayout.Label("CONTACTS", styles.GetSubHeaderStyle());
            
            // Developer Email
            DrawInputField(GeneralStrings.DEV_EMAIL_DESC, GeneralStrings.DEV_EMAIL_TITLE, currentConfig.Contacts.Email,
                key => {
                    currentConfig.Contacts.Email = key;
                });

            // Developer Contacts
            DrawInputField(GeneralStrings.DEV_WEBSITE_DESC, GeneralStrings.DEV_WEBSITE_TITLE, currentConfig.Contacts.Website,
                key => {
                    currentConfig.Contacts.Website = key;
                });
            GUILayout.Space(20);
            
            GUILayout.Label("AND NOW IS DONE", styles.GetSubHeaderStyle());
            GUILayout.Label("Your GameShield is now ready to go.\n\n<b>Do not remove the GAME_SHIELD object from the scene!</b>", styles.GetRegularTextStyle());
            
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
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
                Debug.Log($"{GeneralStrings.LOG_PREFIX} Loading All Available Modules. Found Modules: {availableModules.Count}");
            }

            if (availableModules.Count > 0)
                availableModules.ForEach(DrawModule);
            else {
                GUILayout.Label("No modules found. Please, re-install GameShield.", styles.GetWarningTextStyle(TextAnchor.MiddleCenter));
            }
        }

        /// <summary>
        /// Toggle All Modules
        /// </summary>
        private static void ToggleAllModules() {
            // Toggle All Modules
            List<IShieldModule> allModules = ModuleManager.GetAllModules();
            allModules.ForEach(module => {
                currentConfig.AvailableModules.Add(module.GetType().ToString());
            });
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
        /// Draw General Configs
        /// </summary>
        private void DrawGeneralConfigs() {
            // Developer Key Module
            DrawInputField(GeneralStrings.DEV_KEY_DESC, GeneralStrings.DEV_KEY_TITLE, currentConfig.DeveloperKey,
                key => {
                    currentConfig.DeveloperKey = key;
                });

            // GameShield Backend URL
            DrawInputField(GeneralStrings.BACK_URL_DESC, GeneralStrings.BACK_URL_TITLE, currentConfig.BackendURL,
                key => {
                    currentConfig.BackendURL = key;
                });

            // Auto-Pause Modules
            DrawToggleListElement(GeneralStrings.AUTO_PAUSE_ON_HEADER, GeneralStrings.AUTO_PAUSE_ON_DESC, currentConfig.PauseOnApplicationTerminated,
                () => {
                    currentConfig.PauseOnApplicationTerminated = !currentConfig.PauseOnApplicationTerminated;
                });
            GUILayout.Space(10);
        }

        /// <summary>
        /// Draw Input Field
        /// </summary>
        /// <param name="placeholder"></param>
        /// <param name="label"></param>
        /// <param name="variable"></param>
        /// <param name="onComplete"></param>
        private void DrawInputField(string placeholder, string label, string variable, Action<string> onComplete = null) {
            string newText = "";
            GUILayout.BeginHorizontal(styles.GetListElementStyle());
            GUILayout.BeginVertical();
            GUILayout.Label($"<b>{label}</b>", styles.GetRegularTextStyle(TextAnchor.UpperLeft));
            newText = GUILayout.TextField(variable, 64, styles.GetBasicFieldStyle());
            GUILayout.Label($"<color=#2f2f2f>{placeholder}</color>", styles.GetRegularTextStyle(TextAnchor.UpperLeft));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            
            if(newText != variable)
                onComplete?.Invoke(newText);
        }

        /// <summary>
        /// Draw Toggle List Element
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="toggleVariable"></param>
        /// <param name="onToggle"></param>
        private void DrawToggleListElement(string title, string description, bool toggleVariable, Action onToggle) {
            GUILayout.BeginHorizontal(styles.GetListElementStyle());
            GUILayout.BeginVertical();
            if (GUILayout.Button("", styles.GetSwitchButtonStyle(toggleVariable), GUILayout.ExpandWidth(false))) {
                onToggle?.Invoke();
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Label(title, styles.GetRegularTextStyle(TextAnchor.UpperLeft));
            GUILayout.Label(description, styles.GetRegularTextStyle(TextAnchor.UpperLeft));
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
                Debug.Log($"{GeneralStrings.LOG_PREFIX} Module {nameof(module)} is Enabled");
                return true;
            }
            else {
                currentConfig.AvailableModules.Remove(found);
                Debug.Log($"{GeneralStrings.LOG_PREFIX} Module {nameof(module)} is Disabled");
                return false;
            }
        }

        /// <summary>
        /// Complete Setup
        /// </summary>
        private void CompleteSetup() {
            // Save Configs
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Add GameObject on Scene
            GameShield worker = FindObjectOfType<GameShield>();
            if (worker == null) {
                GameObject workerObject = new GameObject("__GAME_SHIELD__");
                workerObject.AddComponent<GameShield>();
                workerObject.transform.SetAsFirstSibling();
                Debug.Log($"{GeneralStrings.LOG_PREFIX} Added Worker GameObject at Current Scene");
            }
            
            // Close Window
            Debug.Log($"{GeneralStrings.LOG_PREFIX} Setup <color=green><b>is Done</b></color>");
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
                currentConfig.DeveloperKey = Generator.GenerateRandomKey(32);
                ToggleAllModules();
                string pathToConfig = $"{rootPath}Resources/{GeneralConstants.CONFIG_PATH}.asset";
                AssetDatabase.CreateAsset(currentConfig, pathToConfig);
                AssetDatabase.Refresh();
                Debug.Log($"{GeneralStrings.LOG_PREFIX} Config Created At: {pathToConfig}");
                return;
            }

            currentConfig = config;
            
            // Generate key if Null
            if(string.IsNullOrEmpty(currentConfig.DeveloperKey)) 
                currentConfig.DeveloperKey = Generator.GenerateRandomKey(32);
            Debug.Log($"{GeneralStrings.LOG_PREFIX} Loaded Config: {currentConfig}");
            Debug.Log($"{GeneralStrings.LOG_PREFIX} Enabled Modules: {currentConfig.AvailableModules.Count}");
        }
    }
}