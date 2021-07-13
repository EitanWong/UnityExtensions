#if UNITY_EDITOR
using Extensions.MeshPro.MeshEditor.Editor.Scripts.Configs.Internal;
using Extensions.MeshPro.MeshEditor.Editor.Scripts.Manager;
using EzySlice;
using TransformPro.MeshPro.MeshEditor.Editor;
using TransformPro.MeshPro.MeshEditor.Editor.Scripts.Base;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MeshEditor.Editor.Scripts.Windows.WindowPage.Internal
{
    public class MeshBreakerPage : MEDR_Page
    {
        #region 网格破碎相关字段

        private static Material mesh_CustomBreakCutMat; //自定义切面材质

        private bool _meshCustomCutMode; //自定义切割模式


        private Vector3 startPos, endPos;

        private bool _useMouse;

        #endregion

        private MeshBreakerConfig _config;

        private void Awake()
        {
            PageName = "网格破碎";
            //PageIcon = Resources.Load<Texture2D>("Textures/MeshBreaker");
            PageIcon = Resources.Load<Texture2D>(
                "Textures/MeshBreaker");
            PageToolTips = "网格模型破碎制作工具";
            _config = MEDR_ConfigManager.GetConfig<MeshBreakerConfig>();
        }

        protected override void OnGUI()
        {
            MeshBreakMenu();
        }

        protected override void OnSceneGUI()
        {
            CustomCutSceneGUIProgram();
        }


        /// <summary>
        /// 破碎网格编辑菜单
        /// </summary>
        private void MeshBreakMenu()
        {
            if (CheckFields == null || CheckFields.Count <= 0) return;

            var editMeshRenderer = CheckFields[0].Renderer;
            var editMeshFilter = CheckFields[0].Filter;

            _config.MEDR_MeshBreaker_breakIterations =
                EditorGUILayout.IntField("迭代次数", _config.MEDR_MeshBreaker_breakIterations);
            EditorGUILayout.HelpBox("迭代次数越多，切片数量越多", MessageType.Info);
            if (!editMeshRenderer || !editMeshRenderer.sharedMaterial) //没有网格渲染器，或者 没有材质 直接选择自定义材质
            {
                _config.MEDR_MeshBreaker_CustomCutFaceMat = true;
            }
            else
            {
                _config.MEDR_MeshBreaker_CustomCutFaceMat = EditorGUILayout.Toggle(new GUIContent("自定义切面材质"),
                    _config.MEDR_MeshBreaker_CustomCutFaceMat);
            }


            if (_config.MEDR_MeshBreaker_CustomCutFaceMat) //如果选择自定义
            {
                mesh_CustomBreakCutMat =
                    (Material) EditorGUILayout.ObjectField(new GUIContent("切面材质"), mesh_CustomBreakCutMat,
                        typeof(Material),
                        false);
            }
            else if (editMeshRenderer && editMeshRenderer.sharedMaterial)
            {
                mesh_CustomBreakCutMat = editMeshRenderer.sharedMaterial;
            }

            if (mesh_CustomBreakCutMat)
                EditorGUILayout.HelpBox(string.Format("当前使用材质:{0}", mesh_CustomBreakCutMat.name), MessageType.Info);
            else
            {
                EditorGUILayout.HelpBox(string.Format("未选择切面材质"), MessageType.Warning);
            }


            if (GUILayout.Button("破碎网格"))
            {
                if (EditorUtility.DisplayDialog("确定要破碎网格模型吗?\n此操作不可逆哦", "", "确定", "取消"))
                {
                    //MeshBreaker.BreakMesh(editMeshFilter, mesh_CutCascades);
                    if (!mesh_CustomBreakCutMat)
                    {
                        mesh_CustomBreakCutMat = new Material(Shader.Find("Standard"));
                    }

                    foreach (var field in CheckFields)
                    {
                        editMeshFilter = field.Filter;
                        editMeshRenderer = field.Renderer;
                        if (mesh_CustomBreakCutMat)
                            mesh_CustomBreakCutMat = field.Renderer.sharedMaterial;
                        if (ShatterObject(editMeshFilter.transform, editMeshFilter.gameObject,
                            _config.MEDR_MeshBreaker_breakIterations,
                            mesh_CustomBreakCutMat))
                            editMeshRenderer.enabled = false;
                    }

                    MeshEditorWindow.window.ShowNotification(new GUIContent("网格破碎执行成功"), 1f);
                }
            }

            if (GUILayout.Button(_meshCustomCutMode ? "退出自定义切割" : "自定义切割"))
                _meshCustomCutMode = !_meshCustomCutMode;
        }

        /// <summary>
        /// 自定义切割程序
        /// </summary>
        private void CustomCutSceneGUIProgram()
        {
            if (_meshCustomCutMode) //如果进入自定义切割模式
            {
                if (Event.current.button != 0) return;
                // We use hotControl to lock focus onto the editor (to prevent deselection)
                int controlID = GUIUtility.GetControlID(FocusType.Passive);
                switch (Event.current.GetTypeForControl(controlID))
                {
                    case EventType.MouseDown:
                        GUIUtility.hotControl = controlID;
                        Ray startray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        startPos = startray.origin;
                        endPos = startPos;
                        if (Physics.Raycast(startray, out var hitstart, Mathf.Infinity))
                            startPos = hitstart.point;
                        Event.current.Use();
                        _useMouse = true;
                        break;
                    case EventType.MouseDrag:
                        var endray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        endPos = endray.origin;
                        if (Physics.Raycast(endray, out var hitend, Mathf.Infinity))
                            endPos = hitend.point;
                        SceneView.RepaintAll();
                        break;
                    case EventType.MouseUp:
                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                        _useMouse = false;
                        for (int i = CheckFields.Count - 1; i >= 0; i--)
                        {
                            var item = CheckFields[i];
                            if (item == null) continue;
                            Vector3 centre = (startPos + endPos) / 2;
                            Vector3 up = Vector3.Cross((startPos - endPos), (startPos - Camera.current.transform.position))
                                .normalized;
                            var SliceObj = item.Filter.gameObject.SliceInstantiate(centre, up, mesh_CustomBreakCutMat);

                            if (SliceObj == null) continue;
                            foreach (var slice in SliceObj)
                            {
                                if (slice)
                                {
                                    slice.transform.SetParent(item.Filter.transform);
                                    slice.transform.localPosition = Vector3.zero;
                                    MeshEditorWindow.AddMeshField(slice);
                                }
                            }

                            MeshEditorWindow.UncheckField(item);
                            item.Renderer.enabled = false;
                        }

                        MeshEditorWindow.window.Repaint();
                        break;
                }

                if (_useMouse)
                {
                    Handles.color = _config.MEDR_MeshBreaker_CustomCutColor;
                    Handles.DrawDottedLine(startPos, endPos, _config.MEDR_MeshBreaker_CustomCutLineSize);
                }
            }
        }


        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// 网格破碎
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="iterations"></param>
        /// <param name="crossSectionMaterial"></param>
        /// <returns></returns>
        public bool ShatterObject(Transform root, GameObject obj, int iterations, Material crossSectionMaterial)
        {
            if (iterations > 0)
            {
                var scaleOffset = obj.transform.localScale;
                var objPosition = obj.transform.position;
                GameObject[] slices = obj.SliceInstantiate(objPosition + (Random.insideUnitSphere * scaleOffset.magnitude),
                    objPosition + Random.insideUnitSphere,
                    new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f),
                    crossSectionMaterial);

                if (slices != null)
                {
                    // shatter the shattered! 
                    for (int i = 0; i < slices.Length; i++)
                    {
                        slices[i].transform.SetParent(root);
                        slices[i].transform.localPosition = Vector3.zero;
                        if (ShatterObject(root, slices[i], iterations - 1, crossSectionMaterial))
                        {
                            GameObject.DestroyImmediate(slices[i]);
                        }
                    }

                    return true;
                }

                return ShatterObject(root, obj, iterations - 1, crossSectionMaterial);
            }

            return false;
        }
    }
}
#endif