using System;
using HierarchyPro.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HierarchyPro.Editor
{
    [CustomEditor(typeof(HierarchyFolder))]
    internal class HierarchyFolderEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
        }

        public override VisualElement CreateInspectorGUI()
        {
            var script = target as HierarchyFolder;

            var root = new VisualElement();

            IMGUIContainer imguiContainer = new IMGUIContainer(() =>
            {
                script.flattenMode =
                    (HierarchyFolder.FlattenMode) EditorGUILayout.EnumPopup("展平 模式", script.flattenMode);
                if (script.flattenMode != HierarchyFolder.FlattenMode.None)
                {
                    script.flattenSpace =
                        (HierarchyFolder.FlattenSpace) EditorGUILayout.EnumPopup("展平 空间", script.flattenSpace);
                    script.destroyAfterFlatten = EditorGUILayout.Toggle("展平后销毁", script.destroyAfterFlatten);
                }
            });
            root.Add(imguiContainer);

            return root;
        }

        [MenuItem("GameObject/创建 文件夹", priority = 0)]
        static void CreateInstance() => new GameObject("NewFolder", new Type[1] {typeof(HierarchyFolder)});
    }
}