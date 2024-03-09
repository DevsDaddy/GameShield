using System;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes
{
    /// <summary>
    /// Secured Char Type
    /// </summary>
    [System.Serializable]
    public struct SecuredChar : IEquatable<SecuredChar>
    {
        private static char cryptoKey = '\x2014';
        
#if UNITY_EDITOR
        public static char cryptoKeyEditor = cryptoKey;
#endif
        
        [SerializeField] private char currentCryptoKey;
        [SerializeField] private char hiddenValue;
        [SerializeField] private char fakeValue;
        [SerializeField] private bool inited;
        
        private SecuredChar(char value)
        {
            currentCryptoKey = cryptoKey;
            hiddenValue = value;
            fakeValue = '\0';
            inited = true;
        }
        
        public static void SetNewCryptoKey(char newKey)
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
		
		public static char EncryptDecrypt(char value)
		{
			return EncryptDecrypt(value, '\0');
		}
		
		public static char EncryptDecrypt(char value, char key)
		{
			if (key == '\0')
			{
				return (char)(value ^ cryptoKey);
			}
			return (char)(value ^ key);
		}
		
		public char GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}
		
		public void SetEncrypted(char encrypted)
		{
			inited = true;
			hiddenValue = encrypted;
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				fakeValue = InternalDecrypt();
			}
		}
		
		private char InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = EncryptDecrypt('\0');
				fakeValue = '\0';
				inited = true;
			}

			char key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			char decrypted = EncryptDecrypt(hiddenValue, key);

			MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
			if (protector != null && fakeValue != '\0' && decrypted != fakeValue)
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
		
		public static implicit operator SecuredChar(char value)
		{
			SecuredChar obscured = new SecuredChar(EncryptDecrypt(value));
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}

		public static implicit operator char(SecuredChar value)
		{
			return value.InternalDecrypt();
		}
		
		public static SecuredChar operator ++(SecuredChar input)
		{
			char decrypted = (char)(input.InternalDecrypt() + 1);
			input.hiddenValue = EncryptDecrypt(decrypted, input.currentCryptoKey);

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				input.fakeValue = decrypted;
			}
			return input;
		}
		
		public static SecuredChar operator --(SecuredChar input)
		{
			char decrypted = (char)(input.InternalDecrypt() - 1);
			input.hiddenValue = EncryptDecrypt(decrypted, input.currentCryptoKey);

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				input.fakeValue = decrypted;
			}
			return input;
		}
		
		public override bool Equals(object obj)
		{
			if (!(obj is SecuredChar))
				return false;

			SecuredChar ob = (SecuredChar)obj;
			return hiddenValue == ob.hiddenValue;
		}
		
		public bool Equals(SecuredChar obj)
		{
			return hiddenValue == obj.hiddenValue;
		}
		
		public override string ToString()
		{
			return InternalDecrypt().ToString();
		}

		#if !UNITY_WINRT
	    public string ToString(IFormatProvider provider)
		{
			return InternalDecrypt().ToString(provider);
		}
		#endif
	    
		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}
    }
}