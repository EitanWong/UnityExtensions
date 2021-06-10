namespace TransformPro.Scripts
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    ///     Contains all Position based methods for the TransformPro Editor.
    /// </summary>
    public partial class TransformProEditor
    {
        /// <summary>
        ///     Sets all selected transform positions.
        /// </summary>
        public Vector3 Position
        {
            set
            {
                TransformProEditor.RecordUndo("Position");

                bool mixedOld = TransformProEditor.Selected.Select(x => x.Position).MixedAxis();
                foreach (TransformPro transformPro in TransformProEditor.Selected)
                {
                    transformPro.Position = value;
                }

                bool mixedNew = TransformProEditor.Selected.Select(x => x.Position).MixedAxis();

                if (mixedOld != mixedNew)
                {
                    this.serializedObject.SetIsDifferentCacheDirty();
                }
            }
        }

        /// <summary>
        ///     Sets all selected transform X positions.
        /// </summary>
        public float PositionX
        {
            set
            {
                TransformProEditor.RecordUndo("Position");
                foreach (TransformPro transformPro in this)
                {
                    transformPro.PositionX = value;
                }
            }
        }

        /// <summary>
        ///     Sets all selected transform Y positions.
        /// </summary>
        public float PositionY
        {
            set
            {
                TransformProEditor.RecordUndo("Position");
                foreach (TransformPro transformPro in this)
                {
                    transformPro.PositionY = value;
                }
            }
        }

        /// <summary>
        ///     Sets all selected transform Z positions.
        /// </summary>
        public float PositionZ
        {
            set
            {
                TransformProEditor.RecordUndo("Position");
                foreach (TransformPro transformPro in this)
                {
                    transformPro.PositionZ = value;
                }
            }
        }

        private static bool CanGenerateCollider()
        {
            bool canGenerateCollider = false;
            if (TransformProEditor.Selected != null)
            {
                bool hasColliderSize = TransformProEditor.Selected.Any(x =>
                    (x.ColliderBounds != null) && (x.ColliderBounds.Local.size != Vector3.zero));
                bool hasRendererSize = TransformProEditor.Selected.Any(x =>
                    (x.RendererBounds != null) && (x.RendererBounds.Local.size != Vector3.zero));
                canGenerateCollider = hasRendererSize || hasColliderSize;
            }

            return canGenerateCollider;
        }

        /// <summary>
        ///     Nudge the X Position of all selected transforms by the exact amount provided.
        ///     If you want to nudge by the grid size that should be premultiplied into this amount.
        /// </summary>
        /// <param name="amount">The unity distance to Nudge the X Position of the selection by.</param>
        public void NudgePositionX(float amount)
        {
            TransformProEditor.RecordUndo("Position");
            foreach (TransformPro transformPro in this)
            {
                transformPro.PositionX += amount;
            }
        }

        /// <summary>
        ///     Nudge the Y Position of all selected transforms by the exact amount provided.
        ///     If you want to nudge by the grid size that should be premultiplied into this amount.
        /// </summary>
        /// <param name="amount">The unity distance to Nudge the Y Position of the selection by.</param>
        public void NudgePositionY(float amount)
        {
            TransformProEditor.RecordUndo("Position");
            foreach (TransformPro transformPro in this)
            {
                transformPro.PositionY += amount;
            }
        }

        /// <summary>
        ///     Nudge the Y Position of all selected transforms by the exact amount provided.
        ///     If you want to nudge by the grid size that should be premultiplied into this amount.
        /// </summary>
        /// <param name="amount">The unity distance to Nudge the Y Position of the selection by.</param>
        public void NudgePositionZ(float amount)
        {
            TransformProEditor.RecordUndo("Position");
            foreach (TransformPro transformPro in this)
            {
                transformPro.PositionZ += amount;
            }
        }

        /// <summary>
        ///     Draws the core position field.
        ///     This includes all three fields, the reset buttons, and everything relating to the advanced panel.
        /// </summary>
        public void PositionField()
        {
            // Ensure the current saved grid settings match the preferences.
            TransformPro.SnapPositionGrid = TransformProPreferences.SnapPositionGrid;
            TransformPro.SnapPositionOrigin = TransformProPreferences.SnapPositionOrigin;

            EditorGUILayout.BeginHorizontal(FrameBoxStyle);

            GUILayout.Label(TransformProStyles.Icons.PositionSmall, TransformProStyles.LabelSmallIcon,
                GUILayout.Width(16), GUILayout.Height(16));
            Rect toggleRect = GUILayoutUtility.GetLastRect();
            toggleRect.width = 20;
            TransformProPreferences.AdvancedPosition = EditorGUI.Foldout(toggleRect,
                TransformProPreferences.AdvancedPosition, GUIContent.none);

            EditorGUILayout.BeginHorizontal(PickedPixel);
            switch (TransformProEditor.SelectedCount)
            {
                case 0:
                    TransformProEditorCore.DrawAxis('x');
                    TransformProEditorCore.DrawAxis('y');
                    TransformProEditorCore.DrawAxis('z');
                    break;
                case 1:
                    Vector3 position = TransformProEditor.Selected.Single().Position;
                    this.PositionField(ref position);
                    break;
                default:
                    List<Vector3> inputs = TransformProEditor.Selected.Select(x => x.Position).ToList();
                    float axis;
                    if (TransformProEditorCore.DrawAxis('x', inputs.Select(v => v.x).ToList(), out axis))
                    {
                        this.PositionX = axis;
                    }

                    if (TransformProEditorCore.DrawAxis('y', inputs.Select(v => v.y).ToList(), out axis))
                    {
                        this.PositionY = axis;
                    }

                    if (TransformProEditorCore.DrawAxis('z', inputs.Select(v => v.z).ToList(), out axis))
                    {
                        this.PositionZ = axis;
                    }

                    break;
            }

            EditorGUILayout.EndHorizontal();

            GUI.enabled = TransformProEditor.CanAnyChangePosition;
            GUI.backgroundColor = TransformProStyles.ColorReset;
            GUI.contentColor = TransformProStyles.ColorAxisXDeep;
            if (GUILayout.Button("X", TransformProStyles.Buttons.IconTint.Left, GUILayout.Width(16)))
            {
                this.PositionX = 0;
            }

            GUI.contentColor = TransformProStyles.ColorAxisYDeep;
            if (GUILayout.Button("Y", TransformProStyles.Buttons.IconTint.Middle, GUILayout.Width(16)))
            {
                this.PositionY = 0;
            }

            GUI.contentColor = TransformProStyles.ColorAxisZDeep;
            if (GUILayout.Button("Z", TransformProStyles.Buttons.IconTint.Middle, GUILayout.Width(16)))
            {
                this.PositionZ = 0;
            }

            GUI.contentColor = Color.white;
            if (GUILayout.Button("0", TransformProStyles.Buttons.IconTint.Right, GUILayout.Width(16)))
            {
                this.Position = Vector3.zero;
            }

            
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            if (TransformProPreferences.AdvancedPosition)
            {
                this.DrawPositionAdvancedPanel();
            }
        }

        /// <summary>
        ///     Draws a simplified single value position field.
        ///     Only draws the three main fields, and does not reset the value unless a change is made.
        ///     This helps prevent drifting for the single inspector.
        /// </summary>
        /// <param name="position">The position to display. This value is also boxed and returned via the ref keyword.</param>
        public void PositionField(ref Vector3 position)
        {
            bool changed = false;
            changed |= TransformProEditorCore.DrawAxis('x', ref position.x);
            changed |= TransformProEditorCore.DrawAxis('y', ref position.y);
            changed |= TransformProEditorCore.DrawAxis('z', ref position.z);
            if (!changed)
            {
                return;
            }

            this.Position = position;
        }

        /// <summary>
        ///     Draws the advanced position panel.
        ///     This includes all nudges, and any specialized tools on the right.
        ///     A Gizmo stlye system could work well here to abstract responsibility back to the individual tool modules.
        /// </summary>
        private void DrawPositionAdvancedPanel()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            float[] savedNudges = TransformProPreferences.NudgesPosition.ToArray();
            foreach (float savedNudge in savedNudges)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("x{0}", savedNudge), TransformProStyles.LabelTiny, GUILayout.Width(29));
                this.DrawPositionAdvancedPanel(savedNudge);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            // ---- right side split

            EditorGUILayout.BeginVertical(GUILayout.Width(64));

            EditorGUILayout.BeginHorizontal(GUILayout.Width(64));
            GUIContent dropContent = new GUIContent(TransformProStyles.Icons.Drop,
                TransformProStrings.SystemLanguage.TooltipDrop);
            if (GUILayout.Button(dropContent, TransformProStyles.Buttons.Icon.Left))
            {
                int failed = TransformProEditor.Drop();
                if (failed > 0)
                {
                    // Debug.LogWarning(string.Format(
                    //     "[<color=red>TransformPro</color>] {0} drop operations failed.\nCould not find any colliders underneath the transform{1}.",
                    //     failed,
                    //     failed > 1 ? "s" : ""));
                    var sceneView = (SceneView) SceneView.sceneViews[0];
                    if (sceneView != null)
                    {
                        sceneView.ShowNotification(new GUIContent("掉落放置操作失败\n该对象下方无碰撞器"));
                    }
                }
            }

            GUIContent groundContent = new GUIContent(TransformProStyles.Icons.Ground,
                TransformProStrings.SystemLanguage.TooltipGround);
            if (GUILayout.Button(groundContent, TransformProStyles.Buttons.Icon.Right))
            {
                int failed = TransformProEditor.Ground();
                if (failed > 0)
                {
                    // Debug.LogWarning(string.Format(
                    //     "[<color=red>TransformPro</color>] {0} ground operations failed.\nCould not find colliders underneath the transform{1}.",
                    //     failed,
                    //     failed > 1 ? "s" : ""));
                    var sceneView = (SceneView) SceneView.sceneViews[0];
                    if (sceneView != null)
                    {
                        sceneView.ShowNotification(new GUIContent("触地放置操作失败\n该对象下方无碰撞器"));
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            // Generate colliders
            EditorGUILayout.BeginHorizontal(GUILayout.Width(64));
            bool canGenerateCollider = TransformProEditor.CanGenerateCollider();

            GUI.color = canGenerateCollider ? Color.white : new Color(0.4f, 0.4f, 0.4f);
            GUI.backgroundColor = canGenerateCollider ? Color.green : new Color(0.2f, 0.5f, 0.2f);
            GUI.enabled = canGenerateCollider;
            GUIContent createBoxLabel = new GUIContent(TransformProStyles.Icons.ColliderBox,
                TransformProStrings.SystemLanguage.CreateBoxCollider);
            if (GUILayout.Button(createBoxLabel, TransformProStyles.Buttons.Icon.Left, GUILayout.Width(32)))
            {
                TransformProEditor.CreateBoxCollider();
            }

            GUIContent createCapsuleLabel = new GUIContent(TransformProStyles.Icons.ColliderCapsule,
                TransformProStrings.SystemLanguage.CreateCapsuleCollider);
            if (GUILayout.Button(createCapsuleLabel, TransformProStyles.Buttons.Icon.Right, GUILayout.Width(32)))
            {
                TransformProEditor.CreateCapsuleCollider();
            }

            GUI.enabled = true;
            GUI.backgroundColor = Color.white;
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        /// <summary>
        ///     Draws a specific nudge for the position advanced panel.
        ///     Allows customisable nudges to be easily added.
        /// </summary>
        /// <param name="nudge">The nudge amount, in grid tiles.</param>
        private void DrawPositionAdvancedPanel(float nudge)
        {
            // TODO: Allow both grid tiles and units to be used.

            int nudgeResult = TransformProEditorCore.DrawNudges();
            if (nudgeResult > 0)
            {
                if (Event.current.shift || (Event.current.button == 1))
                {
                    Transform[] selectedAll = Selection.GetTransforms(SelectionMode.ExcludePrefab);
                    ICollection<Transform> clonedAll = selectedAll.Select(x => TransformProEditor.Clone(x)).ToList();
                    foreach (Transform cloned in clonedAll)
                    {
                        Undo.RegisterCreatedObjectUndo(cloned.gameObject, "Position");
                    }

                    TransformProEditor.Select(clonedAll);
                }
            }

            switch (nudgeResult)
            {
                case 1:
                    this.NudgePositionX(nudge * TransformPro.SnapPositionGrid.x * -1);
                    break;
                case 2:
                    this.NudgePositionX(nudge * TransformPro.SnapPositionGrid.x);
                    break;
                case 3:
                    this.NudgePositionY(nudge * TransformPro.SnapPositionGrid.y * -1);
                    break;
                case 4:
                    this.NudgePositionY(nudge * TransformPro.SnapPositionGrid.y);
                    break;
                case 5:
                    this.NudgePositionZ(nudge * TransformPro.SnapPositionGrid.z * -1);
                    break;
                case 6:
                    this.NudgePositionZ(nudge * TransformPro.SnapPositionGrid.z);
                    break;
            }
        }
    }
}