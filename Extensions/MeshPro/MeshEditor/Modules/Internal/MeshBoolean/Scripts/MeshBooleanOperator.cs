using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Net3dBool;
using Net3dBool.CommonTool;

namespace N3dBoolExample
{
    public static class MeshBooleanOperator
    {
        static Mesh OperateAndMerge(Solid solid, Transform transform)
        {
            solid.translate((-transform.position).ToVector3Double());

            using (var vs = CollectionPool<Vector3>.ListCell.Create())
            {
                using (var vsd = CollectionPool<Vector3Double>.ListCell.Create())
                {
                    solid.GetVertices(vsd);
                    vs.Body.AddRange(from c in vsd select c.ToVector3());
                }
                // 至此顶点列表有效

                using (var ts = CollectionPool<int>.ListCell.Create())
                {
                    solid.GetTriangles(ts);
                    // 至此三角形列表有效

                    using (ObjectFaceF objF = ObjectFaceF.GetInstance(vs, ts))
                    {
                        objF.MergeNeighbors();
                        var result = objF.ToMesh();
                        result.SetUVinWCS();
                        return result;
                    }
                }
            }
        }

        public static Mesh GetUnion(MeshFilter meshF1, MeshFilter meshF2)
        {
            using (var booleanModeller = new BooleanModeller(meshF1.ToSolidInWCS(), meshF2.ToSolidInWCS()))
            {
                var end = booleanModeller.GetUnion();
                return OperateAndMerge(end, meshF1.transform);
            }
        }

        public static Mesh GetDifference(MeshFilter meshF1, MeshFilter meshF2)
        {
            using (var booleanModeller = new BooleanModeller(meshF1.ToSolidInWCS(), meshF2.ToSolidInWCS()))
            {
                var end = booleanModeller.GetDifference();
                return OperateAndMerge(end, meshF1.transform);
            }
        }

        public static Mesh GetIntersection(MeshFilter meshF1, MeshFilter meshF2)
        {
            using (var booleanModeller = new BooleanModeller(meshF1.ToSolidInWCS(), meshF2.ToSolidInWCS()))
            {
                var end = booleanModeller.GetIntersection();
                return OperateAndMerge(end, meshF1.transform);
            }
        }
    }
}