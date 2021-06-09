namespace TransformPro.Scripts
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    ///     Bounds Gadget.
    ///     Provides a UI to enable/disable the bounds, as well as switching between local and world
    ///     space visualisation.
    ///     Also handles drawing the actual bounds to the scene.
    /// </summary>
    public class TransformProEditorGadgetBounds
        : ITransformProGadgetPanel, ITransformProGadgetSceneHandles
    {
        /// <summary>
        ///     Stores a value indicating whether the bounds render limit was reached.
        ///     Used to display the warning icon on the panel interface.
        /// </summary>
        private bool boundsDrawLimitExceeded;

        /// <inheritdoc />
        public bool Enabled { get { return TransformProPreferences.EnableBounds; } }

        /// <inheritdoc />
        public float Height { get { return 21; } }

        /// <inheritdoc />
        public int Sort { get { return 1; } }

        /// <inheritdoc />
        public void DrawPanelGUI(SceneView sceneView, TransformProEditorGadgets gadgets, Rect rect)
        {
            float tabWidth = rect.width / 3;

            Rect tab = new Rect(rect.x, rect.y, tabWidth, 19);
            GUIContent showRendererContent = new GUIContent(TransformProStyles.Icons.Renderer, TransformProStrings.SystemLanguage.TooltipVisualiseRenderers);
            if (gadgets.Toggle(tab, showRendererContent, TransformProStyles.Buttons.IconPadded.Left, TransformProPreferences.ShowRendererBounds))
            {
                TransformProPreferences.ShowRendererBounds = !TransformProPreferences.ShowRendererBounds;
                //TransformProAnnotationUtility.UpdateRendererGizmoVisibility();
            }

            tab.x += tabWidth;
            GUIContent showColliderContent = new GUIContent(TransformProStyles.Icons.Collider, TransformProStrings.SystemLanguage.TooltipVisualiseColliders);
            if (gadgets.Toggle(tab, showColliderContent, TransformProStyles.Buttons.IconPadded.Middle, TransformProPreferences.ShowColliderBounds))
            {
                TransformProPreferences.ShowColliderBounds = !TransformProPreferences.ShowColliderBounds;
                //TransformProAnnotationUtility.UpdateColliderGizmoVisibility();
            }

            tab.x += tabWidth;
            tab.xMax = rect.xMax;
            GUIContent showMeshesContent = new GUIContent(TransformProPreferences.BoundsDisplayMode == BoundsDisplayMode.World
                                                              ? TransformProStyles.Icons.ToolHandleGlobal
                                                              : TransformProStyles.Icons.ToolHandleLocal,
                                                          TransformProStrings.SystemLanguage.TooltipVisualiseBoundsSpaceMode);
            if (gadgets.Toggle(tab, showMeshesContent, TransformProStyles.Buttons.IconPadded.Right, TransformProPreferences.ShowColliderBounds || TransformProPreferences.ShowRendererBounds))
            {
                TransformProPreferences.BoundsDisplayMode = TransformProPreferences.BoundsDisplayMode == BoundsDisplayMode.World
                                                                ? BoundsDisplayMode.Local
                                                                : BoundsDisplayMode.World;
            }

            if (this.boundsDrawLimitExceeded)
            {
                Rect warningRect = new Rect(rect) {xMin = rect.xMin - 18};
                warningRect.height = warningRect.width = 20;
                GUIContent warningContent = new GUIContent(TransformProStyles.Icons.Warning, TransformProStrings.SystemLanguage.BoundsDrawCallWarning);
                GUI.Box(warningRect, warningContent, TransformProStyles.LabelTiny);
            }
        }

        /// <inheritdoc />
        public void DrawSceneHandles(SceneView sceneView, TransformProEditorGadgets gadgets)
        {
            if (TransformProEditor.SelectedCount == 0)
            {
                return;
            }

            this.boundsDrawLimitExceeded = false;

            int boundingCount = 0;
            foreach (TransformPro transformPro in TransformProEditor.Cache.Selected)
            {
                TransformProEditorGadgetBounds.DrawHandles(transformPro);

                boundingCount++;
                if (boundingCount <= TransformProPreferences.MaximumBoundsDrawn)
                {
                    continue;
                }

                this.boundsDrawLimitExceeded = true;
                break;
            }
        }

        /// <summary>
        ///     Draws the handles for a given <see cref="TransformPro" /> instance.
        /// </summary>
        /// <param name="transformPro">The <see cref="TransformPro" /> instance to draw the handles for.</param>
        private static void DrawHandles(TransformPro transformPro)
        {
            if (!TransformPro.CalculateBounds)
            {
                return;
            }

            bool drawRenderer = TransformProPreferences.ShowRendererBounds && (transformPro.RendererBounds != null);
            bool drawCollider = TransformProPreferences.ShowColliderBounds && (transformPro.ColliderBounds != null);

            if (drawRenderer)
            {
                Color handleColor = TransformProStyles.ColorRenderer;
                handleColor.a = 0.5f;
                Handles.color = handleColor;
                if ((TransformProPreferences.BoundsDisplayMode == BoundsDisplayMode.World) || (TransformProPreferences.BoundsDisplayMode == BoundsDisplayMode.Both))
                {
                    TransformProDrawing.WireCube(transformPro.RendererBounds.World.center, transformPro.RendererBounds.World.size);
                }
                if ((TransformProPreferences.BoundsDisplayMode == BoundsDisplayMode.Local) || (TransformProPreferences.BoundsDisplayMode == BoundsDisplayMode.Both))
                {
                    if (transformPro.Transform != null)
                    {
                        TransformProDrawing.WireCube(transformPro.RendererBounds.Local.center, transformPro.RendererBounds.Local.size, transformPro.Transform.localToWorldMatrix);
                    }
                }
            }

            if (drawCollider)
            {
                Color handleColor = TransformProStyles.ColorCollider;
                handleColor.a = 0.5f;
                Handles.color = handleColor;
                if ((TransformProPreferences.BoundsDisplayMode == BoundsDisplayMode.World) || (TransformProPreferences.BoundsDisplayMode == BoundsDisplayMode.Both))
                {
                    TransformProDrawing.WireCube(transformPro.ColliderBounds.World.center, transformPro.ColliderBounds.World.size);
                }
                if ((TransformProPreferences.BoundsDisplayMode == BoundsDisplayMode.Local) || (TransformProPreferences.BoundsDisplayMode == BoundsDisplayMode.Both))
                {
                    if (transformPro.Transform != null)
                    {
                        TransformProDrawing.WireCube(transformPro.ColliderBounds.Local.center, transformPro.ColliderBounds.Local.size, transformPro.Transform.localToWorldMatrix);
                    }
                }
                Handles.color = Color.white;
            }
        }
    }
}
