using UnityEditor;
using UnityEngine;

namespace MagicLightProbes
{
    [CustomPropertyDrawer(typeof(ShowOnlyAttribute))]
    public class ShowOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            string valueStr;

            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    valueStr = prop.intValue.ToString();
                    break;
                case SerializedPropertyType.Boolean:
                    valueStr = prop.boolValue.ToString();
                    break;
                case SerializedPropertyType.Float:
                    valueStr = prop.floatValue.ToString("0.00000");
                    break;
                case SerializedPropertyType.String:
                    valueStr = prop.stringValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    if (prop.objectReferenceValue == null)
                    {
                        valueStr = prop.displayName;
                    }
                    else
                    {
                        valueStr = prop.objectReferenceValue.name;
                    }
                    break;
                case SerializedPropertyType.Vector3:
                    valueStr = "X " + prop.vector3Value.x.ToString() + " Y " + prop.vector3Value.y.ToString() + " Z " + prop.vector3Value.z.ToString();
                    break;
                default:
                    valueStr = "(not supported)";
                    break;
            }

            EditorGUI.LabelField(position, label.text, valueStr);
        }
    }
}
