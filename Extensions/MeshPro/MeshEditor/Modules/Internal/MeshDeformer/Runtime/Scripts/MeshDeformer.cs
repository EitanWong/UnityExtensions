using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityExtensions.MeshPro.Runtime.MeshDeformer
{
    public static class MeshDeformer
    {
        /// <summary>
        /// 变形
        /// </summary>
        /// <param name="mesh">网格Mesh</param>
        /// <param name="transform">模型Transform</param>
        /// <param name="point">变形点坐标</param>
        /// <param name="direction">变形方向</param>
        /// <param name="radius">半径</param>
        /// <param name="stepRadius">半径步长</param>
        /// <param name="intensity">强度</param>
        /// <param name="intensityStep">强度步长</param>
        public static void Deform(ref Mesh mesh, Transform transform, Vector3 point, Vector3 direction, float radius,
            float stepRadius, float intensity,
            float intensityStep)
        {
            List<Vector3> vertices = mesh.vertices.ToList();

            for (int i = 0; i < vertices.Count; i++)
            {
                var v = transform.TransformPoint(vertices[i]);
                var distense = Vector3.Distance(point, v);
                var s = intensity;
                for (float r = 0.0f; r < radius; r += stepRadius)
                {
                    if (distense < r)
                    {
                        //Vector3 pointToVertex = vertices[i] - point;
                        float attenuatedForce = s / (1f + direction.sqrMagnitude);
                        float velocity = attenuatedForce * Time.deltaTime;
                        var tmpDeformerDir = direction.normalized * velocity;
                        vertices[i] = transform.InverseTransformPoint(v + tmpDeformerDir);
                        break;
                    }

                    s -= intensityStep;
                }
            }

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.SetVertices(vertices);
            mesh.RecalculateTangents();
#if UNITY_2020_1_OR_NEWER
            mesh.RecalculateUVDistributionMetrics();
#endif
        }
        public static void Deform(ref Mesh mesh, Transform transform, Vector3 point, float radius,
            float stepRadius, float intensity,
            float intensityStep)
        {
            List<Vector3> vertices = mesh.vertices.ToList();

            for (int i = 0; i < vertices.Count; i++)
            {
                var v = transform.TransformPoint(vertices[i]);
                var distense = Vector3.Distance(point, v);
                var s = intensity;
                for (float r = 0.0f; r < radius; r += stepRadius)
                {
                    if (distense < r)
                    {
                        Vector3 pointToVertex = vertices[i] - point;
                        float attenuatedForce = s / (1f + pointToVertex.sqrMagnitude);
                        float velocity = attenuatedForce * Time.deltaTime;
                        var tmpDeformerDir = pointToVertex.normalized * velocity;
                        vertices[i] = transform.InverseTransformPoint(v + tmpDeformerDir * Time.deltaTime);
                        break;
                    }

                    s -= intensityStep;
                }
            }

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.SetVertices(vertices);
            mesh.RecalculateTangents();
#if UNITY_2020_1_OR_NEWER
            mesh.RecalculateUVDistributionMetrics();
#endif
        }
    }
}