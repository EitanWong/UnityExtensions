using System.Collections;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

namespace ToolbarExtensions
{
    public enum ExtenderType
    {
        Right,
        Left
    }

    public struct ToolbarElement
    {
        public VisualElement element { get; private set; }
        public ExtenderType type { get; private set; }

        public ToolbarElement(VisualElement element, ExtenderType type)
        {
            this.element = element;
            this.type = type;
        }

        public static ToolbarElement Create(VisualElement element, ExtenderType type)
        {
            return new ToolbarElement(element, type);
        }
    }

    [InitializeOnLoad]
    public class ToolbarExtender
    {
        private const string scenesFolder = "Scenes";
        static ScriptableObject m_currentToolbar;
        static Type m_toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
        private static VisualElement left_parent;
        private static VisualElement right_parent;
        private static List<ToolbarElement> ToolbarElements = new List<ToolbarElement>();

        static ToolbarExtender()
        {
            m_currentToolbar = null;
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        static int lastInstanceID;

        static void OnUpdate()
        {
            if (m_currentToolbar == null)
            {
                var toolbars = Resources.FindObjectsOfTypeAll(m_toolbarType);
                m_currentToolbar = toolbars.Length > 0 ? (ScriptableObject) toolbars[0] : null;
                // UnityEngine.Debug.Log(m_currentToolbar != null);
            }

            // If the windows layour reloaded, we need to re create our GUI
            if (m_currentToolbar != null && left_parent != null && right_parent != null &&
                m_currentToolbar.GetInstanceID() != lastInstanceID)
            {
                left_parent.RemoveFromHierarchy();
                right_parent.RemoveFromHierarchy();
                left_parent = null;
                right_parent = null;
                lastInstanceID = m_currentToolbar.GetInstanceID();
            }

            if (m_currentToolbar != null && left_parent == null && right_parent == null)
            {
                // foreach (var item in m_currentToolbar.GetType().GetRuntimeFields())
                //     UnityEngine.Debug.Log(item.Name + " " + item.FieldType + " " + item.IsPublic);

                var root = m_currentToolbar.GetType()
                    .GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                // UnityEngine.Debug.Log(root);
                if (root != null)
                {
                    var rawRoot = root.GetValue(m_currentToolbar);
                    // UnityEngine.Debug.Log(rawRoot != null);
                    if (rawRoot != null)
                    {
                        // UnityEngine.Debug.Log("Attaching");
                        // UnityEngine.Debug.Log(rawRoot.GetType());
                        var mRoot = rawRoot as VisualElement;
                        // UnityEngine.Debug.Log(mRoot.name);

                        var toolbarZoneLeftAlign = mRoot.Q("ToolbarZoneLeftAlign");
                        var toolbarZoneRightAlign = mRoot.Q("ToolbarZoneRightAlign");


                        if (left_parent != null)
                            left_parent.RemoveFromHierarchy();
                        if (right_parent != null)
                            right_parent.RemoveFromHierarchy();


                        left_parent = null;
                        right_parent = null;


                        left_parent = new VisualElement()
                        {
                            style =
                            {
                                flexGrow = 1,
                                flexDirection = FlexDirection.Row,
                            }
                        };
                        left_parent.Add(new VisualElement()
                        {
                            style =
                            {
                                flexGrow = 1,
                            }
                        });

                        right_parent = new VisualElement()
                        {
                            style =
                            {
                                flexGrow = 1,
                                flexDirection = FlexDirection.RowReverse,
                            }
                        };
                        right_parent.Add(new VisualElement()
                        {
                            style =
                            {
                                flexGrow = 1,
                            }
                        });

                        UpdateElement();
                        //OnAttachToToolbar(left_parent);
                        toolbarZoneLeftAlign.Add(left_parent);
                        toolbarZoneRightAlign.Add(right_parent);
                    }
                }
            }
        }


        #region PrivateMedthod

        private static void UpdateElement()
        {
            foreach (var e in ToolbarElements)
            {
                Extend(e.element, e.type);
            }
        }

        private static void Extend(VisualElement element, ExtenderType type)
        {
            switch (type)
            {
                case ExtenderType.Right:
                    right_parent.Add(element);
                    break;
                case ExtenderType.Left:
                    left_parent.Add(element);
                    break;
            }
        }

        static void FitChildrenStyle(VisualElement element)
        {
            element.AddToClassList("unity-toolbar-button");
            element.AddToClassList("unity-editor-toolbar-element");
            element.RemoveFromClassList("unity-button");
            element.style.marginRight = 0;
            element.style.marginLeft = 0;
        }
        #endregion

        #region PublicMethod

        /// <summary>
        /// AddToolbarElement
        /// </summary>
        /// <param name="element"></param>
        public static void ToolbarExtend(ToolbarElement element)
        {
            if (!ToolbarElements.Contains(element))
                ToolbarElements.Add(element);
        }
        /// <summary>
        /// Create ToolbarButtonVisualElement
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="onClick"></param>
        /// <returns></returns>
        public static VisualElement CreateToolbarButton(string icon, Action onClick)
        {
            Button buttonVE = new Button(onClick);
            FitChildrenStyle(buttonVE);
            VisualElement iconVE = new VisualElement();
            iconVE.AddToClassList("unity-editor-toolbar-element__icon");
            iconVE.style.backgroundImage = Background.FromTexture2D(EditorGUIUtility.FindTexture(icon));
            buttonVE.Add(iconVE);

            return buttonVE;
        }
        /// <summary>
        /// Create ToolbarButtonVisualElement
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="onClick"></param>
        /// <returns></returns>
        public static VisualElement CreateToolbarButton(Texture2D icon, Action onClick)
        {
            Button buttonVE = new Button(onClick);
            FitChildrenStyle(buttonVE);
            VisualElement iconVE = new VisualElement();
            iconVE.AddToClassList("unity-editor-toolbar-element__icon");
            iconVE.style.backgroundImage = icon;
            buttonVE.Add(iconVE);

            return buttonVE;
        }
        #endregion


        // static void OnAttachToToolbar(VisualElement parent)
        // {
        //     // #if !UNITY_2021_1_OR_NEWER
        //     //             parent.Add(CreateToolbarButton("Search On Icon", ShowQuickSearch));
        //     // #endif
        //     //parent.Add(CreateToolbarButton("Package Manager", ShowPackageManager));
        //     parent.Add(CreateToolbarButton("Settings", ShowSettings));
        //     parent.Add(CreateToolbarButton("UnityEditor.SceneHierarchyWindow", ShowScenes));
        //     parent.Add(CreateToolbarButton("UnityEditor.GameView", ShowBootstrapScene));
        // }
        //
        //
 
        //
        // public static T[] GetAtPath<T>(string path)
        // {
        //     ArrayList al = new ArrayList();
        //     string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);
        //
        //     foreach (string fileName in fileEntries)
        //     {
        //         string temp = fileName.Replace("\\", "/");
        //         int index = temp.LastIndexOf("/");
        //         string localPath = "Assets/" + path;
        //
        //         if (index > 0)
        //             localPath += temp.Substring(index);
        //
        //         System.Object t = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));
        //
        //         if (t != null)
        //             al.Add(t);
        //     }
        //
        //     T[] result = new T[al.Count];
        //
        //     for (int i = 0; i < al.Count; i++)
        //         result[i] = (T) al[i];
        //
        //     return result;
        // }

        // private static void ShowScenes()
        // {
        //     var a = new GenericMenu();
        //     var ls = GetAtPath<UnityEngine.Object>(scenesFolder);
        //     foreach (var l in ls)
        //     {
        //         var p = AssetDatabase.GetAssetPath(l);
        //         var n = Path.GetFileName(p);
        //         if (n.EndsWith(".unity"))
        //         {
        //             a.AddItem(new GUIContent(Path.GetFileNameWithoutExtension(p)), false, () =>
        //             {
        //                 if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        //                 {
        //                     EditorSceneManager.OpenScene(p, OpenSceneMode.Single);
        //                     if (p == "bootstrap")
        //                     {
        //                         Selection.activeGameObject = GameObject.FindGameObjectWithTag("Player");
        //                         SceneView.FrameLastActiveSceneView();
        //                     }
        //                 }
        //             });
        //         }
        //     }
        //
        //     a.ShowAsContext();
        // }
        //
        // private static void ShowBootstrapScene()
        // {
        //     var bootstrapPath = "Assets/" + scenesFolder + "/bootstrap.unity";
        //     if (!Application.isPlaying && File.Exists(bootstrapPath))
        //         EditorSceneManager.OpenScene(bootstrapPath, OpenSceneMode.Additive);
        //     Selection.activeGameObject = GameObject.FindGameObjectWithTag("Player");
        //     SceneView.FrameLastActiveSceneView();
        // }
        //
        // private static void ShowQuickSearch()
        // {
        //     EditorApplication.ExecuteMenuItem("Help/Quick Search");
        // }
        //
        // private static void ShowSettings()
        // {
        //     var a = new GenericMenu();
        //     a.AddItem(new GUIContent("Project"), false,
        //         () => EditorApplication.ExecuteMenuItem("Edit/Project Settings..."));
        //     a.AddItem(new GUIContent("Preferences"), false,
        //         () => EditorApplication.ExecuteMenuItem("Edit/Preferences..."));
        //     a.ShowAsContext();
        // }
    }
}