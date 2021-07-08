#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Extensions.MeshPro.MeshEditor.Editor.Scripts.Base.Utilities;
using Extensions.MeshPro.MeshEditor.Editor.Scripts.Configs.Internal;
using Extensions.MeshPro.MeshEditor.Editor.Scripts.Manager;
using Extensions.MeshPro.MeshEditor.Modules.Internal.MeshEdit.Editor.Utilities;
using TransformPro.MeshPro.MeshEditor.Editor.Scripts.Base;
using TransformPro.MeshPro.MeshFilterProEditor.Editor.Enum;
using UnityEditor;
using UnityEngine;

namespace MeshEditor.Editor.Scripts.Windows.WindowPage.Internal
{
    /// <summary>
    /// 网格选择器扩展
    /// </summary>
    public class MeshEditPage : MEDR_Page
    {
        private MeshEditMode current_EditMode, current_EditModeStateCache; //当前编辑模式 与状态缓存(用于检测编辑模式变换)
#pragma warning disable 414
        private MeshEditTool current_EditTool; //当前编辑工具
#pragma warning restore 414

        private static bool IsEdit; //是否编辑
        MeshEditConfig config;

        private MeshFilter selectFilter; //被选择的网格过滤器

        private MeshFilter editObj_Filter; //当前网格网格过滤器
        private MeshCollider editObj_Collider; //编辑对象网格碰撞器

        private GameObject editObj; //编辑对象

        private GameObject current_EditGroup; //当前编辑组

        #region MergeData合并数据

        private List<MeshEditVertex> merge_Vertex = new List<MeshEditVertex>();
        private List<MeshEditFace> merge_Triangles = new List<MeshEditFace>();

        #endregion

        //private int currentHoverTriangleIndex, currentSelectTriangleIndex;
        private static MeshEditFace _mouseClickFace, _mouseHoverFace; //当前移入，或 点击选中 三角面

        #region 编辑网格数据Cache缓存

        private List<MeshEditVertex> edit_VertexCache = new List<MeshEditVertex>(); //编辑的顶点缓存

        private List<MeshEditEdge> edit_EdgeCache = new List<MeshEditEdge>(); //编辑的线框

        private List<MeshEditFace> edit_FaceCache = new List<MeshEditFace>(); //编辑的面片

        #endregion

        #region Event事件

        #region 编辑模式事件

        public Action<MeshEditMode> onEditModeChangeWithParam; //当编辑模式改变事件带参数
        public Action onEditModeChange; //当编辑模式改变事件;


        private Action OnEditStart, OnEditEnd;

        #endregion

        #endregion

        #region GUIContent

        private static GUIContent objectGUIContant;
        private static GUIContent vertexGUIContant;
        private static GUIContent edgeGUIContant;
        private static GUIContent faceGUIContant;
        private static GUIContent finishGUIContant;
        private static GUIContent cancelGUIContant;
        private static GUIContent editGUIContant;
        private static GUIContent saveGUIContant;

        private static Texture2D MeshEditPageIcon;

        #endregion

        #region EditorBehavior 编辑器Bbehavior

        private void Awake()
        {
            PageName = "网格编辑";
            disableTitle = "是否退出编辑";
            disableMessage = "你真的要退出编辑吗？\n所有变更不会保存哦！";
            disableOK = "确定";
            disableCancel = "取消";
            config = MEDR_ConfigManager.GetConfig<MeshEditConfig>();
            OnInit();
        }

        private void OnInit()
        {
            OnEditStart += SubscribeEditModeChangeEvent;
            OnEditEnd += DeSubscribeEditModeChangeEvent;
            var path = "Textures/";
            objectGUIContant =
                new GUIContent("对象模式", Resources.Load<Texture2D>(path + "Object"), "对象模式");
            vertexGUIContant =
                new GUIContent("顶点模式", Resources.Load<Texture2D>(path + "Vertex"), "顶点模式");
            edgeGUIContant = new GUIContent("线框模式", Resources.Load<Texture2D>(path + "Edge"), "线框模式");
            faceGUIContant = new GUIContent("面片模式", Resources.Load<Texture2D>(path + "Face"), "面片模式");
            finishGUIContant =
                new GUIContent("完成编辑", Resources.Load<Texture2D>(path + "Finish"), "完成编辑");
            cancelGUIContant =
                new GUIContent("取消编辑", Resources.Load<Texture2D>(path + "Cancel"), "取消编辑");
            editGUIContant = new GUIContent("简单编辑", Resources.Load<Texture2D>(path + "Edit"), "简单编辑");
            saveGUIContant =
                new GUIContent("保存网格数据", Resources.Load<Texture2D>(path + "Save"), "保存网格数据");
            MeshEditPageIcon = Resources.Load<Texture2D>(path + "MeshEdit");
            PageIcon = MeshEditPageIcon;
            PageToolTips = "网格模型的基础编辑工具";

            //config=  MeshEditSettingPage
        }


