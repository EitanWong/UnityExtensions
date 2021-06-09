using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HierarchyPro
{
    [CustomEditor(typeof(HierarchyLocalData))]
    public class HierarchyLocalDataEditor : Editor
    {
        private HierarchyLocalData hld;

        private void OnEnable()
        {
            hld = target as HierarchyLocalData;
        }

        public override void OnInspectorGUI()
        {
            if (!hld.gameObject.CompareTag("EditorOnly"))
                hld.gameObject.tag = "EditorOnly";

            EditorGUILayout.HelpBox("在Hierarchy上保存行项目的参考自定义数据\n在构建时会被剔除.", MessageType.Info);
            EditorGUILayout.BeginVertical("box");
            base.OnInspectorGUI();
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("清除空引用"))
            {
                hld.ClearNullRef();
            }
        }
    }
}