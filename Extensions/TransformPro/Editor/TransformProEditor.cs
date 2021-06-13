using System;
using System.Reflection;
using UnityEngine;

namespace TransformPro.Scripts
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityObject = UnityEngine.Object;

    /// <summary>
    ///     Extends the default Unity Transform inspector to add extra features.
    ///     More information can be found at http://transformpro.untitledgam.es
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Transform), true)]
    public partial class TransformProEditor : Editor, IEnumerable<TransformPro>
    {
        private static TransformProEditorCache cache;
        private static TransformProEditor instance;
        private static bool _IsDefaultGUIDrawing;
        private static Editor _defaultEditor;

        #region GUIStyle

        private static GUIStyle style_frameBoxStyle;

        public static GUIStyle FrameBoxStyle
        {
            get
            {
                if (style_frameBoxStyle == null) style_frameBoxStyle = new GUIStyle("FrameBox");
                return style_frameBoxStyle;
            }
        }

        private static GUIStyle style_greyBorder;

        public static GUIStyle GreyBorder
        {
            get
            {
                if (style_greyBorder == null)
                    style_greyBorder =
                        new GUIStyle("grey_border");
                return style_greyBorder;
            }
        }

        private static GUIStyle style_PickedPixel;

        public static GUIStyle PickedPixel
        {
            get
            {
                if (style_PickedPixel == null)
                    style_PickedPixel =
                        new GUIStyle("EyeDropperPickedPixel");
                return style_PickedPixel;
            }
        }

        private static GUIStyle style_NodeBox;

        public static GUIStyle NodeBox
        {
            get
            {
                if (style_NodeBox == null)
                    style_NodeBox =
                        new GUIStyle("TE NodeBox");
                return style_NodeBox;
            }
        }

        #endregion


        public static Vector3 AveragePosition
        {
            get { return TransformProEditor.Selected.Select(x => x.Position).Average(); }
        }

        public static Quaternion AverageRotation
        {
            get
            {
                if (TransformProEditor.SelectedCount == 1)
                {
                    return TransformProEditor.Selected.First().Rotation;
                }

                return TransformProEditor.Selected.Select(x => x.Rotation).Average();
            }
        }

        public static Vector3 AverageScale
        {
            get { return TransformProEditor.Selected.Select(x => x.Scale).Average(); }
        }

        public static TransformProEditorCache Cache
        {
            get { return TransformProEditor.cache ?? (TransformProEditor.cache = new TransformProEditorCache()); }
        }

        public static Camera Camera
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if (SceneView.currentDrawingSceneView != null)
                    {
                        if (SceneView.currentDrawingSceneView.camera != null)
                        {
                            return SceneView.currentDrawingSceneView.camera;
                        }
                    }
                }
