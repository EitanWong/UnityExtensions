namespace TransformPro.Scripts
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    ///     Snapping Gadget.
    ///     Currently provides the manual grid snap buttons.
    ///     Will provide the visual grid, options for auto snap, etc.
    /// </summary>
    public class TransformProEditorGadgetGrid
        : ITransformProGadgetPanel
    {
        /// <inheritdoc />
        public bool Enabled { get { return true; } }

        /// <inheritdoc />
        public float Height { get { return 21; } } //34; } }

        /// <inheritdoc />
        public int Sort { get { return 10; } }

        /// <inheritdoc />
        public void DrawPanelGUI(SceneView sceneView, TransformProEditorGadgets gadgets, Rect rect)
        {
            float tabWidth = rect.width / 3;

            GUI.backgroundColor = TransformProStyles.ColorSnap;

            Rect tab = new Rect(rect.x, rect.y, tabWidth, 19);
            GUIContent snapTransformContent = new GUIContent(TransformProStyles.Icons.Snap, TransformProStrings.SystemLanguage.TooltipSnapTransform);
            if (GUI.Button(tab, snapTransformContent, TransformProStyles.Buttons.IconPadded.Left))
            {
                TransformPro.SnapPositionGrid = TransformProPreferences.SnapPositionGrid;
                TransformPro.SnapRotationGrid = TransformProPreferences.SnapRotationGrid;
                TransformProEditor.Snap();
            }

            tab.x += tabWidth;
            GUIContent snapPositionContent = new GUIContent(TransformProStyles.Icons.Position, TransformProStrings.SystemLanguage.TooltipSnapPosition);
            if (GUI.Button(tab, snapPositionContent, TransformProStyles.Buttons.IconPadded.Middle))
            {
                TransformPro.SnapPositionGrid = TransformProPreferences.SnapPositionGrid;
                TransformProEditor.SnapPosition();
            }

            tab.x += tabWidth;
            GUIContent snapRotationContent = new GUIContent(TransformProStyles.Icons.Rotation, TransformProStrings.SystemLanguage.TooltipSnapRotation);
            if (GUI.Button(tab, snapRotationContent, TransformProStyles.Buttons.IconPadded.Right))
            {
                TransformPro.SnapRotationGrid = TransformProPreferences.SnapRotationGrid;
                TransformProEditor.SnapRotation();
            }

            GUI.backgroundColor = Color.white;
        }
    }
}
