#if UNITY_EDITOR
using System.Collections.Generic;
using Extensions.MeshPro.MeshEditor.Editor.Scripts.Configs.Internal;
using Extensions.MeshPro.MeshEditor.Editor.Scripts.Manager;
using TransformPro.MeshPro.MeshEditor.Editor.Scripts.Base;
using UnityEditor;
using UnityEngine;
using UnityExtensions.MeshPro.Runtime.MeshDeformer;

namespace Extensions.MeshPro.MeshEditor.Editor.Scripts.Windows.WindowPage.Internal
{
    enum DeformerDirection
    {
        Normal,
        Direction
    }

    public class MeshDeformerPage : MEDR_Page
    {
        #region Property

        private bool isEdit;
        private DeformerDirection directionType;
        private bool _wasMouseClick;
        private MeshDeformerConfig _config;

        #endregion

        private void Awake()
        {
            PageName = "网格变形器";
            PageIcon = Resources.Load<Texture2D>("Textures/MeshDeformer");
            _config = MEDR_ConfigManager.GetConfig<MeshDeformerConfig>();
        }


        protected override void OnGUI()
        {
            if (GUILayout.Button(isEdit ? "完成编辑" : "开始编辑"))
            {
                isEdit = !isEdit;
            }

            if (!isEdit) return;
            directionType = (DeformerDirection) _config.MEDR_MeshDeformer_directionType;
            _config.MEDR_MeshDeformer_directionType =
                (int) (DeformerDirection) EditorGUILayout.EnumPopup("变形方向", directionType);
            _config.MEDR_MeshDeformer_brushMaxradius =
                Mathf.Abs(EditorGUILayout.FloatField("最大半径", _config.MEDR_MeshDeformer_brushMaxradius));
            _config.MEDR_MeshDeformer_brushradius = EditorGUILayout.Slider("笔刷半径",
                _config.MEDR_MeshDeformer_brushradius, 0, _config.MEDR_MeshDeformer_brushMaxradius);
            _config.MEDR_MeshDeformer_brushRadiusStep =
                Mathf.Abs(EditorGUILayout.FloatField("半径步长", _config.MEDR_MeshDeformer_brushRadiusStep));
            _config.MEDR_MeshDeformer_brushMaxintensity =
                Mathf.Abs(EditorGUILayout.FloatField("最大硬度", _config.MEDR_MeshDeformer_brushMaxintensity));
            _config.MEDR_MeshDeformer_bushIntensity = EditorGUILayout.Slider("笔刷硬度",
                _config.MEDR_MeshDeformer_bushIntensity, 0, _config.MEDR_MeshDeformer_brushMaxintensity);
            _config.MEDR_MeshDeformer_brushIntensityStep =
                Mathf.Abs(EditorGUILayout.FloatField("硬度步长", _config.MEDR_MeshDeformer_brushIntensityStep));
        }

        protected override void OnFieldUnCheck()
        {
            CLoseEdit();
        }

        protected override void OnWindowDestroy()
        {
            OnFieldUnCheck();
        }

        protected override void OnSceneGUI()
        {
            Tools.hidden = isEdit;
            if (!isEdit) return;
            EditProgram();
        }

        #region EditProgram

