using System;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes
{
    /// <summary>
    /// Secured Vector4 Type
    /// </summary>
    [System.Serializable]
    public struct SecuredVector4
    {
        private static int cryptoKey = 120207;
		private static readonly Vector4 initialFakeValue = Vector4.zero;
		
#if UNITY_EDITOR
		public static int cryptoKeyEditor = cryptoKey;
#endif
		
		// Serialized Fields
		[SerializeField] private int currentCryptoKey;
		[SerializeField] private RawEncryptedVector4 hiddenValue;
		[SerializeField] private Vector4 fakeValue;
		[SerializeField] private bool inited;
		
		private SecuredVector4(RawEncryptedVector4 encrypted)
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
				if (protector!=null && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.x) > protector.Config.Vector4Epsilon)
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
				if (protector!=null && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.y) > protector.Config.Vector4Epsilon)
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
				if (protector!=null && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.z) > protector.Config.Vector4Epsilon)
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
		
		public float w
		{
			get
			{
				MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
				float decrypted = InternalDecryptField(hiddenValue.w);
				if (protector!=null && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.w) > protector.Config.Vector4Epsilon)
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
				hiddenValue.w = InternalEncryptField(value);
				if (GameShield.Main.GetModule<MemoryProtector>() != null)
				{
					fakeValue.w = value;
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
					case 3:
						return w;
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
					case 3:
						w = value;
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
		
		public static RawEncryptedVector4 Encrypt(Vector4 value)
		{
			return Encrypt(value, 0);
		}
		
		public static RawEncryptedVector4 Encrypt(Vector4 value, int key)
		{
			if (key == 0)
			{
				key = cryptoKey;
			}

			RawEncryptedVector4 result;
			result.x = SecuredFloat.Encrypt(value.x, key);
			result.y = SecuredFloat.Encrypt(value.y, key);
			result.z = SecuredFloat.Encrypt(value.z, key);
			result.w = SecuredFloat.Encrypt(value.w, key);

			return result;
		}
		
		public static Vector4 Decrypt(RawEncryptedVector4 value)
		{
			return Decrypt(value, 0);
		}
		
		public static Vector4 Decrypt(RawEncryptedVector4 value, int key)
		{
			if (key == 0)
			{
				key = cryptoKey;
			}

			Vector4 result;
			result.x = SecuredFloat.Decrypt(value.x, key);
			result.y = SecuredFloat.Decrypt(value.y, key);
			result.z = SecuredFloat.Decrypt(value.z, key);
			result.w = SecuredFloat.Decrypt(value.w, key);

			return result;
		}
		
		public RawEncryptedVector4 GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}
		
		public void SetEncrypted(RawEncryptedVector4 encrypted)
		{
			inited = true;
			hiddenValue = encrypted;
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				fakeValue = InternalDecrypt();
			}
		}
		
		private Vector4 InternalDecrypt()
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

			Vector4 value;

			value.x = SecuredFloat.Decrypt(hiddenValue.x, key);
			value.y = SecuredFloat.Decrypt(hiddenValue.y, key);
			value.z = SecuredFloat.Decrypt(hiddenValue.z, key);
			value.w = SecuredFloat.Decrypt(hiddenValue.w, key);

			MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
			if (protector != null && !fakeValue.Equals(Vector4.zero) && !CompareVectorsWithTolerance(value, fakeValue))
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
		
		private bool CompareVectorsWithTolerance(Vector4 vector1, Vector4 vector2)
		{
			MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
			float epsilon = (protector!=null) ? protector.Config.Vector4Epsilon : 0.1f;
			return Math.Abs(vector1.x - vector2.x) < epsilon &&
				   Math.Abs(vector1.y - vector2.y) < epsilon &&
				   Math.Abs(vector1.z - vector2.z) < epsilon &&
				   Math.Abs(vector1.w - vector2.w) < epsilon;
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
		public static implicit operator SecuredVector4(Vector4 value)
		{
			SecuredVector4 obscured = new SecuredVector4(Encrypt(value, cryptoKey));
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}
		public static implicit operator Vector4(SecuredVector4 value)
		{
			return value.InternalDecrypt();
		}
		public static SecuredVector4 operator +(SecuredVector4 a, SecuredVector4 b)
		{
			return a.InternalDecrypt() + b.InternalDecrypt();
		}
		public static SecuredVector4 operator +(Vector4 a, SecuredVector4 b)
		{
			return a + b.InternalDecrypt();
		}
		public static SecuredVector4 operator +(SecuredVector4 a, Vector4 b)
		{
			return a.InternalDecrypt() + b;
		}
		public static SecuredVector4 operator -(SecuredVector4 a, SecuredVector4 b)
		{
			return a.InternalDecrypt() - b.InternalDecrypt();
		}
		public static SecuredVector4 operator -(Vector4 a, SecuredVector4 b)
		{
			return a - b.InternalDecrypt();
		}
		public static SecuredVector4 operator -(SecuredVector4 a, Vector4 b)
		{
			return a.InternalDecrypt() - b;
		}
		public static SecuredVector4 operator -(SecuredVector4 a)
		{
			return -a.InternalDecrypt();
		}
		public static SecuredVector4 operator *(SecuredVector4 a, float d)
		{
			return a.InternalDecrypt() * d;
		}
		public static SecuredVector4 operator *(float d, SecuredVector4 a)
		{
			return d * a.InternalDecrypt();
		}
		public static SecuredVector4 operator /(SecuredVector4 a, float d)
		{
			return a.InternalDecrypt() / d;
		}

		public static bool operator ==(SecuredVector4 lhs, SecuredVector4 rhs)
		{
			return lhs.InternalDecrypt() == rhs.InternalDecrypt();
		}
		public static bool operator ==(Vector4 lhs, SecuredVector4 rhs)
		{
			return lhs == rhs.InternalDecrypt();
		}
		public static bool operator ==(SecuredVector4 lhs, Vector4 rhs)
		{
			return lhs.InternalDecrypt() == rhs;
		}

		public static bool operator !=(SecuredVector4 lhs, SecuredVector4 rhs)
		{
			return lhs.InternalDecrypt() != rhs.InternalDecrypt();
		}
		public static bool operator !=(Vector4 lhs, SecuredVector4 rhs)
		{
			return lhs != rhs.InternalDecrypt();
		}
		public static bool operator !=(SecuredVector4 lhs, Vector4 rhs)
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
		public struct RawEncryptedVector4
		{
			public int x;
			public int y;
			public int z;
			public int w;
		}
    }
}