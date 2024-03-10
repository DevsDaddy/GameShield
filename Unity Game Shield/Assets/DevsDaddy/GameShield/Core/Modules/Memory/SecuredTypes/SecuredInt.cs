using System;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes
{
    /// <summary>
    /// Secured Int Type
    /// </summary>
    [System.Serializable]
    public struct SecuredInt : IEquatable<SecuredInt>, IFormattable
    {
        private static int cryptoKey = 444444;
        
		#if UNITY_EDITOR
        public static int cryptoKeyEditor = cryptoKey;
		#endif
        
        [SerializeField]
		private int currentCryptoKey;

		[SerializeField]
		private int hiddenValue;

		[SerializeField]
		private int fakeValue;

		[SerializeField]
		private bool inited;
		
		private SecuredInt(int value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = 0;
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
		
		public static int Encrypt(int value)
		{
			return Encrypt(value, 0);
		}
		
		public static int Encrypt(int value, int key)
		{
			if (key == 0)
			{
				return value ^ cryptoKey;
			}
			return value ^ key;
		}
		
		public static int Decrypt(int value)
		{
			return Decrypt(value, 0);
		}
		
		public static int Decrypt(int value, int key)
		{
			if (key == 0)
			{
				return value ^ cryptoKey;
			}
			return value ^ key;
		}
		
		public int GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}

		public void SetEncrypted(int encrypted)
		{
			inited = true;
			hiddenValue = encrypted;
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				fakeValue = InternalDecrypt();
			}
		}
		
		private int InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = Encrypt(0);
				fakeValue = 0;
				inited = true;
			}

			int key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			int decrypted = Decrypt(hiddenValue, key);

			MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
			if (protector != null && fakeValue != 0 && decrypted != fakeValue)
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
		
		public static implicit operator SecuredInt(int value)
		{
			SecuredInt obscured = new SecuredInt(Encrypt(value));
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}
		public static implicit operator int(SecuredInt value)
		{
			return value.InternalDecrypt();
		}
		
		public static SecuredInt operator ++(SecuredInt input)
		{
			int decrypted = input.InternalDecrypt() + 1;
			input.hiddenValue = Encrypt(decrypted, input.currentCryptoKey);

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				input.fakeValue = decrypted;
			}
			return input;
		}
		
		public static SecuredInt operator --(SecuredInt input)
		{
			int decrypted = input.InternalDecrypt() - 1;
			input.hiddenValue = Encrypt(decrypted, input.currentCryptoKey);

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				input.fakeValue = decrypted;
			}
			return input;
		}
		
		public override bool Equals(object obj)
		{
			if (!(obj is SecuredInt))
				return false;

			SecuredInt oi = (SecuredInt)obj;
			return (hiddenValue == oi.hiddenValue);
		}
		
		public bool Equals(SecuredInt obj)
		{
			return hiddenValue == obj.hiddenValue;
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
		
		public string ToString(IFormatProvider provider)
		{
			return InternalDecrypt().ToString(provider);
		}
		
		public string ToString(string format, IFormatProvider provider)
		{
			return InternalDecrypt().ToString(format, provider);
		}
    }
}