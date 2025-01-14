#if UNITY_EDITOR
namespace PixelSecurity.Editor.PropertyDrawers
{
    using UnityEditor;
    using UnityEngine;
    using System.Runtime.InteropServices;
    using DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes;

    /// <summary>
    /// Secured Int Drawer
    /// </summary>
    [CustomPropertyDrawer(typeof(SecuredFloat))]
    public class SecuredFloatDrawer : SecuredPropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            SerializedProperty hiddenValue = prop.FindPropertyRelative("hiddenValue");
            SetBoldIfValueOverridePrefab(prop, hiddenValue);

            SerializedProperty cryptoKey = prop.FindPropertyRelative("currentCryptoKey");
            SerializedProperty fakeValue = prop.FindPropertyRelative("fakeValue");
            SerializedProperty inited = prop.FindPropertyRelative("inited");

            int currentCryptoKey = cryptoKey.intValue;

            IntBytesUnion union = new IntBytesUnion();
            float val = 0;

            if (!inited.boolValue)
            {
                if (currentCryptoKey == 0)
                {
                    currentCryptoKey = cryptoKey.intValue = SecuredFloat.cryptoKeyEditor;
                }
                hiddenValue.arraySize = 4;  // Ensure the array has 4 elements
                inited.boolValue = true;

                union.i = SecuredFloat.Encrypt(0, currentCryptoKey);

                // Initialize the array with the encrypted values
                hiddenValue.GetArrayElementAtIndex(0).intValue = (int)union.b1;
                hiddenValue.GetArrayElementAtIndex(1).intValue = (int)union.b2;
                hiddenValue.GetArrayElementAtIndex(2).intValue = (int)union.b3;
                hiddenValue.GetArrayElementAtIndex(3).intValue = (int)union.b4;
            }
            else
            {
                int arraySize = hiddenValue.arraySize;

                // Make sure there are at least 4 elements in the array to avoid out-of-bounds errors
                if (arraySize < 4)
                {
                    // If the array size is less than 4, initialize it to 4 elements
                    hiddenValue.arraySize = 4;
                }

                byte[] hiddenValueArray = new byte[4];  // Initialize the array with 4 elements
                for (int i = 0; i < 4; i++)
                {
                    hiddenValueArray[i] = (byte)hiddenValue.GetArrayElementAtIndex(i).intValue; // Cast to byte
                }

                // Assign values to the union
                union.b1 = hiddenValueArray[0];
                union.b2 = hiddenValueArray[1];
                union.b3 = hiddenValueArray[2];
                union.b4 = hiddenValueArray[3];

                val = SecuredFloat.Decrypt(union.i, currentCryptoKey);
            }


            EditorGUI.BeginChangeCheck();
            val = EditorGUI.FloatField(position, label, val);
            if (EditorGUI.EndChangeCheck())
            {
                union.i = SecuredFloat.Encrypt(val, currentCryptoKey);

                // Store the encrypted bytes back into the hiddenValue array
                hiddenValue.GetArrayElementAtIndex(0).intValue = (int)union.b1;
                hiddenValue.GetArrayElementAtIndex(1).intValue = (int)union.b2;
                hiddenValue.GetArrayElementAtIndex(2).intValue = (int)union.b3;
                hiddenValue.GetArrayElementAtIndex(3).intValue = (int)union.b4;
            }

            fakeValue.floatValue = val;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct IntBytesUnion
        {
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
    }
}
#endif
