using System.Collections.Generic;
using MeshEditor.Editor.Scripts.Manager;
using MeshEditor.UnityMeshSimplifier;
using UnityEditor;
using UnityEngine;
using TransformPro.MeshPro.MeshEditor.Editor;
using TransformPro.MeshPro.MeshEditor.Editor.Scripts.Base;

public class MeshSimplifierPage : MEDR_Page
{
    #region 网格压缩相关字段

    private static List<Mesh> SimplifiedMeshs = new List<Mesh>(); //压缩后的网格
    private static float mesh_LastSimplifierRate; //上一个赋值的压缩比例
    private static bool meshSimplifierFold; //是否点压缩选项开折页
    private MeshSimplifierConfig _config;

    #endregion

    private void Awake()
    {
        PageName = "网格压缩";
        PageIcon = Resources.Load<Texture2D>(
            "Textures/MeshSimplifier");
        PageToolTips = "网格模型压缩工具\n有效减少模型大小";
        if (_config == null)
            _config = MEDR_ConfigManager.GetConfig<MeshSimplifierConfig>();
    }

    protected override void OnGUI()
    {
        MeshSimplifierMenu();
    }

    /// <summary>
    /// 压缩网格编辑菜单
    /// </summary>
    private void MeshSimplifierMenu()
    {
        if (CheckFields == null || CheckFields.Count <= 0) return;
        var editMeshRenderer = CheckFields[0].Renderer;
        var editMeshFilter = CheckFields[0].Filter;
        ///————————————————————————————————————————————————————————————————————————————————————————————————————网格压缩
        // meshSimplifierFold = EditorGUILayout.BeginFoldoutHeaderGroup(meshSimplifierFold, new GUIContent("网格压缩"));
        // if (meshSimplifierFold)
        // {
        var meshSimplifierRate = _config.MEDR_MeshSimplifier_SimplifierRate;
        meshSimplifierRate = EditorGUILayout.Slider(new GUIContent("压缩比例%"), meshSimplifierRate, 0f, 100f);
        meshSimplifierRate = meshSimplifierRate > 100 ? 100 : meshSimplifierRate < 0 ? 0 : meshSimplifierRate;
        _config.MEDR_MeshSimplifier_SimplifierRate = meshSimplifierRate;
        if (_config.MEDR_MeshSimplifier_SimplifierRate <= 0)
        {
            EditorGUILayout.HelpBox("未压缩", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("压缩比例越高，顶点数量越少", MessageType.Info);
            if (SimplifiedMeshs != null || SimplifiedMeshs.Count >= 0) //如果已经操作过压缩并生成压缩网格
            {
                int index = 0;
                foreach (var mesh in SimplifiedMeshs)
                {
                    index++;
                    EditorGUILayout.Space(5);
                    EditorGUILayout.HelpBox(
                        string.Format("({0})  压缩后-->顶点数:{1}\n压缩后-->三角面数:{2}"
                            , index
                            , mesh.vertexCount
                            , mesh.triangles.Length)
                        , MessageType.Info, true);
                    EditorGUILayout.Space(5);
                }
            }


            if (mesh_LastSimplifierRate != meshSimplifierRate && GUILayout.Button("预览"))
            {
                SimplifiedMeshs.Clear();
                foreach (var field in CheckFields)
                {
                    editMeshFilter = field.Filter;
                    var smfMesh = SimplifierMesh(editMeshFilter.sharedMesh,
                        (100 - meshSimplifierRate) * 0.01f);
                    SimplifiedMeshs.Add(smfMesh);
                }

                mesh_LastSimplifierRate = meshSimplifierRate;
                //ShowNotification(new GUIContent("压缩已更新"), 1f);
            }

            if (SimplifiedMeshs.Count >= 0 && GUILayout.Button("压缩"))
            {
                if (EditorUtility.DisplayDialog("确定要应用\n压缩后的网格吗?\n此操作不可逆哦", "", "确定", "取消"))
                {
                    for (int i = 0; i < CheckFields.Count; i++)
                    {
                        CheckFields[i].Filter.sharedMesh = SimplifiedMeshs[i];
                    }

                    //editMeshFilter.sharedMesh = SimplifiedMeshs;
                    MeshEditorWindow.window.ShowNotification(new GUIContent("压缩完成"), 1f);
                }
            }
        }

        // }
        //
        // EditorGUILayout.EndFoldoutHeaderGroup();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private Mesh SimplifierMesh(Mesh sourceMesh, float quality)
    {
        var meshSimplifier = new MeshSimplifier();
        meshSimplifier.Initialize(sourceMesh);
        meshSimplifier.SimplifyMesh(quality);
        var mesh = meshSimplifier.ToMesh();
        mesh.name = sourceMesh.name;
        return mesh;
    }
}