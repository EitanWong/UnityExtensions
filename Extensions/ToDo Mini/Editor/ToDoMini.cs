using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ToDoMini
{
    public class ToDoMini : EditorWindow
    {
        static ToDoMini window;

        ToDoMiniData dataHolder;

        ToDoMiniData Data
        {
            get
            {
                if (dataHolder == null || shouldRefreshData)
                {
                    ToDoMiniData testDataHolder =
                        Resources.Load<ToDoMiniData>(dataPath);
                    dataHolder = AssetDatabase.LoadAssetAtPath(dataPath, typeof(ToDoMiniData)) as ToDoMiniData;
                    if (dataHolder == null)
                    {
                        dataHolder = CreateInstance(typeof(ToDoMiniData)) as ToDoMiniData;
                        AssetDatabase.CreateAsset(dataHolder, dataPath);
                        GUI.changed = true;
                    }

                    shouldRefreshData = false;
                }

                return dataHolder;
            }
        }

        readonly string dataPath = "Packages/com.lone.unityextensions/Extensions/ToDo Mini/ToDoMini data.asset";
        public static bool shouldRefreshData;

        readonly string taskAreaName = "miniTaskArea";
        Vector2 scrollPosition;
        bool shouldDrawCompletedTasks;
        GUIStyle taskStyle;
        GUIStyle completedTaskStyle;
        int lastTextArea = -1;

        int ButtonsCompactFontSize
        {
            get
            {
#if UNITY_2019_1_OR_NEWER
                return 11;
#else
                return 9;
#endif
            }
        }

        string newTask = "";
        readonly static string newTaskFieldName = "miniNewTask";
        static bool shouldRefocusOnNewTask;
        bool returnWasPressed;

        bool IsPressingReturn
        {
            get { return Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter; }
        }

        bool IsPressingEsc
        {
            get { return Event.current.keyCode == KeyCode.Escape; }
        }

        public enum DisplayDensity
        {
            Default,
            Compact
        }

        string currentSearch;
        SearchField searchField;


        public void OnGUI()
        {
            DrawSearchAndSettings();

            #region Draw tasks and search results.

            // Update styles.
            taskStyle = new GUIStyle("CN CountBadge")
            {
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };
            if (Data.displayDensity == DisplayDensity.Compact)
            {
                RectOffset taskPadding = taskStyle.padding;
                taskPadding.top = 3;
                taskPadding.bottom = 0;
                taskStyle.padding = taskPadding;
                RectOffset taskMargin = taskStyle.margin;
                taskMargin.top = taskMargin.bottom = 0;
                taskStyle.margin = taskMargin;
            }

            completedTaskStyle = new GUIStyle(taskStyle);
            completedTaskStyle.normal.textColor = completedTaskStyle.hover.textColor = Color.grey;

            // Draw search results, or tasks.
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            if (!string.IsNullOrEmpty(currentSearch))
                DrawSearchResults();
            else
            {
                DrawCurrentTasks();

                if (Data.items.Count > 0 && Data.keepCompletedTasks)
                    DrawCompletedTasksButton();

                if (Data.items.Count > 0 && Data.keepCompletedTasks && shouldDrawCompletedTasks)
                    DrawCompletedTasks();
            }

            EditorGUILayout.EndScrollView();

            #endregion

            DrawNewTaskPanel();

            if (focusedWindow == this && GUIUtility.hotControl != 0)
            {
                // If a task area is selected, save when we change control or when we lose focus.
                if (GUI.GetNameOfFocusedControl() == taskAreaName)
                    lastTextArea = GUIUtility.hotControl;
                else if (lastTextArea >= 0 && lastTextArea != GUIUtility.hotControl)
                {
                    lastTextArea = -1;
                    Save();
                }

                // Stop the search if we are into the newTask field.
                if (GUI.GetNameOfFocusedControl() == newTaskFieldName)
                {
                    EmptySearch();
                    shouldRefocusOnNewTask = true;
                }
            }
        }

        void OnLostFocus()
        {
            // Save the previous text area.
            if (lastTextArea >= 0)
            {
                lastTextArea = -1;
                Save();
            }

            EmptySearch();
        }

        void OnDestroy()
        {
            if (Data)
                Save();
        }

        [MenuItem("Window/通用/ToDo Mini %t")]
        public static void Open()
        {
            // Get existing open window or if none, make a new one.
            window = GetWindow<ToDoMini>(false);
            window.titleContent = new GUIContent("☑ ToDo");
            window.autoRepaintOnSceneChange = false;
            window.minSize = new Vector2(150, 150);
            window.Focus();
            //window.ShowModalUtility();
            shouldRefocusOnNewTask = true;
            window.EmptySearch();
        }

        void DrawSearchAndSettings()
        {
            GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));

            // Searchbar.
#if UNITY_2019_1_OR_NEWER
            GUILayout.Space(3);
#endif
            Rect searchRect = GUILayoutUtility.GetRect(1, 1, 18, 18, GUILayout.ExpandWidth(true));
            searchRect.y += 2;
            if (searchField == null)
                searchField = new SearchField();
            currentSearch = searchField.OnToolbarGUI(searchRect, currentSearch);

            // Settings cog.
#if UNITY_2019_1_OR_NEWER
            GUILayout.Space(-3);
#else
            GUILayout.Space(5);
#endif
            if (GUI.Button(
                EditorGUILayout.GetControlRect(false, 20, GUILayout.MaxWidth(20)),
                EditorGUIUtility.IconContent("_Popup"),
                GUI.skin.FindStyle("IconButton")))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("设置"), false, () =>
                {
                    // Open the inspector and select the Data object.
                    EditorApplication.ExecuteMenuItem(
#if UNITY_2018_1_OR_NEWER
                        "Window/General/Inspector"
#else
                    "Window/Inspector"
#endif
                    );
                    if (Selection.activeObject == Data)
                        EditorGUIUtility.PingObject(Data);
                    else
                        Selection.SetActiveObjectWithContext(Data, null);
                });
                menu.ShowAsContext();
            }