        private void EditProgram()
        {
            var e = Event.current;
            //If the placment brush is selected and the mouse is being dragged across the scene view.
            bool mouseDrag = e.type == EventType.MouseDrag && e.button == 0;
            bool mouseDown = e.type == EventType.MouseDown && e.button == 0;
            bool mouseUp = e.type == EventType.MouseUp && e.button == 0;
            bool mouseLeaveWindow = e.type == EventType.MouseLeaveWindow;

            if (mouseDown)
                _wasMouseClick = true;
            if (mouseUp)
                _wasMouseClick = false;

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            bool isMouseClick = mouseDrag || _wasMouseClick;

            DrawPaintCircle(
                _wasMouseClick
                    ? _config.MEDR_MeshDeformer_ClickColor
                    : _config.MEDR_MeshDeformer_HoverColor,
                _config.MEDR_MeshDeformer_brushradius);

            if (isMouseClick)
            {
                Ray drawPointRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit drawPointHit;

                if (GetHitPoint(drawPointRay.origin, drawPointRay.direction, out drawPointHit))
                {
                    var meshItem = CheckFields.Find(item => item.Filter.transform.Equals(drawPointHit.transform));
                    if (meshItem != null)
                    {
                        var dir = drawPointHit.normal;
                        switch (directionType)
                        {
                            case DeformerDirection.Normal:
                                dir = drawPointHit.normal;
                                break;
                            case DeformerDirection.Direction:
                                dir = drawPointRay.direction;
                                break;
                        }

                        Mesh filterSharedMesh = Copy(meshItem.Filter.sharedMesh);
                        MeshDeformer.Deform(ref filterSharedMesh,
                            drawPointHit.transform,
                            drawPointHit.point,
                            e.shift ? dir : -dir,
                            _config.MEDR_MeshDeformer_brushradius, _config.MEDR_MeshDeformer_brushRadiusStep,
                            _config.MEDR_MeshDeformer_bushIntensity,
                            _config.MEDR_MeshDeformer_brushIntensityStep);
                        meshItem.Filter.sharedMesh = filterSharedMesh;
                        if (meshItem.Filter.TryGetComponent<MeshCollider>(out var collider))
                            collider.sharedMesh = filterSharedMesh;
                    }
                }
            }
        }

        private void CLoseEdit()
        {
            isEdit = false;
        }

//Draw other
        private void DrawPaintCircle(Color circleColour, float radius)
        {
            Ray drawPointRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit drawPointHit;

            if (GetHitPoint(drawPointRay.origin, drawPointRay.direction, out drawPointHit))
            {
                Handles.color = circleColour;
                Handles.DrawSolidDisc(drawPointHit.point, drawPointHit.normal, radius * .5f);
                SceneView.RepaintAll();
            }
        }

        private bool GetHitPoint(Vector3 origin, Vector3 direction, out RaycastHit hit)
        {
            // ReSharper disable once Unity.PreferNonAllocApi
            List<RaycastHit> hits = new List<RaycastHit>(Physics.RaycastAll(origin, direction));
            float minDist = Mathf.Infinity;
            int idToReturn = -1;

            for (int i = 0; i < hits.Count; i++)
            {
                if (CheckFields != null &&
                    CheckFields.Find(item => item.Filter.transform.Equals(hits[i].transform)) == null)
                    continue;
                //If we get this far then it is a valid surface, we just need to check the distance;
                float curDist = Vector3.Distance(origin, hits[i].point);
                if (curDist < minDist)
                {
                    idToReturn = i;
                    minDist = curDist;
                }
            }

            if (hits.Count == 0 || idToReturn == -1)
            {
                hit = new RaycastHit();
                return false;
            }

            hit = hits[idToReturn];
            return true;
        }

        /// <summary>
        /// 复制一个全新的网格(只复制原网格信息，与原网格没有引用关系)
        /// </summary>
        /// <param name="originMesh">原始网格</param>
        /// <returns>新网格</returns>
        public static Mesh Copy(Mesh originMesh)
        {
            Mesh resultMesh = new Mesh();
            resultMesh.bounds = originMesh.bounds;
            resultMesh.vertices = originMesh.vertices;
            resultMesh.triangles = originMesh.triangles;
            resultMesh.normals = originMesh.normals;
            resultMesh.tangents = originMesh.tangents;
            resultMesh.uv = originMesh.uv;
            resultMesh.colors = originMesh.colors;
            resultMesh.bindposes = originMesh.bindposes;
            resultMesh.name = originMesh.name;
            return resultMesh;
        }

        #endregion
    }
}
#endif