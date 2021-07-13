#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Extensions.MeshPro.MeshEditor.Editor.Scripts.Base.Utilities
{
    public static class MEDR_StylesUtility
    {
        #region Style

        public static GUIStyle SelectStyle = new GUIStyle("AssetLabel");
        public static GUIStyle UnSelectStyle = new GUIStyle("AssetLabel Partial");
#if UNITY_2020_1_OR_NEWER
        public static GUIStyle FoldStyle = new GUIStyle("PreviewPackageInUse");
#else
        public static GUIStyle FoldStyle = new GUIStyle("FoldoutHeader");
#endif
        public static GUIStyle FrameStyle = new GUIStyle("FrameBox");
        public static GUIStyle GroupBoxStyle = new GUIStyle("GroupBox");

        public static GUIStyle TitleStyle =
            new GUIStyle(EditorGUIUtility.isProSkin ? "LODLevelNotifyText" : "DefaultCenteredLargeText");

        public static GUIStyle SettingHeaderStyle = new GUIStyle("SettingsHeader");
        public static GUIStyle WarningOverlayStyle = new GUIStyle("WarningOverlay");

        #endregion
    }
}
#endif