#if UNITY_2019_1_OR_NEWER
            GUILayout.Space(-2);
#else
            GUILayout.Space(-5);
#endif
            GUILayout.EndHorizontal();
        }

        void DrawCurrentTasks()
        {
            // IsSearching? Draw search results and stop.
            if (!string.IsNullOrEmpty(currentSearch))
            {
                List<int> items = FindItems(currentSearch);
                foreach (var item in items)
                {
                    if (!Data.items[item].isComplete)
                        DrawTask(item);
                    else
                        DrawCompletedTask(item);
                }

                return;
            }

            // Draw tasks.
            bool hasUncompletedTasks = false;
            for (int i = 0; i < Data.items.Count; i++)
            {
                TodoItem item = Data.items[i];
                if (item.isComplete)
                    continue;

                hasUncompletedTasks = true;
                DrawTask(i);
            }

            // Draw a message if there are no tasks.
            if (!hasUncompletedTasks)
            {
                EditorGUILayout.HelpBox("无任务", MessageType.Info);
            }
        }

        void DrawTask(int i)
        {
            EditorGUILayout.BeginHorizontal();

            // Toggle.
            if (EditorGUILayout.Toggle(Data.items[i].isComplete, GUILayout.Width(12)))
            {
                if (Data.keepCompletedTasks)
                {
                    Data.items[i].isComplete = true;
                    Save();
                }
                else
                {
                    DeleteTask(i);
                    return;
                }
            }

            // Task content.
            GUI.SetNextControlName(taskAreaName);
            //绘制Task
            Data.items[i].task = EditorGUILayout.TextArea(Data.items[i].task, taskStyle);

            // Draw ↑↓✖.
            DrawUpDown(i);
            GUILayout.Space(Data.displayDensity == DisplayDensity.Default ? 1 : -2);
            DrawDelete(i);
            EditorGUILayout.EndHorizontal();
        }

        #region Right-hand side buttons.

        void DrawUpDown(int i)
        {
            GUIStyle upDownStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = Data.displayDensity == DisplayDensity.Compact ? 13 : 15,
                padding = new RectOffset(5, 3, 2, 3),
            };
            if (Data.displayDensity == DisplayDensity.Compact)
            {
                upDownStyle.fixedHeight = 16;
                upDownStyle.fontSize = ButtonsCompactFontSize;
            }

            if (GUILayout.Button("↑", upDownStyle) && i > 0)
            {
                Undo.RecordObject(Data, "上移任务");
                Data.items.Insert(0, Data.items[i]);
                Data.items.RemoveAt(i + 1);
                Save();
            }

            GUILayout.Space(-4);
            if (GUILayout.Button("↓", upDownStyle) && i < Data.items.Count)
            {
                Undo.RecordObject(Data, "下移任务");
                Data.items.Add(Data.items[i]);
                Data.items.RemoveAt(i);

                Save();
            }
        }

        void DrawDelete(int index)
        {
            GUIStyle deleteButtonStyle = new GUIStyle(GUI.skin.button);
            deleteButtonStyle.fixedWidth = Data.displayDensity == DisplayDensity.Compact ? 18 : 20;
            if (Data.displayDensity == DisplayDensity.Compact)
            {
                deleteButtonStyle.fixedHeight = 16;
                deleteButtonStyle.fontSize = ButtonsCompactFontSize;
            }

            deleteButtonStyle.padding = new RectOffset(4, 3, 2, 3);
            if (GUILayout.Button("✖", deleteButtonStyle))
                DeleteTask(index);
        }

        #endregion

        void DeleteTask(int index)
        {
            Undo.RecordObject(Data, "删除任务");
            Data.items.RemoveAt(index);
            Save();
        }

        #region Completed tasks.

        void DrawCompletedTasksButton()
        {
            if (!shouldDrawCompletedTasks)
            {
                if (GUILayout.Button("显示 已完成任务"))
                    shouldDrawCompletedTasks = true;
            }
            else if (GUILayout.Button("隐藏 已完成任务"))
                shouldDrawCompletedTasks = false;
        }

        void DrawCompletedTasks()
        {
            bool hasCompletedTasks = false;
            for (int i = 0; i < Data.items.Count; i++)
            {
                if (!Data.items[i].isComplete)
                    continue;

                hasCompletedTasks = true;
                TodoItem item = Data.items[i];
                EditorGUILayout.BeginHorizontal();
                if (EditorGUILayout.Toggle(item.isComplete, GUILayout.Width(12)) == false)
                {
                    Data.items[i].isComplete = false;
                    Save();
                }

                GUI.SetNextControlName(taskAreaName);
                EditorGUILayout.TextArea(item.task, completedTaskStyle);

                DrawDelete(i);
                EditorGUILayout.EndHorizontal();
            }

            if (!hasCompletedTasks)
            {
                EditorGUILayout.HelpBox("没有 已完成任务.", MessageType.Info);
                //EditorGUILayout.LabelField("没有 已完成任务.", EditorStyles.largeLabel);
            }
        }

        #endregion

        void DrawCompletedTask(int i)
        {
            EditorGUILayout.BeginHorizontal();

            // Toggle.
            if (EditorGUILayout.Toggle(Data.items[i].isComplete, GUILayout.Width(12)) == false)
            {
                Data.items[i].isComplete = false;
                Save();
            }

            // Task content.
            GUI.SetNextControlName(taskAreaName);
            EditorGUILayout.TextArea(Data.items[i].task, completedTaskStyle);

            // Draw ✖.
            DrawDelete(i);
            EditorGUILayout.EndHorizontal();
        }

        void DrawNewTaskPanel()
        {
            // Text field (focus on it if needed).
            GUI.SetNextControlName(newTaskFieldName);
            GUIStyle fieldStyle = new GUIStyle(GUI.skin.textField);
            fieldStyle.wordWrap = true;
            newTask = EditorGUILayout.TextField(newTask, fieldStyle, GUILayout.Height(40));
            if (shouldRefocusOnNewTask)
            {
                EditorGUI.FocusTextInControl(newTaskFieldName);
                shouldRefocusOnNewTask = false;
            }

            // "Add" button.
            if ((GUILayout.Button("+ 添加任务") || returnWasPressed) &&
                !string.IsNullOrEmpty(newTask))
            {
                Data.AddTask(newTask);
                newTask = "";
                shouldRefocusOnNewTask = true;
                Save();
            }

            GUILayout.Space(6);

            // Detect return press.
            if (returnWasPressed && !IsPressingReturn)
                EditorGUI.FocusTextInControl(newTaskFieldName);
            returnWasPressed = IsPressingReturn;


            ExitWindowKeyboardCheck(); //按下退出
        }

        private void ExitWindowKeyboardCheck()
        {
            if (Event.current.keyCode == KeyCode.Escape)
            {
                var editorWindow = EditorWindow.mouseOverWindow;
                if (editorWindow && !editorWindow.docked && editorWindow == this)
                {
                    Close();
                    Event.current.keyCode = KeyCode.None;
                }
            }
        }

        void Save()
        {
            EditorUtility.SetDirty(Data);
            AssetDatabase.SaveAssets();
            if (Selection.activeObject == Data)
                Selection.activeObject = null;
        }


        #region Search.

        List<int> FindItems(string search)
        {
            List<int> items = new List<int>();
            string[] searchItems = search.Split(' ');
            for (int i = 0; i < Data.items.Count; i++)
            {
                bool correctItem = true;
                // The item is correct only if it matches all the search terms.
                for (int j = 0; j < searchItems.Length; j++)
                    if (!Regex.IsMatch(Data.items[i].task, searchItems[j], RegexOptions.IgnoreCase))
                    {
                        correctItem = false;
                        j = searchItems.Length;
                    }

                if (correctItem)
                    items.Add(i);
            }

            return items;
        }

        void EmptySearch()
        {
            currentSearch = "";
        }

        void DrawSearchResults()
        {
            List<int> items = FindItems(currentSearch);
            foreach (var item in items)
            {
                if (!Data.items[item].isComplete)
                    DrawTask(item);
                else
                    DrawCompletedTask(item);
            }
        }

        #endregion
    }
}