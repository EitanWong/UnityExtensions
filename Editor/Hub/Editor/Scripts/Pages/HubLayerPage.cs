using System;
using System.Collections.Generic;
using Hub.Editor.Scripts.Base;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Hub.Editor.Scripts.Pages
{
    public class HubLayerPage : Hub_PageBase
    {
        private List<int>[] _layerGroups = new List<int>[31];
        private static bool[] visible = new bool[31];
        private static bool[] locked = new bool[31];
        private static bool[] Foldout = new bool[31];
        private static Vector2[] scrollPos = new Vector2[31];
        private Action<Event, int> OnContextClick;


        private void OnEnable()
        {
            pageName = "层级管理";
            layer = -2;
            OnContextClick += OnContextClickHandler;
        }


        protected override void OnFocus()
        {
            ReloadAllObjectLayer();
            ReloadAllVisibleAndLockedLayer();
        }

        private void ReloadAllObjectLayer()
        {
            foreach (var @group in _layerGroups)
                if (group != null)
                    group.Clear();
            var allObject = FindObjectsOfType<Transform>();
            foreach (var obj in allObject)
            {
                for (int i = 0; i < _layerGroups.Length; i++)
                {
                    var layerId = obj.gameObject.layer;
                    if (layerId != -1)
                    {
                        var group = _layerGroups[layerId];
                        if (group == null)
                            group = _layerGroups[layerId] = new List<int>();
                        @group?.Add(obj.gameObject.GetInstanceID());
                        break;
                    }
                }
            }
        }

        private void ReloadAllVisibleAndLockedLayer()
        {
            var layers = InternalEditorUtility.layers;
            foreach (var layerName in layers)
            {
                var layerIndex = LayerMask.NameToLayer(layerName);
                var hasvisible = Tools.visibleLayers == (Tools.visibleLayers | (1 << layerIndex));
                var haslocked = Tools.lockedLayers == (Tools.lockedLayers | (1 << layerIndex));
                visible[layerIndex] = !hasvisible;
                locked[layerIndex] = haslocked;
            }
        }

        protected override void OnGUI()
        {
            //Debug.Log(Tools.lockedLayers);
            var layers = InternalEditorUtility.layers;
            for (int i = 0; i < layers.Length; i++)
            {
                var layerName = layers[i];
                var layerIndex = LayerMask.NameToLayer(layerName);
                if (!string.IsNullOrEmpty(layerName))
                {
                    var group = _layerGroups[layerIndex];
                    var dropArea = EditorGUILayout.BeginHorizontal();
                    var count = 0;
                    if (group!=null)
                        count = group.Count;
                    var FoldoutContant = new GUIContent(string.Format("{0}  x{1}", layerName, count));
                    Foldout[layerIndex] = EditorGUILayout.BeginFoldoutHeaderGroup(Foldout[layerIndex], FoldoutContant,
                        Hub_Styles.FoldoutHeader);
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
                                        ((GameObject) obj).layer = layerIndex;
                                    }

                                    ReloadAllObjectLayer();
                                }

                                Event.current.Use();
                                break;
                            case EventType.ContextClick:
                                OnContextClick?.Invoke(evt, layerIndex);
                                break;
                        }
                    }

                    var visibleStyle = visible[layerIndex] ? Hub_Styles.Visible : Hub_Styles.NoneVisible;
                    if (GUILayout.Button(visibleStyle, Hub_Styles.IconButton))
                    {
                        visible[layerIndex] = !visible[layerIndex];
                        RefreshSceneViewLayer();
                        SceneView.RepaintAll();
                    }


                    var lockedStyle = locked[layerIndex] ? Hub_Styles.notpickable : Hub_Styles.pickable_white;

                    if (GUILayout.Button(lockedStyle, Hub_Styles.IconButton))
                    {
                        locked[layerIndex] = !locked[layerIndex];
                        RefreshSceneLockLayer();
                        SceneView.RepaintAll();
                    }

                    EditorGUILayout.EndFoldoutHeaderGroup();
                    EditorGUILayout.EndHorizontal();

                    if (Foldout[layerIndex])
                    {
                        if (group != null)
                        {
                            scrollPos[layerIndex] =
                                EditorGUILayout.BeginScrollView(scrollPos[layerIndex], Hub_Styles.FrameBox);
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
                }
            }
        }

        /// <summary>
        /// 绘制右键菜单
        /// </summary>
        /// <param name="evt"></param>
        private void OnContextClickHandler(Event evt, int layerindex)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("选中全部"), false, SelectAll, layerindex);
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
            int index = (int) userdata;
            var group = _layerGroups[index];
            Selection.instanceIDs = group.ToArray();
        }

        private void RefreshSceneViewLayer()
        {
            Tools.visibleLayers = 0;
            var layers = InternalEditorUtility.layers;
            foreach (var layerName in layers)
            {
                var layerIndex = LayerMask.NameToLayer(layerName);
                Tools.visibleLayers =
                    Tools.visibleLayers | Convert.ToInt32(!visible[layerIndex]) << layerIndex; //进行二进制按位左移 并与下一位做或运算
            }
        }

        private void RefreshSceneLockLayer()
        {
            Tools.lockedLayers = 0;
            var layers = InternalEditorUtility.layers;
            foreach (var layerName in layers)
            {
                var layerIndex = LayerMask.NameToLayer(layerName);
                Tools.lockedLayers =
                    Tools.lockedLayers | Convert.ToInt32(locked[layerIndex]) << layerIndex; //进行二进制按位左移 并与下一位做或运算
            }
        }
    }
}
