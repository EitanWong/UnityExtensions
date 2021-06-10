namespace TransformPro.Scripts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    ///     TransformPro Gadgets system.
    ///     Provides a continous event hook into the unity editor, and manages all selection changes.
    ///     This has fixed many of the problems with a sudden inspector loss when deselecting objects.
    ///     Also manages the standardised gadget event system, including scene gui and handles, standardised side gui, and
    ///     scene clicking.
    /// </summary>
    public class TransformProEditorGadgets : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <summary>
        ///     The height of the default Unity gimbal control.
        /// </summary>
        public const float HeightGimbal = 110;

        /// <summary>
        ///     The padding between individual gadgets.
        /// </summary>
        public const float Padding = 5;

        /// <summary>
        ///     The disabled color of the gadget quick toggle.
        /// </summary>
        private static readonly Color colorDark = new Color(0.2f, 0.2f, 0.2f);

        /// <summary>
        ///     The enabled color of the gadget quick toggle.
        /// </summary>
        private static readonly Color colorLight = new Color(0.7f, 0.7f, 0.7f);

        /// <summary>
        ///     Singleton instance.
        /// </summary>
        private static TransformProEditorGadgets instance;

        /// <summary>
        ///     A value indicating whether the Gadgets system needs to be redrawn.
        /// </summary>
        private static bool repaintAll;

        private static bool repaintInspector;

        /// <summary>
        ///     Holds the collection of all Gadgets that implement the Panel interface.
        /// </summary>
        [SerializeField] private ITransformProGadgetPanel[] gadgetsPanel;

        /// <summary>
        ///     Holds the collection of all Gadgets that implement the Scene Click interface.
        /// </summary>
        [SerializeField] private ITransformProGadgetSceneClick[] gadgetsSceneClick;

        /// <summary>
        ///     Holds the collection of all Gadgets that implement the Scene GUI interface.
        /// </summary>
        [SerializeField] private ITransformProGadgetSceneGUI[] gadgetsSceneGUI;

        /// <summary>
        ///     Holds the collection of all Gadgets that implement the Scene Handles interface.
        /// </summary>
        [SerializeField] private ITransformProGadgetSceneHandles[] gadgetsSceneHandles;

        /// <summary>
        ///     Gets or sets the current singleton object.
        ///     If the singleton field is blank, this property will locate any orphaned gadget managers.
        ///     If there are more than one, the extras will be disposed of.
        ///     If there are no orphaned gadget managers, a new one will be created and assigned.
        /// </summary>
        public static TransformProEditorGadgets Instance
        {
            get
            {
                if (TransformProEditorGadgets.instance != null)
                {
                    TransformProEditorGadgets.instance.hideFlags = HideFlags.DontSave;
                    return TransformProEditorGadgets.instance;
                }

                TransformProEditorGadgets[] gadgetEditors = Resources.FindObjectsOfTypeAll<TransformProEditorGadgets>();

                if ((gadgetEditors == null) || (gadgetEditors.Length <= 0))
                {
                    return null;
                }

                TransformProEditorGadgets.instance = gadgetEditors[0];
                TransformProEditorGadgets.instance.hideFlags = HideFlags.DontSave;

                for (int i = 1; i < gadgetEditors.Length; i++)
                {
                    Object.DestroyImmediate(gadgetEditors[i]);
                }

                return TransformProEditorGadgets.instance;
            }
            set { TransformProEditorGadgets.instance = value; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the gadgets system needs to be redrawn.
        /// </summary>
        public static bool RepaintAll
        {
            get { return TransformProEditorGadgets.repaintAll; }
            set { TransformProEditorGadgets.repaintAll = value; }
        }

        public static bool RepaintInspector
        {
            get { return TransformProEditorGadgets.repaintInspector; }
            set { TransformProEditorGadgets.repaintInspector = value; }
        }

        /// <summary>
        ///     Gets all Gadgets that implement the Panel system.
        /// </summary>
        public IEnumerable<ITransformProGadgetPanel> GadgetsPanel
        {
            get
            {
                if (this.gadgetsPanel == null)
                {
                    this.GetConcrete();
                }

                return this.gadgetsPanel;
            }
        }

        /// <summary>
        ///     Gets all Gadgets that implement the Scene Click system.
        /// </summary>
        public IEnumerable<ITransformProGadgetSceneClick> GadgetsSceneClick
        {
            get
            {
                if (this.gadgetsSceneClick == null)
                {
                    this.GetConcrete();
                }

                return this.gadgetsSceneClick;
            }
        }

        /// <summary>
        ///     Gets all Gadgets that implement the Scene GUI system.
        /// </summary>
        public IEnumerable<ITransformProGadgetSceneGUI> GadgetsSceneGUI
        {
            get
            {
                if (this.gadgetsSceneGUI == null)
                {
                    this.GetConcrete();
                }

                return this.gadgetsSceneGUI;
            }
        }

        /// <summary>
        ///     Gets all Gadgets that implement the Scene Handles system.
        /// </summary>
        public IEnumerable<ITransformProGadgetSceneHandles> GadgetsSceneHandles
        {
            get
            {
                if (this.gadgetsSceneHandles == null)
                {
                    this.GetConcrete();
                }

                return this.gadgetsSceneHandles;
            }
        }

        /// <inheritdoc />
        public void OnBeforeSerialize()
        {
        }

        /// <inheritdoc />
        public void OnAfterDeserialize()
        {
            TransformProEditorGadgets.instance = this;

#pragma warning disable 618
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
#pragma warning restore 618
        }

        /// <summary>
        ///     Create a new Gadgets instance. This instance is not saved to the scene.
        /// </summary>
        public static void Create()
        {
            TransformProEditorGadgets.instance = ScriptableObject.CreateInstance<TransformProEditorGadgets>();
            TransformProEditorGadgets.instance.hideFlags = HideFlags.DontSave;
        }

        public static void Unload()
        {
            if (TransformProEditorGadgets.instance != null)
            {
#pragma warning disable 618
                SceneView.onSceneGUIDelegate -= TransformProEditorGadgets.instance.OnSceneGUI;
#pragma warning restore 618
                EditorApplication.update -= TransformProEditorGadgets.instance.EditorUpdate;
                TransformProEditorGadgets.instance = null;
            }

            TransformProEditorGadgets[] gadgetEditors = Resources.FindObjectsOfTypeAll<TransformProEditorGadgets>();
            if (gadgetEditors != null)
            {
                for (int i = 1; i < gadgetEditors.Length; i++)
                {
                    Object.DestroyImmediate(gadgetEditors[i]);
                }
            }
        }

        /// <summary>
        ///     Draws the background for an individual panel on the Panels system.
        /// </summary>
        /// <param name="x">The x position of the panel.</param>
        /// <param name="y">The y position of the panel.</param>
        /// <param name="width">The width of the panel.</param>
        /// <param name="height">The height of the panel.</param>
        /// <returns>The y position of the bottom of the panel.</returns>
        private static float DrawPanel(float x, float y, float width, float height)
        {
            Rect rect = new Rect(x, y, width, height);
            GUI.color = TransformProPreferences.SceneViewPanelColor;
            GUI.Label(rect, GUIContent.none, TransformProStyles.PanelDeep);
            GUI.color = TransformProPreferences.SceneViewGUIColor;
            return y + height;
        }

        /// <summary>
        ///     Ensures the singleton instance is current, and the Unity events are registered.
        ///     Also calls an initial repaint to make sure the system is up to date when the assemblies are recompiled.
        /// </summary>
        public void Setup()
        {
            TransformProEditorGadgets.instance = this;
            TransformProEditorGadgets.instance.hideFlags = HideFlags.DontSave;

#pragma warning disable 618
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
#pragma warning restore 618

            EditorApplication.update -= this.EditorUpdate;
            EditorApplication.update += this.EditorUpdate;

            this.EditorUpdate();
            SceneView.RepaintAll();
        }

        /// <summary>
        ///     Draws a custom toggle control. Legacy design that only creates a single control ID.
        /// </summary>
        /// <param name="rect">The rectangle to draw the control into.</param>
        /// <param name="texture">An icon to display on the toggle button.</param>
        /// <param name="style">The <see cref="GUIStyle" /> to use for the toggle button.</param>
        /// <param name="pressed">Is the toggle currently pressed?</param>
        /// <returns>Was the toggle button pressed?</returns>
        public bool Toggle(Rect rect, Texture texture, GUIStyle style, bool pressed)
        {
            return this.Toggle(rect, new GUIContent(texture), style, pressed);
        }

        /// <summary>
        ///     Draws a custom toggle control. Legacy design that only creates a single control ID.
        /// </summary>
        /// <param name="rect">The rectangle to draw the control into.</param>
        /// <param name="content">The <see cref="GUIContent" /> to show on the button.</param>
        /// <param name="style">The <see cref="GUIStyle" /> to use for the toggle button.</param>
        /// <param name="pressed">Is the toggle currently pressed?</param>
        /// <returns>Was the toggle button pressed?</returns>
        public bool Toggle(Rect rect, GUIContent content, GUIStyle style, bool pressed)
        {
            Event e = Event.current;

            GUI.backgroundColor = pressed ? TransformProEditorGadgets.colorLight : TransformProEditorGadgets.colorDark;
            GUI.contentColor = pressed ? Color.white : new Color(0, 0, 0, 0.33f);
            GUI.Box(rect, content, style);
            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;

            if (rect.Contains(e.mousePosition) && (e.rawType == EventType.MouseDown))
            {
                SceneView.RepaintAll();
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This method is called every time the editor is updated.
        ///     We now use this to track the current selection, as well as handling the RepaintAll system.
        /// </summary>
        private void EditorUpdate()
        {
            TransformProEditor.Select(Selection.objects);

            if (TransformProEditorGadgets.RepaintAll)
            {
                SceneView.RepaintAll();
                TransformProEditorGadgets.RepaintAll = false;
            }

            if (TransformProEditorGadgets.RepaintInspector)
            {
                if (TransformProEditor.Instance != null)
                {
                    TransformProEditor.Instance.Repaint();
                    TransformProEditorGadgets.RepaintInspector = false;
                }
            }
        }

        /// <summary>
        ///     Gets all the concrete implementations of the various Gadget system interfaces.
        /// </summary>
        private void GetConcrete()
        {
            Type type = typeof(ITransformProGadget);

            List<Type> types = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    types.AddRange(assembly.GetTypes().Where(p => type.IsAssignableFrom(p) && p.IsClass));
                }
                catch
                {
                    // ignored
                }
            }

            if (!types.Any())
            {
                return;
            }

            List<ITransformProGadget> gadgets = types.Select(x => (ITransformProGadget) Activator.CreateInstance(x))
                .ToList();

            this.gadgetsPanel = gadgets.OfType<ITransformProGadgetPanel>().ToArray();
            this.gadgetsSceneGUI = gadgets.OfType<ITransformProGadgetSceneGUI>().ToArray();
            this.gadgetsSceneClick = gadgets.OfType<ITransformProGadgetSceneClick>().ToArray();
            this.gadgetsSceneHandles = gadgets.OfType<ITransformProGadgetSceneHandles>().ToArray();
        }

        /// <summary>
        ///     Draws all the various managed SceneGUI systems.
        /// </summary>
        /// <param name="sceneView">The current SceneView.</param>
        private void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;
            bool useEvent = false;
            int controlID = GUIUtility.GetControlID(this.GetHashCode(), FocusType.Passive);

            if (!TransformProStyles.Load())
            {
                return;
            }

            if (TransformProPreferences.ShowGadgets)
            {
                foreach (ITransformProGadgetSceneHandles gadget in this.GadgetsSceneHandles.Where(gadget =>
                    gadget.Enabled))
                {
                    gadget.DrawSceneHandles(sceneView, this);
                }
            }

            Handles.BeginGUI();

            // Screen.width does not account for DPI scaled displays such as retina displays on OSX.
#if UNITY_5_4_OR_NEWER
            float x = (SceneView.currentDrawingSceneView.camera.pixelRect.width / EditorGUIUtility.pixelsPerPoint) -
                      (100 - TransformProEditorGadgets.Padding);
#else
            float x =
 SceneView.currentDrawingSceneView.camera.pixelRect.width - (100 - TransformProEditorGadgets.Padding);
#endif
            float y = 5;
            float width = 90;

            Rect gadgetToggleRect = new Rect(x + 5, y + 5, 16, 16);

            // // Adds a panel around the default gimbal control to improve visibility and help with consistancy
            y = TransformProEditorGadgets.DrawPanel(x, y, width, TransformProEditorGadgets.HeightGimbal);
            //y += HeightGimbal;
            y += TransformProEditorGadgets.Padding;

            // Draw the gadget toggle button
            Color gadgetColor = TransformProEditorGadgets.colorLight;
            if (!TransformProPreferences.ShowGadgets)
            {
                gadgetColor.a = 0.5f;
            }

            GUI.contentColor = gadgetColor;
            if (GUI.Button(gadgetToggleRect, TransformProStyles.Icons.Gadget, TransformProStyles.LabelTiny))
            {
                TransformProPreferences.ShowGadgets = !TransformProPreferences.ShowGadgets;
            }

            GUI.contentColor = Color.white;

            if (TransformProPreferences.ShowGadgets)
            {
                foreach (ITransformProGadgetSceneGUI gadget in this.GadgetsSceneGUI.Where(gadget => gadget.Enabled))
                {
                    gadget.DrawSceneGUI(sceneView, this);
                }

                foreach (ITransformProGadgetPanel gadget in this.GadgetsPanel.Where(gadget => gadget.Enabled))
                {
                    float nextY = TransformProEditorGadgets.DrawPanel(x, y, width, gadget.Height);

                    Rect rect = new Rect(x, y, width, gadget.Height);
                    gadget.DrawPanelGUI(sceneView, this, rect);
                    useEvent |= rect.Contains(e.mousePosition);

                    y = nextY + TransformProEditorGadgets.Padding;
                }
            }

            Handles.EndGUI();

            if (useEvent && (e.rawType == EventType.Layout))
            {
                HandleUtility.AddDefaultControl(controlID);
            }

            if ((e.rawType == EventType.MouseDown) || (e.rawType == EventType.MouseUp))
            {
                if (!useEvent)
                {
                    useEvent = this.GadgetsSceneClick
                        .Where(gadget => gadget.Enabled)
                        .Any(gadget =>
                            gadget.SceneClick(sceneView, this, e.mousePosition, e.rawType == EventType.MouseDown));
                    if (useEvent)
                    {
                        GUIUtility.hotControl = controlID;
                    }
                }

                if (useEvent)
                {
                    e.Use();
                }
            }
        }
    }
}