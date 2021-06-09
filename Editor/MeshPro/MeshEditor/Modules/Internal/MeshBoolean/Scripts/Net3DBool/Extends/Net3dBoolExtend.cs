using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Net3dBool.CommonTool;
using System;

namespace Net3dBool
{
    public static class Net3dBoolExtend
    {
        #region Vector3Double and Vector3
        public static Vector3Double ToVector3Double(this Vector3 vector)
        {
            return new Vector3Double(vector.x, vector.y, vector.z);
        }

        public static Vector3[] ToVector3Arr(this Vector3Double[] vector3DoubleArr)
        {
            Vector3[] result = new Vector3[vector3DoubleArr.Length];
            for (int i = 0; i < vector3DoubleArr.Length; i++) { result[i] = vector3DoubleArr[i].ToVector3(); }
            return result;
        }

        public static Vector3 ToVector3(this Vector3Double vector)
        {
            return new Vector3((float)vector.x, (float)vector.y, (float)vector.z);
        }

        public static bool Equals(this Vector3Double vd, Vector3 vf)
        {
            return vd.ToVector3().Equals(vf);
        }

        public static bool Equals(this Vector3 vf, Vector3Double vd)
        {
            return vd.ToVector3().Equals(vf);
        }

        /// <summary>
        /// 带公差判等
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="another"></param>
        /// <param name="tol"></param>
        /// <returns></returns>
        public static bool EqualsTol(this Vector3Double vector, Vector3Double another, double tol)
        {
            if (tol < 0) { tol = -tol; }
            var delta = vector - another;
            return Math.Abs(delta.x) < tol && Math.Abs(delta.y) < tol && Math.Abs(delta.z) < tol;
        }
        #endregion

        #region Solid and Mesh
        /// <summary>
        /// 转化<see cref="Solid"/>实例为<see cref="Mesh"/>网格
        /// [!] 转化时重新展开三角形，顶点数显著增加。
        /// [!] 顶点数三倍于三角形数目
        /// </summary>
        /// <param name="solid"></param>
        /// <returns></returns>
        public static Mesh ToMesh(this Solid solid, bool expand = true)
        {
            var triangles = solid.Triangles;
            var solidVs = solid.Vertices;

            Mesh result;
            if (expand)
            {
                Vector3[] vsArr = new Vector3[triangles.Length];
                int[] tsArr = new int[triangles.Length];
                for (int i = 0; i < triangles.Length; i++)
                {
                    vsArr[i] = solidVs[triangles[i]].ToVector3();
                    tsArr[i] = i;
                }
                result = new Mesh
                {
                    vertices = vsArr,
                    triangles = tsArr
                };
            }
            else
            {
                result = new Mesh
                {
                    vertices = solidVs.ToVector3Arr(),
                    triangles = triangles
                };
            }
            result.RecalculateBounds();
            result.RecalculateNormals();
            return result;
        }

        public static Solid ToSolid(this Mesh mesh, Func<Vector3, Vector3Double> transFunc)
        {
            using (var meshVs = CollectionPool<Vector3>.ListCell.Create())
            {
                mesh.GetVertices(meshVs);
                using (var meshTs = CollectionPool<int>.ListCell.Create())
                {
                    mesh.GetTriangles(meshTs, 0);
                    using (var solidVs =
                        CollectionPool<Vector3Double>.ListCell.Create(
                            from c in meshVs select transFunc(c)))
                    {
                        return new Solid(solidVs, meshTs);
                    }
                }
            }
        }

        /// <summary>
        /// 生成<see cref="Solid"/>实例，不换算网格顶点
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static Solid ToSolidLocal(this Mesh mesh) => ToSolid(mesh, ToVector3Double);

        /// <summary>
        /// 生成 <see cref="Solid"/> 实例，换算网格顶点到世界坐标系，无需变换 <see cref="Solid"/> 实例
        /// </summary>
        /// <param name="meshF"></param>
        /// <returns></returns>
        public static Solid ToSolidInWCS(this MeshFilter meshF)
        {
            return ToSolid(meshF.sharedMesh, transformPoint);
            Vector3Double transformPoint(Vector3 c) => meshF.transform.TransformPoint(c).ToVector3Double();
        }

