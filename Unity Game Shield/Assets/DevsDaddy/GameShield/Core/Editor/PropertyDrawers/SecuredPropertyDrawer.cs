
    using System.Reflection;
    using UnityEditor;
    
    /// <summary>
    /// Secured Property Drawer
    /// </summary>
    public class SecuredPropertyDrawer : PropertyDrawer
    {
        protected MethodInfo boldFontMethodInfo = null;
        
        protected void SetBoldIfValueOverridePrefab(SerializedProperty parentProperty, SerializedProperty valueProperty)
        {
            if (parentProperty.isInstantiatedPrefab)
            {
                SetBoldDefaultFont(valueProperty.prefabOverride);
            }
        }

        protected void SetBoldDefaultFont(bool value)
        {
            if (boldFontMethodInfo == null)
            {
                boldFontMethodInfo = typeof(EditorGUIUtility).GetMethod("SetBoldDefaultFont", BindingFlags.Static | BindingFlags.NonPublic);
            }
            boldFontMethodInfo.Invoke(null, new[] { value as object });
        }
    }
