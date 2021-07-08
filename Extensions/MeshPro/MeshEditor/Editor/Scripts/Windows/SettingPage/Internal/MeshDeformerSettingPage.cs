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
    public class MeshDeformerSettingPage : MEDR_SettingPage
    {
        private MeshDeformerConfig _config;
        private void OnEnable()
        {
            PageName = "网格变形工具配置";
            _config = MEDR_ConfigManager.GetConfig<MeshDeformerConfig>();
        }
        
        protected override void OnGUI()
        {
            if (_config == null) return;
            GUILayout.Label(PageName, MEDR_StylesUtility.SettingHeaderStyle);
            _config.MEDR_MeshDeformer_HoverColor = EditorGUILayout.ColorField("高亮颜色", _config.MEDR_MeshDeformer_HoverColor);
            _config.MEDR_MeshDeformer_ClickColor = EditorGUILayout.ColorField("选中颜色", _config.MEDR_MeshDeformer_ClickColor);
            
        }
    }
}
#endif
