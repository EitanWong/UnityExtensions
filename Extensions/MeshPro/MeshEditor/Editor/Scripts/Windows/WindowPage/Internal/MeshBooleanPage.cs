#if UNITY_EDITOR
using Extensions.MeshPro.MeshEditor.Modules.Internal.MeshBoolean.Scripts;
using UnityEditor;
using UnityEngine;
using TransformPro.MeshPro.MeshEditor.Editor.Scripts.Base;

public class MeshBooleanPage : MEDR_Page
{
    private static GUIContent intersectionGUIContent;
    private static GUIContent unionGUIContent;
    private static GUIContent subtractionGUIContent;

    private void Awake()
    {
        PageName = "布尔运算";
        PageIcon = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin
            ? "Textures/MeshBooleanWhite"
            : "Textures/MeshBooleanBlack");
        PageToolTips = "布尔运算工具\n进行模型的布尔运算";
        intersectionGUIContent = new GUIContent("交集", Resources.Load<Texture2D>("Textures/Intersection"));
        unionGUIContent = new GUIContent("并集", Resources.Load<Texture2D>("Textures/Union"));
        subtractionGUIContent = new GUIContent("补集", Resources.Load<Texture2D>("Textures/Subtraction"));
    }

    protected override void OnGUI()
    {
        if (CheckFields.Count <= 1) return;
        DrawBoolFunctionButton();
    }


    private void DrawBoolFunctionButton()
    {
        EditorGUILayout.BeginVertical();
        if (GUILayout.Button(intersectionGUIContent))
        {
            GetBoolean(BooleanType.Intersection);
        }

        if (GUILayout.Button(unionGUIContent))
        {
            GetBoolean(BooleanType.Union);
        }

        if (GUILayout.Button(subtractionGUIContent))
        {
            GetBoolean(BooleanType.Subtraction);
        }

        EditorGUILayout.EndVertical();
    }

    #region Method

    // ReSharper disable Unity.PerformanceAnalysis
    /// <summary>
    /// 交集
    /// </summary>
    private void GetBoolean(BooleanType type)
    {
        Mesh mesh = CheckFields[0].Filter.sharedMesh;
        var obj = CreateNewGameObject(string.Format("New Intersection {0}", CheckFields[0].Filter.sharedMesh.name));
        MeshFilter currentFilter = obj.GetComponent<MeshFilter>();
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = CheckFields[0].Renderer.sharedMaterial;
        currentFilter.sharedMesh = mesh;
        currentFilter.transform.position = CheckFields[0].Filter.transform.position;

        int idx = 0;
        BooleanOperate:
        idx++;
        var bfield = CheckFields[idx];
        if (bfield != null && bfield.Filter)
        {
            switch (type)
            {
                case BooleanType.Intersection:
                    mesh = MeshBooleanOperator.GetIntersection(currentFilter, bfield.Filter);
                    break;
                case BooleanType.Union:
                    mesh = MeshBooleanOperator.GetUnion(currentFilter, bfield.Filter);
                    break;
                case BooleanType.Subtraction:
                    mesh = MeshBooleanOperator.GetDifference(currentFilter, bfield.Filter);
                    break;
            }
        }

        currentFilter.sharedMesh = Copy(mesh);
        if (idx + 1 < CheckFields.Count)
            goto BooleanOperate;
    }


    private GameObject CreateNewGameObject(string name)
    {
        var obj = new GameObject(name);
        var filter = obj.AddComponent<MeshFilter>();
        var render = obj.AddComponent<MeshRenderer>();
        return obj;
        //filter.sharedMesh = mesh;
        //render.sharedMaterial = mat;
    }

    /// <summary>
    /// 复制一个全新的网格(只复制原网格信息，与原网格没有引用关系)
    /// </summary>
    /// <param name="originMesh">原始网格</param>
    /// <returns>新网格</returns>
    private Mesh Copy(Mesh originMesh)
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

    #endregion

    enum BooleanType
    {
        Intersection,
        Union,
        Subtraction,
    }
}
#endif