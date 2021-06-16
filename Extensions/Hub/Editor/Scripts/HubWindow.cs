using System.Collections.Generic;
using System.Linq;
using Hub.Editor.Scripts.Base;
using UnityEditor;
using UnityEngine;

namespace Hub.Editor.Scripts
{
    public class HubWindow : EditorWindow
    {
        private static HubWindow window;
        private static List<Hub_PageBase> SM_Pages = new List<Hub_PageBase>();
        private static int _currentSelectPageIndex;
        private static Vector2 scrollPos;

        [MenuItem("Window/通用/Hub集线器")]
        public static void OpenWindow()
        {
            // Get existing open window or if none, make a new one.
            window = GetWindow<HubWindow>(false);
            window.titleContent = new GUIContent("Hub集线器", EditorGUIUtility.FindTexture("FilterByLabel@2x"), "Hub集线器");
            window.autoRepaintOnSceneChange = true;
            window.minSize = Vector2.one * 150;
            window.Focus();
        }

        private void OnEnable()
        {
            if (SM_Pages == null || SM_Pages.Count <= 0)
                SM_Pages = Hub_Utility.GetAllReflectionClassIns<Hub_PageBase>();
            SM_Pages = SM_Pages.OrderBy(o => o.layer).ToList();
        }

        private void OnFocus()
        {
            if (_currentSelectPageIndex < SM_Pages.Count)
                SM_Pages[_currentSelectPageIndex].PageOnFocus();
        }

        #region EditorBehaviour

        private void OnGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            for (int i = 0; i < SM_Pages.Count; i++)
            {
                var page = SM_Pages[i];
                var isSelect = _currentSelectPageIndex == i;
                if (GUILayout.Toggle(isSelect, page.pageName, EditorStyles.toolbarButton))
                {
                    if (!_currentSelectPageIndex.Equals(i))
                    {
                        _currentSelectPageIndex = i;
                        SM_Pages[_currentSelectPageIndex].PageOnFocus();
                    }
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            SM_Pages[_currentSelectPageIndex].DrawPage();
            GUILayout.EndScrollView();
        }

        #endregion

        // public void DropAreaGUI()
        // {
        //     Event evt = Event.current;
        //     Rect drop_area = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        //     GUI.Box(drop_area, "Add Trigger");
        //
        //     switch (evt.type)
        //     {
        //         case EventType.DragUpdated:
        //         case EventType.DragPerform:
        //             if (!drop_area.Contains(evt.mousePosition))
        //                 return;
        //
        //             DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        //
        //             if (evt.type == EventType.DragPerform)
        //             {
        //                 DragAndDrop.AcceptDrag();
        //
        //                 foreach (Object dragged_object in DragAndDrop.objectReferences)
        //                 {
        //                     // Do On Drag Stuff here
        //                 }
        //             }
        //
        //             break;
        //     }
        // }
    }
}