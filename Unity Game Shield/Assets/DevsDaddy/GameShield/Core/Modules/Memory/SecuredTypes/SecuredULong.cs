using System;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes
{
    /// <summary>
    /// Secured ULong Type
    /// </summary>
    [System.Serializable]
    public class SecuredULong : IEquatable<SecuredULong>, IFormattable
    {
        private static ulong cryptoKey = 444443L;
        
#if UNITY_EDITOR
        public static ulong cryptoKeyEditor = cryptoKey;
#endif

        [SerializeField] private ulong currentCryptoKey;
        [SerializeField] private ulong hiddenValue;
        [SerializeField] private ulong fakeValue;
		[SerializeField] private bool inited;
		
		private SecuredULong(ulong value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = 0;
			inited = true;
		}
		
		public static void SetNewCryptoKey(ulong newKey)
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
		
		public static ulong Encrypt(ulong value)
		{
			return Encrypt(value, 0);
		}
		
		public static ulong Decrypt(ulong value)
		{
			return Decrypt(value, 0);
		}
		
		public static ulong Encrypt(ulong value, ulong key)
		{
			if (key == 0)
			{
				return value ^ cryptoKey;
			}
			return value ^ key;
		}
		
		public static ulong Decrypt(ulong value, ulong key)
		{
			if (key == 0)
			{
				return value ^ cryptoKey;
			}
			return value ^ key;
		}
		
		public ulong GetEncrypted()
		{
			ApplyNewCryptoKey();

			return hiddenValue;
		}
		
		public void SetEncrypted(ulong encrypted)
		{
			inited = true;
			hiddenValue = encrypted;

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				fakeValue = InternalDecrypt();
			}
		}
		
		private ulong InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = Encrypt(0);
				fakeValue = 0;
				inited = true;
			}

			ulong key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			ulong decrypted = Decrypt(hiddenValue, key);

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
		
		public static implicit operator SecuredULong(ulong value)
		{
			SecuredULong obscured = new SecuredULong(Encrypt(value));
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}
		public static implicit operator ulong(SecuredULong value)
		{
			return value.InternalDecrypt();
		}
		
		public static SecuredULong operator ++(SecuredULong input)
		{
			ulong decrypted = input.InternalDecrypt() + 1L;
			input.hiddenValue = Encrypt(decrypted, input.currentCryptoKey);

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				input.fakeValue = decrypted;
			}
			return input;
		}
		
		public static SecuredULong operator --(SecuredULong input)
		{
			ulong decrypted = input.InternalDecrypt() - 1L;
			input.hiddenValue = Encrypt(decrypted, input.currentCryptoKey);

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				input.fakeValue = decrypted;
			}
			return input;
		}
		
		public override bool Equals(object obj)
		{
			if (!(obj is SecuredULong))
				return false;
			
			SecuredULong o = (SecuredULong)obj;
			return (hiddenValue == o.hiddenValue);
		}
		
		public bool Equals(SecuredULong obj)
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