namespace TransformPro.Scripts
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    ///     The main settings for <see cref="TransformPro" />. These are saved using the standard <see cref="EditorPrefs" />
    ///     system.
    ///     This class extends <see cref="EditorWindow" /> to allow us to pop it up as a seperate form from the cog icon.
    /// </summary>
    public class TransformProPreferences : EditorWindow
    {
        private const int ClipboardCount = 5;

        private static Color _sceneViewPanelColor;
        private static Color _sceneViewGUIColor;

        private static readonly Color SceneViewPanelColorDefault =
            new Color(56 / 255f, 56 / 255f, 56 / 255f, 128 / 255f);

        private static readonly Color SceneViewGUIColorDefault = Color.white;

        private static bool advancedPosition;
        private static bool advancedRotation;
        private static bool advancedScale;
        private static bool areLoaded;
        private static int boundsDisplayMode;
        private static bool enableBounds;
        private static int maximumBoundsDrawn;
        private static bool scalarScale;
        private static Vector2 scrollPosition;
        private static bool showColliderBounds;
        private static bool showGadgets;
        private static bool showRendererBounds;
        private static Vector3 snapPositionGrid;
        private static Vector3 snapPositionOrigin;
        private static Vector3 snapRotationGrid;
        private static Vector3 snapRotationOrigin;

        private static TransformProClipboard[] clipboards =
            new TransformProClipboard[TransformProPreferences.ClipboardCount];

        private static bool? installationProblems;

        private static NudgeSettingsPosition nudgesPosition;
        private static NudgeSettingsRotation nudgesRotation;

        private static int selectedClipboard;


        public static Color SceneViewPanelColor
        {
            get { return TransformProPreferences._sceneViewPanelColor; }
            set
            {
                TransformProPreferences._sceneViewPanelColor = value;
                SetColor(nameof(SceneViewPanelColor), value);
            }
        }


        public static Color SceneViewGUIColor
        {
            get { return TransformProPreferences._sceneViewGUIColor; }
            set
            {
                TransformProPreferences._sceneViewGUIColor = value;
                SetColor(nameof(SceneViewGUIColor), value);
            }
        }

        /// <summary>
        ///     A value indicating whether the advanced position panel is currently open.
        /// </summary>
        public static bool AdvancedPosition
        {
            get { return TransformProPreferences.advancedPosition; }
            set
            {
                TransformProPreferences.advancedPosition = value;
                TransformProPreferences.SetBool("advancedPosition", value);
            }
        }

        /// <summary>
        ///     A value indicating whether the advanced rotation panel is currently open.
        /// </summary>
        public static bool AdvancedRotation
        {
            get { return TransformProPreferences.advancedRotation; }
            set
            {
                TransformProPreferences.advancedRotation = value;
                TransformProPreferences.SetBool("advancedRotation", value);
            }
        }

        /// <summary>
        ///     A value indicating whether the advanced scale panel is currently open.
        /// </summary>
        public static bool AdvancedScale
        {
            get { return TransformProPreferences.advancedScale; }
            set
            {
                TransformProPreferences.advancedScale = value;
                TransformProPreferences.SetBool("advancedScale", value);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the preferences are currently loaded.
        /// </summary>
        public static bool AreLoaded
        {
            get { return TransformProPreferences.areLoaded; }
        }

        public static BoundsDisplayMode BoundsDisplayMode
        {
            get { return (BoundsDisplayMode) TransformProPreferences.boundsDisplayMode; }
            set
            {
                int enumValue = (int) value;
                TransformProPreferences.boundsDisplayMode = enumValue;
                TransformProPreferences.SetInt("boundsDisplayMode", enumValue);
            }
        }

        public static TransformProClipboard Clipboard
        {
            get { return TransformProPreferences.GetClipboard(TransformProPreferences.SelectedClipboard); }
        }

        /// <summary>
        ///     Gets or sets a values indicating whether to calculate bounds for the selected <see cref="Transform" /> objects.
        ///     Setting this value to true may cause bounds to be recalculated immediately.
        /// </summary>
        public static bool EnableBounds
        {
            get { return TransformProPreferences.enableBounds; }
            set
            {
                TransformProPreferences.enableBounds = value;
                TransformProPreferences.SetBool("enableBounds", value);

                TransformPro.CalculateBounds = value;
                SceneView.RepaintAll();
            }
        }

        public static int MaximumBoundsDrawn
        {
            get { return TransformProPreferences.maximumBoundsDrawn; }
            set
            {
                TransformProPreferences.maximumBoundsDrawn = value;
                TransformProPreferences.SetInt("maximumBoundsDrawn", value);
            }
        }

        public static NudgeSettingsPosition NudgesPosition
        {
            get
            {
                return TransformProPreferences.nudgesPosition ??
                       (TransformProPreferences.nudgesPosition = new NudgeSettingsPosition());
            }
        }

        public static NudgeSettingsRotation NudgesRotation
        {
            get
            {
                return TransformProPreferences.nudgesRotation ??
                       (TransformProPreferences.nudgesRotation = new NudgeSettingsRotation());
            }
        }

        /// <summary>
        ///     Should the scale edit 3 seperate axis or a single value
        /// </summary>
        public static bool ScalarScale
        {
            get { return TransformProPreferences.scalarScale; }
            set
            {
                TransformProPreferences.scalarScale = value;
                TransformProPreferences.SetBool("scalarScale", value);
            }
        }

        public static int SelectedClipboard
        {
            get { return TransformProPreferences.selectedClipboard; }
            set
            {
                TransformProPreferences.selectedClipboard =
                    Math.Max(0, Math.Min(TransformProPreferences.ClipboardCount - 1, value));
                TransformProPreferences.SetInt("selectedClipboard", TransformProPreferences.selectedClipboard);
            }
        }

        /// <summary>Gets or sets a value indicating whether <see cref="Collider" /> bounds should be shown in the viewport.</summary>
        public static bool ShowColliderBounds
        {
            get { return TransformProPreferences.showColliderBounds; }
            set
            {
                TransformProPreferences.showColliderBounds = value;
                TransformProPreferences.SetBool("showColliderBounds", value);
            }
        }

        /// <summary>Gets or sets a value indicating whether <see cref="Collider" /> bounds should be shown in the viewport.</summary>
        public static bool ShowGadgets
        {
            get { return TransformProPreferences.showGadgets; }
            set
            {
                TransformProPreferences.showGadgets = value;
                TransformProPreferences.SetBool("showGadgets", value);
                SceneView.RepaintAll();
            }
        }

        /// <summary>Gets or sets a value indicating whether <see cref="Renderer" /> bounds should be shown in the viewport.</summary>
        public static bool ShowRendererBounds
        {
            get { return TransformProPreferences.showRendererBounds; }
            set
            {
                TransformProPreferences.showRendererBounds = value;
                TransformProPreferences.SetBool("showRendererBounds", value);
            }
        }

        /// <summary>Gets the snap positioning grid.</summary>
        public static Vector3 SnapPositionGrid
        {
            get { return TransformProPreferences.snapPositionGrid; }
            set
            {
                TransformProPreferences.snapPositionGrid = value;
                TransformProPreferences.SetVector3("snapPositionGrid", value);
            }
        }

        /// <summary>Gets the X axis component of the snap positioning origin.</summary>
        public static Vector3 SnapPositionOrigin
        {
            get { return TransformProPreferences.snapPositionOrigin; }
            set
            {
                TransformProPreferences.snapPositionOrigin = value;
                TransformProPreferences.SetVector3("snapPositionOrigin", value);
            }
        }

        /// <summary>Gets the X axis component of the snap rotation grid.</summary>
        public static Vector3 SnapRotationGrid
        {
            get { return TransformProPreferences.snapRotationGrid; }
            set
            {
                TransformProPreferences.snapRotationGrid = value;
                TransformProPreferences.SetVector3("snapRotationGrid", value);
            }
        }

        /// <summary>Gets the X axis component of the snap rotation origin.</summary>
        public static Vector3 SnapRotationOrigin
        {
            get { return TransformProPreferences.snapRotationOrigin; }
            set
            {
                TransformProPreferences.snapRotationOrigin = value;
                TransformProPreferences.SetVector3("snapRotationOrigin", value);
            }
        }

        public static TransformProClipboard GetClipboard(int index)
        {
            if ((index < 0) || (index > (TransformProPreferences.ClipboardCount - 1)))
            {
                Debug.LogError(string.Format(
                    "[<color=red>TransformPro</color>] Clipboard index '' out of bounds. 10 editor clipboards are available.",
                    index));
                return null;
            }

            return TransformProPreferences.clipboards[index] ??
                   (TransformProPreferences.clipboards[index] = new TransformProClipboard());
        }

        /// <summary>
        ///     Loads the settings and applies them.
        /// </summary>
        public static void Load()
        {
            TransformProPreferences.advancedPosition = TransformProPreferences.GetBool("advancedPosition", false);
            TransformProPreferences.advancedRotation = TransformProPreferences.GetBool("advancedRotation", false);
            TransformProPreferences.advancedScale = TransformProPreferences.GetBool("advancedScale", false);
            TransformProPreferences.scalarScale = TransformProPreferences.GetBool("scalarScale", true);

            TransformProPreferences.snapPositionGrid =
                TransformProPreferences.GetVector3("snapPositionGrid", Vector3.one);
            TransformProPreferences.snapPositionOrigin =
                TransformProPreferences.GetVector3("snapPositionOrigin", Vector3.zero);
            TransformProPreferences.snapRotationGrid =
                TransformProPreferences.GetVector3("snapRotationGrid", new Vector3(90, 90, 90));
            TransformProPreferences.snapRotationOrigin =
                TransformProPreferences.GetVector3("snapRotationOrigin", Vector3.zero);

            TransformProPreferences.enableBounds = TransformProPreferences.GetBool("enableBounds", true);
            TransformProPreferences.boundsDisplayMode = TransformProPreferences.GetInt("boundsDisplayMode", 0);
            TransformProPreferences.maximumBoundsDrawn = TransformProPreferences.GetInt("maximumBoundsDrawn", 128);

            TransformProPreferences.showRendererBounds = TransformProPreferences.GetBool("showRendererBounds", true);
            TransformProPreferences.showColliderBounds = TransformProPreferences.GetBool("showColliderBounds", true);

            TransformProPreferences.showGadgets = TransformProPreferences.GetBool("showGadgets", true);

            TransformProPreferences.SceneViewGUIColor = GetColor(nameof(SceneViewGUIColor), SceneViewGUIColorDefault);
            TransformProPreferences.SceneViewPanelColor = GetColor(nameof(SceneViewPanelColor), SceneViewPanelColorDefault);

            TransformProPreferences.clipboards = new TransformProClipboard[TransformProPreferences.ClipboardCount];
            for (int clipboardIndex = 0; clipboardIndex < TransformProPreferences.ClipboardCount; clipboardIndex++)
            {
                TransformProClipboard clipboard = new TransformProClipboard
                {
                    Position = TransformProPreferences.GetVector3(
                        string.Format("clipboard{0}.Position", clipboardIndex), Vector3.zero),
                    Rotation = TransformProPreferences.GetQuaternion(
                        string.Format("clipboard{0}.Rotation", clipboardIndex), Quaternion.identity),
                    Scale = TransformProPreferences.GetVector3(string.Format("clipboard{0}.Scale", clipboardIndex),
                        Vector3.one)
                };
                TransformProPreferences.clipboards[clipboardIndex] = clipboard;
            }

            TransformProPreferences.selectedClipboard = TransformProPreferences.GetInt("selectedClipboard", 0);

            // Apply all settings to the transform pro.
            TransformPro.SnapPositionGrid = TransformProPreferences.SnapPositionGrid;
            TransformPro.SnapPositionOrigin = TransformProPreferences.SnapPositionOrigin;
            TransformPro.SnapRotationGrid = TransformProPreferences.SnapRotationGrid;
            TransformPro.SnapRotationOrigin = TransformProPreferences.SnapRotationOrigin;
            TransformPro.CalculateBounds = TransformProPreferences.EnableBounds;

            TransformProPreferences.areLoaded = true;
        }

        public static void SaveClipboard()
        {
            TransformProPreferences.SetVector3(
                string.Format("clipboard{0}.Position", TransformProPreferences.SelectedClipboard),
                TransformProPreferences.Clipboard.Position);
            TransformProPreferences.SetQuaternion(
                string.Format("clipboard{0}.Rotation", TransformProPreferences.SelectedClipboard),
                TransformProPreferences.Clipboard.Rotation);
            TransformProPreferences.SetVector3(
                string.Format("clipboard{0}.Scale", TransformProPreferences.SelectedClipboard),
                TransformProPreferences.Clipboard.Scale);
        }

        /// <summary>
        ///     Draws a standard <see cref="Vector3" /> field, but with fixes to prevent it dropping onto two lines in the
        ///     preferences window.
        /// </summary>
        /// <param name="label">The label to show for the field.</param>
        /// <param name="value">The value to show for the field.</param>
        /// <returns>The updated value.</returns>
        private static Vector3 DrawPreferencesField(string label, Vector3 value)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            Rect rectLabel = rect;
            rectLabel.width = EditorGUIUtility.labelWidth;
            GUI.Label(rectLabel, label);
            Rect rectField = rect;
            rectField.xMin += EditorGUIUtility.labelWidth;
            return EditorGUI.Vector3Field(rectField, GUIContent.none, value);
        }

        private static Enum DrawPreferencesField(string label, Enum value)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            Rect rectLabel = rect;
            rectLabel.width = EditorGUIUtility.labelWidth;
            GUI.Label(rectLabel, label);
            Rect rectField = rect;
            rectField.xMin += EditorGUIUtility.labelWidth;
            return EditorGUI.EnumPopup(rectField, value);
        }

        private static int DrawPreferencesField(string label, int value)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            Rect rectLabel = rect;
            rectLabel.width = EditorGUIUtility.labelWidth;
            GUI.Label(rectLabel, label);
            Rect rectField = rect;
            rectField.xMin += EditorGUIUtility.labelWidth;
            return EditorGUI.IntField(rectField, value);
        }

        /// <summary>
        ///     Draws a standard <see cref="bool" /> field, but with fixes to prevent it dropping onto two lines in the preferences
        ///     window.
        /// </summary>
        /// <param name="label">The label to show for the field.</param>
        /// <param name="value">The value to show for the field.</param>
        /// <returns>The updated value.</returns>
        private static bool DrawPreferencesField(string label, bool value)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            Rect rectLabel = rect;
            rectLabel.width = EditorGUIUtility.labelWidth;
            GUI.Label(rectLabel, label);
            Rect rectField = rect;
            rectField.xMin += EditorGUIUtility.labelWidth;
            return EditorGUI.Toggle(rectField, value);
        }

        private static bool GetBool(string name, bool value)
        {
            return EditorPrefs.GetBool(string.Format("TransformPro.{0}.bool", name), value);
        }

        private static float GetFloat(string name, float value)
        {
            return EditorPrefs.GetFloat(string.Format("TransformPro.{0}.float", name), value);
        }

        private static int GetInt(string name, int value)
        {
            return EditorPrefs.GetInt(string.Format("TransformPro.{0}.int", name), value);
        }

        private static Quaternion GetQuaternion(string name, Quaternion quaternion)
        {
            quaternion.x = EditorPrefs.GetFloat(string.Format("TransformPro.{0}.Quaternion.x", name), quaternion.x);
            quaternion.y = EditorPrefs.GetFloat(string.Format("TransformPro.{0}.Quaternion.y", name), quaternion.y);
            quaternion.z = EditorPrefs.GetFloat(string.Format("TransformPro.{0}.Quaternion.z", name), quaternion.z);
            quaternion.w = EditorPrefs.GetFloat(string.Format("TransformPro.{0}.Quaternion.w", name), quaternion.w);
            return quaternion;
        }

        private static string GetString(string name, string value)
        {
            return EditorPrefs.GetString(string.Format("TransformPro.{0}.string", name), value);
        }

        private static Vector3 GetVector3(string name, Vector3 vector3)
        {
            vector3.x = EditorPrefs.GetFloat(string.Format("TransformPro.{0}.Vector3.x", name), vector3.x);
            vector3.y = EditorPrefs.GetFloat(string.Format("TransformPro.{0}.Vector3.y", name), vector3.y);
            vector3.z = EditorPrefs.GetFloat(string.Format("TransformPro.{0}.Vector3.z", name), vector3.z);
            return vector3;
        }

        private static Color GetColor(string name, Color color)
        {
            Color r = color;
            var HtmlColorStr = EditorPrefs.GetString(string.Format("TransformPro.{0}.color", name),
                ColorUtility.ToHtmlStringRGBA(color));

            ColorUtility.TryParseHtmlString(string.Format("#{0}", HtmlColorStr), out r);
            return r;
        }

        /// <summary>
        ///     Draws the preferences GUI. The main portion of the interfaces is found here, and this method is decorated with
        ///     <see cref="UnityEditor.PreferenceItem" /> so that it appears in the default preferences window.
        /// </summary>
