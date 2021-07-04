using MagicLightProbes;
using UnityEditor;
using UnityEngine;

public class MLPInfoWindow : EditorWindow
{    
    private static GUIStyle captionStyle;
    private static GUIStyle centeredLabelStyle;
    private static GUIStyle linkStyle;
    private static GUIStyle greenLabelStyle;

    private static bool stylesInitialized;
    private static string userName;
    private static string userInvoice;    

    [MenuItem("Tools/Magic Tools/Magic Light Probes/About MLP...", priority = 2)]
    static void Init()
    {
        if (EditorPrefs.GetBool("MLP_Authorized"))
        {
            MLPUpdater.forceCheck = true;
        }

        MLPInfoWindow managerWindow = (MLPInfoWindow) GetWindow(typeof(MLPInfoWindow), true, "About MLP...");
        
        Vector2 size = new Vector2(450, 340);
        Vector2 position = new Vector2((Screen.currentResolution.width / 2) - managerWindow.minSize.x, (Screen.currentResolution.height / 2) - managerWindow.minSize.y);
        managerWindow.minSize = size;
        managerWindow.maxSize = size;
        managerWindow.position = new Rect(position, size);
        managerWindow.Show();
    }

    static void InitStyles()
    {
        captionStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            margin = new RectOffset(),
            padding = new RectOffset(10, 10, 5, 5),
            fontSize = 11,
            fontStyle = FontStyle.Bold
        };

        centeredLabelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            margin = new RectOffset(),
            padding = new RectOffset(10, 10, 5, 5),
            fontSize = 11
        };

        linkStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 11,
            fontStyle = FontStyle.Bold
        };

        greenLabelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11
        };

        greenLabelStyle.normal.textColor = Color.green;

        stylesInitialized = true;
    }

    static void ShowAuthorizationForm()
    {
        MLPUpdater.authorization = true;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.BeginHorizontal();

        GUILayout.Label("User Name: ", GUILayout.MinWidth(100));
        userName = EditorGUILayout.TextField(userName);

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();

        GUILayout.Label("Invoice №: ", GUILayout.MinWidth(100));
        userInvoice = EditorGUILayout.TextField(userInvoice);

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();

        if (!MLPUpdater.updateChecking)
        {
            if (GUILayout.Button("Submit"))
            {
                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userInvoice))
                {
                    EditorUtility.DisplayDialog("Magic Light Probes", "For successful authorization, both fields " +
                        "must be filled. Enter the username (for example, the username in the asset store) and the " +
                        "invoice number that you received by email after purchase.", "OK");
                }
                else
                {
                    MLPUpdater.StartDownload(userName, userInvoice);
                }
            }
        }
        else
        {
            if (MLPUpdater.downloading)
            {
                GUILayout.Label("Downloading... " + Mathf.RoundToInt(MLPUpdater.downloadingProgress * 100.0f) + "%", GUILayout.Height(19));
            }
            else
            {
                GUILayout.Label("Authorization...", GUILayout.Height(19));
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }

    void OnGUI()
    {
        if (!stylesInitialized)
        {
            InitStyles();
        }

        GUILayout.Label("Magic Light Probes - High Precision Light Probe Generator", captionStyle);

        GUILayout.Space(10);

        GUILayout.Label("Version Info ", captionStyle);

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.BeginHorizontal();

        GUILayout.Label("Installed Version: ", GUILayout.MinWidth(100));
        GUILayout.Label(MLPUpdater.installedVersion, linkStyle);

        if (EditorPrefs.GetBool("MLP_newVersionAvailable"))
        {
            GUILayout.Label("Version " + EditorPrefs.GetString("MLP_latestVersion") + " Available", greenLabelStyle);
        }
        else
        {
            GUILayout.Label("Latest Version Installed", greenLabelStyle);            
        }

        GUILayout.EndHorizontal();

        if (EditorPrefs.GetBool("MLP_newVersionAvailable"))
        {
            if (!MLPUpdater.authorization)
            {
                if (GUILayout.Button("Download New Version"))
                {
                    if (EditorPrefs.GetBool("MLP_Authorized"))
                    {
                        MLPUpdater.StartDownload(EditorPrefs.GetString("MLP_uName"), EditorPrefs.GetString("MLP_uInvoice"));
                    }
                    else
                    {

                        ShowAuthorizationForm();
                    }
                }

                if (MLPUpdater.downloading)
                {
                    GUILayout.Label("Downloading... " + Mathf.RoundToInt(MLPUpdater.downloadingProgress * 100.0f) + "%", GUILayout.Height(19));
                }
                else
                {
                    if (MLPUpdater.updateChecking)
                    {
                        GUILayout.Label("Connecting to server... ", GUILayout.Height(19));
                    }
                }
            }
            else
            {
                ShowAuthorizationForm();
            }
        }
        else
        {
            if (!MLPUpdater.authorization)
            {
                if (MLPUpdater.forceUpdateChecking)
                {
                    GUILayout.Label("Checking For Updates...", centeredLabelStyle);
                }
                else
                {
                    if (GUILayout.Button("Check For Updates"))
                    {
                        MLPUpdater.forceCheck = true;
                    }
                }
            }
            else
            {
                ShowAuthorizationForm();
            }
        }

        GUILayout.EndVertical();
        GUILayout.Space(10);

        GUILayout.Label("Documentation & Discussion ", captionStyle);

        GUILayout.BeginVertical(GUI.skin.box);  
        GUILayout.BeginHorizontal();

        GUILayout.Label("Online Manual: ", GUILayout.MinWidth(100));               

        if (GUILayout.Button("Magic Light Probes " + MLPUpdater.installedVersion + " (EN)", linkStyle))
        {
            Application.OpenURL("https://docs.google.com/document/d/1vz-SamMynoipQptyq_yvrCRV_cOVm_BdcHIm-54y8f8/edit#heading=h.a6l93cxwdoh5");
        }

        if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
        {
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
        }

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();

        GUILayout.Label("Forum Thread: ", GUILayout.MinWidth(100));

        if (GUILayout.Button("Unity Forum", linkStyle))
        {
            Application.OpenURL("https://forum.unity.com/threads/released-magic-light-probes-high-precision-light-probe-generator.779309/");
        }

        if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
        {
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
        }

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();

        GUILayout.Label("Write a Review: ", GUILayout.MinWidth(100));

        if (GUILayout.Button("Asset Store Page", linkStyle))
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/level-design/magic-light-probes-157812");
        }

        if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
        {
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.Label("Contacts", captionStyle);

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.BeginHorizontal();

        GUILayout.Label("Telegram: ", GUILayout.MinWidth(100));

        if (GUILayout.Button("@EVB45", linkStyle))
        {
            Application.OpenURL("https://t.me/EVB45");
        }

        if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
        {
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
        }

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();

        GUILayout.Label("E-Mail: ", GUILayout.MinWidth(100));

        if (GUILayout.Button("evb45@bk.ru", linkStyle))
        {
            Application.OpenURL("mailto:evb45@bk.ru");
        }

        if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
        {
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
        }

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();

        GUILayout.Label("Discord Channel: ", GUILayout.MinWidth(100));

        if (GUILayout.Button("MLP Discord", linkStyle))
        {
            Application.OpenURL("https://discord.gg/p94azzE");
        }

        if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
        {
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }
}
