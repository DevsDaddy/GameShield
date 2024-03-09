using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes
{
    /// <summary>
    /// Secured Boolean Type
    /// </summary>
    [System.Serializable]
    public struct SecuredBool
    {
        // Crypto Key
        private static byte cryptoKey = 215;
        
#if UNITY_EDITOR
        public static byte cryptoKeyEditor = cryptoKey;
#endif
        
        // Serialized Fields
        [SerializeField] private byte currentCryptoKey;
        [SerializeField] private int hiddenValue;
        [SerializeField] private bool fakeValue;
        [SerializeField] private bool fakeValueChanged;
        [SerializeField] private bool inited;
        
        private SecuredBool(int value)
        {
            currentCryptoKey = cryptoKey;
            hiddenValue = value;
            fakeValue = false;
            fakeValueChanged = false;
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
                hiddenValue = Encrypt(InternalDecrypt(), cryptoKey);
                currentCryptoKey = cryptoKey;
            }
        }
        
        public static int Encrypt(bool value)
        {
            return Encrypt(value, 0);
        }
        
        public static int Encrypt(bool value, byte key)
        {
            if (key == 0)
            {
                key = cryptoKey;
            }

            int encryptedValue = value ? 213 : 181;

            encryptedValue ^= key;

            return encryptedValue;
        }
        
        public static bool Decrypt(int value)
        {
            return Decrypt(value, 0);
        }
        
        public static bool Decrypt(int value, byte key)
        {
            if (key == 0)
            {
                key = cryptoKey;
            }

            value ^= key;

            return value != 181;
        }
        
        public int GetEncrypted()
        {
            ApplyNewCryptoKey();
            return hiddenValue;
        }
        
        public void SetEncrypted(int encrypted)
        {
            inited = true;
            hiddenValue = encrypted;
            if (GameShield.Main.GetModule<MemoryProtector>() != null)
            {
                fakeValue = InternalDecrypt();
                fakeValueChanged = true;
            }
        }
        
        private bool InternalDecrypt()
        {
            if (!inited)
            {
                currentCryptoKey = cryptoKey;
                hiddenValue = Encrypt(false);
                fakeValue = false;
                fakeValueChanged = true;
                inited = true;
            }

            byte key = cryptoKey;

            if (currentCryptoKey != cryptoKey)
            {
                key = currentCryptoKey;
            }

            int value = hiddenValue;
            value ^= key;

            bool decrypted = value != 181;

            MemoryProtector protector = GameShield.Main.GetModule<MemoryProtector>();
            if (protector != null && fakeValueChanged && decrypted != fakeValue) {
                EventMessenger.Main.Publish(new SecurityWarningPayload {
                    Code = 101,
                    Message = MemoryWarnings.TypeHackWarning,
                    IsCritical = true,
                    Module = protector
                });
            }

            return decrypted;
        }
        
        public static implicit operator SecuredBool(bool value)
        {
            SecuredBool obscured = new SecuredBool(Encrypt(value));
            
            if (GameShield.Main.GetModule<MemoryProtector>() != null)
            {
                obscured.fakeValue = value;
                obscured.fakeValueChanged = true;
            }

            return obscured;
        }
        public static implicit operator bool(SecuredBool value)
        {
            return value.InternalDecrypt();
        }
        
        public override bool Equals(object obj)
        {
            if (!(obj is SecuredBool))
                return false;

            SecuredBool oi = (SecuredBool)obj;
            return (hiddenValue == oi.hiddenValue);
        }
        
        public bool Equals(SecuredBool obj)
        {
            return hiddenValue == obj.hiddenValue;
        }
        
        public override int GetHashCode()
        {
            return InternalDecrypt().GetHashCode();
        }
        
        public override string ToString()
        {
            return InternalDecrypt().ToString();
        }
    }
}