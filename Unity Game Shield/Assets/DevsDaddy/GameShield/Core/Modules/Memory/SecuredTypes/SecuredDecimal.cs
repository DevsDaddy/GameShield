using System;
using System.Runtime.InteropServices;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes
{
    /// <summary>
    /// Secured Decimal Type
    /// </summary>
    [System.Serializable]
    public struct SecuredDecimal : IEquatable<SecuredDecimal>, IFormattable
    {
        private static long cryptoKey = 209208L;
        
#if UNITY_EDITOR
        public static long cryptoKeyEditor = cryptoKey;
#endif
        
	    [SerializeField] private long currentCryptoKey;
	    [SerializeField] private byte[] hiddenValue;
        [SerializeField] private decimal fakeValue;
        [SerializeField] private bool inited;
        
        private SecuredDecimal(byte[] value)
        {
            currentCryptoKey = cryptoKey;
            hiddenValue = value;
            fakeValue = 0m;
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
				hiddenValue = InternalEncrypt(InternalDecrypt(), cryptoKey);
				currentCryptoKey = cryptoKey;
			}
		}
		
		public static decimal Encrypt(decimal value)
		{
			return Encrypt(value, cryptoKey);
		}
		
		public static decimal Encrypt(decimal value, long key)
		{
			var u = new DecimalLongBytesUnion();
			u.d = value;
			u.l1 = u.l1 ^ key;
			u.l2 = u.l2 ^ key;

			return u.d;
		}
		
		private static byte[] InternalEncrypt(decimal value)
		{
			return InternalEncrypt(value, 0L);
		}
		
		private static byte[] InternalEncrypt(decimal value, long key)
		{
			long currKey = key;
			if (currKey == 0L)
			{
				currKey = cryptoKey;
			}

			DecimalLongBytesUnion union = new DecimalLongBytesUnion();
			union.d = value;
			union.l1 = union.l1 ^ currKey;
			union.l2 = union.l2 ^ currKey;

			return new[]
			{
				union.b1, union.b2, union.b3, union.b4, union.b5, union.b6, union.b7, union.b8,
				union.b9, union.b10, union.b11, union.b12, union.b13, union.b14, union.b15, union.b16
			};
		}
		
		public static decimal Decrypt(decimal value)
		{
			return Decrypt(value, cryptoKey);
		}
		
		public static decimal Decrypt(decimal value, long key)
		{
			DecimalLongBytesUnion u = new DecimalLongBytesUnion();
			u.d = value;
			u.l1 = u.l1 ^ key;
			u.l2 = u.l2 ^ key;
			return u.d;
		}
		
		public decimal GetEncrypted()
		{
			ApplyNewCryptoKey();

			DecimalLongBytesUnion union = new DecimalLongBytesUnion();
			union.b1 = hiddenValue[0];
			union.b2 = hiddenValue[1];
			union.b3 = hiddenValue[2];
			union.b4 = hiddenValue[3];
			union.b5 = hiddenValue[4];
			union.b6 = hiddenValue[5];
			union.b7 = hiddenValue[6];
			union.b8 = hiddenValue[7];
			union.b9 = hiddenValue[8];
			union.b10 = hiddenValue[9];
			union.b11 = hiddenValue[10];
			union.b12 = hiddenValue[11];
			union.b13 = hiddenValue[12];
			union.b14 = hiddenValue[13];
			union.b15 = hiddenValue[14];
			union.b16 = hiddenValue[15];

			return union.d;
		}
		
		public void SetEncrypted(decimal encrypted)
		{
			inited = true;
			DecimalLongBytesUnion union = new DecimalLongBytesUnion();
			union.d = encrypted;

			hiddenValue = new[]
			{
				union.b1, union.b2, union.b3, union.b4, union.b5, union.b6, union.b7, union.b8,
				union.b9, union.b10, union.b11, union.b12, union.b13, union.b14, union.b15, union.b16
			};

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				fakeValue = InternalDecrypt();
			}
		}
		
		private decimal InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = InternalEncrypt(0m);
				fakeValue = 0m;
				inited = true;
			}

			long key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			DecimalLongBytesUnion union = new DecimalLongBytesUnion();
			union.b1 = hiddenValue[0];
			union.b2 = hiddenValue[1];
			union.b3 = hiddenValue[2];
			union.b4 = hiddenValue[3];
			union.b5 = hiddenValue[4];
			union.b6 = hiddenValue[5];
			union.b7 = hiddenValue[6];
			union.b8 = hiddenValue[7];
			union.b9 = hiddenValue[8];
			union.b10 = hiddenValue[9];
			union.b11 = hiddenValue[10];
			union.b12 = hiddenValue[11];
			union.b13 = hiddenValue[12];
			union.b14 = hiddenValue[13];
			union.b15 = hiddenValue[14];
			union.b16 = hiddenValue[15];

			union.l1 = union.l1 ^ key;
			union.l2 = union.l2 ^ key;

			decimal decrypted = union.d;
			
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
		
		[StructLayout(LayoutKind.Explicit)]
		private struct DecimalLongBytesUnion
		{
			[FieldOffset(0)]
			public decimal d;

			[FieldOffset(0)]
			public long l1;

			[FieldOffset(8)]
			public long l2;

			[FieldOffset(0)]
			public byte b1;

			[FieldOffset(1)]
			public byte b2;

			[FieldOffset(2)]
			public byte b3;

			[FieldOffset(3)]
			public byte b4;

			[FieldOffset(4)]
			public byte b5;

			[FieldOffset(5)]
			public byte b6;

			[FieldOffset(6)]
			public byte b7;

			[FieldOffset(7)]
			public byte b8;

			[FieldOffset(8)]
			public byte b9;

			[FieldOffset(9)]
			public byte b10;

			[FieldOffset(10)]
			public byte b11;

			[FieldOffset(11)]
			public byte b12;

			[FieldOffset(12)]
			public byte b13;

			[FieldOffset(13)]
			public byte b14;

			[FieldOffset(14)]
			public byte b15;

			[FieldOffset(15)]
			public byte b16;
		}
		
		public static implicit operator SecuredDecimal(decimal value)
		{
			SecuredDecimal obscured = new SecuredDecimal(InternalEncrypt(value));
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}
		public static implicit operator decimal(SecuredDecimal value)
		{
			return value.InternalDecrypt();
		}
		
		public static SecuredDecimal operator ++(SecuredDecimal input)
		{
			decimal decrypted = input.InternalDecrypt() + 1m;
			input.hiddenValue = InternalEncrypt(decrypted, input.currentCryptoKey);

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				input.fakeValue = decrypted;
			}

			return input;
		}
		
		public static SecuredDecimal operator --(SecuredDecimal input)
		{
			decimal decrypted = input.InternalDecrypt() - 1m;
			input.hiddenValue = InternalEncrypt(decrypted, input.currentCryptoKey);

			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				input.fakeValue = decrypted;
			}
			return input;
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
		
		public override bool Equals(object obj)
		{
			if (!(obj is SecuredDecimal))
				return false;
			SecuredDecimal d = (SecuredDecimal)obj;
			return d.InternalDecrypt().Equals(InternalDecrypt());
		}
		
		public bool Equals(SecuredDecimal obj)
		{
			return obj.InternalDecrypt().Equals(InternalDecrypt());
		}
		
		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}
    }
}