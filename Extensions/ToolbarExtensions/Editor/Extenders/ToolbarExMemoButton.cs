using System.Collections;
using System.Collections.Generic;
using ToolbarExtensions;
using UnityEditor;
using UnityEngine;
using UnityExtensions.Memo;

[InitializeOnLoad]
static class ToolbarExMemoButton
{
    static ToolbarExMemoButton()
    {
        var button = ToolbarExtender.CreateToolbarButton("d_winbtn_win_max@2x", ShowMemo);
        ToolbarElement element = ToolbarElement.Create(button, ExtenderType.Right);
        ToolbarExtender.ToolbarExtend(element);
    }

    private static void ShowMemo()
    {
        MemoWindow.OpenWindow();
    }
}