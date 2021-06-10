using UnityEditor;
using UnityEngine;

namespace TransformPro.AssetHunterPRO
{
    public class AH_SettingsWindow : EditorWindow
    {
        private const string WINDOWNAME = "设置";
        private Vector2 scrollPos;
        private static AH_SettingsWindow m_window;

        [UnityEditor.MenuItem("Window/构建/资源 查看器/打开 设置")]
        public static void OpenAssetHunter()
        {
            Init(false);
        }

        public static void Init(bool attemptDock)
        {
            bool firstInit = (m_window == null);

            m_window = AH_SettingsWindow.GetWindow<AH_SettingsWindow>(WINDOWNAME, true/*, typeof(AH_Window)*/);
            m_window.titleContent.image = AH_EditorData.Instance.Settings.Icon;

            AH_Window[] mainWindows = Resources.FindObjectsOfTypeAll<AH_Window>();
            if (attemptDock && mainWindows.Length != 0 && firstInit)
            {
                Docker.Dock(mainWindows[0], m_window, Docker.DockPosition.Right);
            }
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnGUI()
        {
            if (!m_window)
                Init(true);

            Heureka_WindowStyler.DrawGlobalHeader(m_window, AH_EditorData.Instance.WindowHeaderIcon.Icon, Heureka_WindowStyler.clr_dBlue, "设置");

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("恢复默认设置"))
            {
                if (EditorUtility.DisplayDialog("恢复默认设置", "你确定要恢复默认设置吗", "确定", "取消"))
                {
                    AH_SettingsManager.Instance.ResetAll();
                }
            }
            if (GUILayout.Button("保存设置到文件"))
                AH_SettingsManager.Instance.SaveToFile();
            if (GUILayout.Button("加载设置"))
                AH_SettingsManager.Instance.LoadFromFile();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            AH_SettingsManager.Instance.DrawSettings();

            EditorGUILayout.Space();

            AH_SettingsManager.Instance.DrawIgnored();

            EditorGUILayout.EndScrollView();
        }
    }
}