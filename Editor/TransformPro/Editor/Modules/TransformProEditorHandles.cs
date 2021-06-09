namespace TransformPro.Scripts
{
    using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    ///     Draws overriden pivots to the scene GUI.
    /// </summary>
    public static class TransformProEditorHandles
    {
        private static string[] optionsSpace;

        public static string[] OptionsSpace { get { return TransformProEditorHandles.optionsSpace ?? (TransformProEditorHandles.optionsSpace = Enum.GetNames(typeof(TransformProSpace))); } }

        public static void DrawGUI(TransformProEditor editor)
        {
            //GUI.color = TransformProStyles.ColorSpace;
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();

            GUI.enabled = TransformPro.Space != TransformProSpace.Local;
            GUIContent localContent = new GUIContent(TransformProStyles.Icons.ToolHandleLocal, TransformProStrings.SystemLanguage.TooltipSpaceLocal);
            if (GUILayout.Button(localContent, TransformProStyles.Buttons.Icon.Left, GUILayout.Width(20)))
            {
                TransformPro.Space = TransformProSpace.Local;
            }
            GUI.enabled = TransformPro.Space != TransformProSpace.World;
            GUIContent worldContent = new GUIContent(TransformProStyles.Icons.ToolHandleGlobal, TransformProStrings.SystemLanguage.TooltipSpaceWorld);
            if (GUILayout.Button(worldContent, TransformProStyles.Buttons.Icon.Middle, GUILayout.Width(20)))
            {
                TransformPro.Space = TransformProSpace.World;
            }

            GUI.enabled = true;
            TransformPro.Space = (TransformProSpace) EditorGUILayout.Popup((int) TransformPro.Space, TransformProEditorHandles.OptionsSpace, TransformProStyles.PopupRight);

            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }

            EditorGUILayout.EndHorizontal();

            /*
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.x += 20;
            rect.width -= 20;
            rect.y -= 16;
            GUI.color = TransformProStyles.ColorLabelSubtle;
            GUI.Label(rect, "Space", TransformProStyles.LabelSmall);
            GUI.color = Color.white;
            */
        }
    }
}
