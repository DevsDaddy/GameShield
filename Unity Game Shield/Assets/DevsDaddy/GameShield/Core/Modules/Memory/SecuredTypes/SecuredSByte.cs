using System;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes
{
    /// <summary>
    /// Secured SByte Type
    /// </summary>
    [System.Serializable]
    public struct SecuredSByte : IEquatable<SecuredSByte>, IFormattable
    {
        private static sbyte cryptoKey = 112;
        
#if UNITY_EDITOR
        public static sbyte cryptoKeyEditor = cryptoKey;
#endif

        [SerializeField] private sbyte currentCryptoKey;
        [SerializeField] private sbyte hiddenValue;
        [SerializeField] private sbyte fakeValue;
        [SerializeField] private bool inited;
        
		private SecuredSByte(sbyte value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = 0;
			inited = true;
		}
		
		public static void SetNewCryptoKey(sbyte newKey)
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
		
		public static sbyte EncryptDecrypt(sbyte value)
		{
			return EncryptDecrypt(value, 0);
		}
		
		public static sbyte EncryptDecrypt(sbyte value, sbyte key)
		{
			if (key == 0)
			{
				return (sbyte)(value ^ cryptoKey);
			}
			return (sbyte)(value ^ key);
		}
		
		public sbyte GetEncrypted()
		{
			ApplyNewCryptoKey();

			return hiddenValue;
		}
		
		public void SetEncrypted(sbyte encrypted)
		{
			inited = true;
			hiddenValue = encrypted;
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				fakeValue = InternalDecrypt();
			}
		}
		
		private sbyte InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = EncryptDecrypt(0);
				fakeValue = 0;
				inited = true;
			}

			sbyte key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			sbyte decrypted = EncryptDecrypt(hiddenValue, key);

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
		
		public static implicit operator SecuredSByte(sbyte value)
		{
			SecuredSByte obscured = new SecuredSByte(EncryptDecrypt(value));
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}
		public static implicit operator sbyte(SecuredSByte value)
		{
			return value.InternalDecrypt();
		}
		
		public static SecuredSByte operator ++(SecuredSByte input)
		{
			sbyte decrypted = (sbyte)(input.InternalDecrypt() + 1);
			input.hiddenValue = EncryptDecrypt(decrypted, input.currentCryptoKey);

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				input.fakeValue = decrypted;
			}
			return input;
		}
		
		public static SecuredSByte operator --(SecuredSByte input)
		{
			sbyte decrypted = (sbyte)(input.InternalDecrypt() - 1);
			input.hiddenValue = EncryptDecrypt(decrypted, input.currentCryptoKey);

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				input.fakeValue = decrypted;
			}
			return input;
		}
		
		public override bool Equals(object obj)
		{
			if (!(obj is SecuredSByte))
				return false;

			SecuredSByte ob = (SecuredSByte)obj;
			return hiddenValue == ob.hiddenValue;
		}
		
		public bool Equals(SecuredSByte obj)
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