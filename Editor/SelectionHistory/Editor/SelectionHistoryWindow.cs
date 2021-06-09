// Staggart Creations http://staggart.xyz
// Copyright protected under Unity asset store EULA

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SelectionHistory.Editor
{
    public class SelectionHistoryWindow : EditorWindow
    {
        [MenuItem("Window/通用/历史选择记录 ^L")]
        public static void Init()
        {
            SelectionHistoryWindow window = GetWindow<SelectionHistoryWindow>();

            //Options
            window.autoRepaintOnSceneChange = true;
            window.titleContent.image = EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin
                ? "d_UnityEditor.SceneHierarchyWindow"
                : "UnityEditor.SceneHierarchyWindow").image;
            window.titleContent.text = "历史选择记录";

            //Show
            window.Show();
        }

        private string iconPrefix => EditorGUIUtility.isProSkin ? "d_" : "";

        public static bool RecordHierarchy
        {
            get { return EditorPrefs.GetBool(PlayerSettings.productName + "_SH_RecordHierachy", true); }
            set { EditorPrefs.SetBool(PlayerSettings.productName + "_SH_RecordHierachy", value); }
        }

        public static bool RecordProject
        {
            get { return EditorPrefs.GetBool(PlayerSettings.productName + "_SH_RecordProject", true); }
            set { EditorPrefs.SetBool(PlayerSettings.productName + "_SH_RecordProject", value); }
        }

        public static int MaxHistorySize
        {
            get { return EditorPrefs.GetInt(PlayerSettings.productName + "_SH_MaxHistorySize", 50); }
            set { EditorPrefs.SetInt(PlayerSettings.productName + "_SH_MaxHistorySize", value); }
        }

        private AnimBool settingAnimation;
        private bool settingExpanded;
        private AnimBool clearAnimation;
        private bool historyVisible = true;

        private static List<Object> selectionHistory = new List<Object>();
        private static bool muteRecording;
        private int selectedIndex = -1;

        private void OnSelectionChange()
        {
            this.Repaint();

            if (muteRecording || !Selection.activeObject) return;
            AddToHistory();
        }

        private void OnFocus()
        {
            //Items have have been deleted and should be removed from history
            selectionHistory = selectionHistory.Where(x => x != null).ToList();
        }

        private void AddToHistory()
        {
            //Skip selected folders and such
            if (Selection.activeObject.GetType() == typeof(UnityEditor.DefaultAsset)) return;

            if (EditorUtility.IsPersistent(Selection.activeObject) && !RecordProject) return;
            if (EditorUtility.IsPersistent(Selection.activeObject) == false && !RecordHierarchy) return;

            if (selectionHistory.Contains(Selection.activeObject) == false)
                selectionHistory.Insert(0, Selection.activeObject);

            //Trim end
            if (selectionHistory.Count - 1 == MaxHistorySize) selectionHistory.RemoveAt(selectionHistory.Count - 1);
        }

        private void OnEnable()
        {
#if !UNITY_2019_1_OR_NEWER
        SceneView.onSceneGUIDelegate += ListenForNavigationInput;
#else
            SceneView.duringSceneGui += ListenForNavigationInput;
#endif

            settingAnimation = new AnimBool(false);
            settingAnimation.valueChanged.AddListener(this.Repaint);
            settingAnimation.speed = 4f;
            clearAnimation = new AnimBool(false);
            clearAnimation.valueChanged.AddListener(this.Repaint);
            clearAnimation.speed = settingAnimation.speed;
        }

        private void OnDisable()
        {
#if !UNITY_2019_1_OR_NEWER
        SceneView.onSceneGUIDelegate -= ListenForNavigationInput;
#else
            SceneView.duringSceneGui -= ListenForNavigationInput;
#endif
        }

        private void ListenForNavigationInput(SceneView sceneView)
        {
            if (Event.current.type == EventType.KeyDown && Event.current.isKey &&
                Event.current.keyCode == KeyCode.LeftBracket)
            {
                SelectPrevious();
            }

            if (Event.current.type == EventType.KeyDown && Event.current.isKey &&
                Event.current.keyCode == KeyCode.RightBracket)
            {
                SelectNext();
            }
        }

        private void SetSelection(Object target, int index)
        {
            muteRecording = true;
            Selection.activeObject = target;
            muteRecording = false;
        }

        private void SelectPrevious()
        {
            selectedIndex--;
            selectedIndex = Mathf.Clamp(selectedIndex, 0, selectionHistory.Count - 1);

            SetSelection(selectionHistory[selectedIndex], selectedIndex);
        }

        private void SelectNext()
        {
            selectedIndex++;
            selectedIndex = Mathf.Clamp(selectedIndex, 0, selectionHistory.Count - 1);

            SetSelection(selectionHistory[selectedIndex], selectedIndex);
        }

        private Vector2 scrollPos;


        private void OnGUI()
        {
            this.Repaint();

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(selectionHistory.Count == 0))
                {
                    using (new EditorGUI.DisabledScope(selectedIndex == selectionHistory.Count - 1))
                    {
                        if (GUILayout.Button(
                            new GUIContent(EditorGUIUtility.IconContent(iconPrefix + "back@2x").image,
                                "Select previous (Left bracket key)"), EditorStyles.miniButtonLeft, GUILayout.Height(20f),
                            GUILayout.Width(30f)))
                        {
                            SelectNext();
                        }
                    }

                    using (new EditorGUI.DisabledScope(selectedIndex == 0))
                    {
                        if (GUILayout.Button(
                            new GUIContent(EditorGUIUtility.IconContent(iconPrefix + "forward@2x").image,
                                "Select next (Right bracket key)"), EditorStyles.miniButtonRight, GUILayout.Height(20),
                            GUILayout.Width(30f)))
                        {
                            SelectPrevious();
                        }
                    }

                    if (GUILayout.Button(
                        new GUIContent(EditorGUIUtility.IconContent(iconPrefix + "TreeEditor.Trash").image,
                            "Clear history"), EditorStyles.miniButton))
                    {
                        historyVisible = false;
                    }
                }

                GUILayout.FlexibleSpace();

                settingExpanded = GUILayout.Toggle(settingExpanded,
                    new GUIContent(EditorGUIUtility.IconContent(iconPrefix + "Settings").image, "Edit settings"),
                    EditorStyles.miniButtonMid);
                settingAnimation.target = settingExpanded;
            }

            if (EditorGUILayout.BeginFadeGroup(settingAnimation.faded))
            {
                EditorGUILayout.Space();

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("记录选项", EditorStyles.boldLabel, GUILayout.Width(100f));
                    RecordHierarchy = EditorGUILayout.ToggleLeft("Hierarchy", RecordHierarchy, GUILayout.MaxWidth(80f));
                    RecordProject = EditorGUILayout.ToggleLeft("Project window", RecordProject);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("记录量", EditorStyles.boldLabel, GUILayout.Width(100f));
                    MaxHistorySize = EditorGUILayout.IntField(MaxHistorySize, GUILayout.MaxWidth(40f));
                }

                EditorGUILayout.Space();
            }

            EditorGUILayout.EndFadeGroup();

            clearAnimation.target = !historyVisible;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, EditorStyles.helpBox,
                GUILayout.MaxHeight(this.maxSize.y - 20f));
            {
                EditorGUILayout.BeginFadeGroup(1f - clearAnimation.faded);

                var prevColor = GUI.color;
                var prevBgColor = GUI.backgroundColor;

                for (int i = 0; i < selectionHistory.Count; i++)
                {
                    if (selectionHistory[i] == null) continue;

                    var rect = EditorGUILayout.BeginHorizontal();

                    GUI.color = i % 2 == 0
                        ? Color.grey * (EditorGUIUtility.isProSkin ? 1f : 1.7f)
                        : Color.grey * (EditorGUIUtility.isProSkin ? 1.05f : 1.66f);

                    //Hover color
                    if (rect.Contains(Event.current.mousePosition) || Selection.activeObject == (selectionHistory[i]))
                    {
                        GUI.color = EditorGUIUtility.isProSkin ? Color.grey * 1.1f : Color.grey * 1.5f;
                    }

                    //Selection outline
                    if (Selection.activeObject == (selectionHistory[i]))
                    {
                        Rect outline = rect;
                        outline.x -= 1;
                        outline.y -= 1;
                        outline.width += 2;
                        outline.height += 2;
                        EditorGUI.DrawRect(outline, EditorGUIUtility.isProSkin ? Color.gray * 1.5f : Color.gray);
                    }

                    //Background
                    EditorGUI.DrawRect(rect, GUI.color);

                    GUI.color = prevColor;
                    GUI.backgroundColor = prevBgColor;

                    if (GUILayout.Button(
                        new GUIContent(" " + selectionHistory[i].name,
                            EditorGUIUtility.ObjectContent(selectionHistory[i], selectionHistory[i].GetType()).image),
                        EditorStyles.label, GUILayout.MaxHeight(17f)))
                    {
                        SetSelection(selectionHistory[i], i);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndFadeGroup();
            }
            EditorGUILayout.EndScrollView();

            //Once the list is collapse, clear the collection
            if (clearAnimation.faded == 1f) selectionHistory.Clear();
            //Reset
            if (selectionHistory.Count == 0) historyVisible = true;
        }
    }
}