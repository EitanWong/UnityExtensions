#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class UVLensEditorWindow : EditorWindow
{
    Mesh mesh;
    Material flatteningMat;
    Material lineMat;
    bool showUV2;

    Rect viewport;
    Vector2 panshift;
    const float DefaultScale = 1.8f;
    float scale = DefaultScale;


    void Awake()
    {
        // Get materials.
        Shader flatteningShader = Shader.Find("Hidden/UVLens/MeshFlattener");
        flatteningMat = new Material(flatteningShader);
        flatteningMat.hideFlags = HideFlags.HideAndDontSave;
        lineMat = new Material(Shader.Find("Hidden/Internal-Colored"));
        lineMat.hideFlags = HideFlags.HideAndDontSave;

        UpdateMesh();
    }


    void OnGUI()
    {
        ExitWindowKeyboardCheck(); //检测退出快捷键
        // Mesh field.
        GUILayout.BeginHorizontal();
        GUILayout.Space(5);
        GUILayout.Label("网格");
        mesh = (Mesh) EditorGUILayout.ObjectField(mesh, typeof(Mesh), true);
        GUILayout.EndHorizontal();

        // UV toolbar.
        if (!mesh)
            GUI.enabled = false;
        showUV2 = GUILayout.Toolbar(showUV2 ? 1 : 0, new string[] {"UV1", "UV2"}) == 1;
        GUI.enabled = true;

        DrawViewport();

        // Reset view button.
        GUILayout.BeginArea(new Rect(position.size - Vector2.one * 22, Vector2.one * 20));
        if (GUILayout.Button(new GUIContent("↺", "Reset view"), GUILayout.Width(20)))
            ResetView();
        GUILayout.EndArea();
    }

    void OnSelectionChange() => UpdateMesh();

    void OnEnable() => EditorApplication.playModeStateChanged += UpdateMeshOnPlayModeChange;
    void OnDisable() => EditorApplication.playModeStateChanged -= UpdateMeshOnPlayModeChange;


    // [MenuItem("Window/模型/UV 查看器 ^U")]
    // [MenuItem("CONTEXT/MeshFilter/UV 查看器")]
    // [MenuItem("CONTEXT/SkinnedMeshRenderer/UV 查看器")]
    // static void Open()
    // {
    //     EditorWindow window = GetWindow(typeof(UVLensEditorWindow),true);
    //     window.titleContent = new GUIContent("❏ UV 查看器");
    //     window.minSize = Vector2.one * 150;
    //     ((UVLensEditorWindow)window).ResetMesh();
    // }
    public void Open()
    {
        EditorWindow window = GetWindow(typeof(UVLensEditorWindow), true);
        window.titleContent = new GUIContent("❏ UV 检查器");
        window.minSize = Vector2.one * 150;
        ((UVLensEditorWindow) window).ResetMesh();
    }

    public void SetMesh(Mesh mesh)
    {
        this.mesh = mesh;
    }

    public void ResetMesh()
    {
        mesh = null;
        UpdateMesh();
    }

    public void UpdateMesh()
    {
        if (!Selection.activeGameObject)
            return;

        if (Selection.activeGameObject.GetComponentInChildren<MeshFilter>())
            mesh = Selection.activeGameObject.GetComponentInChildren<MeshFilter>().sharedMesh;
        else if (Selection.activeGameObject.GetComponentInChildren<SkinnedMeshRenderer>())
            mesh = Selection.activeGameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;

        showUV2 = false;

        Repaint();
    }

    void UpdateMeshOnPlayModeChange(PlayModeStateChange p) => UpdateMesh();

    void DrawViewport()
    {
        viewport = GUILayoutUtility.GetRect(position.width, 10, GUILayout.ExpandHeight(true));

        // React to mouse events.
        Event e = Event.current;
        if (e.type == EventType.MouseDrag)
        {
            GUI.changed = true;
            panshift += new Vector2(e.delta.x, -e.delta.y) * (2 / viewport.width) / scale;
        }
        else if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.F)
                ResetView();
        }
        else if (e.type == EventType.ScrollWheel && viewport.Contains(e.mousePosition))
        {
            GUI.changed = true;
            scale += e.delta.magnitude * Mathf.Sign(Vector2.Dot(e.delta, new Vector2(1, -.1f).normalized)) *
                     (2 / viewport.width) * scale * 6;
            scale = Mathf.Max(scale, .01f);
        }

        if (e.type != EventType.Repaint)
            return;

        // Draw background.
        EditorGUI.DrawRect(viewport, new Color(.3f, .3f, .3f));

        // Setup projection matrix.
        GUI.BeginGroup(viewport);
        Rect screenViewport = viewport;
        screenViewport.y = position.height - viewport.yMax;
        GL.Viewport(EditorGUIUtility.PointsToPixels(screenViewport));
        GL.PushMatrix();
        float aspect = viewport.height / viewport.width;
        GL.LoadProjectionMatrix(Matrix4x4.Ortho(-1, 1, -1 * aspect, 1 * aspect, -1, 1));
        Matrix4x4 viewMatrix = Matrix4x4.Scale(Vector3.one * scale) *
                               Matrix4x4.Translate(panshift + new Vector2(-.5f, -.5f));
        GL.MultMatrix(viewMatrix);

        // Draw grid.
        lineMat.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Color(Color.grey);
        for (int i = 0; i <= 10; i++)
        {
            GL.Vertex3(i * .1f, 0, 0);
            GL.Vertex3(i * .1f, 1, 0);
            GL.Vertex3(0, i * .1f, 0);
            GL.Vertex3(1, i * .1f, 0);
        }

        GL.End();

        // Draw mesh.
        if (mesh != null && (showUV2 ? mesh.uv2 : mesh.uv).Length > 0)
        {
            if (showUV2)
                flatteningMat.EnableKeyword("_UV2");
            else
                flatteningMat.DisableKeyword("_UV2");
            flatteningMat.SetPass(0);
            GL.wireframe = true;
            Graphics.DrawMeshNow(mesh, viewMatrix);
            GL.wireframe = false;
        }

        GL.PopMatrix();
        GUI.EndGroup();
    }

    void ResetView()
    {
        scale = DefaultScale;
        panshift = Vector3.zero;
        GUI.changed = true;
    }

    private void ExitWindowKeyboardCheck()
    {
        if (Event.current.keyCode == KeyCode.Escape)
        {
            var editorWindow = EditorWindow.mouseOverWindow;
#if UNITY_2020_1_OR_NEWER
                if (editorWindow && !editorWindow.docked && editorWindow == this)
#else
            if (editorWindow && !editorWindow && editorWindow == this)
#endif
            {
                Close();
                Event.current.keyCode = KeyCode.None;
            }
        }
    }
}
#endif