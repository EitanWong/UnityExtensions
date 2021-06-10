using UnityEngine;

[System.Serializable]
public class MeshEditFace : object
{
    public int m_id; //三角面Id
    public MeshEditVertex m_vertex1, m_vertex2, m_vertex3;
    public MeshEditEdge m_edge1, m_edge2, m_edge3;


    public Vector3 middle
    {
        get { return (m_vertex1.m_vertex + m_vertex2.m_vertex + m_vertex3.m_vertex) / 3; }
    }

    public Vector3[] m_faceVertex
    {
        get
        {
            return new[]
            {
                m_vertex1.m_vertex,
                m_vertex2.m_vertex,
                m_vertex3.m_vertex,
                m_vertex3.m_vertex,
                m_vertex2.m_vertex,
                m_vertex1.m_vertex,
            };
        }
    }

    public MeshEditFace(int id, ref MeshEditVertex vertex1, ref MeshEditVertex vertex2, ref MeshEditVertex vertex3)
    {
        this.m_id = id;
        this.m_vertex1 = vertex1;
        this.m_vertex2 = vertex2;
        this.m_vertex3 = vertex3;
        BuildEdge();
    }

    /// <summary>
    /// 构建连线
    /// </summary>
    private void BuildEdge()
    {
        m_edge1 = new MeshEditEdge(ref m_vertex1, ref m_vertex2);
        m_edge2 = new MeshEditEdge(ref m_vertex2, ref m_vertex3);
        m_edge3 = new MeshEditEdge(ref m_vertex3, ref m_vertex1);
    }
}