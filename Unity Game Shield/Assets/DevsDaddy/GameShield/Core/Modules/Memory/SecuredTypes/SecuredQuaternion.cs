using System;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes
{
    /// <summary>
    /// Secured Quaternion Type
    /// </summary>
    [System.Serializable]
    public class SecuredQuaternion
    {
        private static int cryptoKey = 120205;
        private static readonly Quaternion initialFakeValue = Quaternion.identity;
        
#if UNITY_EDITOR
        public static int cryptoKeyEditor = cryptoKey;
#endif
        
        // Serialized Params
        [SerializeField] private int currentCryptoKey;
        [SerializeField] private RawEncryptedQuaternion hiddenValue;
        [SerializeField] private Quaternion fakeValue;
        [SerializeField] private bool inited;
        
		private SecuredQuaternion(RawEncryptedQuaternion value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = initialFakeValue;
			inited = true;
		}
		
		public static void SetNewCryptoKey(int newKey)
		{
			cryptoKey = newKey;
		}
		
		public void ApplyNewCryptoKey()
		{
			if (currentCryptoKey != cryptoKey)
			{
				hiddenValue = Encrypt(InternalDecrypt(), cryptoKey);
				currentCryptoKey = cryptoKey;
			}
		}
		
		public static RawEncryptedQuaternion Encrypt(Quaternion value)
		{
			return Encrypt(value, 0);
		}
		
		public static RawEncryptedQuaternion Encrypt(Quaternion value, int key)
		{
			if (key == 0)
			{
				key = cryptoKey;
			}

			RawEncryptedQuaternion result;
			result.x = SecuredFloat.Encrypt(value.x, key);
			result.y = SecuredFloat.Encrypt(value.y, key);
			result.z = SecuredFloat.Encrypt(value.z, key);
			result.w = SecuredFloat.Encrypt(value.w, key);

			return result;
		}
		
		public static Quaternion Decrypt(RawEncryptedQuaternion value)
		{
			return Decrypt(value, 0);
		}
		
		public static Quaternion Decrypt(RawEncryptedQuaternion value, int key)
		{
			if (key == 0)
			{
				key = cryptoKey;
			}

			Quaternion result;
			result.x = SecuredFloat.Decrypt(value.x, key);
			result.y = SecuredFloat.Decrypt(value.y, key);
			result.z = SecuredFloat.Decrypt(value.z, key);
			result.w = SecuredFloat.Decrypt(value.w, key);

			return result;
		}
		
		public RawEncryptedQuaternion GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}
		
		public void SetEncrypted(RawEncryptedQuaternion encrypted)
		{
			inited = true;
			hiddenValue = encrypted;
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				fakeValue = InternalDecrypt();
			}
		}
		
		private Quaternion InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = Encrypt(initialFakeValue);
				fakeValue = initialFakeValue;
				inited = true;
			}

			int key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			Quaternion value;

			value.x = SecuredFloat.Decrypt(hiddenValue.x, key);
			value.y = SecuredFloat.Decrypt(hiddenValue.y, key);
			value.z = SecuredFloat.Decrypt(hiddenValue.z, key);
			value.w = SecuredFloat.Decrypt(hiddenValue.w, key);

			MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
			if (protector != null && !fakeValue.Equals(initialFakeValue) && !CompareQuaternionsWithTolerance(value, fakeValue))
			{
				EventMessenger.Main.Publish(new SecurityWarningPayload {
					Code = 101,
					Message = MemoryWarnings.TypeHackWarning,
					IsCritical = true,
					Module = protector
				});
			}

			return value;
		}
		
		private bool CompareQuaternionsWithTolerance(Quaternion q1, Quaternion q2)
		{
			MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
			float epsilon = (protector != null) ? protector.Config.QuaternionEpsilon : 0.1f;
			return Math.Abs(q1.x - q2.x) < epsilon &&
				   Math.Abs(q1.y - q2.y) < epsilon &&
				   Math.Abs(q1.z - q2.z) < epsilon &&
				   Math.Abs(q1.w - q2.w) < epsilon;
		}
		
		public static implicit operator SecuredQuaternion(Quaternion value)
		{
			SecuredQuaternion obscured = new SecuredQuaternion(Encrypt(value));
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}
		public static implicit operator Quaternion(SecuredQuaternion value)
		{
			return value.InternalDecrypt();
		}
		
		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}
		
		public override string ToString()
		{
			return InternalDecrypt().ToString();
		}
		
		public string ToString(string format)
		{
			return InternalDecrypt().ToString(format);
		}
		
		[System.Serializable]
		public struct RawEncryptedQuaternion
		{
			public int x;
			public int y;
			public int z;
			public int w;
		}
    }
}