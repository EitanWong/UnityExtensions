using UnityEngine;

[System.Serializable]
public class MeshEditEdge : object
{
    public MeshEditVertex m_vertex1, m_vertex2;

    public Vector3 middle //中点
    {
        get { return (m_vertex1.m_vertex + m_vertex2.m_vertex) / 2; }
    }

    public MeshEditEdge(ref MeshEditVertex vertex1, ref MeshEditVertex vertex2)
    {
        this.m_vertex1 = vertex1;
        this.m_vertex2 = vertex2;
    }
}