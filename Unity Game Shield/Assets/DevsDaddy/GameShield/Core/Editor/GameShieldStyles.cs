using UnityEngine;

namespace DevsDaddy.GameShield.Core.Editor
{
    internal class GameShieldStyles
    {
        /// <summary>
        /// Get Header Style
        /// </summary>
        /// <returns></returns>
        public GUIStyle GetHeaderStyle() {
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 16;
            style.padding = new RectOffset(20, 20, 10, 10);
            style.normal.background = MakeTex( 2, 2, Color.white);
            return style;
        }

        /// <summary>
        /// Get Footer Button Style
        /// </summary>
        /// <returns></returns>
        public GUIStyle GetFooterButtonStyle(bool isBack = false) {
            Color[] normalColors = {new Color(0f, 0.54f, 0.77f, 1f), new Color(0f, 0.3f, 0.77f, 1f) };
            Color[] backColors = {new Color(0.8f, 0, 0.2f, 1f), new Color(0.6f, 0, 0.2f, 1f)};
            
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.MiddleCenter;
            style.padding = new RectOffset(20, 20, 10, 10);
            style.margin = new RectOffset(10, 10, 10, 10);
            style.border = new RectOffset(0, 0, 0, 0);
            style.fontSize = 14;
            style.fontStyle = (isBack) ? FontStyle.Normal : FontStyle.Bold;
            style.normal.background = MakeTex( 2, 2, isBack ? backColors[0] : normalColors[0]);
            style.hover.background = MakeTex( 2, 2, isBack ? backColors[1] : normalColors[1]);
            style.focused.background = MakeTex( 2, 2, isBack ? backColors[0] : normalColors[0]);
            style.normal.textColor = Color.white;
            style.focused.textColor = Color.white;
            style.hover.textColor = Color.white;
            return style;
        }
        
        /// <summary>
        /// Get Footer Button Style
        /// </summary>
        /// <returns></returns>
        public GUIStyle GetBasicButtonSyle() {
            Color[] normalColors = {new Color(0.6f, 0.6f, 0.8f, 1f), new Color(0.6f, 0.9f, 0.8f, 1f) };
            
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.MiddleCenter;
            style.padding = new RectOffset(20, 20, 10, 10);
            style.margin = new RectOffset(10, 10, 10, 10);
            style.border = new RectOffset(0, 0, 0, 0);
            style.fontSize = 14;
            style.fontStyle = FontStyle.Normal;
            style.normal.background = MakeTex( 2, 2, normalColors[0]);
            style.hover.background = MakeTex( 2, 2, normalColors[1]);
            style.focused.background = MakeTex( 2, 2, normalColors[0]);
            style.normal.textColor = Color.white;
            style.focused.textColor = Color.white;
            style.hover.textColor = Color.white;
            return style;
        }

        /// <summary>
        /// Get Footer Area Style
        /// </summary>
        /// <returns></returns>
        public GUIStyle GetFooterAreaStyle() {
            GUIStyle style = new GUIStyle();
            style.normal.background = MakeTex( 2, 2, Color.white);
            return style;
        }
        
        /// <summary>
        /// Get Body Area Style
        /// </summary>
        /// <returns></returns>
        public GUIStyle GetBodyAreaStyle() {
            GUIStyle style = new GUIStyle();
            style.padding = new RectOffset(20, 20, 10, 10);
            style.normal.background = MakeTex( 2, 2, new Color(0.9f, 0.9f, 0.9f));
            style.alignment = TextAnchor.MiddleCenter;
            return style;
        }

        /// <summary>
        /// Get Regular Text Style
        /// </summary>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public GUIStyle GetRegularTextStyle(TextAnchor anchor = TextAnchor.UpperLeft) {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.black;
            style.wordWrap = true;
            style.alignment = anchor;
            style.fontSize = 12;
            style.margin = new RectOffset(0, 0, 10, 10);
            return style;
        }
        
        /// <summary>
        /// Get Sub Header Style
        /// </summary>
        /// <returns></returns>
        public GUIStyle GetSubHeaderStyle() {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.black;
            style.wordWrap = true;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 16;
            style.fontStyle = FontStyle.Bold;
            style.padding = new RectOffset(20, 20, 10, 10);
            style.normal.background = MakeTex( 2, 2, new Color(0.7f, 0.7f, 0.7f));
            return style;
        }
        
        /// <summary>
        /// Get Warning Text Style
        /// </summary>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public GUIStyle GetWarningTextStyle(TextAnchor anchor = TextAnchor.UpperLeft) {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.red;
            style.wordWrap = true;
            style.alignment = anchor;
            style.fontSize = 12;
            style.fontStyle = FontStyle.Bold;
            style.margin = new RectOffset(0, 0, 10, 10);
            return style;
        }

        /// <summary>
        /// Get BG Texture
        /// </summary>
        /// <returns></returns>
        public Texture GetBGTexture() {
            return MakeTex(2, 2, new Color(0.9f, 0.9f, 0.9f));
        }
        
        /// <summary>
        /// Generate Texture
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        private Texture2D MakeTex( int width, int height, Color col )
        {
            Color[] pix = new Color[width * height];
            for( int i = 0; i < pix.Length; ++i )
            {
                pix[ i ] = col;
            }
            Texture2D result = new Texture2D( width, height );
            result.SetPixels( pix );
            result.Apply();
            return result;
        }
    }
}