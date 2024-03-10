using System;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes
{
    /// <summary>
    /// Secured Short Type
    /// </summary>
    [System.Serializable]
    public struct SecuredShort : IEquatable<SecuredShort>, IFormattable
    {
        private static short cryptoKey = 214;
        
#if UNITY_EDITOR
        public static short cryptoKeyEditor = cryptoKey;
#endif
        
	    [SerializeField] private short currentCryptoKey;
	    [SerializeField] private short hiddenValue;
	    [SerializeField] private short fakeValue;
	    [SerializeField] private bool inited;
	    
		private SecuredShort(short value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = 0;
			inited = true;
		}
		
		public static void SetNewCryptoKey(short newKey)
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
		
		public static short EncryptDecrypt(short value)
		{
			return EncryptDecrypt(value, 0);
		}
		
		public static short EncryptDecrypt(short value, short key)
		{
			if (key == 0)
			{
				return (short)(value ^ cryptoKey);
			}
			return (short)(value ^ key);
		}
		
		public short GetEncrypted()
		{
			ApplyNewCryptoKey();

			return hiddenValue;
		}
		
		public void SetEncrypted(short encrypted)
		{
			inited = true;
			hiddenValue = encrypted;
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				fakeValue = InternalDecrypt();
			}
		}
		
		private short InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = EncryptDecrypt(0);
				fakeValue = 0;
				inited = true;
			}

			short key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			short decrypted = EncryptDecrypt(hiddenValue, key);

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
		
		public static implicit operator SecuredShort(short value)
		{
			SecuredShort obscured = new SecuredShort(EncryptDecrypt(value));
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}
		public static implicit operator short(SecuredShort value)
		{
			return value.InternalDecrypt();
		}
		
		public static SecuredShort operator ++(SecuredShort input)
		{
			short decrypted = (short)(input.InternalDecrypt() + 1);
			input.hiddenValue = EncryptDecrypt(decrypted);

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				input.fakeValue = decrypted;
			}
			return input;
		}
		
		public static SecuredShort operator --(SecuredShort input)
		{
			short decrypted = (short)(input.InternalDecrypt() - 1);
			input.hiddenValue = EncryptDecrypt(decrypted);

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				input.fakeValue = decrypted;
			}
			return input;
		}
		
		public override bool Equals(object obj)
		{
			if (!(obj is SecuredShort))
				return false;
			
			SecuredShort ob = (SecuredShort)obj;
			return hiddenValue == ob.hiddenValue;
		}
		
		public bool Equals(SecuredShort obj)
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