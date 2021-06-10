using UnityEditor;
using UnityEngine;

namespace Hub.Editor.Scripts
{
    public static class Hub_Styles
    {
        public static GUIContent Visible = EditorGUIUtility.IconContent("animationvisibilitytoggleoff@2x");
        public static GUIContent NoneVisible = EditorGUIUtility.IconContent("animationvisibilitytoggleon@2x");
        public static GUIContent Scene = EditorGUIUtility.IconContent("BuildSettings.SelectedIcon");
        public static GUIContent pickable = EditorGUIUtility.IconContent("d_scenepicking_pickable_hover@2x");
        public static GUIContent pickable_white = EditorGUIUtility.IconContent("d_scenepicking_pickable@2x");
        public static GUIContent notpickable = EditorGUIUtility.IconContent("d_scenepicking_notpickable_hover@2x");
        public static GUIContent gameobjectIcon = EditorGUIUtility.IconContent("SceneViewOrtho");

        public static GUIStyle FrameBox = new GUIStyle("FrameBox");
        public static GUIStyle IconButton = new GUIStyle("IconButton");
        public static GUIStyle HeaderButton = new GUIStyle("HeaderButton");
        public static GUIStyle PreviewPackageInUse = new GUIStyle("PreviewPackageInUse");
        public static GUIStyle ChannelStripAttenuationMarkerSquare = new GUIStyle("ChannelStripAttenuationMarkerSquare");
        public static GUIStyle OL_Ping = new GUIStyle("OL Ping");
    
        public static GUIStyle FoldoutHeader = new GUIStyle("FoldoutHeader");
    }
}