        protected override void OnFieldUnCheck()
        {
            OnWindowDestroy();
        }

        protected override void OnWindowDestroy()
        {
            DeSubscribeEditModeChangeEvent();
            SetEditMode(false); //设置编辑模式取消
            //SceneView.duringSceneGui -= OnSceneGUIUpdate;
        }

        protected override void OnGUI()
        {
            //DrawDefaultInspector(); //绘制默认检查器面板
            //Selection.activeObject = fields[0].Filter.gameObject;
            //Selection.activeObject = SelectedFields[0].Filter.gameObject;

            if (CheckFields == null || CheckFields.Count <= 0)
            {
                return;
            }

            DrawEditModeGUIButton(); //绘制编辑器按钮
            DrawSaveGUIButton();
        }


        /// <summary>
        /// 绘制场景时
        /// </summary>
        protected override void OnSceneGUI()
        {
            if (!IsEdit || CheckFields == null || CheckFields.Count <= 0) return; //如果没有在编辑直接返回
            Selection.activeObject = CheckFields[0].Filter.gameObject;
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            ToolStateCheck(); //工具状态检测
            CaptureHoverTarget(); //捕捉鼠标移入对象
            CaptureClickTarget(); //捕捉鼠标点击对象
            EditProgramHandler(); //编辑程序处理
            if (GUI.changed) //当GUI改变时
            {
                RefreshMesh(); //刷新Mesh
            }

            if (SceneView.sceneViews.Count >= 0)
                SceneView.RepaintAll(); //重新绘制场景
        }

        #endregion


        #region UI绘制

        private void DrawSaveGUIButton()
        {
            if (IsEdit) return;
            if (GUILayout.Button(saveGUIContant))
            {
                if (!selectFilter)
                    selectFilter = selectFilter = (MeshFilter) CheckFields[0].Filter;
                if (selectFilter)
                {
                    var path = EditorUtility.SaveFilePanelInProject("模型网格另存为",
                        string.Format("New{0}", selectFilter.sharedMesh.name), "mesh", "请选择你要存放的位置");
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        var saveMesh = MeshEditUtility.GetNewMesh(selectFilter.sharedMesh);
                        AssetDatabase.CreateAsset(saveMesh, path);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }


        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// 绘制Inspector面板按钮
        /// </summary>
        private void DrawEditModeGUIButton()
        {
            if (GUILayout.Button(IsEdit ? cancelGUIContant : editGUIContant))
            {
                if (!IsEdit) IsEdit = true; //如果没有编辑
                else
                {
                    //如果UnCheckField==True 说明取消编辑项成功 IsEdit状态为False
                    IsEdit = !UnCheckField();
                    if (IsEdit) //如果IsEdit状态不变说明继续编辑 保持原状
                    {
                        MEDR_Internal_Utility.ShowNotificationOnSceneView("取消退出", 1f);
                        return;
                    }
                }

                SetEditMode(IsEdit);
            }

            if (!IsEdit)
                return;

            if (GUILayout.Button(finishGUIContant))
            {
                FinishEditMesh();
            }


            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(objectGUIContant))
            {
                current_EditMode = MeshEditMode.Object;
                MEDR_Internal_Utility.ShowNotificationOnSceneView("对象模式", 1f);
            }

            if (GUILayout.Button(vertexGUIContant))
            {
                current_EditMode = MeshEditMode.Vertex;
                MEDR_Internal_Utility.ShowNotificationOnSceneView("顶点模式", 1f);
            }

            if (GUILayout.Button(edgeGUIContant))
            {
                current_EditMode = MeshEditMode.Edge;
                MEDR_Internal_Utility.ShowNotificationOnSceneView("线框模式", 1f);
            }

            if (GUILayout.Button(faceGUIContant))
            {
                current_EditMode = MeshEditMode.Face;
                MEDR_Internal_Utility.ShowNotificationOnSceneView("面片模式", 1f);
            }

            EditorGUILayout.EndHorizontal();
            if (!Equals(current_EditModeStateCache, current_EditMode)) //如果编辑模式改变
            {
                onEditModeChangeWithParam?.Invoke(current_EditMode); //FireEvent
                onEditModeChange?.Invoke(); //FireEvent
                current_EditModeStateCache = current_EditMode; //updateCache
            }
        }

        #endregion

        #region 检测与捕捉

        /// <summary>
        /// 鼠标移入后检测
        /// </summary>
        private void CaptureHoverTarget()
        {
            if (Event.current.type == EventType.MouseMove)
            {
                if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out var hit))
                {
                    var currentHoverTriangleIndex = hit.triangleIndex;
                    _mouseHoverFace = merge_Triangles.Find(t => t.m_id == currentHoverTriangleIndex);
                }
                else
                {
                    _mouseHoverFace = null;
                }
            }

