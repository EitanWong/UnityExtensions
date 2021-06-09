namespace TransformPro.Scripts
{
    using UnityEditor;
    using UnityEngine;

    public static class TransformProStylesButtonTextures
    {
        public static GUIStyle GetStyleLeft()
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniButtonLeft)
                             {
                                 margin = new RectOffset(4, 0, 2, 2),
                                 border = new RectOffset(4, 2, 12, 4),
                                 padding = new RectOffset(6, 6, 2, 2),
                                 overflow = EditorGUIUtility.isProSkin
                                                ? new RectOffset(0, 0, -1, 2)
                                                : new RectOffset(0, 0, 0, 2)
                             };
            return style;
        }

        public static GUIStyle GetStyleMiddle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniButtonMid)
                             {
                                 padding = new RectOffset(6, 6, 2, 2),
                                 margin = new RectOffset(0, 0, 2, 2),
                                 border = new RectOffset(3, 3, 12, 4),
                                 overflow = EditorGUIUtility.isProSkin
                                                ? new RectOffset(0, 0, -1, 2)
                                                : new RectOffset(0, 0, 0, 2)
                             };
            return style;
        }

        public static GUIStyle GetStyleRight()
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniButtonRight)
                             {
                                 padding = new RectOffset(6, 6, 2, 2),
                                 margin = new RectOffset(0, 4, 2, 2),
                                 border = new RectOffset(2, 4, 12, 4),
                                 overflow = EditorGUIUtility.isProSkin
                                                ? new RectOffset(0, 0, -1, 2)
                                                : new RectOffset(0, 0, 0, 2)
                             };
            return style;
        }

        public static GUIStyle GetStyleSingle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniButton)
                             {
                                 padding = new RectOffset(6, 6, 2, 2),
                                 margin = new RectOffset(4, 4, 2, 2),
                                 border = new RectOffset(6, 6, 12, 4),
                                 overflow = EditorGUIUtility.isProSkin
                                                ? new RectOffset(0, 0, -1, 2)
                                                : new RectOffset(0, 0, 0, 2)
                             };
            return style;
        }

        public static GUIStyle GetTinyLeft()
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniButtonLeft)
                             {
                                 margin = new RectOffset(1, 0, 1, 1),
                                 border = new RectOffset(3, 0, 3, 3)
                             };
            TransformProStylesButtonTextures.SetTinyTextures(style, "TinyLeft");
            return style;
        }

        public static GUIStyle GetTinyMiddle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniButtonMid)
                             {
                                 margin = new RectOffset(0, 0, 1, 1),
                                 border = new RectOffset(1, 0, 3, 3)
                             };
            TransformProStylesButtonTextures.SetTinyTextures(style, "TinyMiddle");
            return style;
        }

        public static GUIStyle GetTinyRight()
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniButtonRight)
                             {
                                 margin = new RectOffset(1, 0, 1, 1),
                                 border = new RectOffset(1, 3, 3, 3)
                             };
            TransformProStylesButtonTextures.SetTinyTextures(style, "TinyRight");
            return style;
        }

        public static GUIStyle GetTinySingle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniButton)
                             {
                                 margin = new RectOffset(1, 1, 1, 1),
                                 border = new RectOffset(3, 3, 3, 3)
                             };
            TransformProStylesButtonTextures.SetTinyTextures(style, "Tiny");
            return style;
        }

        private static void SetTinyTextures(GUIStyle style, string button)
        {
            style.normal.background = TransformProStylesIcons.GetCustomIcon("Button" + button);
            style.normal.textColor = Color.white;
            style.onNormal.background = style.normal.background;
            style.active.background = style.normal.background;
            style.onActive.background = style.normal.background;
            style.focused.background = style.normal.background;
            style.onFocused.background = style.normal.background;
            style.padding = new RectOffset(0, 0, 0, 0);
            style.overflow = new RectOffset(0, 0, 0, 0);
        }
    }
}
