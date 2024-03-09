using System.Collections.Generic;
using DevsDaddy.GameShield.Core.Modules;
using UnityEngine;

namespace DevsDaddy.GameShield.Core
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "GameShieldConfig", menuName = "GameShield/Config/General", order = 0)]
    public class GameShieldConfig : ScriptableObject
    {
        public int ConfigRevision = 0;
        public List<IShieldModule> AvailableModules = new List<IShieldModule>();
    }
}