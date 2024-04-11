using System;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes
{
    /// <summary>
    /// Secured Color Type
    /// </summary>
    [System.Serializable]
    public struct SecuredColor
    {
        private static int cryptoKey = 120222;
        private static readonly Color initialFakeValue = Color.black;
        
        #if UNITY_EDITOR
        public static int cryptoKeyEditor = cryptoKey;
        #endif
        
        // Serialized Fields
        [SerializeField] private int currentCryptoKey;
        [SerializeField] private RawEncryptedColor hiddenValue;
        [SerializeField] private Color fakeValue;
        [SerializeField] private bool inited;
        
        private SecuredColor(RawEncryptedColor encrypted)
        {
            currentCryptoKey = cryptoKey;
            hiddenValue = encrypted;
            fakeValue = initialFakeValue;
            inited = true;
        }
        
        public float r
        {
            get
            {
	            MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
                float decrypted = InternalDecryptField(hiddenValue.r);
                if (protector != null && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.r) > protector.Config.ColorEpsilon)
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
	            hiddenValue.r = InternalEncryptField(value);
                if (GameShield.Main.GetModule<MemoryProtector>() != null)
                {
                    fakeValue.r = value;
                }
            }
        }
        
        public float g
        {
            get
            {
	            MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
                float decrypted = InternalDecryptField(hiddenValue.g);
                if (protector != null && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.g) > protector.Config.ColorEpsilon)
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
	            hiddenValue.g = InternalEncryptField(value);
                if (GameShield.Main.GetModule<MemoryProtector>() != null)
                {
                    fakeValue.g = value;
                }
            }
        }
        
        public float b
        {
            get
            {
	            MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
                float decrypted = InternalDecryptField(hiddenValue.b);
                if (protector != null && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.b) > protector.Config.ColorEpsilon)
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
	            hiddenValue.b = InternalEncryptField(value);
                if (GameShield.Main.GetModule<MemoryProtector>() != null)
                {
                    fakeValue.b = value;
                }
            }
        }
        
        public float a
        {
            get
            {
	            MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
                float decrypted = InternalDecryptField(hiddenValue.a);
                if (protector != null && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.a) > protector.Config.ColorEpsilon)
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
	            hiddenValue.a = InternalEncryptField(value);
                if (GameShield.Main.GetModule<MemoryProtector>() != null)
                {
                    fakeValue.a = value;
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
                        return r;
                    case 1:
                        return g;
                    case 2:
                        return b;
                    case 3:
                        return a;
                    default:
                        throw new IndexOutOfRangeException("Invalid SecuredColor index!");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        r = value;
                        break;
                    case 1:
                        g = value;
                        break;
                    case 2:
                        b = value;
                        break;
                    case 3:
                        a = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid SecuredColor index!");
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
		
		public static RawEncryptedColor Encrypt(Color value)
		{
			return Encrypt(value, 0);
		}
		
		public static RawEncryptedColor Encrypt(Color value, int key)
		{
			if (key == 0)
			{
				key = cryptoKey;
			}

			RawEncryptedColor result;
			result.r = SecuredFloat.Encrypt(value.r, key);
			result.g = SecuredFloat.Encrypt(value.g, key);
			result.b = SecuredFloat.Encrypt(value.b, key);
			result.a = SecuredFloat.Encrypt(value.a, key);

			return result;
		}
		
		public static Color Decrypt(RawEncryptedColor value)
		{
			return Decrypt(value, 0);
		}
		
		public static Color Decrypt(RawEncryptedColor value, int key)
		{
			if (key == 0)
			{
				key = cryptoKey;
			}

			Color result;
			result.r = SecuredFloat.Decrypt(value.r, key);
			result.g = SecuredFloat.Decrypt(value.g, key);
			result.b = SecuredFloat.Decrypt(value.b, key);
			result.a = SecuredFloat.Decrypt(value.a, key);

			return result;
		}
		
		public RawEncryptedColor GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}
		
		public void SetEncrypted(RawEncryptedColor encrypted)
		{
			inited = true;
			hiddenValue = encrypted;
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				fakeValue = InternalDecrypt();
			}
		}
		
		private Color InternalDecrypt()
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

			Color value;

			value.r = SecuredFloat.Decrypt(hiddenValue.r, key);
			value.g = SecuredFloat.Decrypt(hiddenValue.g, key);
			value.b = SecuredFloat.Decrypt(hiddenValue.b, key);
			value.a = SecuredFloat.Decrypt(hiddenValue.a, key);

			MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
			if (protector != null && !fakeValue.Equals(Color.black) && !CompareVectorsWithTolerance(value, fakeValue))
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
		
		private bool CompareVectorsWithTolerance(Color color1, Color color2)
		{
			MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
			float epsilon = (protector != null) ? protector.Config.ColorEpsilon : 0.1f;
			return Math.Abs(color1.r - color2.r) < epsilon &&
				   Math.Abs(color1.g - color2.g) < epsilon &&
				   Math.Abs(color1.b - color2.b) < epsilon &&
				   Math.Abs(color1.a - color2.a) < epsilon;
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
		public static implicit operator SecuredColor(Color value)
		{
			SecuredColor obscured = new SecuredColor(Encrypt(value, cryptoKey));
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}
		public static implicit operator Color(SecuredColor value)
		{
			return value.InternalDecrypt();
		}
		public static SecuredColor operator +(SecuredColor a, SecuredColor b)
		{
			return a.InternalDecrypt() + b.InternalDecrypt();
		}
		public static SecuredColor operator +(Color a, SecuredColor b)
		{
			return a + b.InternalDecrypt();
		}
		public static SecuredColor operator +(SecuredColor a, Color b)
		{
			return a.InternalDecrypt() + b;
		}
		public static SecuredColor operator -(SecuredColor a, SecuredColor b)
		{
			return a.InternalDecrypt() - b.InternalDecrypt();
		}
		public static SecuredColor operator -(Color a, SecuredColor b)
		{
			return a - b.InternalDecrypt();
		}
		public static SecuredColor operator -(SecuredColor a, Color b)
		{
			return a.InternalDecrypt() - b;
		}
		public static SecuredColor operator *(SecuredColor a, float d)
		{
			return a.InternalDecrypt() * d;
		}
		public static SecuredColor operator *(float d, SecuredColor a)
		{
			return d * a.InternalDecrypt();
		}
		public static SecuredColor operator /(SecuredColor a, float d)
		{
			return a.InternalDecrypt() / d;
		}

		public static bool operator ==(SecuredColor lhs, SecuredColor rhs)
		{
			return lhs.InternalDecrypt() == rhs.InternalDecrypt();
		}
		public static bool operator ==(Color lhs, SecuredColor rhs)
		{
			return lhs == rhs.InternalDecrypt();
		}
		public static bool operator ==(SecuredColor lhs, Color rhs)
		{
			return lhs.InternalDecrypt() == rhs;
		}

		public static bool operator !=(SecuredColor lhs, SecuredColor rhs)
		{
			return lhs.InternalDecrypt() != rhs.InternalDecrypt();
		}
		public static bool operator !=(Color lhs, SecuredColor rhs)
		{
			return lhs != rhs.InternalDecrypt();
		}
		public static bool operator !=(SecuredColor lhs, Color rhs)
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
		public struct RawEncryptedColor
		{
			public int r;
			public int g;
			public int b;
			public int a;
		}
    }
}