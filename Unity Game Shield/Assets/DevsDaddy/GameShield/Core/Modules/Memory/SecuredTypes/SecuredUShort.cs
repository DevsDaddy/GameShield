using System;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes
{
    /// <summary>
    /// Secured UShort Type
    /// </summary>
    [System.Serializable]
    public class SecuredUShort : IEquatable<SecuredUShort>, IFormattable
    {
        private static ushort cryptoKey = 224;
        
#if UNITY_EDITOR
        public static ushort cryptoKeyEditor = cryptoKey;
#endif

	    [SerializeField] private ushort currentCryptoKey;
	    [SerializeField] private ushort hiddenValue;
	    [SerializeField] private ushort fakeValue;
	    [SerializeField] private bool inited;
	    
		private SecuredUShort(ushort value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = 0;
			inited = true;
		}
		
		public static void SetNewCryptoKey(ushort newKey)
		{
			cryptoKey = newKey;
		}
		
		public void ApplyNewCryptoKey()
		{
			if (currentCryptoKey != cryptoKey)
			{
				hiddenValue = EncryptDecrypt(InternalDecrypt(), cryptoKey);
				currentCryptoKey = cryptoKey;
			}
		}
		
		public static ushort EncryptDecrypt(ushort value)
		{
			return EncryptDecrypt(value, 0);
		}
		
		public static ushort EncryptDecrypt(ushort value, ushort key)
		{
			if (key == 0)
			{
				return (ushort)(value ^ cryptoKey);
			}
			return (ushort)(value ^ key);
		}
		
		public ushort GetEncrypted()
		{
			ApplyNewCryptoKey();

			return hiddenValue;
		}
		
		public void SetEncrypted(ushort encrypted)
		{
			inited = true;
			hiddenValue = encrypted;
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				fakeValue = InternalDecrypt();
			}
		}
		
		private ushort InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = EncryptDecrypt(0);
				fakeValue = 0;
				inited = true;
			}

			ushort key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			ushort decrypted = EncryptDecrypt(hiddenValue, key);

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
		
		public static implicit operator SecuredUShort(ushort value)
		{
			SecuredUShort obscured = new SecuredUShort(EncryptDecrypt(value));
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}
		public static implicit operator ushort(SecuredUShort value)
		{
			return value.InternalDecrypt();
		}
		
		public static SecuredUShort operator ++(SecuredUShort input)
		{
			ushort decrypted = (ushort)(input.InternalDecrypt() + 1);
			input.hiddenValue = EncryptDecrypt(decrypted, input.currentCryptoKey);

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				input.fakeValue = decrypted;
			}
			return input;
		}
		
		public static SecuredUShort operator --(SecuredUShort input)
		{
			ushort decrypted = (ushort)(input.InternalDecrypt() - 1);
			input.hiddenValue = EncryptDecrypt(decrypted, input.currentCryptoKey);

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				input.fakeValue = decrypted;
			}
			return input;
		}
		
		public override bool Equals(object obj)
		{
			if (!(obj is SecuredUShort))
				return false;
			
			SecuredUShort ob = (SecuredUShort)obj;
			return hiddenValue == ob.hiddenValue;
		}
		
		public bool Equals(SecuredUShort obj)
		{
			return hiddenValue == obj.hiddenValue;
		}
		
		public override string ToString()
		{
			return InternalDecrypt().ToString();
		}
		
		public string ToString(string format)
		{
			return InternalDecrypt().ToString(format);
		}
		
		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
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