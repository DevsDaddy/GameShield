using System;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes
{
    /// <summary>
    /// Secured Vector2 Type
    /// </summary>
    [System.Serializable]
    public struct SecuredVector2
    {
        private static int cryptoKey = 120206;
		private static readonly Vector2 initialFakeValue = Vector2.zero;
		
#if UNITY_EDITOR
		public static int cryptoKeyEditor = cryptoKey;
#endif

		// Serialized Fields
		[SerializeField] private int currentCryptoKey;
		[SerializeField] private RawEncryptedVector2 hiddenValue;
		[SerializeField] private Vector2 fakeValue;
		[SerializeField] private bool inited;
		
		private SecuredVector2(RawEncryptedVector2 value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = initialFakeValue;
			inited = true;
		}
		
		public float x
		{
			get
			{
				MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
				float decrypted = InternalDecryptField(hiddenValue.x);
				if (protector != null && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.x) > protector.Config.Vector2Epsilon)
				{
					EventMessenger.Main.Publish(new SecurityWarningPayload {
						Code = 101,
						Message = MemoryWarnings.TypeHackWarning,
						IsCritical = true,
						Module = protector
					});
				}
				return decrypted;
			}

			set
			{
				hiddenValue.x = InternalEncryptField(value);
				if (GameShield.Main.GetModule<MemoryProtector>() != null)
				{
					fakeValue.x = value;
				}
			}
		}
		
		public float y
		{
			get
			{
				MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
				float decrypted = InternalDecryptField(hiddenValue.y);
				if (protector != null && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.y) > protector.Config.Vector2Epsilon)
				{
					EventMessenger.Main.Publish(new SecurityWarningPayload {
						Code = 101,
						Message = MemoryWarnings.TypeHackWarning,
						IsCritical = true,
						Module = protector
					});
				}
				return decrypted;
			}

			set
			{
				hiddenValue.y = InternalEncryptField(value);
				if (GameShield.Main.GetModule<MemoryProtector>() != null)
				{
					fakeValue.y = value;
				}
			}
		}
		
		public float this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return x;
					case 1:
						return y;
					default:
						throw new IndexOutOfRangeException("Invalid SecuredVector2 index!");
				}
			}
			set
			{
				switch (index)
				{
					case 0:
						x = value;
						break;
					case 1:
						y = value;
						break;
					default:
						throw new IndexOutOfRangeException("Invalid SecuredVector2 index!");
				}
			}
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
		
		public static RawEncryptedVector2 Encrypt(Vector2 value)
		{
			return Encrypt(value, 0);
		}
		
		public static RawEncryptedVector2 Encrypt(Vector2 value, int key)
		{
			if (key == 0)
			{
				key = cryptoKey;
			}

			RawEncryptedVector2 result;
			result.x = SecuredFloat.Encrypt(value.x, key);
			result.y = SecuredFloat.Encrypt(value.y, key);

			return result;
		}
		
		public static Vector2 Decrypt(RawEncryptedVector2 value)
		{
			return Decrypt(value, 0);
		}
		
		public static Vector2 Decrypt(RawEncryptedVector2 value, int key)
		{
			if (key == 0)
			{
				key = cryptoKey;
			}

			Vector2 result;
			result.x = SecuredFloat.Decrypt(value.x, key);
			result.y = SecuredFloat.Decrypt(value.y, key);

			return result;
		}
		
		public RawEncryptedVector2 GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}
		
		public void SetEncrypted(RawEncryptedVector2 encrypted)
		{
			inited = true;
			hiddenValue = encrypted;
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				fakeValue = InternalDecrypt();
			}
		}
		
		private Vector2 InternalDecrypt()
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

			Vector2 value;

			value.x = SecuredFloat.Decrypt(hiddenValue.x, key);
			value.y = SecuredFloat.Decrypt(hiddenValue.y, key);

			MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
			if (protector != null && !fakeValue.Equals(initialFakeValue) && !CompareVectorsWithTolerance(value, fakeValue))
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
		
		private bool CompareVectorsWithTolerance(Vector2 vector1, Vector2 vector2)
		{
			MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
			float epsilon = (protector != null) ? protector.Config.Vector2Epsilon : 0.1f;
			return Math.Abs(vector1.x - vector2.x) < epsilon &&
				   Math.Abs(vector1.y - vector2.y) < epsilon;
		}
		
		private float InternalDecryptField(int encrypted)
		{
			int key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			float result = SecuredFloat.Decrypt(encrypted, key);
			return result;
		}
		
		private int InternalEncryptField(float encrypted)
		{
			int result = SecuredFloat.Encrypt(encrypted, cryptoKey);
			return result;
		}
		
		public static implicit operator SecuredVector2(Vector2 value)
		{
			SecuredVector2 obscured = new SecuredVector2(Encrypt(value));
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}
		public static implicit operator Vector2(SecuredVector2 value)
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
		public struct RawEncryptedVector2
		{
			public int x;
			public int y;
		}
    }
}