            ShowHoverTarget(); //显示捕捉的对象
            //TransformChange(trans);
        }

        /// <summary>
        /// 显示指针移动到到目标
        /// </summary>
        private void ShowHoverTarget()
        {
            if (_mouseHoverFace == null) return; //如果没有HoverTriangle没有鼠标没有在三角面上 那直接返回

            Handles.color = config.MEDR_MeshEdit_HoverColor;
            switch (current_EditMode)
            {
                case MeshEditMode.Vertex:
                    //var size = (editObj.transform.localScale.magnitude * 0.01f);
                    var size = HandleUtility.GetHandleSize(editObj_Filter.transform.position) * 0.1f *
                               config.MEDR_MeshEdit_VertexSize_Multiplier;
                    Handles.SphereHandleCap(0, _mouseHoverFace.m_vertex1.m_vertex, Quaternion.identity,
                        size, EventType.Repaint);
                    Handles.SphereHandleCap(0, _mouseHoverFace.m_vertex2.m_vertex, Quaternion.identity,
                        size, EventType.Repaint);
                    Handles.SphereHandleCap(0, _mouseHoverFace.m_vertex3.m_vertex, Quaternion.identity,
                        size, EventType.Repaint);
                    break;
                case MeshEditMode.Edge:
                    var edge_size = 1f * config.MEDR_MeshEdit_EdgeSize_Multiplier;
#if UNITY_2020_1_OR_NEWER
                    Handles.DrawLine(_mouseHoverFace.m_edge1.m_vertex1.m_vertex,
                        _mouseHoverFace.m_edge1.m_vertex2.m_vertex, edge_size);
                    Handles.DrawLine(_mouseHoverFace.m_edge2.m_vertex1.m_vertex,
                        _mouseHoverFace.m_edge2.m_vertex2.m_vertex, edge_size);
                    Handles.DrawLine(_mouseHoverFace.m_edge3.m_vertex1.m_vertex,
                        _mouseHoverFace.m_edge3.m_vertex2.m_vertex, edge_size);
#else
                Handles.DrawLine(_mouseHoverFace.m_edge1.m_vertex1.m_vertex,
                    _mouseHoverFace.m_edge1.m_vertex2.m_vertex);
                Handles.DrawLine(_mouseHoverFace.m_edge2.m_vertex1.m_vertex,
                    _mouseHoverFace.m_edge2.m_vertex2.m_vertex);
                Handles.DrawLine(_mouseHoverFace.m_edge3.m_vertex1.m_vertex,
                    _mouseHoverFace.m_edge3.m_vertex2.m_vertex);
#endif


                    break;
                case MeshEditMode.Face:
                    Handles.DrawSolidRectangleWithOutline(_mouseHoverFace.m_faceVertex, Handles.color,
                        config.MEDR_MeshEdit_ClickColor);
                    break;
            }
        }

