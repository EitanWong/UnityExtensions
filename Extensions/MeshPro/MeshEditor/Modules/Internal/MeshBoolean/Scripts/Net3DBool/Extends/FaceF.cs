using Net3dBool.CommonTool;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Net3dBool
{
    /// <summary>
    /// 在并查集中表示三角面和所在平面的信息，确认邻接关系；
    /// 一个<see cref="FaceF"/>实例是并查集的一个成员
    /// </summary>
    public class FaceF : IDisposable
    {
        const float TOL = 1e-5f;

        FaceF root = null;
        public FaceF Root { get { return root == null ? this : root.Root; } }
        public bool IsRoot() { return root == null; }

        // 顶点集合
        CollectionPool<Vector3>.ListCell weak_vertices;
        public List<Vector3> Vertices { get { return IsRoot() ? weak_vertices : Root.weak_vertices; } }

        // 确定所在平面
        public Vector3 PlaneNormal { get; private set; }
        public float DisOrigin2plane { get; private set; }

        public float Area
        {
            get
            {
                return Vector3.Cross(Vertices[1] - Vertices[0], Vertices[2] - Vertices[0]).magnitude / 2;
            }
        }

        public FaceF(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            InitMember(p1, p2, p3);
        }
        public FaceF InitMember(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            if (weak_vertices != null) { weak_vertices.Dispose(); }
            weak_vertices = CollectionPool<Vector3>.ListCell.Create(p1, p2, p3);
            PlaneNormal = Vector3.Cross(p2 - p1, p3 - p1).normalized;
            DisOrigin2plane = Vector3.Dot(PlaneNormal, p1);
            disposedValue = false;
            return this;
        }
        private FaceF() { }

        public bool SamePlane(FaceF another)
        {
            return SameVector(PlaneNormal, another.PlaneNormal) && 
                Mathf.Abs(DisOrigin2plane - another.DisOrigin2plane) < TOL;
        }

        public bool SameFace(FaceF another)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (!SameVector(Vertices[i], another.Vertices[j])) { return false; }
                }
            }
            return true;
        }

        public void SetParent(FaceF parent)
        {
            if (IsRoot()) { root = parent.Root; }
            else { Root.SetParent(parent); }
        }

        static bool V3Collinear(Vector3 a, Vector3 b, Vector3 c)
        {
            return Mathf.Abs(Vector3.Cross(b - a, c - a).magnitude) < TOL;
        }

        public static bool SameVector(Vector3 a, Vector3 b)
        {
            return Mathf.Abs(a.x - b.x) < TOL && Mathf.Abs(a.y - b.y) < TOL && Mathf.Abs(a.z - b.z) < TOL;
        }

        public static bool FaceClearOf(FaceF fa, FaceF fb, float epsilon = float.Epsilon)
        {
            Vector3 getMin(IList<Vector3> vs)
            {
                return new Vector3(Mathf.Min(vs[0].x, vs[1].x, vs[2].x), 
                    Mathf.Min(vs[0].y, vs[1].y, vs[2].y), Mathf.Min(vs[0].z, vs[1].z, vs[2].z));
            }
            Vector3 getMax(IList<Vector3> vs)
            {
                return new Vector3(Mathf.Max(vs[0].x, vs[1].x, vs[2].x),
                    Mathf.Max(vs[0].y, vs[1].y, vs[2].y), Mathf.Max(vs[0].z, vs[1].z, vs[2].z));
            }
            Vector3 faMin = getMin(fa.Vertices);
            Vector3 faMax = getMax(fa.Vertices);
            Vector3 fbMin = getMin(fb.Vertices);
            Vector3 fbMax = getMax(fb.weak_vertices);

            return faMin.x > fbMax.x + epsilon || faMax.x < fbMin.x - epsilon ||
                faMin.y > fbMax.y + epsilon || faMax.y < fbMin.y - epsilon ||
                faMin.z > fbMax.z + epsilon || faMax.z < fbMin.z - epsilon;
        }

        /// <summary>
        /// 确定两个面是否可以合并
        /// </summary>
        /// <param name="fa"></param>
        /// <param name="fb"></param>
        /// <param name="faCommon">共同点的下标 i，另一个下标为 (i+2)%3</param>
        /// <param name="fbCommon">共同点的下标 j，另一个下标为 (j+1)%3</param>
        /// <returns></returns>
        public static bool TryConfirmNeighbor(FaceF fa, FaceF fb, out int faCommon, out int fbCommon)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (SameVector(fa.Vertices[i], fb.Vertices[j]) &&
                        SameVector(fa.Vertices[(i + 2) % 3], fb.Vertices[(j + 1) % 3]) &&
                        V3Collinear(fa.Vertices[(i + 1) % 3], fa.Vertices[i], fb.Vertices[(j + 2) % 3]))
                    {
                        faCommon = i;
                        fbCommon = j;
                        return true;
                    }
                }
            }
            faCommon = -1;
            fbCommon = -1;
            return false;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    weak_vertices.Dispose();
                    weak_vertices = null;
                    ObjectPool<FaceF>.Recycle(this);
                }
                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。
                root = null;

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~FaceF() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    /// <summary>
    /// 由<see cref="FaceF"/>的集合描述的 3D 形状
    /// </summary>
    public class ObjectFaceF : IDisposable
    {
        const float areaEpsilon = 1e-5f;

        CollectionPool<FaceF>.ListCell weak_faceFs;

        public ObjectFaceF(IList<Vector3> vertices, IList<int> triangles)
        {
            InitMember(vertices, triangles);
        }
        public ObjectFaceF InitMember(IList<Vector3> vertices, IList<int> triangles)
        {
            if (weak_faceFs != null) { weak_faceFs.Dispose(); }
            weak_faceFs = CollectionPool<FaceF>.ListCell.Create();
            for (int i = 0; i + 2 < triangles.Count; i += 3)
            {
                var faceF = ObjectPool<FaceF>.Create(initFacef, construct);
                weak_faceFs.Add(faceF);

                void initFacef(FaceF cell) => cell.InitMember(
                        vertices[triangles[i]],
                        vertices[triangles[i + 1]],
                        vertices[triangles[i + 2]]);
                FaceF construct() => new FaceF(
                    vertices[triangles[i]],
                    vertices[triangles[i + 1]],
                    vertices[triangles[i + 2]]);
            }
            disposedValue = false;
            return this;
        }
        public static ObjectFaceF GetInstance(IList<Vector3> vertices, IList<int> triangles)
        {
            return ObjectPool<ObjectFaceF>.Create(initFromPool, construct);

            ObjectFaceF construct() => new ObjectFaceF(vertices, triangles);
            void initFromPool(ObjectFaceF o) => o.InitMember(vertices, triangles);
        }

        void GetVertices(List<Vector3> vertices)
        {
            for (int i = 0; i < weak_faceFs.Count; i++)
            {
                var face = weak_faceFs[i];
                if (!face.IsRoot() || face.Area < areaEpsilon) { continue; }
                vertices.Add(face.Vertices[0]);
                vertices.Add(face.Vertices[1]);
                vertices.Add(face.Vertices[2]);
            }
        }

        public Mesh ToMesh()
        {
            using (var vertices = CollectionPool<Vector3>.ListCell.Create())
            {
                GetVertices(vertices);
                using (var triangles = CollectionPool<int>.ListCell.Create())
                {
                    for (int i = 0; i < vertices.Count; i++) { triangles.Add(i); }
                    Mesh result = new Mesh();
                    result.SetVertices(vertices);
                    result.SetTriangles(triangles, 0);
                    result.RecalculateBounds();
                    result.RecalculateNormals();
                    return result;
                }
            }
        }

        /// <summary>
        /// 合并小三角形为大三角形
        /// </summary>
        public void MergeNeighbors()
        {
            int faceNum = weak_faceFs.Count;
            for (int i = 0; i < faceNum; i++)
            {
                for (int j = 0; j < faceNum; j++)
                {
                    if (i == j) { continue; }
                    FaceF fa = weak_faceFs[i];
                    FaceF fb = weak_faceFs[j];

                    if (fa.SameFace(fb) || !fa.SamePlane(fb)) { continue; }

                    if (FaceF.TryConfirmNeighbor(fa, fb, out int faC, out int fbC))
                    {
                        fa.Vertices[faC] = fb.Vertices[(fbC + 2) % 3];
                        fb.SetParent(fa);  // 根节点是 fa
                    }
                }
            }
        }

        void UpdateVertice(Vector3 oldValue, Vector3 newValue)
        {
            for (int faceCount = 0; faceCount < weak_faceFs.Count; faceCount++)
            {
                var f = weak_faceFs[faceCount];
                if (!f.IsRoot()) { continue; }
                var vertices = f.Vertices;
                for (int i = 0; i < vertices.Count; i++)
                {
                    if (vertices[i].Equals(oldValue))
                    {
                        vertices[i] = newValue;
                        break;
                    }
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    for (int i = 0; i < weak_faceFs.Count; i++)
                    {
                        weak_faceFs[i].Dispose();
                    }
                    weak_faceFs.Dispose();
                    weak_faceFs = null;
                    ObjectPool<ObjectFaceF>.Recycle(this);
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~ObjectFaceF() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
