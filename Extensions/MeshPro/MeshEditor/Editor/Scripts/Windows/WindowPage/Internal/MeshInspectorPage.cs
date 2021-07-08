#if UNITY_EDITOR
using UnityEngine;
using TransformPro.MeshPro.MeshEditor.Editor.Scripts.Base;

public class MeshInspectorPage : MEDR_Page
{
    private UVLensEditorWindow uvLensEditorWindow;

    private void Awake()
    {
        PageName = "网格检查器";
        PageIcon = Resources.Load<Texture2D>(
            "Textures/Inspector");
        uvLensEditorWindow = ScriptableObject.CreateInstance<UVLensEditorWindow>();
    }

    protected override void OnGUI()
    {
        if (CheckFields[0] != null)
            uvLensEditorWindow.SetMesh(CheckFields[0].Filter.sharedMesh);
        if (GUILayout.Button("UV检查器"))
            uvLensEditorWindow.Open();
    }
}
#endif