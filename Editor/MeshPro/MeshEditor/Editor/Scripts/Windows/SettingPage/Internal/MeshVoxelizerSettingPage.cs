using MeshEditor.Editor.Scripts.Base;
using MeshEditor.Editor.Scripts.Base.Utilities;
using MeshEditor.Editor.Scripts.Configs.Internal;
using MeshEditor.Editor.Scripts.Manager;
using UnityEditor;
using UnityEngine;

namespace MeshEditor.Editor.Scripts.Windows.SettingPage.Internal
{
    public class MeshVoxelizerSettingPage : MEDR_SettingPage
    {
        private MeshVoxelizerConfig _config;
        
        private void OnEnable()
        {
            PageName = "网格体素工具配置";
            _config = MEDR_ConfigManager.GetConfig<MeshVoxelizerConfig>();
        }

        protected override void OnGUI()
        {
            if (_config == null) return;
            GUILayout.Label(PageName, MEDR_StylesUtility.SettingHeaderStyle);
            _config.MEDR_MeshVoxelizer_PixelSize =
                EditorGUILayout.IntField("像素大小", _config.MEDR_MeshVoxelizer_PixelSize);
            _config.MEDR_MeshVoxelizer_CustomVoxelMat =
                EditorGUILayout.Toggle("自定义材质", _config.MEDR_MeshVoxelizer_CustomVoxelMat);
        }
    }
}