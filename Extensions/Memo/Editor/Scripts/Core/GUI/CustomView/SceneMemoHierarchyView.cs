using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using HierarchyPro.Editor;
using Assembly = System.Reflection.Assembly;

namespace UnityExtensions.Memo
{
    [InitializeOnLoad]
    internal static class SceneMemoHierarchyView
    {
        public static Assembly hierarchyProAssembly => Assembly.Load("HierarchyPro");

        static SceneMemoHierarchyView()
        {
            if (!EditorMemoPrefs.UnitySceneMemoActive)
                return;

            SceneMemoHelper.Initialize();
            if (SceneMemoHelper.Data == null)
                return;


            if (hierarchyProAssembly != null)
            {
                HierarchyProEditor.hierarchyproWindowItemOnGUI += OnHierarchyView;
            }
            else
            {
                EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyView;
            }

            Undo.undoRedoPerformed += () =>
            {
                EditorApplication.RepaintHierarchyWindow();
                for (int i = 0; i < EditorSceneManager.sceneCount; i++)
                    SceneMemoHelper.InitializeSceneMemo(EditorSceneManager.GetSceneAt(i));
            };

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += (view) => { SceneMemoSceneView.OnGUI(currentMemo); };
#else
            // draw at SceneView
            SceneView.onSceneGUIDelegate += ( view ) => {
                UnitySceneMemoSceneView.OnGUI( currentMemo );
            };
#endif
        }

        private static SceneMemo currentMemo;

        public static void OnHierarchyView(int instanceID, Rect selectionRect)
        {
            if (Application.isPlaying)
                return;
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj == null)
                return;

            var localIdentifier = SceneMemoHelper.GetLocalIdentifierInFile(obj);
            if (localIdentifier == 0)
                return;

            if (CheckNoGameObjectSelected())
                currentMemo = null;

            var gameObject = obj as GameObject;
            var buttonRect = ButtonRect(selectionRect, gameObject.transform.childCount > 0);
            var isSelected = CheckSelected(instanceID);

            var memo = SceneMemoHelper.GetMemo(gameObject, localIdentifier);
            if (memo == null)
            {
                if (isSelected)
                {
                    if (GUI.Button(buttonRect, ""))
                    {
                        MemoUndoHelper.SceneMemoUndo(MemoUndoHelper.UNDO_SCENEMEMO_ADD);
                        SceneMemoHelper.AddMemo(obj as GameObject, localIdentifier);
                    }

                    GUI.Label(buttonRect, "+");
                }
            }
            else
            {
                GUI.color = MemoGUIHelper.Colors.LabelColor(memo.Label);
                GUI.DrawTexture(buttonRect, MemoGUIHelper.Textures.Balloon);
                if (GUI.Button(buttonRect, "", GUIStyle.none))
                {
                    SceneMemoHelper.PopupWindowContent.Initialize(memo);
                    PopupWindow.Show(selectionRect, SceneMemoHelper.PopupWindowContent);
                }

                GUI.color = Color.white;

                //SceneView.RepaintAll();

                if (isSelected)
                    currentMemo = memo;
            }
        }

        //======================================================================
        // private
        //======================================================================

        private static Rect ButtonRect(Rect rect, bool hasChild)
        {
            var buttonRect = rect;
            buttonRect.width = 15;
            buttonRect.height = 15;
            buttonRect.x -= hasChild ? 29 : 17;
            return buttonRect;
        }

        private static bool CheckSelected(int instanceID)
        {
            var selection = Selection.gameObjects;
            for (int i = 0; i < selection.Length; i++)
            {
                if (selection[i].GetInstanceID() == instanceID)
                    return true;
            }

            return false;
        }

        private static bool CheckNoGameObjectSelected()
        {
            return Selection.gameObjects.Length == 0;
        }
    }
}