using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace NightOwl
{
    public static class EditorThemeChanger
    {
        private enum Theme
        {
            Light,
            Dark
        }


        private static Theme themeToSet;


        public static void SetLightTheme()
        {
            themeToSet = Theme.Light;
            EditorApplication.update += EditorThemeUpdate;
        }

        public static void SetDarkTheme()
        {
            themeToSet = Theme.Dark;
            EditorApplication.update += EditorThemeUpdate;
        }

        private static void EditorThemeUpdate()
        {
            if (themeToSet == Theme.Light && EditorGUIUtility.isProSkin)
            {
                var sceneView = (SceneView) SceneView.sceneViews[0];
                if (sceneView)
                {
                    sceneView.ShowNotification(new GUIContent("正在切换...\n编辑器\n亮白模式"), 1f);
                }

                //Debug.Log("Auto Dark Theme: Switching to light theme.");
                EditorPrefs.SetInt("UserSkin", 0);
                InternalEditorUtility.SwitchSkinAndRepaintAllViews();
            }
            else if (themeToSet == Theme.Dark && !EditorGUIUtility.isProSkin)
            {
                var sceneView = (SceneView) SceneView.sceneViews[0];
                if (sceneView)
                {
                    sceneView.ShowNotification(new GUIContent("正在切换...\n编辑器\n暗黑模式"), 1f);
                }

                // Debug.Log("Auto Dark Theme: Switching to dark theme.");
                EditorPrefs.SetInt("UserSkin", 1);
                InternalEditorUtility.SwitchSkinAndRepaintAllViews();
            }

            EditorApplication.update -= EditorThemeUpdate;
        }
    }
}