using System;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes
{
    /// <summary>
    /// Secured String Type
    /// </summary>
    [System.Serializable]
    public sealed class SecuredString
    {
        private static string cryptoKey = "4441";
        
#if UNITY_EDITOR
        public static string cryptoKeyEditor = cryptoKey;
#endif
        
        // Serialized Params
        [SerializeField] private string currentCryptoKey;
        [SerializeField] private byte[] hiddenValue;
        [SerializeField] private string fakeValue;
        [SerializeField] private bool inited;

        
        // for serialization purposes
		private SecuredString(){}
		
		private SecuredString(byte[] value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = null;
			inited = true;
		}
		
		public static void SetNewCryptoKey(string newKey)
		{
			cryptoKey = newKey;
		}
		
		public void ApplyNewCryptoKey()
		{
			if (currentCryptoKey != cryptoKey)
			{
				hiddenValue = InternalEncrypt(InternalDecrypt());
				currentCryptoKey = cryptoKey;
			}
		}
		
		public static string EncryptDecrypt(string value)
		{
			return EncryptDecrypt(value, "");
		}
		
		public static string EncryptDecrypt(string value, string key)
		{
			if (string.IsNullOrEmpty(value))
			{
				return "";
			}

			if (string.IsNullOrEmpty(key))
			{
				key = cryptoKey;
			}

			int keyLength = key.Length;
			int valueLength = value.Length;

			char[] result = new char[valueLength];

			for (int i = 0; i < valueLength; i++)
			{
				result[i] = (char)(value[i] ^ key[i % keyLength]);
			}

			return new string(result);
		}
		
		public string GetEncrypted()
		{
			ApplyNewCryptoKey();
			return GetString(hiddenValue);
		}
		
		public void SetEncrypted(string encrypted)
		{
			inited = true;
			hiddenValue = GetBytes(encrypted);
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				fakeValue = InternalDecrypt();
			}
		}
		
		private static byte[] InternalEncrypt(string value)
		{
			return GetBytes(EncryptDecrypt(value, cryptoKey));
		}
		
		private string InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = InternalEncrypt("");
				fakeValue = "";
				inited = true;
			}

			string key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			if (string.IsNullOrEmpty(key))
			{
				key = cryptoKey;
			}

			string result = EncryptDecrypt(GetString(hiddenValue), key);

			MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
			if (protector!=null && !string.IsNullOrEmpty(fakeValue) && result != fakeValue)
			{
				EventMessenger.Main.Publish(new SecurityWarningPayload {
					Code = 101,
					Message = MemoryWarnings.TypeHackWarning,
					IsCritical = true,
					Module = protector
				});
			}

			return result;
		}
		
		public static implicit operator SecuredString(string value)
		{
			if (value == null)
			{
				return null;
			}

			SecuredString obscured = new SecuredString(InternalEncrypt(value));
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}
		public static implicit operator string(SecuredString value)
		{
			if (value == null)
			{
				return null;
			}
			return value.InternalDecrypt();
		}
		
		public override string ToString()
		{
			return InternalDecrypt();
		}
		
		public static bool operator ==(SecuredString a, SecuredString b)
		{
			if (ReferenceEquals(a, b))
			{
				return true;
			}

			if (((object)a == null) || ((object)b == null))
			{
				return false;
			}

			return ArraysEquals(a.hiddenValue, b.hiddenValue);
		}
		
		public static bool operator !=(SecuredString a, SecuredString b)
		{
			return !(a == b);
		}
		
		public override bool Equals(object obj)
		{
			SecuredString strA = obj as SecuredString;
			string strB = null;
			if (strA != null) strB = GetString(strA.hiddenValue);

			return string.Equals(hiddenValue, strB);
		}
		
		public bool Equals(SecuredString value)
		{
			byte[] a = null;
			if (value != null) a = value.hiddenValue;

			return ArraysEquals(hiddenValue, a);
		}
		
		public bool Equals(SecuredString value, StringComparison comparisonType)
		{
			string strA = null;
			if (value != null) strA = value.InternalDecrypt();

			return string.Equals(InternalDecrypt(), strA, comparisonType);
		}
		
		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}

		static byte[] GetBytes(string str)
		{
			byte[] bytes = new byte[str.Length * sizeof(char)];
			System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}

		static string GetString(byte[] bytes)
		{
			char[] chars = new char[bytes.Length / sizeof(char)];
			System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
			return new string(chars);
		}

		static bool ArraysEquals(byte[] a1, byte[] a2)
		{
			if (a1 == a2)
			{
				return true;
			}

			if ((a1 != null) && (a2 != null))
			{
				if (a1.Length != a2.Length)
				{
					return false;
				}
				for (int i = 0; i < a1.Length; i++)
				{
					if (a1[i] != a2[i])
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}
    }
}