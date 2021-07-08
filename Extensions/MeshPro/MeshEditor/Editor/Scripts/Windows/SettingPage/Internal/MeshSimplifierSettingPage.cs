#if UNITY_EDITOR
using Extensions.MeshPro.MeshEditor.Editor.Scripts.Base.Utilities;
using Extensions.MeshPro.MeshEditor.Editor.Scripts.Configs.Internal;
using Extensions.MeshPro.MeshEditor.Editor.Scripts.Manager;
using MeshEditor.Editor.Scripts.Base;
using MeshEditor.Editor.Scripts.Base.Utilities;
using UnityEditor;
using UnityEngine;

namespace Extensions.MeshPro.MeshEditor.Editor.Scripts.Windows.SettingPage.Internal
{
    public class MeshSimplifierSettingPage : MEDR_SettingPage
    {
        private MeshSimplifierConfig _config;

        private void OnEnable()
        {
            PageName = "网格压缩工具配置";
            _config = MEDR_ConfigManager.GetConfig<MeshSimplifierConfig>();
        }

        protected override void OnGUI()
        {
            if (_config == null) return;
            GUILayout.Label(PageName, MEDR_StylesUtility.SettingHeaderStyle);
            var meshSimplifierRate = _config.MEDR_MeshSimplifier_SimplifierRate;
            meshSimplifierRate = EditorGUILayout.Slider("压缩百分比:", meshSimplifierRate, 0f, 100f);
            meshSimplifierRate = meshSimplifierRate > 100 ? 100 : meshSimplifierRate < 0 ? 0 : meshSimplifierRate;
            _config.MEDR_MeshSimplifier_SimplifierRate = meshSimplifierRate;
            EditorGUILayout.LabelField(string.Format("当前LOD保存地址:  {0}", _config.MEDR_MeshSimplifier_LODSavePath));
            if (GUILayout.Button("设置保存地址"))
            {
                var path = EditorUtility.SaveFilePanelInProject("选择LOD网格保存目录", "LODs", "", "生成的LOD网格将保存在该目录下");
                if (!string.IsNullOrWhiteSpace(path))
                {
                    _config.MEDR_MeshSimplifier_LODSavePath = path;
                }
            }
        }
    }
}
#endif