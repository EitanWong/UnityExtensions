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
    public class MeshBreakerSettingPage : MEDR_SettingPage
    {
        private MeshBreakerConfig config;

        private void OnEnable()
        {
            PageName = "网格破碎工具配置";
            config = MEDR_ConfigManager.GetConfig<MeshBreakerConfig>();
        }

        protected override void OnGUI()
        {
            if (config == null) return;
            GUILayout.Label(PageName, MEDR_StylesUtility.SettingHeaderStyle);
            config.MEDR_MeshBreaker_breakIterations =
                EditorGUILayout.IntField("破碎迭代次数", config.MEDR_MeshBreaker_breakIterations);
            config.MEDR_MeshBreaker_CustomCutFaceMat =
                EditorGUILayout.Toggle("自定义切面材质", config.MEDR_MeshBreaker_CustomCutFaceMat);

            config.MEDR_MeshBreaker_CustomCutColor =
                EditorGUILayout.ColorField("自定义切线颜色:", config.MEDR_MeshBreaker_CustomCutColor);

            config.MEDR_MeshBreaker_CustomCutLineSize = EditorGUILayout.FloatField("自定义切线尺寸:",config.MEDR_MeshBreaker_CustomCutLineSize);
        }
    }
}
#endif