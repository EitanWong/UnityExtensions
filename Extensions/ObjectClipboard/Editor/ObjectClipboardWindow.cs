using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// 프로젝트나 씬의 오브젝트들을 잠시 담아두는 유틸리티
namespace ObjectClipboard.Editor
{
    public class ObjectClipboardWindow : EditorWindow
    {
        private const string mc_EditorPrefsWord = "SoxObjCB";
        private const string mc_EditorPrefsWordLock = "SoxObjCBLock";

        // The maximum number to record in the registry.
        private const int mc_maxCount = 50;

        private GUIContent m_iconToolSelect;
        private GUIContent m_iconToolMove;
        private GUIContent m_iconToolTrash;
        private GUIContent m_iconToolLock;

        private GUIContent m_iconCamViewToGame;
        string m_iconTooltipCamViewToGame = "";
        private GUIContent m_iconCamGameToView;
        string m_iconTooltipCamGameToView = "";
        string m_iconTooltipCamDisabled = "";

        // 토글 버튼용 스타일
        private static GUIStyle ToggleButtonStyleNormal = null;
        private static GUIStyle ToggleButtonStyleToggled = null;

        [System.Serializable]
        public class
            ClippedObject // 값 타입인 struct를 쓰면 m_objects[i].m_object = EditorGUILayout.ObjectField 에서 문제됨 (참조 타입을 사용해야...)
        {
            public Object m_object;
            public bool m_locked; // 잠긴 것은 삭제되지 않는다.

            public ClippedObject(Object obj, bool locked)
            {
                m_object = obj;
                m_locked = locked;
            }
        }

        public List<ClippedObject> m_objects = new List<ClippedObject>();

        public static Vector2 scrollPos;

        private bool m_controlKeyDown = false;

        [MenuItem("Window/通用/对象 剪贴板")]
        public static void ShowWindow()
        {
            ObjectClipboardWindow window =
                (ObjectClipboardWindow) EditorWindow.GetWindow(typeof(ObjectClipboardWindow), false);
            window.titleContent = new GUIContent("对象 剪贴板", EditorGUIUtility.FindTexture("d_Prefab On Icon"), "对象剪贴板工具");
            window.minSize = new Vector2(200, 150);
        }

        void OnValidate()
        {
            UpdateEditorPrefs();
        }

        void OnSelectionChange()
        {
            Repaint();
        }


        void OnEnable()
        {
#if UNITY_2017_2_OR_NEWER
            string selectIconName = "Grid.Default"; // 2017.2 부터 추가된 아이콘
#else
        string selectIconName = "ViewToolZoom On";
#endif
            m_iconToolSelect = EditorGUIUtility.IconContent(selectIconName);
            m_iconToolSelect.tooltip = "选择";
            m_iconToolMove = EditorGUIUtility.IconContent("d_CollabMoved Icon");
            m_iconToolMove.tooltip = "在项目中移动资源";
            m_iconToolTrash = EditorGUIUtility.IconContent("TreeEditor.Trash");
            m_iconToolTrash.tooltip = "擦除对象.与 Control 键一起使用时，该项目将被完全删除.";
            m_iconToolLock = EditorGUIUtility.IconContent("InspectorLock");
            m_iconToolLock.tooltip = "锁定它，使其无法移动或擦除.";

            m_iconCamViewToGame = EditorGUIUtility.IconContent("Camera Icon");
            m_iconTooltipCamViewToGame = "改变游戏摄像机:\n改变游戏摄像机（与场景视图中的编辑器摄像机相同）";
            m_iconCamGameToView = EditorGUIUtility.IconContent("Camera Gizmo");
            m_iconTooltipCamGameToView = "改变编辑器相机 :\n改变了场景视图的编辑相机（与Hierarchy的游戏相机相同）";
            m_iconTooltipCamDisabled = "当在层次结构视图中选择摄像机时，该功能被激活.";

            GetEditorPrefs();
            AutoListCount(); // 리스트가 하나도 없는지 등을 검사
        }

        void OnDisable()
        {
            UpdateEditorPrefs();
        }

