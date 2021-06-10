using ObjectClipboard.Editor;
using ToolbarExtensions;
using UnityEditor;

namespace Extensions.ToolbarExtensions.Editor.Extenders
{
    [InitializeOnLoad]
    static class ToolbarExObjectClipboardButton
    {
        static ToolbarExObjectClipboardButton()
        {
            var button = ToolbarExtender.CreateToolbarButton("d_Prefab On Icon", ShowObjectClipboard);
            ToolbarElement element = ToolbarElement.Create(button, ExtenderType.Right);
            ToolbarExtender.ToolbarExtend(element);
        }

        private static void ShowObjectClipboard()
        {
            ObjectClipboardWindow.ShowWindow();
        }
    }
}