        /// <summary>
        /// 生成 <see cref="Solid"/> 实例，换算网格顶点到世界坐标系，无需变换 <see cref="Solid"/> 实例
        /// </summary>
        /// <param name="mesh">网格</param>
        /// <param name="root">网格所在的坐标系变换</param>
        /// <returns></returns>
        public static Solid ToSolidInWCS(this Mesh mesh, Transform root)
        {
            return ToSolid(mesh, transformPoint);
            Vector3Double transformPoint(Vector3 c) => root.TransformPoint(c).ToVector3Double();
        }
        #endregion

        #region Reset UV
        #region Project normal
        static readonly Vector3 X = Vector3.right;  // x axis
        static readonly Vector3 Y = Vector3.up;  // y axis
        static readonly Vector3 Z = Vector3.forward;  // z axis
        static readonly Vector3[] axis = new Vector3[6] { X, Y, Z, -X, -Y, -Z };
        static Vector3 NearNormal(Vector3 v)
        {
            Vector3 near = Vector3.zero;
            float nDot = -2;
            for (int i = 0; i < axis.Length; i++)
            {
                if (v == axis[i]) { return axis[i]; }
                float dot = Vector3.Dot(v, axis[i]);
                if (dot > nDot)
                {
                    nDot = dot;
                    near = axis[i];
                }
            }
            return near;
        }
        // 求点投影后的 uv 坐标
        static Vector2 Project2UV(Vector3 normal, Vector3 point)
        {
            Vector3 n = NearNormal(normal);  // X, Y or Z
            Vector2 result;
            if (n == X) { return new Vector2(point.z, point.y); }
            else if (n == -X) { result = new Vector2(-point.z, point.y); }
            else if (n == Y) { result = new Vector2(point.x, point.z); }
            else if (n == -Y) { result = new Vector2(-point.x, point.z); }
            else if (n == Z) { result = new Vector2(-point.x, point.y); }
            else if (n == -Z) { result = new Vector2(point.x, point.y); }
            else
            {
                Debug.LogWarning("预料之外的 uv 方向，使用缺省值");
                result = new Vector2(point.x, point.y);
            }
            //Debug.Log($"result = {result}");
            return result;
        }
        #endregion Project normal
        public static void SetUVinWCS(this Mesh mesh, Transform root = null, int whichUV = 1)
        {
            var uvArr = mesh.UVinWCS(root);
            if (whichUV == 1) { mesh.uv = uvArr; }
            else { typeof(Mesh).GetProperty($"uv{whichUV}").SetValue(mesh, uvArr); }
        }

        public static void SetUVinWCS(this MeshFilter meshF, int whichUV = 1)
        {
            var mesh = meshF.mesh;
            var uvArr = mesh.UVinWCS(meshF.transform);
            if (whichUV == 1) { mesh.uv = uvArr; }
            else { typeof(Mesh).GetProperty($"uv{whichUV}").SetValue(mesh, uvArr); }
        }

        public static Vector2[] UVinWCS(this Mesh mesh, Transform root = null)
        {
            Vector3 transPoint(Vector3 o) { return root != null ? root.TransformPoint(o) : o; }
            var triangles = mesh.triangles;
            var vertices = mesh.vertices;  // 本地坐标
            for (int i = 0; i < vertices.Length; i++) { vertices[i] = transPoint(vertices[i]); }

            Vector2[] uvs = new Vector2[vertices.Length];
            bool[] uvDirty = new bool[uvs.Length];
            void setOnce(int index, Vector2 value)
            {
                if (!uvDirty[index])
                {
                    uvDirty[index] = true;
                    uvs[index] = value;
                }
            }

            for (int i = 0; i + 2 < triangles.Length; i += 3)  // 对每个三角面
            {
                var va = vertices[triangles[i]];
                var vb = vertices[triangles[i + 1]];
                var vc = vertices[triangles[i + 2]];
                Vector3 normal = Vector3.Cross(vb - va, vc - va).normalized;  // 求法线，投影到平面
                setOnce(triangles[i], Project2UV(normal, va));
                setOnce(triangles[i + 1], Project2UV(normal, vb));
                setOnce(triangles[i + 2], Project2UV(normal, vc));
            }
            return uvs;
        }
        #endregion Reset UV

        #region Collection
        public static void DisposeAt<T>(this List<T> list, int index) where T:IDisposable
        {
            if (index < 0 || index >= list.Count || list.Count == 0 || list == null) { return; }
            T cell = list[index];
            cell.Dispose();
            list.RemoveAt(index);
        }
        #endregion
    }
}
