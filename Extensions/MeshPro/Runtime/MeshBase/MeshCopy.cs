using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshCopy
{
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
}