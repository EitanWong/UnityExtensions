using UnityEditor;
using UnityEngine;

namespace ToolbarExtensions.Editor.Extenders
{
    [InitializeOnLoad]
    static class ToolbarExPrefabBrushButton
    {
        static ToolbarExPrefabBrushButton()
        {
            var icon = Resources.Load<Texture2D>("Textures/PB_Icon");
            var button = ToolbarExtender.CreateToolbarButton(icon, ShowPrefabBushWindow);
            ToolbarElement element = ToolbarElement.Create(button, ExtenderType.Right);
            ToolbarExtender.ToolbarExtend(element);
        }

        private static void ShowPrefabBushWindow()
        {
            PrefabBrush.PrefabBrushUI.PrefabBrush.ShowWindow();
        }
    }
}