using System;
using System.Collections;
using System.Collections.Generic;
using MeshEditor.Editor.Scripts.Base;
using MeshEditor.Editor.Scripts.Base.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using TransformPro.MeshPro.MeshEditor.Editor.Scripts.Base;

public class MeshEditorPreference : Editor
{
    private static string Version = "v0.1.1";
    private static Vector2 scrollPositionSettingArea;
    private static Vector2 scrollPositionPageButtonArea;
    private static Vector2 scrollPositionTitle;

    private static int iconHeight;
    private static int iconWidth;

    private static List<MEDR_SettingPage> settingPages = new List<MEDR_SettingPage>(); //配置页面

    private static MEDR_SettingPage currentSelectPage; //当前选中ConfigPage

#pragma warning disable 618
    [PreferenceItem("UnityExtensions/MeshEditor")]
#pragma warning restore 618
    public static void PreferencesGUI()
    {
        InitSettingPage();
        EditorGUILayout.BeginHorizontal();
        DrawLeftAreaGUI();
        DrawSettingGUIArea();
        EditorGUILayout.EndHorizontal();
    }

    #region EditorBehaviour

    private static void InitSettingPage()
    {
        if (settingPages == null || settingPages.Count <= 0)
            settingPages = MEDR_Internal_Utility.GetAllReflectionClassIns<MEDR_SettingPage>();
    }

    #endregion

    #region DrawGUI

    private static void DrawLeftAreaGUI()
    {
        scrollPositionPageButtonArea =
            EditorGUILayout.BeginScrollView(scrollPositionPageButtonArea, GUILayout.MinWidth(250));
        DrawTitleInfo();
        EditorGUILayout.Space(250);
        DrawPagesButtonGUI();
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// 绘制标题信息
    /// </summary>
    private static void DrawTitleInfo()
    {
        var icon = Resources.Load<Texture2D>("Textures/MeshEditorIcon");
        iconWidth = icon.width / 10;
        iconHeight = icon.height / 10;
        // EditorGUILayout.BeginScrollView(scrollPositionTitle, MeshEditorStylesUtility.FrameStyle,
        //     GUILayout.MaxWidth(iconWidth),GUILayout.MaxHeight(iconHeight*1.25f));
        GUI.DrawTexture(new Rect(25, iconHeight * 0.05f, iconWidth, iconHeight), icon, ScaleMode.StretchToFill, true,
            10.0F);
        var titleStyle = MEDR_StylesUtility.TitleStyle;
        GUI.Label(new Rect(25, iconHeight, iconWidth, iconHeight * 0.1f), "网格模型编辑器", titleStyle);
        GUI.Label(new Rect(25, iconHeight * 1.1f, iconWidth, iconHeight * 0.1f), Version, titleStyle);
        // EditorGUILayout.EndScrollView();
    }

    private static void DrawPagesButtonGUI()
    {
        EditorGUILayout.BeginVertical(MEDR_StylesUtility.GroupBoxStyle);
        if (settingPages == null || settingPages.Count <= 0) return;
        foreach (var page in settingPages)
        {
            if (GUILayout.Button(page.PageName, MEDR_StylesUtility.WarningOverlayStyle))
                currentSelectPage = page;
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制设置区域
    /// </summary>
    private static void DrawSettingGUIArea()
    {
        scrollPositionSettingArea = EditorGUILayout.BeginScrollView(scrollPositionSettingArea,
            MEDR_StylesUtility.FrameStyle);
        // var titleStyle = MeshEditorStylesUtility.TitleStyle;
        // GUILayout.Label("施工区", titleStyle);
        // GUILayout.Label("配置功能开发中");
        // ReSharper disable once Unity.NoNullPropagation
        if (currentSelectPage != null)
            currentSelectPage?.UpdateGUI();
        EditorGUILayout.EndScrollView();
    }

    #endregion
}