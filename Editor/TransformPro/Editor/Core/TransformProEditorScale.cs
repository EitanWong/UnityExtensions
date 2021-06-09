namespace TransformPro.Scripts
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    ///     Contains all Scale based methods for the TransformPro Editor.
    /// </summary>
    public partial class TransformProEditor
    {
        /// <summary>
        ///     Sets all selected transform positions.
        /// </summary>
        public Vector3 Scale
        {
            set
            {
                TransformProEditor.RecordUndo("Scale");

                bool mixedOld = TransformProEditor.Selected.Select(x => x.Scale).MixedAxis();
                foreach (TransformPro transformPro in this)
                {
                    transformPro.Scale = value;
                }

                bool mixedNew = TransformProEditor.Selected.Select(x => x.Scale).MixedAxis();

                if (mixedOld != mixedNew)
                {
                    this.serializedObject.SetIsDifferentCacheDirty();
                }
            }
        }

        /// <summary>
        ///     Sets all selected transform X positions.
        /// </summary>
        public float ScaleX
        {
            set
            {
                TransformProEditor.RecordUndo("Scale");
                foreach (TransformPro transformPro in this)
                {
                    transformPro.ScaleX = value;
                }
            }
        }

        /// <summary>
        ///     Sets all selected transform Y positions.
        /// </summary>
        public float ScaleY
        {
            set
            {
                TransformProEditor.RecordUndo("Scale");
                foreach (TransformPro transformPro in this)
                {
                    transformPro.ScaleY = value;
                }
            }
        }

        /// <summary>
        ///     Sets all selected transform Z positions.
        /// </summary>
        public float ScaleZ
        {
            set
            {
                TransformProEditor.RecordUndo("Scale");
                foreach (TransformPro transformPro in this)
                {
                    transformPro.ScaleZ = value;
                }
            }
        }

        public void ScaleField(ref Vector3 scale)
        {
            bool changed = false;
            if (TransformProPreferences.ScalarScale && scale.IsScalar())
            {
                changed |= TransformProEditorCore.DrawAxisScalar(ref scale.x);
                if (changed)
                {
                    scale.y = scale.x;
                    scale.z = scale.x;
                }
            }
            else
            {
                changed |= TransformProEditorCore.DrawAxis('x', ref scale.x);
                changed |= TransformProEditorCore.DrawAxis('y', ref scale.y);
                changed |= TransformProEditorCore.DrawAxis('z', ref scale.z);
            }

            if (!changed)
            {
                return;
            }

            this.Scale = scale;
        }

        public void ScaleField()
        {
            EditorGUILayout.BeginHorizontal(FrameBoxStyle);

            bool scalarScale = TransformProPreferences.ScalarScale;
            bool scalarValues;

            switch (TransformProEditor.SelectedCount)
            {
                case 0:
                    scalarValues = true;
                    break;
                case 1:
                    scalarValues = TransformProEditor.Selected.First().Scale.IsScalar();
                    break;
                default:
                    scalarValues = TransformProEditor.Selected.Select(x => x.Scale).All(x => x.IsScalar());
                    break;
            }


            GUI.color = !scalarValues ? Color.grey : Color.white;
            if (!scalarValues)
            {
                scalarScale = false;
            }

            if (GUILayout.Button(
                scalarScale ? TransformProStyles.Icons.EditVector1 : TransformProStyles.Icons.EditVector3,
                TransformProStyles.LabelSmallIcon,
                GUILayout.Width(16),
                GUILayout.Height(16)))
            {
                TransformProPreferences.ScalarScale = !TransformProPreferences.ScalarScale;
                scalarScale = TransformProPreferences.ScalarScale;
            }

            GUI.color = Color.white;

            Rect toggleRect = GUILayoutUtility.GetLastRect();
            toggleRect.width = 20;
            //TransformProPreferences.AdvancedScale = EditorGUI.Foldout(toggleRect, TransformProPreferences.AdvancedScale, GUIContent.none);
            EditorGUILayout.BeginHorizontal(PickedPixel);
            switch (TransformProEditor.SelectedCount)
            {
                case 0: // This covers what should be an impossible situation, drawing a contextless inspector
                    if (scalarScale)
                    {
                        TransformProEditorCore.DrawAxisScalar();
                    }
                    else
                    {
                        TransformProEditorCore.DrawAxis('x');
                        TransformProEditorCore.DrawAxis('y');
                        TransformProEditorCore.DrawAxis('z');
                    }

                    break;
                case 1:
                    Vector3 scale = TransformProEditor.Selected.First().Scale;
                    this.ScaleField(ref scale);
                    break;
                default:
                    List<Vector3> inputs = TransformProEditor.Selected.Select(x => x.Scale).ToList();
                    float axis;
                    if (scalarScale)
                    {
                        if (TransformProEditorCore.DrawAxisScalar(inputs.Select(v => v.x).ToList(), out axis))
                        {
                            this.ScaleX = axis;
                            this.ScaleY = axis;
                            this.ScaleZ = axis;
                        }
                    }
                    else
                    {
                        if (TransformProEditorCore.DrawAxis('x', inputs.Select(v => v.x).ToList(), out axis))
                        {
                            this.ScaleX = axis;
                        }

                        if (TransformProEditorCore.DrawAxis('y', inputs.Select(v => v.y).ToList(), out axis))
                        {
                            this.ScaleY = axis;
                        }

                        if (TransformProEditorCore.DrawAxis('z', inputs.Select(v => v.z).ToList(), out axis))
                        {
                            this.ScaleZ = axis;
                        }
                    }

                    break;
            }

            EditorGUILayout.EndHorizontal();
            GUI.enabled = TransformProEditor.CanAnyChangeScale;
            GUI.backgroundColor = TransformProStyles.ColorReset;
            GUI.contentColor = TransformProStyles.ColorAxisXDeep;

            if (GUILayout.Button("X", TransformProStyles.Buttons.IconTint.Left, GUILayout.Width(16)))
            {
                this.ScaleX = 1;
            }

            GUI.contentColor = TransformProStyles.ColorAxisYDeep;
            if (GUILayout.Button("Y", TransformProStyles.Buttons.IconTint.Middle, GUILayout.Width(16)))
            {
                this.ScaleY = 1;
            }

            GUI.contentColor = TransformProStyles.ColorAxisZDeep;
            if (GUILayout.Button("Z", TransformProStyles.Buttons.IconTint.Middle, GUILayout.Width(16)))
            {
                this.ScaleZ = 1;
            }

            GUI.contentColor = Color.white;
            if (GUILayout.Button("1", TransformProStyles.Buttons.IconTint.Right, GUILayout.Width(16)))
            {
                this.Scale = Vector3.one;
            }

            GUI.backgroundColor = Color.white;
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            if (TransformProPreferences.AdvancedScale)
            {
                this.DrawScaleAdvancedPanel();
            }
        }

        private void DrawScaleAdvancedPanel()
        {
            // TODO: High performance cache of the combined root bounds to allow for complex size testing and scaling in local and world space
            /*
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 14;

            EditorGUILayout.BeginHorizontal();

            Rect rectLabel = EditorGUILayout.GetControlRect(false, GUILayout.Width(16));
            rectLabel.xMin -= 10;
            GUI.Label(rectLabel, "Size", TransformProStyles.LabelSmall);

            float sizeX = 0;
            GUI.color = TransformProStyles.ColorAxisXLight;
            EditorGUILayout.FloatField("X", sizeX);

            float sizeY = 0;
            GUI.color = TransformProStyles.ColorAxisYLight;
            EditorGUILayout.FloatField("Y", sizeY);

            float sizeZ = 0;
            GUI.color = TransformProStyles.ColorAxisZLight;
            EditorGUILayout.FloatField("Z", sizeZ);
            GUI.color = Color.white;

            GUILayout.Label(" ", TransformProStyles.LabelSmall, GUILayout.Width(64));

            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = labelWidth;
            */
        }
    }
}