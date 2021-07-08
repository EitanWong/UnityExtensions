#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Extensions.MeshPro.MeshEditor.Modules.Internal.MeshEdit.Editor.Utilities
{
    /// <summary>
    /// Mesh编辑器帮助类
    /// </summary>
    public static class MeshEditUtility
    {
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// 生成编辑对象
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static GameObject GenerateEditGameObject(GameObject origin)
        {
            var result = new GameObject(origin.name);
            result.transform.SetParent(origin.transform);
            result.transform.localPosition = Vector3.zero;
            result.transform.localRotation = Quaternion.Euler(Vector3.zero);
            result.transform.localScale = Vector3.one;
            var targetFilter = result.AddComponent<MeshFilter>();
            var originFilter = origin.GetComponent<MeshFilter>();
            if (originFilter)
                targetFilter.sharedMesh = originFilter.sharedMesh;
            var renderer = result.AddComponent<MeshRenderer>();
            var originRenderer = origin.GetComponent<MeshRenderer>();
            if (originRenderer)
                renderer.sharedMaterial = originRenderer.sharedMaterial;
            var collider = result.AddComponent<MeshCollider>();
            if (targetFilter.sharedMesh)
                collider.sharedMesh = targetFilter.sharedMesh;
            //result.hideFlags = HideFlags.HideInHierarchy;
            return result;
        }

        public static Mesh GetNewMesh(Mesh origin)
        {
            Mesh r=new Mesh();
            r.name = origin.name;
            r.vertices = origin.vertices;
            r.triangles = origin.triangles;
            r.uv = origin.uv;
            r.normals = origin.normals;
            r.SetVertices(origin.vertices);
            r.RecalculateBounds();
            r.RecalculateNormals();
            r.RecalculateTangents();
            return r;
        }

        /// <summary>
        /// 合并网格顶点
        /// </summary>
        public static List<MeshEditVertex> MergeMeshVertex(Vector3[] MeshVertex,Transform trans)
        {
            List<MeshEditVertex> result = new List<MeshEditVertex>();
            for (int i = 0; i < MeshVertex.Length; i++)
            {
                var currentVertex = trans.TransformPoint(MeshVertex[i]);
                MeshEditVertex merge = result.Find(m => m.m_vertex == currentVertex);
                if (merge == null || merge.Count <= 0)
                {
                    merge = new MeshEditVertex(i, currentVertex);
                    result.Add(merge);
                }

                merge.AddVertexId(i);
            }
            return result;
        }


        /// <summary>
        /// 合并三角面
        /// </summary>
        /// <param name="MeshTriangle"></param>
        /// <param name="mergeVertex"></param>
        /// <returns></returns>
        public static List<MeshEditFace> MergeTriangles( int[] MeshTriangle, List<MeshEditVertex> mergeVertex)
        {
            List<MeshEditFace> result = new List<MeshEditFace>();
            for (int i = 0; (i + 2) < MeshTriangle.Length; i += 3)
            {
                var v1 = mergeVertex.Find(v => v.m_vertexIndexs.Contains(MeshTriangle[i]));
                var v2 = mergeVertex.Find(v => v.m_vertexIndexs.Contains(MeshTriangle[i + 1]));
                var v3 = mergeVertex.Find(v => v.m_vertexIndexs.Contains(MeshTriangle[i + 2]));
                MeshEditFace face = new MeshEditFace(result.Count, ref v1, ref v2, ref v3);
                result.Add(face);
            }

            return result;
        }


        /// <summary>
        /// 根据坐标获取顶点
        /// </summary>
        /// <param name="face"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static MeshEditVertex GetVertexByPoint(this MeshEditFace face, Vector3 point)
        {
            var distance1 = Vector3.Distance( face.m_vertex1.m_vertex, point);
            var distance2 = Vector3.Distance( face.m_vertex2.m_vertex, point);
            var distance3 = Vector3.Distance( face.m_vertex3.m_vertex, point);
            if (distance1 < distance2 && distance1 < distance3)
                return face.m_vertex1;
            if (distance2 < distance1 && distance2 < distance3)
                return face.m_vertex2;
            if (distance3 < distance1 && distance3 < distance2)
                return face.m_vertex3;
        
            return null;
        }

        /// <summary>
        /// 获取距离鼠标最近的边
        /// </summary>
        public static MeshEditEdge GetEdgeByPoint(this MeshEditFace face, Vector3 clickPoint)
        {
            float distance1 = HandleUtility.DistancePointLine(clickPoint, face.m_edge1.m_vertex1.m_vertex, face.m_edge1.m_vertex2.m_vertex);
            float distance2 = HandleUtility.DistancePointLine(clickPoint, face.m_edge2.m_vertex1.m_vertex, face.m_edge2.m_vertex2.m_vertex);
            float distance3 = HandleUtility.DistancePointLine(clickPoint, face.m_edge3.m_vertex1.m_vertex, face.m_edge3.m_vertex2.m_vertex);

            if (distance1 < distance2 && distance1 < distance3)
                return face.m_edge1;
            if (distance2 < distance1 && distance2 < distance3)
                return face.m_edge2;
            if (distance3 < distance1 && distance3 < distance2)
                return face.m_edge3;
            return face.m_edge1;
        }

        /// <summary>
        /// 获取中点坐标
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetMiddlePosition(this List<MeshEditVertex> vertices)
        {
            Vector3 result = default;
            foreach (var vertex in vertices)
            {
                result += vertex.m_vertex;
            }
            return result/ vertices.Count;
        }
    
        /// <summary>
        /// 获取中点坐标
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetMiddlePosition(this List<MeshEditEdge> edges)
        {
            Vector3 result = default;
            foreach (var edge in edges)
            {
                result += edge.middle;
            }
            return result/ edges.Count;
        }

        public static Vector3 GetMiddlePosition(this List<MeshEditFace> faces)
        {
            Vector3 result = default;
            foreach (var face in faces)
            {
                result += face.middle;
            }
            return result/ faces.Count;
        }


        public static bool IsToolControlId(int id)
        {
            int[] toolId = new[] {4822, 4821, 4820, 4823, 4825, 4824};
            return toolId.Contains(id);
        }
    }
}
#endif