#pragma warning disable 618
        [PreferenceItem("UnityExtensions/TransformPro")]
#pragma warning restore 618
        private static void OnPreferencesGUI()
        {
            if (!TransformProPreferences.AreLoaded)
            {
                TransformProPreferences.Load();
            }

            EditorGUILayout.BeginHorizontal();
            //GUILayout.Label("(c) 2017 Untitled Games", TransformProStyles.LabelCopyright);
            GUILayout.Label(TransformPro.Version);
            EditorGUILayout.EndHorizontal();

            // ------------------------------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(10);

            if (TransformProPreferences.installationProblems == null)
            {
                TransformProPreferences.installationProblems = TransformProInspectorDebug.OtherInspectorsInstalled();
            }

            if (TransformProPreferences.installationProblems == true)
            {
                GUI.backgroundColor = new Color(1, 1, 0, 0.5f);
                GUILayout.BeginHorizontal(TransformProStyles.Panel);
                GUILayout.Label("监测到资源冲突");
                if (GUILayout.Button("在控制台中显示"))
                {
                    TransformProInspectorDebug.OutputInspectors();
                }

                GUILayout.EndHorizontal();
                GUI.backgroundColor = Color.white;
            }

            // ------------------------------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Snapping(对齐捕捉)", TransformProStyles.LabelHeading2, GUILayout.Height(24));
            GUI.color = TransformProStyles.ColorReset;
            if (GUILayout.Button("重置"))
            {
                TransformProPreferences.SnapPositionGrid = Vector3.one;
                TransformProPreferences.SnapPositionOrigin = Vector3.zero;
                TransformProPreferences.SnapRotationGrid = new Vector3(90, 90, 90);
                TransformProPreferences.SnapRotationOrigin = Vector3.zero;
                TransformProPreferences.SceneViewGUIColor = SceneViewGUIColorDefault;
                TransformProPreferences.SceneViewPanelColor = SceneViewPanelColorDefault;
            }
            
            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Position(坐标)", TransformProStyles.LabelHeading3, GUILayout.Height(19));
            TransformProPreferences.SnapPositionGrid =
                TransformProPreferences.DrawPreferencesField("尺寸", TransformProPreferences.SnapPositionGrid);
            TransformProPreferences.SnapPositionOrigin =
                TransformProPreferences.DrawPreferencesField("初始", TransformProPreferences.SnapPositionOrigin);

            EditorGUILayout.LabelField("Rotation(旋转)", TransformProStyles.LabelHeading3, GUILayout.Height(19));
            TransformProPreferences.SnapRotationGrid =
                TransformProPreferences.DrawPreferencesField("尺寸", TransformProPreferences.SnapRotationGrid);
            TransformProPreferences.SnapRotationOrigin =
                TransformProPreferences.DrawPreferencesField("初始", TransformProPreferences.SnapRotationOrigin);

            // ------------------------------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(5);

            EditorGUILayout.LabelField("Bounds(边界)", TransformProStyles.LabelHeading2, GUILayout.Height(24));
            TransformProPreferences.EnableBounds =
                TransformProPreferences.DrawPreferencesField("启用计算功能",
                    TransformProPreferences.EnableBounds);
            GUI.enabled = TransformProPreferences.EnableBounds;
            TransformProPreferences.BoundsDisplayMode =
                (BoundsDisplayMode) TransformProPreferences.DrawPreferencesField("边界显示模式",
                    TransformProPreferences.BoundsDisplayMode);
            TransformProPreferences.MaximumBoundsDrawn = Mathf.Max(1,
                TransformProPreferences.DrawPreferencesField("最大范围",
                    TransformProPreferences.MaximumBoundsDrawn));
            GUI.enabled = true;

            // ------------------------------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(5);

            EditorGUILayout.LabelField("快捷值", TransformProStyles.LabelHeading2, GUILayout.Height(24));

            EditorGUI.BeginChangeCheck();

            TransformProPreferences.NudgesPosition.Count =
                EditorGUILayout.IntSlider("坐标", TransformProPreferences.NudgesPosition.Count, 1, 5);
            for (int nudge = 0; nudge < TransformProPreferences.NudgesPosition.Count; nudge++)
            {
                EditorGUILayout.BeginHorizontal();
                TransformProPreferences.NudgesPosition[nudge] = EditorGUILayout.FloatField(
                    string.Format("坐标值 {0}", nudge + 1), TransformProPreferences.NudgesPosition[nudge]);
                GUI.enabled = false;
                EditorGUILayout.Popup(0, new[] {"Grid Units"});
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(5);

            TransformProPreferences.NudgesRotation.Count =
                EditorGUILayout.IntSlider("旋转", TransformProPreferences.NudgesRotation.Count, 1, 5);
            for (int nudge = 0; nudge < TransformProPreferences.NudgesRotation.Count; nudge++)
            {
                EditorGUILayout.BeginHorizontal();
                TransformProPreferences.NudgesRotation[nudge] = EditorGUILayout.FloatField(
                    string.Format("旋转值 {0}", nudge + 1), TransformProPreferences.NudgesRotation[nudge]);
                GUI.enabled = false;
                EditorGUILayout.Popup(0, new[] {"Degrees"});
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(5);
            //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField("SceneMenu(场景菜单)", TransformProStyles.LabelHeading2, GUILayout.Height(24));
            SceneViewPanelColor = EditorGUILayout.ColorField("背景颜色", SceneViewPanelColor);
            SceneViewGUIColor = EditorGUILayout.ColorField("GUI颜色", SceneViewGUIColor);
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                TransformProEditorGadgets.RepaintInspector = true;
            }
        }

        private static void SetBool(string name, bool value)
        {
            EditorPrefs.SetBool(string.Format("TransformPro.{0}.bool", name), value);
        }

        private static void SetFloat(string name, float value)
        {
            EditorPrefs.SetFloat(string.Format("TransformPro.{0}.float", name), value);
        }

        private static void SetInt(string name, int value)
        {
            EditorPrefs.SetInt(string.Format("TransformPro.{0}.int", name), value);
        }


        private static void SetQuaternion(string name, Quaternion quaternion)
        {
            EditorPrefs.SetFloat(string.Format("TransformPro.{0}.Quaternion.x", name), quaternion.x);
            EditorPrefs.SetFloat(string.Format("TransformPro.{0}.Quaternion.y", name), quaternion.y);
            EditorPrefs.SetFloat(string.Format("TransformPro.{0}.Quaternion.z", name), quaternion.z);
            EditorPrefs.SetFloat(string.Format("TransformPro.{0}.Quaternion.w", name), quaternion.w);
        }

        private static void SetString(string name, string value)
        {
            EditorPrefs.SetString(string.Format("TransformPro.{0}.string", name), value);
        }

        private static void SetVector3(string name, Vector3 vector3)
        {
            EditorPrefs.SetFloat(string.Format("TransformPro.{0}.Vector3.x", name), vector3.x);
            EditorPrefs.SetFloat(string.Format("TransformPro.{0}.Vector3.y", name), vector3.y);
            EditorPrefs.SetFloat(string.Format("TransformPro.{0}.Vector3.z", name), vector3.z);
        }

        private static void SetColor(string name, Color color)
        {
            EditorPrefs.SetString(string.Format("TransformPro.{0}.color", name), ColorUtility.ToHtmlStringRGBA(color));
        }

        /// <summary>
        ///     Sets the window <see cref="EditorWindow.titleContent" />.
        /// </summary>
        private void OnEnable()
        {
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_3_0 || UNITY_5_4_OR_NEWER
            if (!TransformProStyles.Load())
            {
                return;
            }

            this.titleContent = new GUIContent("TransformPro", TransformProStyles.Icons.Logo);
#else
            this.title = "TransformPro";
#endif

            this.minSize = new Vector2(240, 400);
        }

        /// <summary>
        ///     Draws the stand alone version of the preferences window.
        ///     Adds a replacement header, then calls <see cref="OnPreferencesGUI" />.
        /// </summary>
        private void OnGUI()
        {
            if (TransformProStyles.Icons == null)
            {
                return;
            }

            TransformProPreferences.scrollPosition =
                EditorGUILayout.BeginScrollView(TransformProPreferences.scrollPosition);

            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(64));
            Rect rectIcon = rect;
            rectIcon.width = 64;
            rect.xMin += 70;
            //EditorGUI.DrawPreviewTexture(rectIcon, TransformProStyles.Icons.Logo, TransformProStyles.MaterialIconTransparent);
            GUI.DrawTexture(rectIcon, TransformProStyles.Icons.Logo);
            GUI.Label(rect, "TransformPro", TransformProStyles.LabelHeading1);

            TransformProPreferences.OnPreferencesGUI();

            EditorGUILayout.EndScrollView();
        }

        public abstract class NudgeSettings : IEnumerable<float>
        {
            private int count;
            private float[] items;

            protected NudgeSettings()
            {
                this.items = new float[0];
                this.Count = TransformProPreferences.GetInt(string.Format("{0}.count", this.Name), 3);
            }

            protected abstract string Name { get; }
            protected abstract string Units { get; }

            public int Count
            {
                get { return this.count; }
                set
                {
                    this.count = Mathf.Clamp(value, 1, 5);
                    TransformProPreferences.SetInt(string.Format("{0}.count", this.Name), this.count);
                    Array.Resize(ref this.items, this.count);
                }
            }

            public float this[int index]
            {
                get
                {
                    if ((index < 0) || (index >= this.items.Length))
                    {
                        return 1;
                    }

                    float result = this.items[index];
                    if (Mathf.Approximately(result, 0))
                    {
                        result = this.GetValue(index);
                        this[index] = result;
                    }

                    return result;
                }
                set
                {
                    if ((index < 0) || (index >= this.items.Length))
                    {
                        return;
                    }

                    float valueFixed = value;
                    if (valueFixed <= 0)
                    {
                        valueFixed = this.GetDefaultValue(index);
                    }

                    this.items[index] = valueFixed;
                    TransformProPreferences.SetFloat(string.Format("{0}.nudge{1}", this.Name, index), valueFixed);
                }
            }

            /// <inheritdoc />
            public IEnumerator<float> GetEnumerator()
            {
                for (int index = 0; index < this.Count; index++)
                {
                    yield return this[index];
                }
            }

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public abstract float GetDefaultValue(int index);

            private float GetValue(int index)
            {
                return TransformProPreferences.GetFloat(string.Format("{0}.nudge{1}", this.Name, index),
                    this.GetDefaultValue(index));
            }
        }

        public class NudgeSettingsPosition : NudgeSettings
        {
            /// <inheritdoc />
            protected override string Name
            {
                get { return "nudgesPosition"; }
            }

            /// <inheritdoc />
            protected override string Units
            {
                get { return "Grid Units"; }
            }

            /// <inheritdoc />
            public override float GetDefaultValue(int index)
            {
                return Mathf.Pow(2, index * 2);
            }
        }

        public class NudgeSettingsRotation : NudgeSettings
        {
            private static readonly float[] defaults = {30, 45, 90, 120, 200};

            /// <inheritdoc />
            protected override string Name
            {
                get { return "nudgesRotation"; }
            }

            /// <inheritdoc />
            protected override string Units
            {
                get { return "Degrees"; }
            }

            /// <inheritdoc />
            public override float GetDefaultValue(int index)
            {
                return NudgeSettingsRotation.defaults[index];
            }
        }
    }

    [Serializable]
    public enum BoundsDisplayMode
    {
        Local = 0,
        World = 1,
        Both = 2
    }
}