        private void GetEditorPrefs()
        {
            m_objects.Clear();
            string key = "";
            string keyLock = "";
            int id = 0;
            bool locked = false;
            string projectName = GetProjectName();
            for (int i = 0; i < mc_maxCount; i++)
            {
                key = mc_EditorPrefsWord + projectName + i.ToString();
                keyLock = mc_EditorPrefsWordLock + projectName + i.ToString();
                if (EditorPrefs.HasKey(key))
                {
                    id = EditorPrefs.GetInt(key);
                    if (id == 0)
                    {
                        m_objects.Add(new ClippedObject(null, false));
                    }
                    else
                    {
                        Object idObj = EditorUtility.InstanceIDToObject(id);
                        locked = EditorPrefs.GetBool(keyLock);
                        m_objects.Add(new ClippedObject(idObj, locked));
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void SetEditorPrefs()
        {
            if (m_objects.Count == 0)
                return;

            string key = "";
            string keyLock = "";
            string projectName = GetProjectName();
            for (int i = 0; i < m_objects.Count; i++)
            {
                key = mc_EditorPrefsWord + projectName + i.ToString();
                keyLock = mc_EditorPrefsWordLock + projectName + i.ToString();
                if (m_objects[i].m_object != null)
                {
                    EditorPrefs.SetInt(key, m_objects[i].m_object.GetInstanceID());
                    EditorPrefs.SetBool(keyLock, m_objects[i].m_locked);
                }
                else
                {
                    EditorPrefs.SetInt(key, 0);
                    //EditorPrefs.SetBool(keyLock, false); // 오브젝트가 없으면 Lock 여부를 저장할 필요 없음
                }
            }
        }

        private void UpdateEditorPrefs()
        {
            ClearEditorPrefs();
            SetEditorPrefs();
        }

        private void ClearEditorPrefs()
        {
            string key = "";
            string projectName = GetProjectName();
            for (int i = 0; i < mc_maxCount; i++)
            {
                key = mc_EditorPrefsWord + projectName + i.ToString();
                if (EditorPrefs.HasKey(key))
                {
                    EditorPrefs.DeleteKey(key);
                }
                else
                {
                    break;
                }
            }
        }

        private string GetProjectName()
        {
            string[] s = Application.dataPath.Split('/');
            string projectName = s[s.Length - 2];
            return projectName;
        }

        private void InitToggleButtonStyle()
        {
            ToggleButtonStyleNormal = "Button";
            ToggleButtonStyleToggled = new GUIStyle(ToggleButtonStyleNormal);
            Texture2D activeButtonReadOnly = ToggleButtonStyleToggled.active.background;
            Texture2D buttonTex;
            Color[] buttonColors;
            if (activeButtonReadOnly != null)
            {
                // 이쪽 코드에서 유니티 신버전(2019.3)은 buttonTex 를 얻어오지 못해 null 에러가 나고, 구버전에서 에러가 나지 않더라도 rgb 값을 제대로 얻어오지 못하는 문제가 있으나 차후 수정 예정
                buttonTex = new Texture2D(activeButtonReadOnly.width, activeButtonReadOnly.height);
                buttonTex.LoadImage(activeButtonReadOnly
                    .GetRawTextureData()); // 빌트인 UI 텍스쳐는 Read/Write가 꺼져있어서 이런 식으로 우회해서 새로운 텍스쳐럴 복제한다.
                buttonColors = buttonTex.GetPixels();
                for (int i = 0; i < buttonColors.Length; i++)
                {
                    // 50% 어두운 빨간 색으로
                    buttonColors[i].r *= 0.5f;
                    buttonColors[i].g = 0f;
                    buttonColors[i].b = 0f;
                }
            }
            else
            {
                buttonTex = new Texture2D(1, 1);
                buttonColors = new Color[] {new Color(0.5f, 0f, 0f)};
            }

            buttonTex.SetPixels(buttonColors);
            buttonTex.Apply();
            ToggleButtonStyleToggled.normal.background = buttonTex;
        }

        // 1컬럼, 2컬럼, 씬에 상관 없이 마지막으로 클릭된 애셋이나 폴더의 경로를 리턴한다. 문제가 있으면 공백 리턴
        private string GetLastActivePath()
        {
            // http://blog.codestage.ru/2015/03/30/select-in-project-browser/ 프로젝트 브라우저 접근하는 방법 소개
            // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/ProjectBrowser.cs 프로젝트 브라우저 소스코드 (리플렉션으로 무엇을 가져올지 코드를 알면 편함)
            System.Type projectBrowserType = System.Type.GetType("UnityEditor.ProjectBrowser,UnityEditor");
            FieldInfo lastProjectBrowser = projectBrowserType.GetField("s_LastInteractedProjectBrowser",
                BindingFlags.Static | BindingFlags.Public);
            object lastProjectBrowserInstance = lastProjectBrowser.GetValue(null);

            FieldInfo projectBrowserViewMode =
                projectBrowserType.GetField("m_ViewMode", BindingFlags.Instance | BindingFlags.NonPublic);
            int viewMode = (int) projectBrowserViewMode.GetValue(lastProjectBrowserInstance);

            string path = "";

            // Asset인 경우에만 경로 세팅
            // 0 - one column, 1 - two column
            if (viewMode == 1)
            {
                // Two column
                MethodInfo activeFolderPathInfo = projectBrowserType.GetMethod("GetActiveFolderPath",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                path = (string) activeFolderPathInfo.Invoke(lastProjectBrowserInstance, new object[] { });
            }
            else
            {
                // One column
                if (Selection.activeObject != null)
                {
                    if (AssetDatabase.Contains(Selection.activeObject))
                    {
                        // activeObject가 프로젝트에서 선택된 경우
                        path = AssetDatabase.GetAssetPath(Selection.activeObject);
                        if (Directory.Exists(path) == false)
                        {
                            // 폴더가 아닌 애셋이 선택된 경우 path에서 파일명을 없애야함
                            path = Path.GetDirectoryName(path);
                        }
                    }
                    else
                    {
                        // activeObject가 씬에서 선택된 경우
                        return "";
                    }
                }
                else
                {
                    // null
                    return "";
                }
            }

            return path;
        }

        void OnGUI()
        {
            float buttonWidth = 27f;
            float buttonHeight = 22f;

            // 토글 버튼 스타일 정의
            if (ToggleButtonStyleNormal == null ||
                ToggleButtonStyleToggled.normal.background ==
                null) // ToggleButtonStyleToggled.normal.background 은 에디터 실행 종료 직후 null로 초기화되기때문에 다시 빨간 텍스쳐로 입혀줘야함.
            {
                InitToggleButtonStyle();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("清除"))
            {
                for (int i = 0; i < m_objects.Count; i++)
                {
                    if (m_objects[i].m_locked == false)
                        m_objects[i] = new ClippedObject(null, false);
                }

                UpdateEditorPrefs();
            }

            /* AutoListCount() 자동 리스트 관리 기능 들어가면서 리스트 추가 삭제 버튼 제거
        if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus")))
        {
            if (m_objects.Count > 1)
            {
                int index = m_objects.Count - 1;
                bool removed = false;
                while (removed == false && index >= 0)
                {
                    if (m_objects[index].m_locked == false)
                    {
                        m_objects.RemoveAt(index);
                        removed = true;
                    }
                    index--;
                }
            }
            UpdateEditorPrefs();
        }

        if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus")))
        {
            m_objects.Add(new ClippedObject(null, false));
            UpdateEditorPrefs();
        }
        */

            if (GUILayout.Button(new GUIContent("▼", "下移")))
            {
                m_objects.Insert(0, new ClippedObject(null, false));
                UpdateEditorPrefs();
            }

            if (GUILayout.Button(new GUIContent("▲", "上移")))
            {
                if (m_objects.Count > 1 && m_objects[0].m_locked == false)
                {
                    m_objects.RemoveAt(0);
                    m_objects.Add(new ClippedObject(null, false));
                    UpdateEditorPrefs();
                }
            }

            // 참고용 메모, EditorWindow의 가로 폭은 OnGUI에 기본으로 사용되는 position 변수가 들고있음. https://forum.unity.com/threads/solved-how-to-get-the-size-of-editorwindow.39263/

            GUILayout.Space(14f);

            // 씬 하이러키에서 카메라가 선택된 상태인지 체크
            Camera gameCam = GameCameraSelected();
            if (gameCam == null)
            {
                m_iconCamViewToGame.tooltip = m_iconTooltipCamDisabled;
                m_iconCamGameToView.tooltip = m_iconTooltipCamDisabled;
                GUI.enabled = false;
            }
            else
            {
                m_iconCamViewToGame.tooltip = m_iconTooltipCamViewToGame;
                m_iconCamGameToView.tooltip = m_iconTooltipCamGameToView;
            }

            if (GUILayout.Button(m_iconCamViewToGame, GUILayout.Width(40f), GUILayout.Height(buttonHeight)))
            {
                ViewToGameCamera(gameCam);
            }

            if (GUILayout.Button(m_iconCamGameToView, GUILayout.Width(40f), GUILayout.Height(buttonHeight)))
            {
                GameToViewCamera(gameCam);
            }

            GUI.enabled = true;

            GUILayout.EndHorizontal();

            // Control 키가 눌려있는지 검사. Input.GetKey 방식은 OnGUI에서 작동하지 않는다. 또한 버튼이 눌러진 직후 검사하면 그 순간에 다른 이벤트가 작동하고있어서 이렇게 미리 이벤트 발생할 때마다 플래그를 세팅해야한다.
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.LeftControl)
            {
                m_controlKeyDown = true;
            }

            if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.LeftControl)
            {
                m_controlKeyDown = false;
            }

            AutoListCount(); // 자동 리스트 카운트 변경

            string newPath = GetLastActivePath(); // 최적화를 위해 for 밖으로 이동

            // Draw List
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            //EditorGUILayout.PropertyField(m_objectsSerial, true);
            for (int i = 0; i < m_objects.Count;)
            {
                GUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                m_objects[i].m_object = EditorGUILayout.ObjectField(m_objects[i].m_object, typeof(Object), true);
                if (EditorGUI.EndChangeCheck())
                    UpdateEditorPrefs();

                if (m_objects[i].m_object != null)
                {
                    // IconContent의 툴팁은 안나오는데, 아마 아이콘에 포함되어있는듯.
                    if (GUILayout.Button(m_iconToolSelect, GUILayout.Width(buttonWidth),
                        GUILayout.Height(buttonHeight)))
                    {
                        if (m_objects[i].m_object != null)
                        {
                            Selection.activeObject = m_objects[i].m_object;
                        }
                    }

                    /*
                // Control 키 입력에 아이콘이 연동되는 기능인데, OnGUI라서 이벤트가 즉각즉각 반응하지 않아서 봉인
                string removeIconName;
                if (m_controlKeyDown)
                {
                    removeIconName = "TreeEditor.Trash";
                }
                else
                {
                    removeIconName = "Grid.EraserTool";
                }
                */

                    // 이동버튼은 프로젝트에 없거나 잠겨있거나 경로를 얻어올 수 없는 상태면 비활성
                    if (AssetDatabase.Contains(m_objects[i].m_object) == false || m_objects[i].m_locked ||
                        newPath == "")
                        GUI.enabled = false;

                    if (GUILayout.Button(m_iconToolMove, GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
                    {
                        string oldPath = AssetDatabase.GetAssetPath(m_objects[i].m_object);
                        newPath += "/";
                        newPath += Path.GetFileName(oldPath);
                        if (newPath != "")
                        {
                            string error = AssetDatabase.MoveAsset(oldPath, newPath);
                            if (error != "")
                            {
                                Debug.Log(error);
                            }
                        }
                    }

                    GUI.enabled = true;
                } // 오브젝트가 비어있어도 삭제버튼부터는 기능이 작동하도록 여기까지만 조건 체크

                // 잠겨있으면 삭제 버튼 비활성화
                if (m_objects[i].m_locked)
                    GUI.enabled = false;

                if (GUILayout.Button(m_iconToolTrash, GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
                {
                    // Control 키를 누른 채 지우면 해당 배열이 완전히 제거된다.
                    if (m_controlKeyDown)
                    {
                        m_objects.RemoveAt(i);
                        UpdateEditorPrefs();
                        continue; // i++ 없이 다음 루프로 진입
                    }
                    else
                    {
                        m_objects[i].m_object = null;
                        UpdateEditorPrefs();
                    }
                }

                GUI.enabled = true;
                if (GUILayout.Button(m_iconToolLock,
                    m_objects[i].m_locked ? ToggleButtonStyleToggled : ToggleButtonStyleNormal,
                    GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
                {
                    m_objects[i].m_locked = !m_objects[i].m_locked;
                    if (m_objects[i].m_object == null)
                    {
                        m_objects[i].m_locked = false;
                    }

                    UpdateEditorPrefs();
                }

                GUILayout.EndHorizontal();

                i++; // 지워진게 없으면 i++
            }

            EditorGUILayout.EndScrollView();
        }

        // Hierarchy에서 Selection.activeObject 가 카메라면 카메라 리턴
        private Camera GameCameraSelected()
        {
            Object activeObj = Selection.activeObject;
            if (activeObj != null)
            {
                // activeObject가 씬에서 선택된 경우
                if (AssetDatabase.Contains(activeObj) == false)
                {
                    // 중복체크. Package Manager 창에서 선택된 오브젝트는 AssetDatabase.Contains 에서 false임.
                    // GameObject.Find 만으로 하지 않는 이유는, 이름만으로 체크하기에는 불안해서. 우연히 같은 이름이 있을지도.
                    if (GameObject.Find(activeObj.name) != null)
                    {
                        GameObject go = activeObj as GameObject;
                        if (go == null) // 유니티 2D본같은 경우 씬에서 선택은 가능하지만 GameObject는 아니라서 null
                        {
                            return null;
                        }

                        Camera gameCam = go.GetComponent<Camera>();
                        if (gameCam != null)
                        {
                            return gameCam;
                        }
                    }
                }
            }

            return null;
        }

        private void ViewToGameCamera(Camera gameCam)
        {
            Camera viewCam = SceneView.lastActiveSceneView.camera;
            if (gameCam != null && viewCam != null)
            {
                gameCam.fieldOfView = viewCam.fieldOfView;
                gameCam.transform.position = viewCam.transform.position;
                gameCam.transform.rotation = viewCam.transform.rotation;
            }
        }

        private void GameToViewCamera(Camera gameCam)
        {
            Camera viewCam = SceneView.lastActiveSceneView.camera;
            if (gameCam != null && viewCam != null)
            {
                // 뷰 카메라는 pivot을 중심으로 Orbit 회전함. 그래서 gameCam의 forward 방향으로 특정 거리 위치를 pivot으로 삼아야함 (여기서는 기존 뷰카메라 거리를 재활용)
                // FOV를 먼저 세팅해야 DIstance 관련한 문제가 생기지 않는다. (순서 중요)
#if UNITY_2019_1_OR_NEWER // 에디터 카메라의 FOV 변경은 2019부터 가능
                SceneView.lastActiveSceneView.cameraSettings.fieldOfView = gameCam.fieldOfView;
#endif
                SceneView.lastActiveSceneView.pivot =
                    gameCam.transform.TransformPoint(Vector3.forward * SceneView.lastActiveSceneView.cameraDistance);
                SceneView.lastActiveSceneView.rotation = gameCam.transform.rotation;
                SceneView.lastActiveSceneView.Repaint();
            }
        }

        // m_objects 리스트 수를 자동으로 제어한다. 주로 리스트 마지막을 검사 (중간에 빈 것들은 무시)
        private void AutoListCount()
        {
            // 리스트 수가 전혀 없으면 true 리턴
            if (m_objects.Count == 0)
            {
                m_objects.Add(new ClippedObject(null, false));
                UpdateEditorPrefs();
                return;
            }

            int end = m_objects.Count - 1;

            // 리스트 끝이 빈 칸이 아니면 빈 칸을 하나 추가
            if (m_objects[end].m_object != null)
            {
                m_objects.Add(new ClippedObject(null, false));
                UpdateEditorPrefs();
                return;
            }

            // 리스트 끝에 빈 칸이 두 개 있으면 마지막 하나를 지운다
            if (m_objects.Count >= 2) // 일단 리스트가 두 개 이상인 경우만 검사
            {
                if (m_objects[end].m_object == null && m_objects[end - 1].m_object == null)
                {
                    m_objects.RemoveAt(end);
                    UpdateEditorPrefs();
                    return;
                }
            }
        }
    }
}