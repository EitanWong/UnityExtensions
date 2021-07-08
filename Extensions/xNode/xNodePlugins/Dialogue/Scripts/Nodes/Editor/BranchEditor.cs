#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Dialogue;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using XNodeEditor;

namespace DialogueEditor {
    [CustomNodeEditor(typeof(Branch))]
    public class BranchEditor : NodeEditor {

        private ReorderableList reorderableList;
        public override void OnBodyGUI() {
            serializedObject.Update();

            Branch node = target as Branch;
            //node.name = "分支";
            NodeEditorGUILayout.PortField(target.GetInputPort("input"));
            EditorGUILayout.Space();
            
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("conditions"));//,new GUIContent("条件")
            NodeEditorGUILayout.PortField(new GUIContent("True"),target.GetOutputPort("pass"));
            NodeEditorGUILayout.PortField(new GUIContent("False"),target.GetOutputPort("fail"));

            serializedObject.ApplyModifiedProperties();
        }

        public override int GetWidth() {
            return 336;
        }
    }
}
#endif