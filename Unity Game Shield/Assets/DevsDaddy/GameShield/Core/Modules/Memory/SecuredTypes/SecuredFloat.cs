using System;
using System.Runtime.InteropServices;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes
{
    [System.Serializable]
    public struct SecuredFloat : IEquatable<SecuredFloat>, IFormattable
    {
        private static int cryptoKey = 230887;
        
#if UNITY_EDITOR
        public static int cryptoKeyEditor = cryptoKey;
#endif

        [SerializeField]
		private int currentCryptoKey;

		[SerializeField]
		private byte[] hiddenValue;

		[SerializeField]
		private float fakeValue;

		[SerializeField]
		private bool inited;
		
		private SecuredFloat(byte[] value)
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
				hiddenValue = InternalEncrypt(InternalDecrypt(), cryptoKey);
				currentCryptoKey = cryptoKey;
			}
		}
		
		public static int Encrypt(float value)
		{
			return Encrypt(value, cryptoKey);
		}
		
		public static int Encrypt(float value, int key)
		{
			var u = new FloatIntBytesUnion();
			u.f = value;
			u.i = u.i ^ key;

			return u.i;
		}
		
		private static byte[] InternalEncrypt(float value)
		{
			return InternalEncrypt(value, 0);
		}
		
		private static byte[] InternalEncrypt(float value, int key)
		{
			int currKey = key;
			if (currKey == 0)
			{
				currKey = cryptoKey;
			}

			var u = new FloatIntBytesUnion();
			u.f = value;
			u.i = u.i ^ currKey;

			return new[] { u.b1, u.b2, u.b3, u.b4 };
		}
		
		public static float Decrypt(int value)
		{
			return Decrypt(value, cryptoKey);
		}
		
		public static float Decrypt(int value, int key)
		{
			var u = new FloatIntBytesUnion();
			u.i = value ^ key;
			return u.f;
		}
		
		public int GetEncrypted()
		{
			ApplyNewCryptoKey();

			var union = new FloatIntBytesUnion();
			union.b1 = hiddenValue[0];
			union.b2 = hiddenValue[1];
			union.b3 = hiddenValue[2];
			union.b4 = hiddenValue[3];

			return union.i;
		}
		
		public void SetEncrypted(int encrypted)
		{
			inited = true;
			FloatIntBytesUnion union = new FloatIntBytesUnion();
			union.i = encrypted;

			hiddenValue = new[] { union.b1, union.b2, union.b3, union.b4 };

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				fakeValue = InternalDecrypt();
			}
		}
		
		private float InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = InternalEncrypt(0);
				fakeValue = 0;
				inited = true;
			}

			int key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			var union = new FloatIntBytesUnion();
			union.b1 = hiddenValue[0];
			union.b2 = hiddenValue[1];
			union.b3 = hiddenValue[2];
			union.b4 = hiddenValue[3];

			union.i = union.i ^ key;

			float decrypted = union.f;

            var gameShield = GameShield.Main;
            if (gameShield != null)
            {
                var protector = gameShield.GetModule<MemoryProtector>();
                if (protector != null && fakeValue != 0 && Math.Abs(decrypted - fakeValue) > protector.Config.FloatEpsilon)
                {
                    EventMessenger.Main.Publish(new SecurityWarningPayload
                    {
                        Code = 101,
                        Message = MemoryWarnings.TypeHackWarning,
                        IsCritical = true,
                        Module = protector
                    });
                }
            }

			return decrypted;
		}
		
		[StructLayout(LayoutKind.Explicit)]
		private struct FloatIntBytesUnion
		{
			[FieldOffset(0)]
			public float f;

			[FieldOffset(0)]
			public int i;

			[FieldOffset(0)]
			public byte b1;

			[FieldOffset(1)]
			public byte b2;

			[FieldOffset(2)]
			public byte b3;

			[FieldOffset(3)]
			public byte b4;
		}
		
		public static implicit operator SecuredFloat(float value)
		{
			SecuredFloat obscured = new SecuredFloat(InternalEncrypt(value));
            var gameShield = GameShield.Main;
			if (gameShield != null)
			{
				if (GameShield.Main.GetModule<MemoryProtector>() != null)
				{
					obscured.fakeValue = value;
				}
			}
			return obscured;
		}
		public static implicit operator float(SecuredFloat value)
		{
			return value.InternalDecrypt();
		}
		
		public static SecuredFloat operator ++(SecuredFloat input)
		{
			float decrypted = input.InternalDecrypt() + 1f;
			input.hiddenValue = InternalEncrypt(decrypted, input.currentCryptoKey);

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				input.fakeValue = decrypted;
			}

			return input;
		}
		
		public static SecuredFloat operator --(SecuredFloat input)
		{
			float decrypted = input.InternalDecrypt() - 1f;
			input.hiddenValue = InternalEncrypt(decrypted, input.currentCryptoKey);

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				input.fakeValue = decrypted;
			}

			return input;
		}
		
		public override bool Equals(object obj)
		{
			if (!(obj is SecuredFloat))
				return false;
			SecuredFloat d = (SecuredFloat)obj;
			float dParam = d.InternalDecrypt();
			float dThis = InternalDecrypt();
			if ((double)dParam == (double)dThis)
				return true;
			return float.IsNaN(dParam) && float.IsNaN(dThis);
		}
		
		public bool Equals(SecuredFloat obj)
		{
			float dParam = obj.InternalDecrypt();
			float dThis = InternalDecrypt();


			if ((double)dParam == (double)dThis)
				return true;
			return float.IsNaN(dParam) && float.IsNaN(dThis);
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