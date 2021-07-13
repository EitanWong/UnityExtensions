#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Extensions.MeshPro.MeshEditor.Editor.Scripts.Configs.Internal;
using Extensions.MeshPro.MeshEditor.Editor.Scripts.Manager;
using Extensions.MeshPro.MeshEditor.Modules.Internal.MeshSimplifier.Runtime;
using TransformPro.MeshPro.MeshEditor.Editor;
using TransformPro.MeshPro.MeshEditor.Editor.Scripts.Base;
using UnityEditor;
using UnityEngine;

public class MeshSimplifierPage : MEDR_Page
{
    #region 网格压缩相关字段

    private static List<Mesh> SimplifiedMeshs = new List<Mesh>(); //压缩后的网格
    private static float mesh_LastSimplifierRate; //上一个赋值的压缩比例
    private static bool meshSimplifierFold; //是否点压缩选项开折页
    private MeshSimplifierConfig _config;
    // ReSharper disable once InconsistentNaming

    public float[] percentOfVerticesForEachLod =
        {100.0f, 75.0f, 65.0f, 45.0f, 35.0f, 25.0f, 15.0f, 10.0f, 5.0f}; //9

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
        EditorGUILayout.Space(20);
        LODGenerationMenu();
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
    }

    /// <summary>
    /// 生成LODMenu
    /// </summary>
    private void LODGenerationMenu()
    {
        if (CheckFields == null || CheckFields.Count <= 0) return;
        var editMeshRenderer = CheckFields[0].Renderer;
        var editMeshFilter = CheckFields[0].Filter;
        EditorGUILayout.BeginVertical("box");
        GUILayout.Space(10);
        EditorGUILayout.LabelField("LOD生成设置", EditorStyles.boldLabel);
        GUILayout.Space(10);
        EditorGUI.indentLevel += 1;
        _config.MEDR_MeshSimplifier_LODDistanceMultiplier = EditorGUILayout.Slider(new GUIContent("LOD距离倍乘数"),
            _config.MEDR_MeshSimplifier_LODDistanceMultiplier, 0.001f, 0.999f);
        GUILayout.Space(10);
        _config.MEDR_MeshSimplifier_DetailsCount = EditorGUILayout.IntSlider(new GUIContent("LOD级数",
                "LOD 系统应生成的 LOD 数量."),
            _config.MEDR_MeshSimplifier_DetailsCount, 1, 8);
        GUILayout.Space(10);
        _config.MEDR_MeshSimplifier_LODGroupFadeMode = (int) (LODFadeMode) EditorGUILayout.EnumPopup("LOD组过度模式",
            (LODFadeMode) _config.MEDR_MeshSimplifier_LODGroupFadeMode);
        GUILayout.Space(10);
        EditorGUI.indentLevel -= 1;
        EditorGUILayout.LabelField("LOD层级设置", EditorStyles.boldLabel);
        EditorGUI.indentLevel += 1;
        for (int i = 0; i <= _config.MEDR_MeshSimplifier_DetailsCount; i++)
        {
            if (i == 0)
                continue;
            percentOfVerticesForEachLod[i] = EditorGUILayout.Slider(new GUIContent("LOD 顶点百分比 " + i,
                    "它将包含在 LOD 中的顶点百分比" + i + " 在网格中."),
                percentOfVerticesForEachLod[i], 1f, 100f);
        }

        EditorGUI.indentLevel -= 1;
        if (GUILayout.Button("一键生成LOD"))
        {
            try
            {
                EditorUtility.DisplayProgressBar("生成LOD", "生成LOD中", 0f);
                var screenRelativeTransitionHeight = 1f;
                foreach (var item in CheckFields)
                {
                    var lodGroup = item.Filter.gameObject.GetComponent<LODGroup>();
                    if (!lodGroup)
                        lodGroup = item.Filter.gameObject.AddComponent<LODGroup>();
                    DestroyChildrenGameObjectByName(lodGroup.gameObject, "LODs");
                    var groupRoot = new GameObject("LODs");
                    groupRoot.transform.SetParent(lodGroup.transform);
                    groupRoot.transform.localPosition = Vector3.zero;
                    groupRoot.transform.localScale = Vector3.one;
                    groupRoot.transform.localEulerAngles = Vector3.zero;

                    var rootName = string.Format("{0}_{1}", item.Filter.sharedMesh.name,
                        item.Filter.sharedMesh.GetInstanceID());
                    ReCreateDirectory(_config.MEDR_MeshSimplifier_LODSavePath + string.Format("/{0}", rootName));

                    LOD[] lods = new LOD[_config.MEDR_MeshSimplifier_DetailsCount];
                    Renderer[] defaultLODRenderers = new Renderer[1];
                    defaultLODRenderers[0] = item.Renderer;
                    screenRelativeTransitionHeight *= _config.MEDR_MeshSimplifier_LODDistanceMultiplier;
                    lods[0] = new LOD(screenRelativeTransitionHeight, defaultLODRenderers);
                    for (int i = 1; i < _config.MEDR_MeshSimplifier_DetailsCount; i++)
                    {
                        GameObject lodObj =
                            new GameObject(string.Format("{0}_LOD_Level{1}", rootName, i));
                        lodObj.transform.parent = groupRoot.transform;
                        lodObj.transform.localPosition = Vector3.zero;
                        lodObj.transform.localScale = Vector3.one;
                        lodObj.transform.localEulerAngles = Vector3.zero;
                        var filter = lodObj.AddComponent<MeshFilter>();
                        // ReSharper disable once PossibleLossOfFraction


                        var simpMesh = SimplifierMesh(item.Filter.sharedMesh,
                            percentOfVerticesForEachLod[i] / 100f);
                        simpMesh.name = lodObj.name;
                        // ReSharper disable once InconsistentNaming
                        var LODMesh = SaveLOD(rootName, simpMesh);

                        filter.sharedMesh = LODMesh;
                        // ReSharper disable once InconsistentNaming
                        Renderer[] LODRenderers = new Renderer[1];
                        LODRenderers[0] = lodObj.AddComponent<MeshRenderer>();
                        LODRenderers[0].sharedMaterial = item.Renderer.sharedMaterial;
                        screenRelativeTransitionHeight *= _config.MEDR_MeshSimplifier_LODDistanceMultiplier;
                        lods[i] = new LOD(screenRelativeTransitionHeight, LODRenderers);
                    }

                    lodGroup.fadeMode = (LODFadeMode) _config.MEDR_MeshSimplifier_LODGroupFadeMode;
                    lodGroup.SetLODs(lods);
                    lodGroup.RecalculateBounds();
                    //DestroyImmediate(item.Renderer);
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(item.Filter.gameObject.scene);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("生成LOD失败!", ex.Message, "确定");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        EditorGUILayout.EndVertical();
    }


    private void DestroyChildrenGameObjectByName(GameObject parent, string name)
    {
        var children = parent.GetComponentsInChildren<Transform>();
        for (int i = children.Length - 1; i > 0; i--)
        {
            if (children[i].gameObject.name.Equals(name))
            {
                DestroyImmediate(children[i].gameObject);
                DestroyChildrenGameObjectByName(parent, name);
                break;
            }
        }
    }

    private Mesh SaveLOD(string folderName, Mesh lodMesh)
    {
        if (_config.MEDR_MeshSimplifier_LODSavePath == null)
            return lodMesh;
        var path = _config.MEDR_MeshSimplifier_LODSavePath + string.Format("/{0}", folderName);
        var assetPath = path + string.Format("/{0}.mesh", lodMesh.name);
        var saveMesh = CopyNewMesh(lodMesh);
        AssetDatabase.CreateAsset(saveMesh, assetPath);
        AssetDatabase.SaveAssets();
        return saveMesh;
    }

    /// <summary>
    /// 重新创建目录
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static bool ReCreateDirectory(string url)
    {
        try
        {
            if (Directory.Exists(url))
                AssetDatabase.DeleteAsset(url);
            if (!Directory.Exists(url)) //如果不存在就创建file文件夹　　             　　              
                Directory.CreateDirectory(url); //创建该文件夹
            return true;
        }
#pragma warning disable 168
        catch (Exception ex)
#pragma warning restore 168
        {
            return false;
        }
    }

    public static Mesh CopyNewMesh(Mesh origin)
    {
        Mesh r = new Mesh();
        r.name = origin.name;
        r.vertices = origin.vertices;
        r.triangles = origin.triangles;
        r.uv = origin.uv;
        r.normals = origin.normals;
        r.SetVertices(origin.vertices);
        r.RecalculateBounds();
        r.RecalculateNormals();
        r.RecalculateTangents();
        return r;
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
#endif