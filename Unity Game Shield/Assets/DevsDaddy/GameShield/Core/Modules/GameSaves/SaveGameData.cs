using System;

namespace DevsDaddy.GameShield.Core.Modules.GameSaves
{
    [System.Serializable]
    public class SaveGameData : ISaveObject
    {
        public DateTime Date = DateTime.Now;
        public ISaveObject Object;
    }
}