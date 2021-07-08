#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace Dialogue
{
    [CustomNodeEditor(typeof(Chat))]
    public class ChatEditor : NodeEditor
    {
        public override void OnBodyGUI()
        {
            serializedObject.Update();

            Chat node = target as Chat;
            //node.name = "对话";

            //EditorGUILayout.PropertyField(serializedObject.FindProperty("character"), new GUIContent("角色"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Chat.character)), GUIContent.none);
            if (node.options.Count == 0)
            {
                GUILayout.BeginHorizontal();
                NodeEditorGUILayout.PortField(GUIContent.none, target.GetInputPort("input"), GUILayout.MinWidth(0));
                NodeEditorGUILayout.PortField(GUIContent.none, target.GetOutputPort("output"), GUILayout.MinWidth(0));
                GUILayout.EndHorizontal();
            }
            else
            {
                NodeEditorGUILayout.PortField(GUIContent.none, target.GetInputPort("input"));
            }

            GUILayout.Space(-30);
            
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Chat.maintext)), GUIContent.none);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Chat.conditionTexts)));

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Chat.mainTexture)), GUIContent.none);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Chat.conditionTextures)));
            
#pragma warning disable 618
            NodeEditorGUILayout.InstancePortList(nameof(Chat.options), typeof(DialogueBaseNode), serializedObject,
                NodePort.IO.Output, Node.ConnectionType.Override);
#pragma warning restore 618
            serializedObject.ApplyModifiedProperties();
        }

        public override int GetWidth()
        {
            return 300;
        }

        public override Color GetTint()
        {
            Chat node = target as Chat;
            if (node.character == null) return base.GetTint();
            else
            {
                Color col = node.character.color;
                col.a = 1;
                return col;
            }
        }
    }
}
#endif