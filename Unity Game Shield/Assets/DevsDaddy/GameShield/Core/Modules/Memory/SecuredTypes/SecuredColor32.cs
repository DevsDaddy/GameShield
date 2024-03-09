using System;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes
{
    /// <summary>
    /// Secured Color32 Type
    /// </summary>
    [System.Serializable]
    public struct SecuredColor32
    {
        private static int cryptoKey = 120223;
        private static readonly Color32 initialFakeValue = new Color32(0,0,0,1);
        
        #if UNITY_EDITOR
        public static int cryptoKeyEditor = cryptoKey;
        #endif
        
        // Serialized Fields
        [SerializeField] private int currentCryptoKey;
        [SerializeField] private RawEncryptedColor32 hiddenValue;
        [SerializeField] private Color32 fakeValue;
        [SerializeField] private bool inited;
        
        private SecuredColor32(RawEncryptedColor32 encrypted)
        {
            currentCryptoKey = cryptoKey;
            hiddenValue = encrypted;
            fakeValue = initialFakeValue;
            inited = true;
        }
        
        public byte r
        {
            get
            {
	            MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
                byte decrypted = InternalDecryptField(hiddenValue.r);
                if (protector != null && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.r) > protector.Config.Color32Epsilon)
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

            set
            {
	            hiddenValue.r = InternalEncryptField(value);
                if (GameShield.Main.GetModule<MemoryProtector>() != null)
                {
                    fakeValue.r = value;
                }
            }
        }
        
        public byte g
        {
            get
            {
	            MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
                byte decrypted = InternalDecryptField(hiddenValue.g);
                if (protector != null && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.g) > protector.Config.Color32Epsilon)
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

            set
            {
	            hiddenValue.g = InternalEncryptField(value);
                if (GameShield.Main.GetModule<MemoryProtector>() != null)
                {
                    fakeValue.g = value;
                }
            }
        }
        
        public byte b
        {
            get
            {
	            MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
                byte decrypted = InternalDecryptField(hiddenValue.b);
                if (protector != null && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.b) > protector.Config.Color32Epsilon)
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

            set
            {
	            hiddenValue.b = InternalEncryptField(value);
                if (GameShield.Main.GetModule<MemoryProtector>() != null)
                {
                    fakeValue.b = value;
                }
            }
        }
        
        public byte a
        {
            get
            {
	            MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
                byte decrypted = InternalDecryptField(hiddenValue.a);
                if (protector != null && !fakeValue.Equals(initialFakeValue) && Math.Abs(decrypted - fakeValue.a) > protector.Config.Color32Epsilon)
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

            set
            {
	            hiddenValue.a = InternalEncryptField(value);
                if (GameShield.Main.GetModule<MemoryProtector>() != null)
                {
                    fakeValue.a = value;
                }
            }
        }
        
        public byte this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return r;
                    case 1:
                        return g;
                    case 2:
                        return b;
                    case 3:
                        return a;
                    default:
                        throw new IndexOutOfRangeException("Invalid SecuredColor32 index!");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        r = value;
                        break;
                    case 1:
                        g = value;
                        break;
                    case 2:
                        b = value;
                        break;
                    case 3:
                        a = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid SecuredColor32 index!");
                }
            }
        }
        
		public static void SetNewCryptoKey(int newKey)
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
		
		public static RawEncryptedColor32 Encrypt(Color32 value)
		{
			return Encrypt(value, 0);
		}
		
		public static RawEncryptedColor32 Encrypt(Color32 value, int key)
		{
			if (key == 0)
			{
				key = cryptoKey;
			}

			RawEncryptedColor32 result;
			result.r = SecuredByte.EncryptDecrypt((byte)value.r, (byte)key);
			result.g = SecuredByte.EncryptDecrypt((byte)value.g, (byte)key);
			result.b = SecuredByte.EncryptDecrypt((byte)value.b, (byte)key);
			result.a = SecuredByte.EncryptDecrypt((byte)value.a, (byte)key);

			return result;
		}
		
		public static Color32 Decrypt(RawEncryptedColor32 value)
		{
			return Decrypt(value, 0);
		}
		
		public static Color32 Decrypt(RawEncryptedColor32 value, int key)
		{
			if (key == 0)
			{
				key = cryptoKey;
			}

			Color32 result = new Color32();
			result.r = SecuredByte.EncryptDecrypt((byte)value.r, (byte)key);
			result.g = SecuredByte.EncryptDecrypt((byte)value.g, (byte)key);
			result.b = SecuredByte.EncryptDecrypt((byte)value.b, (byte)key);
			result.a = SecuredByte.EncryptDecrypt((byte)value.a, (byte)key);

			return result;
		}
		
		public RawEncryptedColor32 GetEncrypted()
		{
			ApplyNewCryptoKey();
			return hiddenValue;
		}
		
		public void SetEncrypted(RawEncryptedColor32 encrypted)
		{
			inited = true;
			hiddenValue = encrypted;
			if (GameShield.Main.GetModule<MemoryProtector>() != null)
			{
				fakeValue = InternalDecrypt();
			}
		}
		
		private Color32 InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = Encrypt(initialFakeValue, cryptoKey);
				fakeValue = initialFakeValue;
				inited = true;
			}

			int key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			Color32 value = new Color32();

			value.r = SecuredByte.EncryptDecrypt((byte)hiddenValue.r, (byte)key);
			value.g = SecuredByte.EncryptDecrypt((byte)hiddenValue.g, (byte)key);
			value.b = SecuredByte.EncryptDecrypt((byte)hiddenValue.b, (byte)key);
			value.a = SecuredByte.EncryptDecrypt((byte)hiddenValue.a, (byte)key);

			MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
			if (protector != null && !fakeValue.Equals(Color.black) && !CompareVectorsWithTolerance(value, fakeValue))
			{
				EventMessenger.Main.Publish(new SecurityWarningPayload {
					Code = 101,
					Message = MemoryWarnings.TypeHackWarning,
					IsCritical = true,
					Module = protector
				});
			}

			return value;
		}
		
		private bool CompareVectorsWithTolerance(Color32 color1, Color32 color2)
		{
			MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
			float epsilon = (protector != null) ? protector.Config.Color32Epsilon : 1;
			return Math.Abs(color1.r - color2.r) < epsilon &&
				   Math.Abs(color1.g - color2.g) < epsilon &&
				   Math.Abs(color1.b - color2.b) < epsilon &&
				   Math.Abs(color1.a - color2.a) < epsilon;
		}
		
		private byte InternalDecryptField(int encrypted)
		{
			int key = cryptoKey;

			if (currentCryptoKey != cryptoKey)
			{
				key = currentCryptoKey;
			}

			byte result = SecuredByte.EncryptDecrypt((byte)encrypted, (byte)key);
			return result;
		}
		
		private int InternalEncryptField(float encrypted)
		{
			int result = SecuredFloat.Encrypt(encrypted, cryptoKey);
			return result;
		}
		
		public override bool Equals(object other)
		{
			return InternalDecrypt().Equals(other);
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
		
		[System.Serializable]
		public struct RawEncryptedColor32
		{
			public int r;
			public int g;
			public int b;
			public int a;
		}
    }
}