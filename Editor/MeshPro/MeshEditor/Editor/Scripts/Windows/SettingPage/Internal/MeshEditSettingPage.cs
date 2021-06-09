using System;
using System.Collections;
using System.Collections.Generic;
using MeshEditor.Editor.Scripts.Base;
using MeshEditor.Editor.Scripts.Base.Utilities;
using MeshEditor.Editor.Scripts.Configs.Internal;
using MeshEditor.Editor.Scripts.Manager;
using UnityEditor;
using UnityEngine;

public class MeshEditSettingPage : MEDR_SettingPage
{
    MeshEditConfig config;

    private void OnEnable()
    {
        PageName = "网格编辑工具配置";
        config = MEDR_ConfigManager.GetConfig<MeshEditConfig>();
    }

    protected override void OnGUI()
    {
        if (config == null) return;
        GUILayout.Label(PageName, MEDR_StylesUtility.SettingHeaderStyle);
        config.MEDR_MeshEdit_HoverColor = EditorGUILayout.ColorField("高亮颜色", config.MEDR_MeshEdit_HoverColor);
        config.MEDR_MeshEdit_ClickColor = EditorGUILayout.ColorField("选中颜色", config.MEDR_MeshEdit_ClickColor);
        config.MEDR_MeshEdit_VertexSize_Multiplier =
            EditorGUILayout.FloatField("顶点Gizmo大小倍数", config.MEDR_MeshEdit_VertexSize_Multiplier);
        config.MEDR_MeshEdit_EdgeSize_Multiplier =
            EditorGUILayout.FloatField("线框Gizmo粗细倍数", config.MEDR_MeshEdit_EdgeSize_Multiplier);
    }
}