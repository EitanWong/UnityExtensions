using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class MeshEditVertex : object
{
    public int m_id;
    public Vector3 m_vertex;
    public List<int> m_vertexIndexs = new List<int>(); //顶点Id

     public GameObject editObj; //编辑对象


    public int Count
    {
        get { return m_vertexIndexs.Count; }
    }

    public MeshEditVertex(int id, Vector3 vertex)
    {
        this.m_id = id;
        this.m_vertex = vertex;
    }

    public void AddVertexId(int id)
    {
        if (!m_vertexIndexs.Contains(id))
            m_vertexIndexs.Add(id);
    }

    /// <summary>
    /// 生成编辑顶点
    /// </summary>
    public void GenerateEditObj()
    {
        DestoryEditObj();
        editObj = new GameObject(string.Format("EditVertex{0}:{1}", m_id, m_vertex));
        editObj.transform.position = m_vertex;
    }

    /// <summary>
    /// 生成编辑顶点
    /// </summary>
    /// <param name="parent"></param>
    public void GenerateEditObj(Transform parent)
    {
        GenerateEditObj();
        if (parent)
            editObj.transform.SetParent(parent);
    }

    /// <summary>
    /// 销毁编辑顶点
    /// </summary>
    public void DestoryEditObj()
    {
        if (editObj)
            MonoBehaviour.DestroyImmediate(editObj);
    }
    
    /// <summary>
    /// 更新顶点从编辑对象上
    /// </summary>
    public void UpdateEditObjPosToVertex()
    {
        if (editObj)
            m_vertex = editObj.transform.position;
    }
}