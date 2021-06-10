namespace TransformPro.Scripts
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    ///     Contains all Rotation based methods for the TransformPro Editor.
    /// </summary>
    public partial class TransformProEditor
    {
        /// <summary>
        ///     Sets all selected transform rotations.
        /// </summary>
        public Vector3 RotationEuler
        {
            set
            {
                TransformProEditor.RecordUndo("Rotate");

                bool mixedOld = TransformProEditor.Selected.Select(x => x.RotationEuler).MixedAxis();
                foreach (TransformPro transformPro in this)
                {
                    transformPro.RotationEuler = value;
                }

                bool mixedNew = TransformProEditor.Selected.Select(x => x.RotationEuler).MixedAxis();

                if (mixedOld != mixedNew)
                {
                    this.serializedObject.SetIsDifferentCacheDirty();
                }
            }
        }

        /// <summary>
        ///     Sets all selected transform X rotations.
        /// </summary>
        public float RotationEulerX
        {
            set
            {
                TransformProEditor.RecordUndo("Rotate");
                foreach (TransformPro transformPro in this)
                {
                    transformPro.RotationEulerX = value;
                }
            }
        }

        /// <summary>
        ///     Sets all selected transform Y rotations.
        /// </summary>
        public float RotationEulerY
        {
            set
            {
                TransformProEditor.RecordUndo("Rotate");
                foreach (TransformPro transformPro in this)
                {
                    transformPro.RotationEulerY = value;
                }
            }
        }

        /// <summary>
        ///     Sets all selected transform Z rotations.
        /// </summary>
        public float RotationEulerZ
        {
            set
            {
                TransformProEditor.RecordUndo("Rotate");
                foreach (TransformPro transformPro in this)
                {
                    transformPro.RotationEulerZ = value;
                }
            }
        }

        /// <summary>
        ///     Nudge the X Rotation of all selected transforms by the exact amount provided.
        ///     If you want to nudge by the grid size that should be premultiplied into this amount.
        /// </summary>
        /// <param name="amount">The unity distance to Nudge the X Rotation of the selection by.</param>
        public void NudgeRotationX(float amount)
        {
            TransformProEditor.RecordUndo("Rotate");
            foreach (TransformPro transformPro in this)
            {
                transformPro.RotationEulerX += amount;
            }
        }

        /// <summary>
        ///     Nudge the Y Rotation of all selected transforms by the exact amount provided.
        ///     If you want to nudge by the grid size that should be premultiplied into this amount.
        /// </summary>
        /// <param name="amount">The unity distance to Nudge the Y Rotation of the selection by.</param>
        public void NudgeRotationY(float amount)
        {
            TransformProEditor.RecordUndo("Rotate");
            foreach (TransformPro transformPro in this)
            {
                transformPro.RotationEulerY += amount;
            }
        }

        /// <summary>
        ///     Nudge the Z Rotation of all selected transforms by the exact amount provided.
        ///     If you want to nudge by the grid size that should be premultiplied into this amount.
        /// </summary>
        /// <param name="amount">The unity distance to Nudge the Z Rotation of the selection by.</param>
        public void NudgeRotationZ(float amount)
        {
            TransformProEditor.RecordUndo("Rotate");
            foreach (TransformPro transformPro in this)
            {
                transformPro.RotationEulerZ += amount;
            }
        }

        /// <summary>
        ///     Randomises the rotations of all the selected transforms.
        ///     Applies random values to all three axes.
        ///     Each transform will get a different value.
        /// </summary>
        public void RandomiseRotation()
        {
            TransformProEditor.RecordUndo("Rotate");
            foreach (TransformPro transformPro in this)
            {
                transformPro.RotationEulerX = (float) (TransformPro.Random.NextDouble() * 360);
                transformPro.RotationEulerY = (float) (TransformPro.Random.NextDouble() * 360);
                transformPro.RotationEulerZ = (float) (TransformPro.Random.NextDouble() * 360);
            }
        }

        /// <summary>
        ///     Randomises the X axis rotation of all the selected transforms.
        ///     Each transform will get a different value.
        /// </summary>
        public void RandomiseRotationX()
        {
            TransformProEditor.RecordUndo("Rotate");
            foreach (TransformPro transformPro in this)
            {
                transformPro.RotationEulerZ = (float) (TransformPro.Random.NextDouble() * 360);
            }
        }

        /// <summary>
        ///     Randomises the Y axis rotation of all the selected transforms.
        ///     Each transform will get a different value.
        /// </summary>
        public void RandomiseRotationY()
        {
            TransformProEditor.RecordUndo("Rotate");
            foreach (TransformPro transformPro in this)
            {
                transformPro.RotationEulerY = (float) (TransformPro.Random.NextDouble() * 360);
            }
        }

        /// <summary>
        ///     Randomises the Z axis rotation of all the selected transforms.
        ///     Each transform will get a different value.
        /// </summary>
        public void RandomiseRotationZ()
        {
            TransformProEditor.RecordUndo("Rotate");
            foreach (TransformPro transformPro in this)
            {
                transformPro.RotationEulerZ = (float) (TransformPro.Random.NextDouble() * 360);
            }
        }

        /// <summary>
        ///     Draws the core rotation field.
        ///     This includes all three fields, the reset buttons, and everything relating to the advanced panel.
        /// </summary>
        public void RotationField()
        {
            // Ensure the current saved grid settings match the preferences.
            TransformPro.SnapRotationGrid = TransformProPreferences.SnapRotationGrid;
            TransformPro.SnapRotationOrigin = TransformProPreferences.SnapRotationOrigin;

            EditorGUILayout.BeginHorizontal(FrameBoxStyle);
            GUILayout.Label(TransformProStyles.Icons.RotationSmall, TransformProStyles.LabelSmallIcon,
                GUILayout.Width(16), GUILayout.Height(16));
            Rect toggleRect = GUILayoutUtility.GetLastRect();
            toggleRect.width = 20;
            TransformProPreferences.AdvancedRotation = EditorGUI.Foldout(toggleRect,
                TransformProPreferences.AdvancedRotation, GUIContent.none);
            EditorGUILayout.BeginHorizontal(PickedPixel);
            switch (TransformProEditor.SelectedCount)
            {
                case 0:
                    TransformProEditorCore.DrawAxis('x');
                    TransformProEditorCore.DrawAxis('y');
                    TransformProEditorCore.DrawAxis('z');
                    break;
                case 1:
                    Vector3 rotation = TransformProEditor.Selected.First().RotationEuler;
                    this.RotationField(ref rotation);
                    break;
                default:
                    List<Vector3> inputs = TransformProEditor.Selected.Select(x => x.RotationEuler).ToList();
                    float axis;
                    if (TransformProEditorCore.DrawAxis('x', inputs.Select(v => v.x).ToList(), out axis))
                    {
                        this.RotationEulerX = axis;
                    }

                    if (TransformProEditorCore.DrawAxis('y', inputs.Select(v => v.y).ToList(), out axis))
                    {
                        this.RotationEulerY = axis;
                    }

                    if (TransformProEditorCore.DrawAxis('z', inputs.Select(v => v.z).ToList(), out axis))
                    {
                        this.RotationEulerZ = axis;
                    }

                    break;
            }

            EditorGUILayout.EndHorizontal();
            GUI.enabled = TransformProEditor.CanAnyChangeRotation;
            GUI.backgroundColor = TransformProStyles.ColorReset;
            GUI.contentColor = TransformProStyles.ColorAxisXDeep;
            if (GUILayout.Button("X", TransformProStyles.Buttons.IconTint.Left, GUILayout.Width(16)))
            {
                this.RotationEulerX = 0;
            }

            GUI.contentColor = TransformProStyles.ColorAxisYDeep;
            if (GUILayout.Button("Y", TransformProStyles.Buttons.IconTint.Middle, GUILayout.Width(16)))
            {
                this.RotationEulerY = 0;
            }

            GUI.contentColor = TransformProStyles.ColorAxisZDeep;
            if (GUILayout.Button("Z", TransformProStyles.Buttons.IconTint.Middle, GUILayout.Width(16)))
            {
                this.RotationEulerZ = 0;
            }

            GUI.contentColor = Color.white;
            if (GUILayout.Button("0", TransformProStyles.Buttons.IconTint.Right, GUILayout.Width(16)))
            {
                this.RotationEuler = Vector3.zero;
            }

            
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            if (TransformProPreferences.AdvancedRotation)
            {
                this.DrawRotationAdvancedPanel();
            }
        }

        /// <summary>
        ///     Draws a simplified single value rotation field.
        ///     Only draws the three main fields, and does not reset the value unless a change is made.
        ///     This helps prevent drifting for the single inspector.
        /// </summary>
        /// <param name="rotation">The rotation to display. This value is also boxed and returned via the ref keyword.</param>
        public void RotationField(ref Vector3 rotation)
        {
            bool changed = false;
            changed |= TransformProEditorCore.DrawAxis('x', ref rotation.x);
            changed |= TransformProEditorCore.DrawAxis('y', ref rotation.y);
            changed |= TransformProEditorCore.DrawAxis('z', ref rotation.z);
            if (!changed)
            {
                return;
            }

            this.RotationEuler = rotation;
        }

        /// <summary>
        ///     Draws the advanced rotation panel.
        ///     This includes all nudges, and any specialized tools on the right.
        ///     A Gizmo stlye system could work well here to abstract responsibility back to the individual tool modules.
        /// </summary>
        private void DrawRotationAdvancedPanel()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            float[] savedNudges = TransformProPreferences.NudgesRotation.ToArray();
            foreach (float savedNudge in savedNudges)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("{0}º", savedNudge), TransformProStyles.LabelTiny, GUILayout.Width(29));
                this.DrawRotationAdvancedPanel(savedNudge);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            // ---- right side split

            EditorGUILayout.BeginVertical(GUILayout.Width(64));

            EditorGUILayout.BeginHorizontal(GUILayout.Width(64));
            GUI.contentColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            if (GUILayout.Button("X", TransformProStyles.Buttons.IconTint.Left, GUILayout.Width(15)))
            {
                this.RandomiseRotationX();
            }

            if (GUILayout.Button("Y", TransformProStyles.Buttons.IconTint.Middle, GUILayout.Width(15)))
            {
                this.RandomiseRotationY();
            }

            if (GUILayout.Button("Z", TransformProStyles.Buttons.IconTint.Middle, GUILayout.Width(15)))
            {
                this.RandomiseRotationZ();
            }

            if (GUILayout.Button(TransformProStyles.Icons.Random, TransformProStyles.Buttons.Icon.Right,
                GUILayout.Width(19)))
            {
                this.RandomiseRotation();
            }

            GUI.contentColor = Color.white;
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = TransformProStyles.ColorPaste;

            GUIContent contentLookAt = new GUIContent("Clipboard", TransformProStyles.Icons.LookAt,
                TransformProStrings.SystemLanguage.TooltipLookAt);
            if (GUILayout.Button(contentLookAt, TransformProStyles.Buttons.Icon.Single))
            {
                TransformProEditor.RecordUndo("Look At");
                foreach (TransformPro transform in this)
                {
                    transform.LookAt(TransformProEditor.Clipboard);
                }
            }

            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        /// <summary>
        ///     Draws a specific nudge for the rotation advanced panel.
        ///     Allows customisable nudges to be easily added.
        /// </summary>
        /// <param name="nudge">The nudge amount, in degrees.</param>
        private void DrawRotationAdvancedPanel(float nudge)
        {
            int nudgeResult = TransformProEditorCore.DrawNudges();
            if (nudgeResult > 0)
            {
                if (Event.current.shift || (Event.current.button == 1))
                {
                    Transform[] selectedAll = Selection.GetTransforms(SelectionMode.ExcludePrefab);
                    ICollection<Transform> clonedAll = selectedAll.Select(x => TransformProEditor.Clone(x)).ToList();
                    foreach (Transform cloned in clonedAll)
                    {
                        Undo.RegisterCreatedObjectUndo(cloned.gameObject, "Rotate");
                    }

                    TransformProEditor.Select(clonedAll);
                }
            }

            switch (nudgeResult)
            {
                case 1:
                    this.NudgeRotationX(nudge * -1);
                    break;
                case 2:
                    this.NudgeRotationX(nudge);
                    break;
                case 3:
                    this.NudgeRotationY(nudge * -1);
                    break;
                case 4:
                    this.NudgeRotationY(nudge);
                    break;
                case 5:
                    this.NudgeRotationZ(nudge * -1);
                    break;
                case 6:
                    this.NudgeRotationZ(nudge);
                    break;
            }
        }
    }
}