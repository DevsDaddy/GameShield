#if UNITY_EDITOR
namespace PixelSecurity.Editor.PropertyDrawers
{
    using UnityEditor;
    using UnityEngine;
    using DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Secured Int Drawer
    /// </summary>
    [CustomPropertyDrawer(typeof(SecuredFloat))]
    public class SecuredVector3Drawer : SecuredPropertyDrawer
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
                    currentCryptoKey = cryptoKey.intValue = SecuredVector3.cryptoKeyEditor;
                }
                hiddenValue.arraySize = 4;
                inited.boolValue = true;

                union.i = SecuredFloat.Encrypt(0, currentCryptoKey);

                hiddenValue.GetArrayElementAtIndex(0).intValue = union.b1;
                hiddenValue.GetArrayElementAtIndex(1).intValue = union.b2;
                hiddenValue.GetArrayElementAtIndex(2).intValue = union.b3;
                hiddenValue.GetArrayElementAtIndex(3).intValue = union.b4;
            }
            else
            {
                int arraySize = hiddenValue.arraySize;
                byte[] hiddenValueArray = new byte[arraySize];
                for (int i = 0; i < arraySize; i++)
                {
                    hiddenValueArray[i] = (byte)hiddenValue.GetArrayElementAtIndex(i).intValue;
                }

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

                hiddenValue.GetArrayElementAtIndex(0).intValue = union.b1;
                hiddenValue.GetArrayElementAtIndex(1).intValue = union.b2;
                hiddenValue.GetArrayElementAtIndex(2).intValue = union.b3;
                hiddenValue.GetArrayElementAtIndex(3).intValue = union.b4;
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