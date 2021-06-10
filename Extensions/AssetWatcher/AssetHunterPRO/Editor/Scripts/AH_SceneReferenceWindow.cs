using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace TransformPro.AssetHunterPRO
{
    public class AH_SceneReferenceWindow : EditorWindow
    {
        private static AH_SceneReferenceWindow m_window;
        private Vector2 scrollPos;

        [SerializeField] private float btnMinWidthSmall = 50;

        private List<String> m_allScenesInProject;
        private List<String> m_allScenesInBuildSettings;
        private List<String> m_allEnabledScenesInBuildSettings;
        private List<String> m_allUnreferencedScenes;
        private List<String> m_allDisabledScenesInBuildSettings;

        private static readonly string WINDOWNAME = "场景概况";

        [MenuItem("Window/构建/资源 查看器/打开 场景概况")]
        public static void Init()
        {
            m_window = AH_SceneReferenceWindow.GetWindow<AH_SceneReferenceWindow>(WINDOWNAME, true, typeof(AH_Window));
            m_window.titleContent.image = AH_EditorData.Instance.SceneIcon.Icon;
            m_window.GetSceneInfo();
        }

        private void GetSceneInfo()
        {
            m_allScenesInProject = AH_Utils.GetAllSceneNames().ToList<string>();
            m_allScenesInBuildSettings = AH_Utils.GetAllSceneNamesInBuild().ToList<string>();
            m_allEnabledScenesInBuildSettings = AH_Utils.GetEnabledSceneNamesInBuild().ToList<string>();
            m_allDisabledScenesInBuildSettings =
                SubtractSceneArrays(m_allScenesInBuildSettings, m_allEnabledScenesInBuildSettings);
            m_allUnreferencedScenes = SubtractSceneArrays(m_allScenesInProject, m_allScenesInBuildSettings);
        }

        //Get the subset of scenes where we subtract "secondary" from "main"
        private List<String> SubtractSceneArrays(List<String> main, List<String> secondary)
        {
            return main.Except<string>(secondary).ToList<string>();
        }

        private void OnFocus()
        {
            GetSceneInfo();
        }

        private void OnGUI()
        {
            if (!m_window)
                Init();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            Heureka_WindowStyler.DrawGlobalHeader(m_window, AH_EditorData.Instance.WindowHeaderIcon.Icon,
                Heureka_WindowStyler.clr_Dark, "场景概况");

            //Show all used types
            EditorGUILayout.BeginVertical();

            //Make sure this window has focus to update contents
            Repaint();

            if (m_allEnabledScenesInBuildSettings.Count == 0)
                Heureka_WindowStyler.DrawCenteredMessage(m_window, AH_EditorData.Instance.WindowHeaderIcon.Icon, 310f,
                    110f, "在构建设置中没有启用的场景");

            drawScenes("这些场景是在构建设置中添加和启用的", m_allEnabledScenesInBuildSettings);
            drawScenes("这些场景被添加到构建设置中，但被禁用", m_allDisabledScenesInBuildSettings);
            drawScenes("这些场景在构建设置中的任何地方都没有被引用", m_allUnreferencedScenes);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void drawScenes(string headerMsg, List<string> scenes)
        {
            if (scenes.Count > 0)
            {
                EditorGUILayout.HelpBox(headerMsg, MessageType.Info);
                foreach (string scenePath in scenes)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("查找", GUILayout.Width(btnMinWidthSmall)))
                    {
                        Selection.activeObject = AssetDatabase.LoadAssetAtPath(scenePath, typeof(UnityEngine.Object));
                        EditorGUIUtility.PingObject(Selection.activeObject);
                    }

                    EditorGUILayout.LabelField(scenePath);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Separator();
            }
        }
    }
}