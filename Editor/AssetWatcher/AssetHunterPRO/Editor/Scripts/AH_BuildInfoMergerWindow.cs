using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace TransformPro.AssetHunterPRO
{
    public class AH_BuildInfoMergerWindow : EditorWindow
    {
        private static AH_BuildInfoMergerWindow m_window;
        private string buildInfoFolder;

        private Vector2 scrollPos;
        private List<BuildInfoSelection> buildInfoFiles;

        [MenuItem("Window/构建/资源 查看器/打开 合并构建信息工具")]
        public static void Init()
        {
            m_window = GetWindow<AH_BuildInfoMergerWindow>("合并构建信息工具", true, typeof(AH_Window));

            m_window.titleContent.image = AH_EditorData.Instance.MergerIcon.Icon;

            m_window.buildInfoFolder = AH_SerializationHelper.GetBuildInfoFolder();
            m_window.updateBuildInfoFiles();
        }

        private void updateBuildInfoFiles()
        {
            buildInfoFiles = new List<BuildInfoSelection>();

            System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(buildInfoFolder);
            if (directoryInfo.Exists)
            {
                foreach (var item in directoryInfo.GetFiles("*." + AH_SerializationHelper.BuildInfoExtension)
                    .OrderByDescending(val => val.LastWriteTime))
                {
                    buildInfoFiles.Add(new BuildInfoSelection(item));
                }
            }
            else
            {
                ShowNotification(new GUIContent("路径无效"),1f);
            }
        }

        private void OnGUI()
        {
            if (!m_window)
                Init();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            Heureka_WindowStyler.DrawGlobalHeader(m_window, AH_EditorData.Instance.WindowHeaderIcon.Icon,
                Heureka_WindowStyler.clr_Dark, "构建信息合并");
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("选择一个包含构建信息文件的文件夹", MessageType.Info);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("修改", GUILayout.ExpandWidth(false)))
            {
                buildInfoFolder = EditorUtility.OpenFolderPanel("构建信息文件夹", buildInfoFolder, "");
                updateBuildInfoFiles();
            }

            EditorGUILayout.LabelField("当前文件夹: " + buildInfoFolder);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            //Show all used types
            EditorGUILayout.BeginVertical();

            foreach (var item in buildInfoFiles)
            {
                item.Selected = EditorGUILayout.ToggleLeft(item.BuildInfoFile.Name, item.Selected);
            }

            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(buildInfoFiles.Count(val => val.Selected == true) < 2);
            if (GUILayout.Button("合并已选择", GUILayout.ExpandWidth(false)))
            {
                AH_SerializedBuildInfo merged = new AH_SerializedBuildInfo();
                foreach (var item in buildInfoFiles.FindAll(val => val.Selected))
                {
                    merged.MergeWith(item.BuildInfoFile.FullName);
                }

                merged.SaveAfterMerge();

                EditorUtility.DisplayDialog("合并完成",
                    "一个新的构建信息已创建", "确定");
                //Reset
                buildInfoFiles.ForEach(val => val.Selected = false);
                updateBuildInfoFiles();
            }

            EditorGUI.EndDisabledGroup();
            //Make sure this window has focus to update contents
            Repaint();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        [System.Serializable]
        private class BuildInfoSelection
        {
            public System.IO.FileInfo BuildInfoFile;
            public bool Selected = false;

            public BuildInfoSelection(System.IO.FileInfo buildInfoFile)
            {
                this.BuildInfoFile = buildInfoFile;
            }
        }
    }
}