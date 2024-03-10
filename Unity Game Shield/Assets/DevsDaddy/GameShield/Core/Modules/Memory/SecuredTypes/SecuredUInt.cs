using System;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes
{
    /// <summary>
    /// Secured UInt Type
    /// </summary>
    [System.Serializable]
    public class SecuredUInt : IEquatable<SecuredUInt>, IFormattable
    {
        private static uint cryptoKey = 240513;
        
#if UNITY_EDITOR
        public static uint cryptoKeyEditor = cryptoKey;
#endif

	    [SerializeField] private uint currentCryptoKey;
	    [SerializeField] private uint hiddenValue;
	    [SerializeField] private uint fakeValue;
	    [SerializeField] private bool inited;
	    
		private SecuredUInt(uint value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = 0;
			inited = true;
		}
		
		public static void SetNewCryptoKey(uint newKey)
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
		
		public static uint Encrypt(uint value)
		{
			return Encrypt(value, 0);
		}
		
		public static uint Decrypt(uint value)
		{
			return Decrypt(value, 0);
		}
		
		public static uint Encrypt(uint value, uint key)
		{
			if (key == 0)
			{
				return value ^ cryptoKey;
			}
			return value ^ key;
		}
		
		public static uint Decrypt(uint value, uint key)
		{
			if (key == 0)
			{
				return value ^ cryptoKey;
			}
			return value ^ key;
		}
		
		public uint GetEncrypted()
		{
			ApplyNewCryptoKey();

			return hiddenValue;
		}
		
		public void SetEncrypted(uint encrypted)
		{
			inited = true;
			hiddenValue = encrypted;
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				fakeValue = InternalDecrypt();
			}
		}
		
		private uint InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = Encrypt(0);
				fakeValue = 0;
				inited = true;
			}

			uint key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			uint decrypted = Decrypt(hiddenValue, key);

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
		
		public static implicit operator SecuredUInt(uint value)
		{
			SecuredUInt obscured = new SecuredUInt(Encrypt(value));
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}
		public static implicit operator uint(SecuredUInt value)
		{
			return value.InternalDecrypt();
		}
		
		public static SecuredUInt operator ++(SecuredUInt input)
		{
			uint decrypted = input.InternalDecrypt() + 1;
			input.hiddenValue = Encrypt(decrypted, input.currentCryptoKey);

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				input.fakeValue = decrypted;
			}
			return input;
		}
		
		public static SecuredUInt operator --(SecuredUInt input)
		{
			uint decrypted = input.InternalDecrypt() - 1;
			input.hiddenValue = Encrypt(decrypted, input.currentCryptoKey);

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				input.fakeValue = decrypted;
			}
			return input;
		}
		
		public override bool Equals(object obj)
		{
			if (!(obj is SecuredUInt))
				return false;
			
			SecuredUInt oi = (SecuredUInt)obj;
			return ((int)hiddenValue == (int)oi.hiddenValue);
		}
		
		public bool Equals(SecuredUInt obj)
		{
			return (int)hiddenValue == (int)obj.hiddenValue;
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