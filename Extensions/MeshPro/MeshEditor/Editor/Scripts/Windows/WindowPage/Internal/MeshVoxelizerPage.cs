#if UNITY_EDITOR
using Extensions.MeshPro.MeshEditor.Editor.Scripts.Configs.Internal;
using Extensions.MeshPro.MeshEditor.Editor.Scripts.Manager;
using TransformPro.MeshPro.MeshEditor.Editor.Scripts.Base;
using UnityEditor;
using UnityEngine;

namespace MeshEditor.Editor.Scripts.Windows.WindowPage.Internal
{
    public class MeshVoxelizerPage : MEDR_Page
    {
        #region 体素网格相关字段

        private bool meshVoxelizerFold; //是否点开体素网格选项
        private static Material mesh_CustomVoxelMat; //自定义体素材质
        private MeshVoxelizerConfig _config;

        #endregion

        private void Awake()
        {
            PageName = "体素网格";
            //PageIcon= Resources.Load<Texture2D>("Textures/MeshVoxelizer");
            PageIcon = Resources.Load<Texture2D>("Textures/MeshVoxelizer");
            PageToolTips = "体素网格生成器\n生成体素网格";
            _config = MEDR_ConfigManager.GetConfig<MeshVoxelizerConfig>();
        }


        protected override void OnGUI()
        {
            MeshVoxelizerMenu();
        }

        /// <summary>
        /// 体素网格编辑菜单
        /// </summary>
        private void MeshVoxelizerMenu()
        {
            var editMeshRenderer = CheckFields[0].Renderer;
            var editMeshFilter = CheckFields[0].Filter;
            ///————————————————————————————————————————————————————————————————————————————————————————————————————体素网格
            // meshVoxelizerFold = EditorGUILayout.BeginFoldoutHeaderGroup(meshVoxelizerFold, new GUIContent("体素网格"));
            // if (meshVoxelizerFold)
            // {
            _config.MEDR_MeshVoxelizer_PixelSize =
                EditorGUILayout.IntField(new GUIContent("像素大小"), _config.MEDR_MeshVoxelizer_PixelSize);
            if (!editMeshRenderer || !editMeshRenderer.sharedMaterial) //没有网格渲染器，或者 没有材质 直接选择自定义材质
            {
                _config.MEDR_MeshVoxelizer_CustomVoxelMat = true;
            }
            else
            {
                _config.MEDR_MeshVoxelizer_CustomVoxelMat = EditorGUILayout.Toggle(new GUIContent("自定义体素材质"),
                    _config.MEDR_MeshVoxelizer_CustomVoxelMat);
            }

            if (_config.MEDR_MeshVoxelizer_CustomVoxelMat) //如果选择自定义
            {
                mesh_CustomVoxelMat =
                    (Material) EditorGUILayout.ObjectField(new GUIContent("体素材质"), mesh_CustomVoxelMat,
                        typeof(Material),
                        false);
            }
            else if (editMeshRenderer && editMeshRenderer.sharedMaterial)
            {
                mesh_CustomVoxelMat = editMeshRenderer.sharedMaterial;
            }

            if (mesh_CustomVoxelMat)
                EditorGUILayout.HelpBox(string.Format("当前使用材质:{0}", mesh_CustomVoxelMat.name), MessageType.Info);
            else
            {
                EditorGUILayout.HelpBox(string.Format("未选择体素材质"), MessageType.Warning);
            }

            if (GUILayout.Button("创建体素网格"))
            {
                for (int i = 0; i < CheckFields.Count; i++)
                {
                    editMeshFilter = CheckFields[i].Filter;
                    if (!_config.MEDR_MeshVoxelizer_CustomVoxelMat)
                        mesh_CustomVoxelMat = editMeshRenderer.sharedMaterial;

                    var voxelMesh = VoxelMeshBuilder.ConversionVoxelMesh(editMeshFilter.sharedMesh,
                        _config.MEDR_MeshVoxelizer_PixelSize);
                    if (voxelMesh)
                    {
                        voxelMesh.name = editMeshFilter.sharedMesh.name;
                        GameObject newVoxelGameObj =
                            new GameObject(string.Format("{0}Voxelized", editMeshFilter.transform.name));
                        newVoxelGameObj.transform.position = editMeshFilter.transform.position;
                        newVoxelGameObj.transform.localScale = editMeshFilter.transform.localScale;
                        newVoxelGameObj.transform.rotation = editMeshFilter.transform.rotation;

                        var filter = newVoxelGameObj.AddComponent<MeshFilter>();
                        var renderer = newVoxelGameObj.AddComponent<MeshRenderer>();

                        filter.sharedMesh = voxelMesh;
                        renderer.sharedMaterial = mesh_CustomVoxelMat;
                    }
                }
            }
        }
    }
}
#endif