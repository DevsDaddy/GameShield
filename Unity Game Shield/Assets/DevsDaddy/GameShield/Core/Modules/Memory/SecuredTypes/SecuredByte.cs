using System;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes
{
    /// <summary>
    /// Secured Byte Type
    /// </summary>
    [System.Serializable]
    public struct SecuredByte : IEquatable<SecuredByte>, IFormattable
    {
        private static byte cryptoKey = 244;
        
#if UNITY_EDITOR
        public static byte cryptoKeyEditor = cryptoKey;
#endif
        
	    [SerializeField] private byte currentCryptoKey;
	    [SerializeField] private byte hiddenValue;
	    [SerializeField] private byte fakeValue;
	    [SerializeField] private bool inited;
	    
        private SecuredByte(byte value)
        {
            currentCryptoKey = cryptoKey;
            hiddenValue = value;
            fakeValue = 0;
            inited = true;
        }
        
		public static void SetNewCryptoKey(byte newKey)
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
		
		public static byte EncryptDecrypt(byte value)
		{
			return EncryptDecrypt(value, 0);
		}
		
		public static byte EncryptDecrypt(byte value, byte key)
		{
			if (key == 0)
			{
				return (byte)(value ^ cryptoKey);
			}
			return (byte)(value ^ key);
		}
		
		public byte GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}
		
		public void SetEncrypted(byte encrypted)
		{
			inited = true;
			hiddenValue = encrypted;
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				fakeValue = InternalDecrypt();
			}
		}
		
		private byte InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = EncryptDecrypt(0);
				fakeValue = 0;
				inited = true;
			}

			byte key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			byte decrypted = EncryptDecrypt(hiddenValue, key);

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

		public static implicit operator SecuredByte(byte value)
		{
			SecuredByte obscured = new SecuredByte(EncryptDecrypt(value));
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}

		public static implicit operator byte(SecuredByte value)
		{
			return value.InternalDecrypt();
		}
		
		public static SecuredByte operator ++(SecuredByte input)
		{
			byte decrypted = (byte)(input.InternalDecrypt() + 1);
			input.hiddenValue = EncryptDecrypt(decrypted, input.currentCryptoKey);

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				input.fakeValue = decrypted;
			}
			return input;
		}
		
		public static SecuredByte operator --(SecuredByte input)
		{
			byte decrypted = (byte)(input.InternalDecrypt() - 1);
			input.hiddenValue = EncryptDecrypt(decrypted, input.currentCryptoKey);

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				input.fakeValue = decrypted;
			}
			return input;
		}
		
		public override bool Equals(object obj)
		{
			if (!(obj is SecuredByte))
				return false;

			SecuredByte ob = (SecuredByte)obj;
			return hiddenValue == ob.hiddenValue;
		}
		
		public bool Equals(SecuredByte obj)
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