using System.Linq;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Constants
{
    public static class GeneralConstants
    {
        // Current Shield Version
        public const string GAME_SHIELD_VERSION = "2024-03R1";
        
        // General Worker Parameters
        public const string WORKER_OBJECT_NAME = "GAMESHIELD_WORKER";
        public const string CONFIG_PATH = "Config/GameShieldConfig";
        public const string EDITOR_WIZZARD_KEY = "IsGSWizzardShown";
        public const string EDITOR_WIZZARD_HEADER = "Wizzard/Header";
        public const string EDITOR_WIZZARD_IMG_PATH = "Wizzard";

        // Injection Scanner
        public const string ASSMDB_PATH = "Config/ADB";
        public const string INJECTION_SERVICE_FOLDER = "InjectionDetectorData";
        public const string INJECTION_DEFAULT_WHITELIST_FILE = "DefaultWhitelist.gsa";
        public const string INJECTION_USER_WHITELIST_FILE = "UserWhitelist.gsa";
        public const string INJECTION_DATA_FILE = "ADB.gsa";
        public const string INJECTION_DATA_SEPARATOR = ":";
        public const string ASSEMBLIES_PATH_RELATIVE = "Library/ScriptAssemblies";
        public static readonly string ASSETS_PATH = Application.dataPath;
        public static readonly string RESOURCES_PATH = ASSETS_PATH + "/Resources/";
        public static readonly string ASSEMBLIES_PATH = ASSETS_PATH + "/../" + ASSEMBLIES_PATH_RELATIVE;
        public static readonly string INJECTION_DATA_PATH = RESOURCES_PATH + INJECTION_DATA_FILE;
    }
}