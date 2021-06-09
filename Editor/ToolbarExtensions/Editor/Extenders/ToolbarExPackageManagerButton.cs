using UnityEditor;

namespace ToolbarExtensions.Editor.Extenders
{
    [InitializeOnLoad]
    static class ToolbarExPackageManagerButton
    {
        static ToolbarExPackageManagerButton()
        {
            var button = ToolbarExtender.CreateToolbarButton("Package Manager", ShowPackageManager);
            ToolbarElement element = ToolbarElement.Create(button, ExtenderType.Right);
            ToolbarExtender.ToolbarExtend(element);
        }
        private static void ShowPackageManager()
        {
            UnityEditor.PackageManager.UI.Window.Open("");
        }
    }
}