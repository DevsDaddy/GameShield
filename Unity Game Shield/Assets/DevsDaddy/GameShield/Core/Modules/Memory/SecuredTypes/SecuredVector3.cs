using System;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes
{
    /// <summary>
    /// Secured Vector3 Type
    /// </summary>
    [System.Serializable]
    public struct SecuredVector3
    {
        private static int cryptoKey = 120207;
		private static readonly Vector3 initialFakeValue = Vector3.zero;
		
#if UNITY_EDITOR
		public static int cryptoKeyEditor = cryptoKey;
#endif
		
		// Serialized Fields
		[SerializeField] private int currentCryptoKey;
		[SerializeField] private RawEncryptedVector3 hiddenValue;
		[SerializeField] private Vector3 fakeValue;
		[SerializeField] private bool inited;
		
		private SecuredVector3(RawEncryptedVector3 encrypted)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = encrypted;
			fakeValue = initialFakeValue;
			inited = true;
		}
		
		public float x
		{
			get
			{
				MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
				float decrypted = InternalDecryptField(hiddenValue.x);
				if (protector!=null && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.x) > protector.Config.Vector3Epsilon)
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
				if (protector!=null && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.y) > protector.Config.Vector3Epsilon)
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
		
		public float z
		{
			get
			{
				MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
				float decrypted = InternalDecryptField(hiddenValue.z);
				if (protector!=null && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.z) > protector.Config.Vector3Epsilon)
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
				hiddenValue.z = InternalEncryptField(value);
				if (GameShield.Main.GetModule<MemoryProtector>() != null)
				{
					fakeValue.z = value;
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
					case 2:
						return z;
					default:
						throw new IndexOutOfRangeException("Invalid SecuredVector3 index!");
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
					case 2:
						z = value;
						break;
					default:
						throw new IndexOutOfRangeException("Invalid SecuredVector3 index!");
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
		
		public static RawEncryptedVector3 Encrypt(Vector3 value)
		{
			return Encrypt(value, 0);
		}
		
		public static RawEncryptedVector3 Encrypt(Vector3 value, int key)
		{
			if (key == 0)
			{
				key = cryptoKey;
			}

			RawEncryptedVector3 result;
			result.x = SecuredFloat.Encrypt(value.x, key);
			result.y = SecuredFloat.Encrypt(value.y, key);
			result.z = SecuredFloat.Encrypt(value.z, key);

			return result;
		}
		
		public static Vector3 Decrypt(RawEncryptedVector3 value)
		{
			return Decrypt(value, 0);
		}
		
		public static Vector3 Decrypt(RawEncryptedVector3 value, int key)
		{
			if (key == 0)
			{
				key = cryptoKey;
			}

			Vector3 result;
			result.x = SecuredFloat.Decrypt(value.x, key);
			result.y = SecuredFloat.Decrypt(value.y, key);
			result.z = SecuredFloat.Decrypt(value.z, key);

			return result;
		}
		
		public RawEncryptedVector3 GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}
		
		public void SetEncrypted(RawEncryptedVector3 encrypted)
		{
			inited = true;
			hiddenValue = encrypted;
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				fakeValue = InternalDecrypt();
			}
		}
		
		private Vector3 InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = Encrypt(initialFakeValue, cryptoKey);
				fakeValue = initialFakeValue;
				inited = true;
			}

			int key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			Vector3 value;

			value.x = SecuredFloat.Decrypt(hiddenValue.x, key);
			value.y = SecuredFloat.Decrypt(hiddenValue.y, key);
			value.z = SecuredFloat.Decrypt(hiddenValue.z, key);

			MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
			if (protector!=null && !fakeValue.Equals(Vector3.zero) && !CompareVectorsWithTolerance(value, fakeValue))
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
		
		private bool CompareVectorsWithTolerance(Vector3 vector1, Vector3 vector2)
		{
			MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
			float epsilon = (protector!=null) ? protector.Config.Vector3Epsilon : 0.1f;
			return Math.Abs(vector1.x - vector2.x) < epsilon &&
				   Math.Abs(vector1.y - vector2.y) < epsilon &&
				   Math.Abs(vector1.z - vector2.z) < epsilon;
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

		#region Operators
		public static implicit operator SecuredVector3(Vector3 value)
		{
			SecuredVector3 obscured = new SecuredVector3(Encrypt(value, cryptoKey));
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}
		public static implicit operator Vector3(SecuredVector3 value)
		{
			return value.InternalDecrypt();
		}
		public static SecuredVector3 operator +(SecuredVector3 a, SecuredVector3 b)
		{
			return a.InternalDecrypt() + b.InternalDecrypt();
		}
		public static SecuredVector3 operator +(Vector3 a, SecuredVector3 b)
		{
			return a + b.InternalDecrypt();
		}
		public static SecuredVector3 operator +(SecuredVector3 a, Vector3 b)
		{
			return a.InternalDecrypt() + b;
		}
		public static SecuredVector3 operator -(SecuredVector3 a, SecuredVector3 b)
		{
			return a.InternalDecrypt() - b.InternalDecrypt();
		}
		public static SecuredVector3 operator -(Vector3 a, SecuredVector3 b)
		{
			return a - b.InternalDecrypt();
		}
		public static SecuredVector3 operator -(SecuredVector3 a, Vector3 b)
		{
			return a.InternalDecrypt() - b;
		}
		public static SecuredVector3 operator -(SecuredVector3 a)
		{
			return -a.InternalDecrypt();
		}
		public static SecuredVector3 operator *(SecuredVector3 a, float d)
		{
			return a.InternalDecrypt() * d;
		}
		public static SecuredVector3 operator *(float d, SecuredVector3 a)
		{
			return d * a.InternalDecrypt();
		}
		public static SecuredVector3 operator /(SecuredVector3 a, float d)
		{
			return a.InternalDecrypt() / d;
		}

		public static bool operator ==(SecuredVector3 lhs, SecuredVector3 rhs)
		{
			return lhs.InternalDecrypt() == rhs.InternalDecrypt();
		}
		public static bool operator ==(Vector3 lhs, SecuredVector3 rhs)
		{
			return lhs == rhs.InternalDecrypt();
		}
		public static bool operator ==(SecuredVector3 lhs, Vector3 rhs)
		{
			return lhs.InternalDecrypt() == rhs;
		}

		public static bool operator !=(SecuredVector3 lhs, SecuredVector3 rhs)
		{
			return lhs.InternalDecrypt() != rhs.InternalDecrypt();
		}
		public static bool operator !=(Vector3 lhs, SecuredVector3 rhs)
		{
			return lhs != rhs.InternalDecrypt();
		}
		public static bool operator !=(SecuredVector3 lhs, Vector3 rhs)
		{
			return lhs.InternalDecrypt() != rhs;
		}
		#endregion
		
		public override bool Equals(object other)
		{
			return InternalDecrypt().Equals(other);
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
		public struct RawEncryptedVector3
		{
			public int x;
			public int y;
			public int z;
		}
    }
}