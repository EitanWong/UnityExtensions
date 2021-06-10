using UnityEditor;
using UnityEngine;
using UnityExtensions.EditorBase.ContentDrawer.Attribute;

namespace UnityExtensions.EditorBase.ContentDrawer.Editor
{
    [CustomPropertyDrawer(typeof(CustomContent))]
    public class ContentDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, new GUIContent(((CustomContent) attribute).ContentName));
            //base.OnGUI(position, property, label);
        }
    }
}