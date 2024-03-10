using System;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes
{
	/// <summary>
	/// Secured Long Type
	/// </summary>
	[System.Serializable]
    public struct SecuredLong : IEquatable<SecuredLong>, IFormattable
    {
        private static long cryptoKey = 444442L;
        
        #if UNITY_EDITOR
        public static long cryptoKeyEditor = cryptoKey;
#endif
        
	    [SerializeField] private long currentCryptoKey;
		[SerializeField] private long hiddenValue;
		[SerializeField] private long fakeValue;
		[SerializeField] private bool inited;
		
		private SecuredLong(long value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = 0;
			inited = true;
		}
		
		public static void SetNewCryptoKey(long newKey)
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
		
		public static long Encrypt(long value)
		{
			return Encrypt(value, 0);
		}
		
		public static long Decrypt(long value)
		{
			return Decrypt(value, 0);
		}
		
		public static long Encrypt(long value, long key)
		{
			if (key == 0)
			{
				return value ^ cryptoKey;
			}
			return value ^ key;
		}
		
		public static long Decrypt(long value, long key)
		{
			if (key == 0)
			{
				return value ^ cryptoKey;
			}
			return value ^ key;
		}
		
		public long GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}
		
		public void SetEncrypted(long encrypted)
		{
			inited = true;
			hiddenValue = encrypted;

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				fakeValue = InternalDecrypt();
			}
		}
		
		private long InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = Encrypt(0);
				fakeValue = 0;
				inited = true;
			}

			long key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			long decrypted = Decrypt(hiddenValue, key);

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
		
		
		public static implicit operator SecuredLong(long value)
		{
			SecuredLong obscured = new SecuredLong(Encrypt(value));
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}
		public static implicit operator long(SecuredLong value)
		{
			return value.InternalDecrypt();
		}
		
		public static SecuredLong operator ++(SecuredLong input)
		{
			long decrypted = input.InternalDecrypt() + 1L;
			input.hiddenValue = Encrypt(decrypted, input.currentCryptoKey);

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				input.fakeValue = decrypted;
			}
			return input;
		}
		
		public static SecuredLong operator --(SecuredLong input)
		{
			long decrypted = input.InternalDecrypt() - 1L;
			input.hiddenValue = Encrypt(decrypted, input.currentCryptoKey);

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				input.fakeValue = decrypted;
			}
			return input;
		}
		
		public override bool Equals(object obj)
		{
			if (!(obj is SecuredLong))
				return false;
			
			SecuredLong o = (SecuredLong)obj;
			return (hiddenValue == o.hiddenValue);
		}
		
		public bool Equals(SecuredLong obj)
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