#endif
                return Camera.main;
            }
        }

        public static bool CanAllChangePosition
        {
            get
            {
                return (TransformProEditor.Selected != null) &&
                       TransformProEditor.Selected.All(x => x.CanChangePosition);
            }
        }

        public static bool CanAllChangeRotation
        {
            get
            {
                return (TransformProEditor.Selected != null) &&
                       TransformProEditor.Selected.All(x => x.CanChangeRotation);
            }
        }

        public static bool CanAllChangeScale
        {
            get
            {
                return (TransformProEditor.Selected != null) && TransformProEditor.Selected.All(x => x.CanChangeScale);
            }
        }

        public static bool CanAnyChangePosition
        {
            get
            {
                return (TransformProEditor.Selected != null) &&
                       TransformProEditor.Selected.Any(x => x.CanChangePosition);
            }
        }

        public static bool CanAnyChangeRotation
        {
            get
            {
                return (TransformProEditor.Selected != null) &&
                       TransformProEditor.Selected.Any(x => x.CanChangeRotation);
            }
        }

        public static bool CanAnyChangeScale
        {
            get
            {
                return (TransformProEditor.Selected != null) && TransformProEditor.Selected.Any(x => x.CanChangeScale);
            }
        }

        public static TransformProClipboard Clipboard
        {
            get { return TransformProPreferences.Clipboard; }
        }

        public static TransformProEditor Instance
        {
            get { return TransformProEditor.instance; }
        }

        public static IEnumerable<TransformPro> Selected
        {
            get { return TransformProEditor.Cache.Selected; }
        }

        public static int SelectedClipboard
        {
            get { return TransformProPreferences.SelectedClipboard; }
            set { TransformProPreferences.SelectedClipboard = value; }
        }

        public static int SelectedCount
        {
            get { return TransformProEditor.Cache.SelectedCount; }
        }

        /// <inheritdoc />
        public IEnumerator<TransformPro> GetEnumerator()
        {
            return TransformProEditor.Cache.Selected.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return TransformProEditor.Cache.Selected.GetEnumerator();
        }

        public static Transform Clone(Transform transform)
        {
            GameObject gameObjectOld = transform.gameObject;
            GameObject gameObjectNew;
            if (transform.IsPrefab())
            {
#pragma warning disable 618
                UnityObject prefab = PrefabUtility.GetPrefabParent(gameObjectOld);
#pragma warning restore 618
                gameObjectNew = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            }
            else
            {
                gameObjectNew = UnityObject.Instantiate(gameObjectOld);
            }

            gameObjectNew.name = gameObjectOld.name; // Get rid of the (Clone)(Clone)(Clone)(Clone) madness

            Transform transformNew = gameObjectNew.transform;
            transformNew.SetParent(transform.parent);
            transformNew.localPosition = transform.localPosition;
            transformNew.localRotation = transform.localRotation;
            transformNew.localScale = transform.localScale;

            return transformNew;
        }

        public static int Drop()
        {
            TransformProEditor.RecordUndo("Drop");
            return TransformProEditor.Selected.OrderBy(x => x.Position.y).Count(transformPro => !transformPro.Drop());
        }

        public static int Ground()
        {
            TransformProEditor.RecordUndo("Ground");
            return TransformProEditor.Selected.OrderBy(x => x.Position.y).Count(transformPro => !transformPro.Ground());
        }

        public static void RecordUndo(string name)
        {
            Undo.RecordObjects(Selection.GetTransforms(SelectionMode.ExcludePrefab).Cast<UnityObject>().ToArray(),
                string.Format("TransformPro {0}", name));
        }

        public static void Select(IEnumerable<UnityObject> objects)
        {
            TransformProEditor.Cache.Select(objects);
        }

        public static void Select(IEnumerable<Transform> transforms)
        {
            TransformProEditor.Cache.Select(transforms.Cast<UnityObject>());
        }

        /// <summary>
        ///     Shows a form containing the preferences UI. Doesn't use the default Editor preferences due to being unable to
        ///     reflect the internal structs used to define preferences tabs.
        /// </summary>
        public static void ShowPreferences()
        {
            TransformProPreferences preferences = EditorWindow.GetWindow<TransformProPreferences>();
            preferences.Show();
        }

        //public static bool AutoSnap { get { return TransformProEditor.autoSnap; } set { TransformProEditor.autoSnap = value; } }

        public static void Snap()
        {
            TransformProEditor.RecordUndo("Snap Transform");
            foreach (TransformPro transform in TransformProEditor.Selected)
            {
                transform.SnapPosition();
                transform.SnapRotation();
            }
        }

        public static void SnapPosition()
        {
            TransformProEditor.RecordUndo("Snap Position");
            foreach (TransformPro transform in TransformProEditor.Selected)
            {
                transform.SnapPosition();
            }
        }

        public static void SnapRotation()
        {
            TransformProEditor.RecordUndo("Snap Rotation");
            foreach (TransformPro transform in TransformProEditor.Selected)
            {
                transform.SnapRotation();
            }
        }

        private static void CreateBoxCollider()
        {
            if (!TransformProEditor.CanGenerateCollider())
            {
                var sceneView = (SceneView) SceneView.sceneViews[0];
                if (sceneView)
                {
                    sceneView.ShowNotification(new GUIContent("无法生成盒形碰撞器\n对象没有碰撞器或渲染器的边界"));
                }

                //Debug.LogWarning("[<color=red>TransformPro</color>] Cannot generate a box collider. Object has no collider or renderer bounds.");
                return;
            }

            foreach (TransformPro transformPro in TransformProEditor.Selected)
            {
                BoxCollider boxCollider = Undo.AddComponent<BoxCollider>(transformPro.Transform.gameObject);
                if (transformPro.RendererBounds.Local.size.sqrMagnitude > 0)
                {
                    boxCollider.size = transformPro.RendererBounds.Local.size;
                    boxCollider.center = transformPro.RendererBounds.Local.center;
                }
                else if (transformPro.ColliderBounds.Local.size.sqrMagnitude > 0)
                {
                    boxCollider.size = transformPro.ColliderBounds.Local.size;
                    boxCollider.center = transformPro.ColliderBounds.Local.center;
                }
            }
        }

        private static void CreateCapsuleCollider()
        {
            if (!TransformProEditor.CanGenerateCollider())
            {
                var sceneView = (SceneView) SceneView.sceneViews[0];
                if (sceneView)
                {
                    sceneView.ShowNotification(new GUIContent("无法生成胶囊碰撞器\n对象没有碰撞器或渲染器的边界"));
                }

                //Debug.LogWarning("[<color=red>TransformPro</color>] Cannot generate a capsule collider. Object has no collider or renderer bounds.");
                return;
            }

            foreach (TransformPro transformPro in TransformProEditor.Selected)
            {
                CapsuleCollider capsuleCollider = Undo.AddComponent<CapsuleCollider>(transformPro.Transform.gameObject);
                if (transformPro.RendererBounds.Local.size.sqrMagnitude > 0)
                {
                    capsuleCollider.radius = Mathf.Max(transformPro.RendererBounds.Local.size.x,
                        transformPro.RendererBounds.Local.size.z) / 2;
                    capsuleCollider.height = transformPro.RendererBounds.Local.size.y;
                    capsuleCollider.center = transformPro.RendererBounds.Local.center;
                }
                else if (transformPro.ColliderBounds.Local.size.sqrMagnitude > 0)
                {
                    capsuleCollider.radius = Mathf.Max(transformPro.ColliderBounds.Local.size.x,
                        transformPro.ColliderBounds.Local.size.z) / 2;
                    capsuleCollider.height = transformPro.ColliderBounds.Local.size.y;
                    capsuleCollider.center = transformPro.ColliderBounds.Local.center;
                }
            }
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            if (TransformProEditor.SelectedCount == 0)
            {
                return;
            }

            this.serializedObject.Update();
            EditorGUILayout.BeginVertical(GreyBorder);
            this.InspectorGUI();
            EditorGUILayout.EndVertical();
            this.serializedObject.ApplyModifiedProperties();
        }


        // ReSharper disable Unity.PerformanceAnalysis
        private bool DefaultInspectorModeGUI()
        {
            //if (targets == null || targets.Length <= 0) return false;
            if (Instance && _defaultEditor == null)
            {
                var type = System.Type.GetType("UnityEditor.TransformInspector, UnityEditor");
                if (type != null)
                {
#if UNITY_EDITOR
                    CreateCachedEditor(targets, type, ref _defaultEditor);
#endif
                }

                _IsDefaultGUIDrawing = false;
                return _IsDefaultGUIDrawing;
            }

            GUIContent transModeSwitchContent = new GUIContent(
                _IsDefaultGUIDrawing ? TransformProStyles.Icons.Logo : TransformProStyles.Icons.Transform);

            if (GUILayout.Button(transModeSwitchContent, TransformProStyles.Buttons.IconLarge.Single,
                GUILayout.Width(28)))
            {
                _IsDefaultGUIDrawing = !_IsDefaultGUIDrawing;
            }

            if (_IsDefaultGUIDrawing && _defaultEditor)
            {
                EditorGUILayout.BeginVertical(FrameBoxStyle);
                // ReSharper disable once Unity.NoNullPropagation
#if UNITY_EDITOR
                _defaultEditor.OnInspectorGUI();
#endif


                EditorGUILayout.EndVertical();
            }

            return _IsDefaultGUIDrawing;
        }


        // internal static bool DoDrawDefaultTransformInspector(SerializedObject obj)
        // {
        //     EditorGUI.BeginChangeCheck();
        //     obj.UpdateIfRequiredOrScript();
        //     SerializedProperty iterator = obj.GetIterator();
        //     for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
        //     {
        //         using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
        //             EditorGUILayout.PropertyField(iterator, true);
        //     }
        //     obj.ApplyModifiedProperties();
        //     return EditorGUI.EndChangeCheck();
        // }


        private void InspectorGUI()
        {
            if (!TransformProStyles.Load())
            {
                return;
            }

            GUISkin resetSkin = GUI.skin;
            GUI.skin = TransformProStyles.Skin;

            // Rect brandingRect = EditorGUILayout.GetControlRect(false, 0);
            // brandingRect.y -= 16;
            // brandingRect.x += 103;
            // brandingRect.width = 72;
            // brandingRect.height = 20;
            // GUI.color = new Color(0.8f, 0.8f, 0.8f, 0.8f);
            // GUI.Label(brandingRect, "Pro");
            // GUI.color = Color.white;


            // if (TransformProStyles.Icons.Icon != null)
            // {
            //     Rect iconRect = new Rect(brandingRect)
            //                     {
            //                         x = brandingRect.x - 101,
            //                         width = 16,
            //                         height = 16
            //                     };
            //     Color backgroundColor = EditorGUIUtility.isProSkin ? new Color32(56, 56, 56, 255) : new Color32(194, 194, 194, 255);
            //     EditorGUI.DrawRect(iconRect, backgroundColor);
            //     GUI.DrawTexture(iconRect, TransformProStyles.Icons.Icon);
            // }

            EditorGUILayout.BeginHorizontal(GreyBorder);
            if (DefaultInspectorModeGUI())
            {
                EditorGUILayout.EndHorizontal();
                return;
            }


            // Preferences cog (move to prefs class?
            GUIContent preferencesContent = new GUIContent(TransformProStyles.Icons.Cog,
                TransformProStrings.SystemLanguage.TooltipPreferences);
            GUI.contentColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            if (GUILayout.Button(preferencesContent, TransformProStyles.Buttons.Icon.Single, GUILayout.Width(28)))
            {
                if (Event.current.shift || (Event.current.button == 1))
                {
                    TransformProEditor.Cache.Clear();
                }
                else
                {
                    TransformProEditor.ShowPreferences();
                }
            }

            GUI.contentColor = Color.white;

            // Space mode controls
            TransformProEditorHandles.DrawGUI(this);

            // Reset button main
            GUI.backgroundColor = TransformProStyles.ColorReset;
            GUI.contentColor = Color.white;
            GUIContent resetContent = new GUIContent("Reset", TransformProStrings.SystemLanguage.TooltipResetTransform);
            if (GUILayout.Button(resetContent, TransformProStyles.Buttons.IconTint.Single, GUILayout.Width(64)))
            {
                TransformProEditor.RecordUndo("Reset");
                foreach (TransformPro transform in this)
                {
                    transform.Reset();
                }
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            // Position, Rotation, Scale
            EditorGUILayout.BeginVertical(FrameBoxStyle);
            this.PositionField();
            this.RotationField();
            this.ScaleField();
            EditorGUILayout.EndVertical();

            GUI.skin = resetSkin;
        }

        /// <summary>
        ///     Deregister events.
        /// </summary>
        private void OnDisable()
        {
            TransformProEditor.instance = null;

            Undo.undoRedoPerformed -= this.UndoRedoPerformed;
        }

        /// <summary>
        ///     Ensure everything is loaded and register events.
        /// </summary>
        private void OnEnable()
        {
            TransformProEditor.instance = this;
            // why doesnt prefs handle this check? expose a force bool if needed
            if (!TransformProPreferences.AreLoaded)
            {
                TransformProPreferences.Load();
            }

            Undo.undoRedoPerformed += this.UndoRedoPerformed;
        }


        /// <summary>
        ///     <see cref="Undo.undoRedoPerformed" /> delegate. Ensures the bounds are recalculated when Undo or Redo are used.
        /// </summary>
        private void UndoRedoPerformed()
        {
            foreach (TransformPro transform in this)
            {
                transform.SetComponentsDirty();
            }
        }
    }
}