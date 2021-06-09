using System.Collections;
using System.Collections.Generic;
using MeshVoxelizerProject;
using UnityEngine;

public static class VoxelMeshBuilder
{
    #region 外部方法

    /// <summary>
    /// 转换成体素网格
    /// </summary>
    /// <param name="origin_mesh">原始网格</param>
    /// <param name="PixelSize">像素尺寸</param>
    /// <returns></returns>
    public static Mesh ConversionVoxelMesh(Mesh origin_mesh, int PixelSize)
    {
        if (!origin_mesh)
            return null;
        Box3 bounds = new Box3(origin_mesh.bounds.min, origin_mesh.bounds.max);

        var m_voxelizer = new MeshVoxelizer(PixelSize, PixelSize, PixelSize);
        m_voxelizer.Voxelize(origin_mesh.vertices, origin_mesh.triangles, bounds);

        Vector3 scale = new Vector3(bounds.Size.x / PixelSize, bounds.Size.y / PixelSize, bounds.Size.z / PixelSize);
        Vector3 m = new Vector3(bounds.Min.x, bounds.Min.y, bounds.Min.z);
        return CreateMesh(m_voxelizer.Voxels, scale, m, PixelSize);
    }

    #endregion

    #region 内部方法

    private static Mesh CreateMesh(int[,,] voxels, Vector3 scale, Vector3 min, int size)
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> indices = new List<int>();

        for (int z = 0; z < size; z++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (voxels[x, y, z] != 1) continue;

                    Vector3 pos = min + new Vector3(x * scale.x, y * scale.y, z * scale.z);

                    if (x == size - 1 || voxels[x + 1, y, z] == 0)
                        AddRightQuad(verts, indices, scale, pos);

                    if (x == 0 || voxels[x - 1, y, z] == 0)
                        AddLeftQuad(verts, indices, scale, pos);

                    if (y == size - 1 || voxels[x, y + 1, z] == 0)
                        AddTopQuad(verts, indices, scale, pos);

                    if (y == 0 || voxels[x, y - 1, z] == 0)
                        AddBottomQuad(verts, indices, scale, pos);

                    if (z == size - 1 || voxels[x, y, z + 1] == 0)
                        AddFrontQuad(verts, indices, scale, pos);

                    if (z == 0 || voxels[x, y, z - 1] == 0)
                        AddBackQuad(verts, indices, scale, pos);
                }
            }
        }

        if (verts.Count > 65000)
        {
            Debug.Log("Mesh has too many verts. You will have to add code to split it up.");
            return new Mesh();
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetTriangles(indices, 0);

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }

    private static void AddRightQuad(List<Vector3> verts, List<int> indices, Vector3 scale, Vector3 pos)
    {
        int count = verts.Count;

        verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 1 * scale.z));
        verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 0 * scale.z));
        verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 0 * scale.z));

        verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 1 * scale.z));
        verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 1 * scale.z));
        verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 0 * scale.z));

        indices.Add(count + 2);
        indices.Add(count + 1);
        indices.Add(count + 0);
        indices.Add(count + 5);
        indices.Add(count + 4);
        indices.Add(count + 3);
    }

    private static void AddLeftQuad(List<Vector3> verts, List<int> indices, Vector3 scale, Vector3 pos)
    {
        int count = verts.Count;

        verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 1 * scale.z));
        verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 0 * scale.z));
        verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 0 * scale.z));

        verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 1 * scale.z));
        verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 1 * scale.z));
        verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 0 * scale.z));

        indices.Add(count + 0);
        indices.Add(count + 1);
        indices.Add(count + 2);
        indices.Add(count + 3);
        indices.Add(count + 4);
        indices.Add(count + 5);
    }

    private static void AddTopQuad(List<Vector3> verts, List<int> indices, Vector3 scale, Vector3 pos)
    {
        int count = verts.Count;

        verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 1 * scale.z));
        verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 0 * scale.z));
        verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 0 * scale.z));

        verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 1 * scale.z));
        verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 1 * scale.z));
        verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 0 * scale.z));

        indices.Add(count + 0);
        indices.Add(count + 1);
        indices.Add(count + 2);
        indices.Add(count + 3);
        indices.Add(count + 4);
        indices.Add(count + 5);
    }

    private static void AddBottomQuad(List<Vector3> verts, List<int> indices, Vector3 scale, Vector3 pos)
    {
        int count = verts.Count;

        verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 1 * scale.z));
        verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 0 * scale.z));
        verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 0 * scale.z));

        verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 1 * scale.z));
        verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 1 * scale.z));
        verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 0 * scale.z));

        indices.Add(count + 2);
        indices.Add(count + 1);
        indices.Add(count + 0);
        indices.Add(count + 5);
        indices.Add(count + 4);
        indices.Add(count + 3);
    }

    private static void AddFrontQuad(List<Vector3> verts, List<int> indices, Vector3 scale, Vector3 pos)
    {
        int count = verts.Count;

        verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 1 * scale.z));
        verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 1 * scale.z));
        verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 1 * scale.z));

        verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 1 * scale.z));
        verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 1 * scale.z));
        verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 1 * scale.z));

        indices.Add(count + 2);
        indices.Add(count + 1);
        indices.Add(count + 0);
        indices.Add(count + 5);
        indices.Add(count + 4);
        indices.Add(count + 3);
    }

    private static void AddBackQuad(List<Vector3> verts, List<int> indices, Vector3 scale, Vector3 pos)
    {
        int count = verts.Count;

        verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 0 * scale.z));
        verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 0 * scale.z));
        verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 0 * scale.z));

        verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 0 * scale.z));
        verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 0 * scale.z));
        verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 0 * scale.z));

        indices.Add(count + 0);
        indices.Add(count + 1);
        indices.Add(count + 2);
        indices.Add(count + 3);
        indices.Add(count + 4);
        indices.Add(count + 5);
    }

    #endregion
}