        /// <summary>
        /// 捕捉点击目标
        /// </summary>
        private void CaptureClickTarget()
        {
            if (Event.current.type != EventType.MouseDrag && !Event.current.control && Event.current.isMouse &&
                Event.current.type == EventType.MouseDown && Event.current.button == 0) //如果鼠标左键按下
            {
                RaycastHit hit;
                if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out hit))
                {
                    var currentClickTriangleIndex = hit.triangleIndex;
                    _mouseClickFace = merge_Triangles.Find(t => t.m_id == currentClickTriangleIndex);
                }
                else
                {
                    _mouseClickFace = null;
                }


                #region 选中后编辑信息处理

                if (_mouseClickFace == null || !current_EditGroup) return; //如果没有选中三角面直接返回

                if (!Event.current.command)
                    CleanCache();
                CleanEditObj(); //清理编辑子对象，让其刷新
                var groupTrans = current_EditGroup.transform;
                switch (current_EditMode)
                {
                    case MeshEditMode.Vertex: //定点编辑Hand
                        var vertex = MeshEditUtility.GetVertexByPoint(_mouseClickFace, hit.point);
                        if (!edit_VertexCache.Contains(vertex)) edit_VertexCache.Add(vertex);
                        else edit_VertexCache.Remove(vertex);

                        if (edit_VertexCache.Count <= 0) return;
                        groupTrans.position = edit_VertexCache.GetMiddlePosition(); //设置为顶点中点坐标

                        //创建新的编辑对象
                        foreach (var editVertex in edit_VertexCache)
                        {
                            editVertex.GenerateEditObj(current_EditGroup.transform);
                        }

                        break;
                    case MeshEditMode.Edge: //线框编辑Hand
                        var edge = MeshEditUtility.GetEdgeByPoint(_mouseClickFace, hit.point);
                        if (!edit_EdgeCache.Contains(edge)) edit_EdgeCache.Add(edge);
                        else edit_EdgeCache.Remove(edge);

                        if (edit_EdgeCache.Count <= 0) return;
                        groupTrans.position = edit_EdgeCache.GetMiddlePosition();

                        //创建新的编辑对象
                        foreach (var editEdge in edit_EdgeCache)
                        {
                            editEdge.m_vertex1.GenerateEditObj(current_EditGroup.transform);
                            editEdge.m_vertex2.GenerateEditObj(current_EditGroup.transform);
                        }

                        break;
                    case MeshEditMode.Face:
                        var face = _mouseClickFace;
                        if (!edit_FaceCache.Contains(face)) edit_FaceCache.Add(face);
                        else edit_FaceCache.Remove(face);
                        if (edit_FaceCache.Count <= 0) return;

                        groupTrans.position = MeshEditUtility.GetMiddlePosition(edit_FaceCache);
                        foreach (var editFace in edit_FaceCache)
                        {
                            editFace.m_vertex1.GenerateEditObj(current_EditGroup.transform);
                            editFace.m_vertex2.GenerateEditObj(current_EditGroup.transform);
                            editFace.m_vertex3.GenerateEditObj(current_EditGroup.transform);
                        }

                        break;
                }

