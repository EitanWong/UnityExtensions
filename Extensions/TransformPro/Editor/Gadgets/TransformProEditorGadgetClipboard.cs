namespace TransformPro.Scripts
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    ///     Clipboard Gadget.
    ///     Handles copying and pasting transformation data.
    ///     Also draws the interface allowing you to pick from up to 5 clipboard.
    ///     Also draws the scene view 3d cursor based on the currently selected clipboard.
    /// </summary>
    public class TransformProEditorGadgetClipboard : ITransformProGadgetSceneHandles, ITransformProGadgetSceneClick, ITransformProGadgetPanel
    {
        /// <summary>
        ///     Color used for the unselected clipboard button.
        /// </summary>
        private readonly Color colorLight = new Color(0.6f, 0.6f, 0.6f);

        /// <summary>
        ///     Color used for the selected clipboard button.
        /// </summary>
        private readonly Color colorDark = new Color(0.2f, 0.2f, 0.2f);

        // <inheritdoc />
        public float Height { get { return 34; } }

        /// <inheritdoc />
        public void DrawPanelGUI(SceneView sceneView, TransformProEditorGadgets gadgets, Rect rect)
        {
            float tabWidth = rect.width / 4;
            GUI.backgroundColor = Event.current.shift ? TransformProStyles.ColorPaste : TransformProStyles.ColorCopy;

            Rect tab = new Rect(rect.x, rect.y, tabWidth, 19);
            GUIContent copyTransformContent = new GUIContent(TransformProStyles.Icons.Clipboard, TransformProStrings.SystemLanguage.TooltipCopyPasteTransform);
            if (GUI.Button(tab, copyTransformContent, TransformProStyles.Buttons.IconPadded.Left))
            {
                TransformProEditorGadgetClipboard.CopyPaste(true, true, true);
            }

            tab.x += tabWidth;
            GUIContent copyPositionContent = new GUIContent(TransformProStyles.Icons.Position, TransformProStrings.SystemLanguage.TooltipCopyPastePosition);
            if (GUI.Button(tab, copyPositionContent, TransformProStyles.Buttons.IconPadded.Middle))
            {
                TransformProEditorGadgetClipboard.CopyPaste(true, false, false);
            }

            tab.x += tabWidth;
            GUIContent copyRotationContent = new GUIContent(TransformProStyles.Icons.Rotation, TransformProStrings.SystemLanguage.TooltipCopyPasteRotation);
            if (GUI.Button(tab, copyRotationContent, TransformProStyles.Buttons.IconPadded.Middle))
            {
                TransformProEditorGadgetClipboard.CopyPaste(false, true, false);
            }

            tab.x += tabWidth;
            GUIContent copyScaleContent = new GUIContent(TransformProStyles.Icons.Scale, TransformProStrings.SystemLanguage.TooltipCopyPasteScale);
            if (GUI.Button(tab, copyScaleContent, TransformProStyles.Buttons.IconPadded.Right))
            {
                TransformProEditorGadgetClipboard.CopyPaste(false, false, true);
            }

            GUI.backgroundColor = Color.white;

            tabWidth = (rect.width - 2) / 5;
            Rect rectClipboard = new Rect(rect)
                                 {
                                     yMin = rect.yMin + 21,
                                     width = tabWidth,
                                     height = 12
                                 };
            int index = 0;
            for (int row = 0; row < 1; row++)
            {
                rectClipboard.x = rect.x + 1;

                for (int column = 0; column < 5; column++)
                {
                    GUIStyle style;
                    switch (column)
                    {
                        case 0:
                            style = TransformProStyles.Buttons.Tiny.Left;
                            break;
                        case 4:
                            style = TransformProStyles.Buttons.Tiny.Right;
                            break;
                        default:
                            style = TransformProStyles.Buttons.Tiny.Middle;
                            break;
                    }

                    GUI.backgroundColor = TransformProEditor.SelectedClipboard == index ? Color.white : this.colorDark;
                    GUI.contentColor = TransformProEditor.SelectedClipboard == index ? this.colorDark : this.colorLight;
                    if (GUI.Button(rectClipboard, (index + 1).ToString(), style))
                    {
                        TransformProEditor.SelectedClipboard = index;
                        if (Event.current.shift || (Event.current.button == 1))
                        {
                            TransformProEditor.Clipboard.Reset();
                        }
                    }
                    GUI.backgroundColor = Color.white;
                    GUI.contentColor = Color.white;
                    index++;

                    rectClipboard.x += tabWidth;
                }

                rectClipboard.y += rectClipboard.height + 1;
            }
        }

        /// <inheritdoc />
        public int Sort { get { return 5; } }

        /// <inheritdoc />
        public bool SceneClick(SceneView sceneView, TransformProEditorGadgets gadgets, Vector2 mousePosition, bool executeClick)
        {
            if (!Event.current.control || (Event.current.button != 1))
            {
                return false;
            }

            if (executeClick)
            {
                Camera camera = TransformProEditor.Camera;
                Vector3 screenPosition = new Vector3(mousePosition.x, camera.pixelHeight - mousePosition.y, 0);
                Ray screenRay = camera.ScreenPointToRay(screenPosition);

                RaycastHit hitInfo;
                if (Physics.Raycast(screenRay, out hitInfo))
                {
                    //Quaternion rotation = new AngleAxis(0, hitInfo.normal).ToQuaternion();
                    Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal) * Quaternion.identity;
                    TransformProEditor.Clipboard.Copy(hitInfo.point, rotation, Vector3.one);
                    TransformProPreferences.SaveClipboard();
                    return true;
                }

                Plane plane = new Plane(Vector3.up, Vector3.zero);
                float distance;
                Vector3 position = screenRay.origin;
                if (plane.Raycast(screenRay, out distance))
                {
                    position += screenRay.direction * distance;
                    TransformProEditor.Clipboard.Copy(position, Quaternion.identity, Vector3.one);
                    TransformProPreferences.SaveClipboard();
                    return true;
                }

                position += screenRay.direction * 25;
                TransformProEditor.Clipboard.Copy(position, Quaternion.identity, Vector3.one);
                TransformProPreferences.SaveClipboard();
                return true;
            }

            return true;
        }

        /// <inheritdoc />
        public bool Enabled { get { return true; } }

        /// <inheritdoc />
        public void DrawSceneHandles(SceneView sceneView, TransformProEditorGadgets gadgets)
        {
            Quaternion rotation = TransformProEditor.Clipboard.Rotation * Quaternion.LookRotation(Vector3.up);
            Vector3 rotationEuler = TransformProEditor.Clipboard.Rotation * Vector3.up;

            Vector3 position = TransformProEditor.Clipboard.Position;
            Vector3 positionTop = position + (rotationEuler * 0.25f);

            float size = (HandleUtility.WorldToGUIPoint(position) - HandleUtility.WorldToGUIPoint(positionTop)).magnitude / 8f;

            Handles.color = TransformProStyles.ColorClipboard;
            Handles.DrawSolidDisc(position, rotationEuler, 0.2f);
            Handles.DrawAAPolyLine(Texture2D.whiteTexture, size, position, positionTop);
#if UNITY_5_5_OR_NEWER
            Handles.ConeHandleCap(0, positionTop, rotation, 0.2f, EventType.Repaint);
#else
            Handles.ConeCap(0, positionTop, rotation, 0.1f);
#endif
        }

        /// <summary>
        ///     Copies or pastes the currently selected transform, based on the current keyboard or mouse event data.
        ///     Use shift click or right click to paste.
        ///     If you copy and multiple transforms are selected, the average position is used.
        /// </summary>
        /// <param name="includePosition">Copy/Paste position data.</param>
        /// <param name="includeRotation">Copy/Paste rotation data.</param>
        /// <param name="includeScale">Copy/Paste scale data.</param>
        private static void CopyPaste(bool includePosition, bool includeRotation, bool includeScale)
        {
            string operation = TransformProEditorGadgetClipboard.GetOperationString(includePosition, includeRotation, includeScale);

            if (Event.current.shift || (Event.current.button == 1))
            {
                TransformProEditor.RecordUndo(string.Format("Paste {0}", operation));
                if (TransformProEditor.SelectedCount == 1)
                {
                    // Single paste kept like this to avoid any floating issues with the more complex group paste system.
                    TransformPro transform = TransformProEditor.Selected.First();
                    if (transform.CanChangePosition || transform.CanChangeRotation || transform.CanChangeScale)
                    {
                        if (includePosition)
                        {
                            TransformProEditor.Clipboard.PastePosition(transform);
                        }
                        if (includeRotation)
                        {
                            TransformProEditor.Clipboard.PasteRotation(transform);
                        }
                        if (includeScale)
                        {
                            TransformProEditor.Clipboard.PasteScale(transform);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[<color=red>TransformPro</color>] Could not paste full transform.");
                    }
                }
                else
                {
                    TransformProEditorGadgetClipboard.PasteGroup(TransformProEditor.Selected, includePosition, includeRotation, includeScale);
                }
            }
            else
            {
                if (includePosition)
                {
                    TransformProEditor.Clipboard.CopyPosition(TransformProEditor.AveragePosition);
                }
                if (includeRotation)
                {
                    TransformProEditor.Clipboard.CopyRotation(TransformProEditor.AverageRotation);
                }
                if (includeScale)
                {
                    TransformProEditor.Clipboard.CopyScale(TransformProEditor.AverageScale);
                }
                TransformProPreferences.SaveClipboard();
            }
        }

        /// <summary>
        ///     Gets the string to be used for the Undo operation.
        ///     If all three are copied or pasted, this will return "Transform".
        ///     If a single core component is copied or pasted, the single core component name will be returned directly.
        ///     If two core components are copied or pasted, the two names will be joined together with an ampersand.
        /// </summary>
        /// <param name="includePosition">Include Position name in result.</param>
        /// <param name="includeRotation">Include Rotation name in result.</param>
        /// <param name="includeScale">Include Scale name in result.</param>
        /// <returns>A string indicating whether </returns>
        private static string GetOperationString(bool includePosition, bool includeRotation, bool includeScale)
        {
            if (includePosition && includeRotation && includeScale)
            {
                return "Transform";
            }

            List<string> operations = new List<string>();
            if (includePosition)
            {
                operations.Add("Position");
            }
            if (includeRotation)
            {
                operations.Add("Rotation");
            }
            if (includeScale)
            {
                operations.Add("Scale");
            }
            return string.Join(" & ", operations.ToArray());
        }

        private static void PasteGroup(IEnumerable<TransformPro> transforms, bool includePosition, bool includeRotation, bool includeScale)
        {
            // First get the averages of the current group. We are going to move this point while retaining the individual deltas.
            // Note that the averaging system copies the currently selected space, so it is still possibly shift group transforms into different spatial systems.
            Vector3 averagePosition = TransformProEditor.AveragePosition;
            Vector3 deltaPosition = TransformProEditor.Clipboard.Position - averagePosition;

            Quaternion averageRotation = TransformProEditor.AverageRotation;
            Quaternion deltaRotation = TransformProEditor.Clipboard.Rotation * averageRotation;

            Vector3 averageScale = TransformProEditor.AverageScale;
            Vector3 deltaScale = new Vector3(TransformProEditor.Clipboard.Scale.x / averageScale.x,
                                             TransformProEditor.Clipboard.Scale.y / averageScale.y,
                                             TransformProEditor.Clipboard.Scale.z / averageScale.z);

            Matrix4x4 matrix = Matrix4x4.TRS(deltaPosition, deltaRotation, deltaScale);

            foreach (TransformPro transform in transforms)
            {
                transform.Position = matrix.MultiplyPoint3x4(transform.Position);
                transform.Rotation = transform.Rotation * matrix.ToQuaternion();
                transform.Scale = Vector3.Scale(transform.Scale, deltaScale);
            }
        }
    }
}
