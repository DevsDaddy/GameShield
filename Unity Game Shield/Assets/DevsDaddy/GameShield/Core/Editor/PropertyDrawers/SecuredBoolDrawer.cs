
#if UNITY_EDITOR
namespace PixelSecurity.Editor.PropertyDrawers
{
    using UnityEditor;
    using UnityEngine;
    using DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes;

    /// <summary>
    /// Secured Int Drawer
    /// </summary>
    [CustomPropertyDrawer(typeof(SecuredBool))]
    public class SecuredBoolDrawer : SecuredPropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            SerializedProperty hiddenValue = prop.FindPropertyRelative("hiddenValue");
            SetBoldIfValueOverridePrefab(prop, hiddenValue);

            SerializedProperty cryptoKey = prop.FindPropertyRelative("currentCryptoKey");
            SerializedProperty fakeValue = prop.FindPropertyRelative("fakeValue");
            SerializedProperty fakeValueChanged = prop.FindPropertyRelative("fakeValueChanged");
            SerializedProperty inited = prop.FindPropertyRelative("inited");

            int currentCryptoKey = cryptoKey.intValue;
            bool val = false;

            if (!inited.boolValue)
            {
                if (currentCryptoKey == 0)
                {
                    currentCryptoKey = cryptoKey.intValue = SecuredBool.cryptoKeyEditor;
                }
                inited.boolValue = true;
                hiddenValue.intValue = SecuredBool.Encrypt(val, (byte)currentCryptoKey);
            }
            else
            {
                val = SecuredBool.Decrypt(hiddenValue.intValue, (byte)currentCryptoKey);
            }

            EditorGUI.BeginChangeCheck();
            val = EditorGUI.Toggle(position, label, val);
            if (EditorGUI.EndChangeCheck())
                hiddenValue.intValue = SecuredBool.Encrypt(val, (byte)currentCryptoKey);

            fakeValue.boolValue = val;
            fakeValueChanged.boolValue = true;
        }
    }
}
#endif