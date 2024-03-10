using System;
using DevsDaddy.Shared.EventFramework.Core.Payloads;

namespace DevsDaddy.GameShield.Core.Modules.GameSaves.Payloads
{
    /// <summary>
    /// Save Game Request
    /// </summary>
    [System.Serializable]
    public class SaveGamePayload : IPayload
    {
        public string Path = "";
        public ISaveObject Object;
        public Action OnComplete;
    }

    /// <summary>
    /// Load Game Request
    /// </summary>
    [System.Serializable]
    public class LoadGamePayload : IPayload
    {
        public string Path = "";
        public Action<SaveGameData, ISaveObject> OnComplete;
    }
}