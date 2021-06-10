using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityExtensions.MeshPro.Runtime.MeshDeformer
{
    public static class MeshDeformer
    {
        public static void Deform(ref Mesh mesh, Transform transform, Vector3 point, Vector3 direction, float radius,
            float stepRadius, float intensity,
            float stepIntensity)
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
                        vertices[i] = transform.InverseTransformPoint(v + (direction * s));
                        break;
                    }

                    s -= stepIntensity;
                }
            }

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.SetVertices(vertices);
            mesh.RecalculateTangents();
            mesh.RecalculateUVDistributionMetrics();
        }
    }
}