using UnityEditor;
using UnityEngine;
using TransformPro.MeshPro.MeshEditor.Editor.Scripts.Base;

namespace MeshEditor.Editor.Scripts.Base.Utilities
{
    public static class MEDR_StylesUtility
    {
        #region Style

        public static GUIStyle SelectStyle = new GUIStyle("AssetLabel");
        public static GUIStyle UnSelectStyle = new GUIStyle("AssetLabel Partial");
        public static GUIStyle FoldStyle = new GUIStyle("PreviewPackageInUse");
        public static GUIStyle FrameStyle = new GUIStyle("FrameBox");
        public static GUIStyle GroupBoxStyle = new GUIStyle("GroupBox");

        public static GUIStyle TitleStyle =
            new GUIStyle(EditorGUIUtility.isProSkin ? "LODLevelNotifyText" : "DefaultCenteredLargeText");

        public static GUIStyle SettingHeaderStyle = new GUIStyle("SettingsHeader");
        public static GUIStyle WarningOverlayStyle = new GUIStyle("WarningOverlay");

        #endregion
    }
}