/*
 * Pixel Anti Cheat
 * ======================================================
 * This library allows you to organize a simple anti-cheat
 * for your game and take care of data security. You can
 * use it in your projects for free.
 *
 * Note that it does not guarantee 100% protection for
 * your game. If you are developing a multiplayer game -
 * never trust the client and check everything on
 * the server.
 * ======================================================
 * @developer       TinyPlay
 * @author          Ilya Rastorguev
 * @url             https://github.com/TinyPlay/Pixel-Anticheat
 */
namespace PixelSecurity.Editor.PropertyDrawers
{
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
}