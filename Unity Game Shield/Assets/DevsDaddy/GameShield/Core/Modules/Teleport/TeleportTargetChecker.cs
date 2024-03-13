using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.Teleport
{
    public class TeleportTargetChecker
    {
        public Transform Target;
        public Vector3 LastPosition;
        public float MaxSpeed = 3f;
    }
}