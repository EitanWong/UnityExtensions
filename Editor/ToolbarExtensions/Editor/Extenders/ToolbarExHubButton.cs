using Hub.Editor.Scripts;
using UnityEditor;

namespace ToolbarExtensions.Editor.Extenders
{
    [InitializeOnLoad]
    static class ToolbarExHubButton
    {
        static ToolbarExHubButton()
        {
            var button = ToolbarExtender.CreateToolbarButton("FilterByLabel@2x", ShowHubWindow);
            ToolbarElement element = ToolbarElement.Create(button, ExtenderType.Right);
            ToolbarExtender.ToolbarExtend(element);
        }

        private static void ShowHubWindow()
        { 
            HubWindow.OpenWindow();
        }
    }
}