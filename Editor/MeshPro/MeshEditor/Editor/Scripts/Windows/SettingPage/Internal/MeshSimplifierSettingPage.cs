using System;
using MeshEditor.Editor.Scripts.Base;
using MeshEditor.Editor.Scripts.Base.Utilities;
using MeshEditor.Editor.Scripts.Manager;
using UnityEditor;
using UnityEngine;

namespace MeshEditor.Editor.Scripts.Windows.SettingPage.Internal
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
        }
    }
}