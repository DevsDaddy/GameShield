
#if UNITY_EDITOR
namespace PixelSecurity.Editor.PropertyDrawers
{
    using UnityEditor;
    using UnityEngine;
    using DevsDaddy.GameShield.Core.Modules.Memory.SecuredTypes;

    /// <summary>
    /// Secured Int Drawer
    /// </summary>
    [CustomPropertyDrawer(typeof(SecuredInt))]
    public class SecuredIntDrawer : SecuredPropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            SerializedProperty hiddenValue = prop.FindPropertyRelative("hiddenValue");
            SetBoldIfValueOverridePrefab(prop, hiddenValue);

            SerializedProperty cryptoKey = prop.FindPropertyRelative("currentCryptoKey");
            SerializedProperty fakeValue = prop.FindPropertyRelative("fakeValue");
            SerializedProperty inited = prop.FindPropertyRelative("inited");

            int currentCryptoKey = cryptoKey.intValue;
            int val = 0;

            if (!inited.boolValue)
            {
                if (currentCryptoKey == 0)
                {
                    currentCryptoKey = cryptoKey.intValue = SecuredInt.cryptoKeyEditor;
                }
                hiddenValue.intValue = SecuredInt.Encrypt(0, currentCryptoKey);
                inited.boolValue = true;
            }
            else
            {
                val = SecuredInt.Decrypt(hiddenValue.intValue, currentCryptoKey);
            }

            EditorGUI.BeginChangeCheck();
            val = EditorGUI.IntField(position, label, val);
            if (EditorGUI.EndChangeCheck())
                hiddenValue.intValue = SecuredInt.Encrypt(val, currentCryptoKey);

            fakeValue.intValue = val;
        }
    }
}
#endif