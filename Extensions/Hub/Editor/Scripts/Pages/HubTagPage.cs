using System;
using System.Collections.Generic;
using Hub.Editor.Scripts.Base;
using UnityEditor;
using UnityEngine;

namespace Hub.Editor.Scripts.Pages
{
    public class HubTagPage : Hub_PageBase
    {
        private static Dictionary<string, List<int>> tagGroups = new Dictionary<string, List<int>>();
        private static List<bool> FoldOuts = new List<bool>();
        private static List<Vector2> scrollPos = new List<Vector2>();
        private Action<Event, string> OnContextClick;


        private void OnEnable()
        {
            pageName = "标签管理";
            layer = -3;
            OnContextClick += OnContextClickHandler;
        }


        protected override void OnFocus()
        {
            RefreshAllTagObject();
        }

        protected override void OnGUI()
        {
            string[] tags = UnityEditorInternal.InternalEditorUtility.tags;
            for (int i = 0; i < tags.Length; i++)
            {
                var tag = tags[i];
                if (FoldOuts.Count < i + 1)
                    FoldOuts.Add(false);
                if (scrollPos.Count < i + 1)
                    scrollPos.Add(Vector2.zero);


                var dropArea = EditorGUILayout.BeginHorizontal();
                var hasGroup = tagGroups.TryGetValue(tag, out var group);
                var count = hasGroup ? group.Count : 0;
                var FoldoutContant = new GUIContent(string.Format("{0}  x{1}", tag, count));
                FoldOuts[i] = EditorGUILayout.BeginFoldoutHeaderGroup(FoldOuts[i], FoldoutContant);
                Event evt = Event.current;
                if (dropArea.Contains(evt.mousePosition))
                {
                    switch (evt.type)
                    {
                        case EventType.DragUpdated:
                        case EventType.DragPerform:
                            if (!dropArea.Contains(evt.mousePosition))
                                return;

                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                            if (evt.type == EventType.DragPerform)
                            {
                                DragAndDrop.AcceptDrag();

                                foreach (var obj in DragAndDrop.objectReferences)
                                {
                                    ((GameObject) obj).tag = tag;
                                }

                                RefreshAllTagObject();
                            }

                            Event.current.Use();
                            break;
                        case EventType.ContextClick:
                            OnContextClick?.Invoke(evt, tag);
                            break;
                    }
                }

                EditorGUILayout.EndHorizontal();
                if (FoldOuts[i])
                {
                    if (hasGroup)
                    {
                        scrollPos[i] = EditorGUILayout.BeginScrollView(scrollPos[i], Hub_Styles.FrameBox);
                        foreach (var insID in group)
                        {
                            GameObject objIns = EditorUtility.InstanceIDToObject(insID) as GameObject;
                            if (objIns)
                            {
                                var content = Hub_Styles.gameobjectIcon;
                                content.text = objIns.name;
                                if (GUILayout.Button(content, EditorStyles.objectField))
                                {
                                    Selection.activeInstanceID = insID;
                                }
                            }
                        }

                        EditorGUILayout.EndScrollView();
                    }
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        /// <summary>
        /// 绘制右键菜单
        /// </summary>
        /// <param name="evt"></param>
        private void OnContextClickHandler(Event evt, string tag)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("选中全部"), false, SelectAll, tag);
            // menu.AddSeparator("");
            // menu.AddItem(new GUIContent("选中全部"), false, CheckAllField, "CheckAll");
            // menu.AddItem(new GUIContent("取消全部"), false, UnCheckAllField, "UnCheckAll");
            // //menu.AddSeparator ("");
            // //menu.AddItem (new GUIContent ("SubMenu/MenuItem3"), false, null, "item 3");
            menu.ShowAsContext();
            evt.Use();
        }

        private void SelectAll(object userdata)
        {
            string tag = (string) userdata;
            if (tagGroups.TryGetValue(tag, out var group))
                Selection.instanceIDs = group.ToArray();
        }

        private void RefreshAllTagObject()
        {
            tagGroups.Clear();
            var allObject = FindObjectsOfType<Transform>();
            foreach (var obj in allObject)
            {
                var tag = obj.gameObject.tag;
                var instanceId = obj.gameObject.GetInstanceID();
                var hasKey = tagGroups.TryGetValue(tag, out var group);
                if (!hasKey)
                {
                    group = new List<int>();
                    group.Add(instanceId);
                    tagGroups.Add(tag, group);
                }
                else
                {
                    if (!group.Contains(instanceId))
                        group.Add(instanceId);
                }
            }
        }
    }
}