                #endregion
            }
        }

        /// <summary>
        /// 工具状态检测
        /// </summary>
        private void ToolStateCheck()
        {
            switch (Tools.current)
            {
                case Tool.Move:
                    current_EditTool = MeshEditTool.Move;
                    MEDR_Internal_Utility.ShowNotificationOnSceneView("移动工具", 0.5f);
                    break;
                case Tool.Rotate:
                    current_EditTool = MeshEditTool.Rotate;
                    MEDR_Internal_Utility.ShowNotificationOnSceneView("旋转工具", 0.5f);
                    break;
                case Tool.Scale:
                    current_EditTool = MeshEditTool.Scale;
                    MEDR_Internal_Utility.ShowNotificationOnSceneView("缩放工具", 0.5f);
                    break;
            }

            if (current_EditMode != MeshEditMode.Object)
                Tools.current = Tool.None;
        }

        #endregion

        #region 事件订阅

        /// <summary>
        /// 订阅编辑模式改变事件
        /// </summary>
        private void SubscribeEditModeChangeEvent()
        {
            onEditModeChange += EditChange;
        }

        private void DeSubscribeEditModeChangeEvent()
        {
            onEditModeChange -= EditChange;
        }

        #endregion

        #region 内部方法

        /// <summary>
        /// 刷新网格（只刷新网格顶点的位置）
        /// </summary>
        public void RefreshMesh()
        {
            if (!editObj_Filter) return;
            var _mesh = new Mesh();
            _mesh.name = editObj_Filter.sharedMesh.name;
            var trans = editObj_Filter.transform;
            Vector3[] _allVertexs = editObj_Filter.sharedMesh.vertices;

            for (int i = 0; i < _allVertexs.Length; i++)
            {
                var resultEditVertex = merge_Vertex.Find(v => v.m_vertexIndexs.Contains(i));
                resultEditVertex.UpdateEditObjPosToVertex();
                _allVertexs[i] =
                    trans.InverseTransformPoint(resultEditVertex.m_vertex);
            }

            _mesh.vertices = _allVertexs;
            var sharedMesh = editObj_Filter.sharedMesh;
            _mesh.triangles = sharedMesh.triangles;
            _mesh.uv = sharedMesh.uv;
            _mesh.normals = sharedMesh.normals;

            _mesh.SetVertices(_allVertexs);
            _mesh.RecalculateBounds();
            _mesh.RecalculateNormals();
            _mesh.RecalculateTangents();

            editObj_Filter.sharedMesh = _mesh;
            if (editObj_Collider)
                editObj_Collider.sharedMesh = _mesh;
        }

        /// <summary>
        /// 设置编辑模式
        /// </summary>
        /// <param name="mode"></param>
        private void SetEditMode(bool mode)
        {
            IsEdit = mode;
            Locked = IsEdit;
            Tools.current = Tool.None;

            if (CheckFields != null && CheckFields.Count > 0)
            {
                selectFilter = (MeshFilter) CheckFields[0].Filter;
                if (selectFilter)
                {
                    if (selectFilter.GetComponent<MeshRenderer>())
                        selectFilter.GetComponent<MeshRenderer>().enabled = !IsEdit;
                    if (selectFilter.GetComponent<Collider>())
                        selectFilter.GetComponent<Collider>().enabled = !IsEdit;
                }
            }

            if (editObj)
                DestroyImmediate(editObj);
            if (IsEdit)
            {
                if (selectFilter)
                    editObj = MeshEditUtility.GenerateEditGameObject(selectFilter.gameObject);
                if (editObj)
                {
                    editObj.transform.tag = "EditorOnly";
                    editObj_Filter = editObj.GetComponent<MeshFilter>();
                    editObj_Collider = editObj.GetComponent<MeshCollider>();
                }

                EditChange();
                OnEditStart?.Invoke();
            }
            else
            {
                CleanEditData();
                OnEditEnd?.Invoke();
            }
        }


        /// <summary>
        /// 完成编辑
        /// </summary>
        private void FinishEditMesh()
        {
            if (EditorUtility.DisplayDialog("完成模型网格", "确定完成模型网格编辑了吗？\n编辑好的模型将替换原有的模型哦", "确定", "取消"))
            {
                selectFilter.sharedMesh = editObj_Filter.sharedMesh;
                OnFieldUnCheck();
            }
        }

        /// <summary>
        /// 更新合并信息
        /// </summary>
        private void UpdateMergeData()
        {
            merge_Vertex = MeshEditUtility.MergeMeshVertex(editObj_Filter.sharedMesh.vertices, editObj.transform);
            merge_Triangles = MeshEditUtility.MergeTriangles(editObj_Filter.sharedMesh.triangles, merge_Vertex);
        }

        /// <summary>
        /// 开始编辑
        /// </summary>
        private void EditChange()
        {
            if (!current_EditGroup)
            {
                current_EditGroup = new GameObject("EditGroup");
                current_EditGroup.tag = "EditorOnly";
                current_EditGroup.transform.SetParent(editObj.transform);
            }

            //current_EditEdge.hideFlags = HideFlags.HideInHierarchy;
            CleanEditData();
            UpdateMergeData(); //更新合并Mesh
        }

        /// <summary>
        /// 清理编辑数据
        /// </summary>
        private void CleanEditData()
        {
            CleanCache();
            CleanEditObj();
            if (current_EditGroup)
            {
                current_EditGroup.transform.localPosition = Vector3.zero;
                current_EditGroup.transform.rotation = Quaternion.identity;
                current_EditGroup.transform.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        private void CleanCache()
        {
            edit_VertexCache.Clear(); //顶点缓存清理
            edit_EdgeCache.Clear(); //线框缓存清理
            edit_FaceCache.Clear(); //片面缓存清理
        }

        private void CleanEditObj()
        {
            if (!current_EditGroup) return;
            var groupTrans = current_EditGroup.transform;

            var child = groupTrans.GetComponentsInChildren<Transform>();
            if (child.Length <= 0) return;
            for (int i = 0; i < child.Length; i++)
            {
                if (child[i] != groupTrans)
                    DestroyImmediate(child[i].gameObject);
            }
        }

        #endregion

        #region EditProgram编辑程序

        /// <summary>
        /// 编辑程序处理器
        /// </summary>
        private void EditProgramHandler()
        {
            if (!current_EditGroup || current_EditGroup.transform.childCount <= 0) return;
            //HighLight高亮显示
            switch (current_EditMode)
            {
                case MeshEditMode.Vertex:
                    VertexEditHighLight();
                    break;
                case MeshEditMode.Edge:
                    EdgeEditHighLight();
                    break;
                case MeshEditMode.Face:
                    FaceEditHighLight();
                    break;
            }

            var groupTrans = current_EditGroup.transform;
            switch (current_EditTool)
            {
                case MeshEditTool.Move:
                    groupTrans.position = Handles.PositionHandle(groupTrans.position, groupTrans.rotation);
                    break;
                case MeshEditTool.Rotate:
                    groupTrans.rotation = Handles.RotationHandle(groupTrans.rotation, groupTrans.position);
                    break;
                case MeshEditTool.Scale:
                    groupTrans.localScale =
                        Handles.ScaleHandle(groupTrans.localScale, groupTrans.position, groupTrans.rotation,
                            HandleUtility.GetHandleSize(groupTrans.position));
                    break;
            }
        }

        /// <summary>
        /// 定点编辑
        /// </summary>
        private void VertexEditHighLight()
        {
            if (edit_VertexCache == null) return;

            Handles.color = config.MEDR_MeshEdit_ClickColor;
            // var middlePos = MeshEditUtility.GetMiddlePosition(edit_VertexCache);
            // var offset = new List<Vector3>();
            foreach (var vertex in edit_VertexCache)
            {
                // var size = (editObj.transform.localScale.magnitude * 0.02f);
                var size = HandleUtility.GetHandleSize(editObj_Filter.transform.position) * 0.11f *
                           config.MEDR_MeshEdit_VertexSize_Multiplier;
                Handles.SphereHandleCap(0, vertex.m_vertex, Quaternion.identity,
                    size,
                    EventType.Repaint);
                //offset.Add(middlePos - vertex.m_vertex);
            }

            // var newPos = Handles.PositionHandle(middlePos, Quaternion.identity);
            // if (newPos != middlePos)
            // {
            //     for (int i = 0; i < edit_VertexCache.Count; i++)
            //     {
            //         edit_VertexCache[i].m_vertex = newPos - offset[i];
            //     }
            // }
        }

        private void EdgeEditHighLight()
        {
            Handles.color = config.MEDR_MeshEdit_ClickColor;
            foreach (var edge in edit_EdgeCache)
            {
#if UNITY_2020_1_OR_NEWER
                Handles.DrawLine(edge.m_vertex1.m_vertex, edge.m_vertex2.m_vertex,
                    2f * config.MEDR_MeshEdit_EdgeSize_Multiplier);
#elif UNITY_2019
            Handles.DrawLine(edge.m_vertex1.m_vertex, edge.m_vertex2.m_vertex);
#endif
            }
        }

        private void FaceEditHighLight()
        {
            foreach (var face in edit_FaceCache)
            {
                Handles.DrawSolidRectangleWithOutline(face.m_faceVertex, config.MEDR_MeshEdit_ClickColor,
                    config.MEDR_MeshEdit_HoverColor);
            }
        }

        #endregion
    }
}
#endif