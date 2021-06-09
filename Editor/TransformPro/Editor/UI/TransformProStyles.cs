namespace TransformPro.Scripts
{
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    ///     Holds all of the custom styles used by the system.
    ///     The properties of this class will remain mostly undocumented as it's fairly self explainatory.
    /// </summary>
    public static class TransformProStyles
    {
        private static TransformProStylesButtons buttons;
        private static Color[] colorAxis;
        private static Color colorAxisX = new Color(1, 0.8f, 0.8f);
        private static Color colorAxisXDeep = new Color(1, 0.5f, 0.5f);
        private static Color colorAxisXLight = new Color(1, 0.9f, 0.9f);
        private static Color colorAxisY = new Color(0.8f, 1, 0.8f);
        private static Color colorAxisYDeep = new Color(0.5f, 1, 0.5f);
        private static Color colorAxisYLight = new Color(0.9f, 1, 0.9f);
        private static Color colorAxisZ = new Color(0.8f, 0.8f, 1);
        private static Color colorAxisZDeep = new Color(0.5f, 0.5f, 1);
        private static Color colorAxisZLight = new Color(0.9f, 0.9f, 1);
        private static Color colorClipboard = new Color(1, 0.5f, 0);
        private static Color colorCollider = new Color(0f, 1, 0f);
        private static Color colorCopy = new Color(1, 0.8f, 0.6f);
        private static Color colorCopyDeep = new Color(0.6f, 0.3f, 0f);
        private static Color colorHelp = new Color(0.7f, 0.7f, 1);
        private static Color colorLabelSubtle = new Color(0.6f, 0.6f, 0.6f);
        private static Color colorPaste = new Color(0.6f, 1, 0.8f);
        private static Color colorPasteDeep = new Color(0f, 0.6f, 0.3f);
        private static Color colorRenderer = new Color(0f, 0f, 1);
        private static Color colorReset = new Color(0.95f, 0.95f, 0.95f);
        private static Color colorResetPro = new Color(0.5f, 0.5f, 0.5f);
        private static Color colorSnap = new Color(0.8f, 0.6f, 1);
        private static Texture2D iconPanel;
        private static Texture2D iconPanelDeep;
        private static TransformProStylesIcons icons;
        private static bool initialised;
        private static GUIStyle labelCopyright;
        private static GUIStyle labelHeading1;
        private static GUIStyle labelHeading2;
        private static GUIStyle labelHeading3;
        private static GUIStyle labelSmall;
        private static GUIStyle labelSmallIcon;
        private static GUIStyle labelTiny;
        private static Material materialIconTransparent;
        private static GUIStyle panel;
        private static GUIStyle panelDeep;
        private static string pathRoot;
        private static GUIStyle popup;
        private static GUIStyle popupRightPersonal;
        private static GUIStyle popupRightProfessional;
        private static GUISkin skin;
        private static GUIStyle toggle;

        public static TransformProStylesButtons Buttons { get { return TransformProStyles.buttons; } }

        public static Color[] ColorAxis
        {
            get
            {
                return TransformProStyles.colorAxis ??
                       (TransformProStyles.colorAxis = new[] {TransformProStyles.ColorAxisX, TransformProStyles.ColorAxisY, TransformProStyles.ColorAxisZ});
            }
        }

        public static Color ColorAxisX { get { return TransformProStyles.colorAxisX; } }
        public static Color ColorAxisXDeep { get { return TransformProStyles.colorAxisXDeep; } }
        public static Color ColorAxisXLight { get { return TransformProStyles.colorAxisXLight; } }
        public static Color ColorAxisY { get { return TransformProStyles.colorAxisY; } }
        public static Color ColorAxisYDeep { get { return TransformProStyles.colorAxisYDeep; } }
        public static Color ColorAxisYLight { get { return TransformProStyles.colorAxisYLight; } }
        public static Color ColorAxisZ { get { return TransformProStyles.colorAxisZ; } }
        public static Color ColorAxisZDeep { get { return TransformProStyles.colorAxisZDeep; } }
        public static Color ColorAxisZLight { get { return TransformProStyles.colorAxisZLight; } }
        public static Color ColorClipboard { get { return TransformProStyles.colorClipboard; } }
        public static Color ColorCollider { get { return TransformProStyles.colorCollider; } }
        public static Color ColorCopy { get { return TransformProStyles.colorCopy; } }
        public static Color ColorCopyDeep { get { return TransformProStyles.colorCopyDeep; } }
        public static Color ColorHelp { get { return TransformProStyles.colorHelp; } }
        public static Color ColorLabelSubtle { get { return TransformProStyles.colorLabelSubtle; } }
        public static Color ColorPaste { get { return TransformProStyles.colorPaste; } }
        public static Color ColorPasteDeep { get { return TransformProStyles.colorPasteDeep; } }
        public static Color ColorRenderer { get { return TransformProStyles.colorRenderer; } }
        public static Color ColorReset { get { return EditorGUIUtility.isProSkin ? TransformProStyles.colorResetPro : TransformProStyles.colorReset; } }
        public static Color ColorSnap { get { return TransformProStyles.colorSnap; } }

        public static Texture2D IconPanel { get { return TransformProStyles.iconPanel ?? (TransformProStyles.iconPanel = TransformProStylesIcons.GetCustomIcon("Panel")); } }
        public static Texture2D IconPanelDeep { get { return TransformProStyles.iconPanelDeep ?? (TransformProStyles.iconPanelDeep = TransformProStylesIcons.GetCustomIcon("PanelDeep")); } }

        public static TransformProStylesIcons Icons { get { return TransformProStyles.icons; } }

        public static GUIStyle LabelCopyright
        {
            get
            {
                return TransformProStyles.labelCopyright ??
                       (TransformProStyles.labelCopyright = new GUIStyle(TransformProStyles.LabelSmall));
            }
        }

        public static GUIStyle LabelHeading1
        {
            get
            {
                return TransformProStyles.labelHeading1 ??
                       (TransformProStyles.labelHeading1 = new GUIStyle(EditorStyles.largeLabel)
                                                           {
                                                               fontSize = 20,
                                                               fontStyle = FontStyle.Bold,
                                                               alignment = TextAnchor.MiddleLeft
                                                           });
            }
        }

        public static GUIStyle LabelHeading2
        {
            get
            {
                return TransformProStyles.labelHeading2 ??
                       (TransformProStyles.labelHeading2 = new GUIStyle(EditorStyles.largeLabel)
                                                           {
                                                               fontSize = 17,
                                                               fontStyle = FontStyle.Bold,
                                                               alignment = TextAnchor.MiddleLeft
                                                           });
            }
        }

        public static GUIStyle LabelHeading3
        {
            get
            {
                return TransformProStyles.labelHeading3 ??
                       (TransformProStyles.labelHeading3 = new GUIStyle(EditorStyles.largeLabel)
                                                           {
                                                               fontSize = 14,
                                                               fontStyle = FontStyle.Bold,
                                                               alignment = TextAnchor.MiddleLeft
                                                           });
            }
        }

        public static GUIStyle LabelSmall
        {
            get
            {
                return TransformProStyles.labelSmall ??
                       (TransformProStyles.labelSmall = new GUIStyle(GUI.skin.label)
                                                        {
                                                            fontSize = 9,
                                                            alignment = TextAnchor.MiddleCenter
                                                        });
            }
        }

        public static GUIStyle LabelSmallIcon
        {
            get
            {
                return TransformProStyles.labelSmallIcon ??
                       (TransformProStyles.labelSmallIcon = new GUIStyle(GUI.skin.label)
                                                            {
                                                                fontSize = 9,
                                                                alignment = TextAnchor.MiddleCenter,
                                                                padding = new RectOffset(0, 0, 0, 0)
                                                            });
            }
        }

        public static GUIStyle LabelTiny
        {
            get
            {
                return TransformProStyles.labelTiny ??
                       (TransformProStyles.labelTiny = new GUIStyle(GUI.skin.label)
                                                       {
                                                           fontSize = 8,
                                                           alignment = TextAnchor.MiddleCenter
                                                       });
            }
        }

        public static Material MaterialIconTransparent
        {
            get
            {
                return TransformProStyles.materialIconTransparent ??
                       (TransformProStyles.materialIconTransparent = new Material(Shader.Find("Unlit/Transparent")));
            }
        }

        public static GUIStyle Panel
        {
            get
            {
                return TransformProStyles.panel ??
                       (TransformProStyles.panel = new GUIStyle(EditorStyles.helpBox)
                                                   {
                                                       normal = {background = TransformProStyles.IconPanel},
                                                       onNormal = {background = TransformProStyles.IconPanel},
                                                       active = {background = TransformProStyles.IconPanel},
                                                       onActive = {background = TransformProStyles.IconPanel},
                                                       focused = {background = TransformProStyles.IconPanel},
                                                       onFocused = {background = TransformProStyles.IconPanel}
                                                   });
            }
        }

        public static GUIStyle PanelDeep
        {
            get
            {
                return TransformProStyles.panelDeep ??
                       (TransformProStyles.panelDeep = new GUIStyle(EditorStyles.helpBox)
                                                       {
                                                           normal = {background = TransformProStyles.IconPanelDeep},
                                                           onNormal = {background = TransformProStyles.IconPanelDeep},
                                                           active = {background = TransformProStyles.IconPanelDeep},
                                                           onActive = {background = TransformProStyles.IconPanelDeep},
                                                           focused = {background = TransformProStyles.IconPanelDeep},
                                                           onFocused = {background = TransformProStyles.IconPanelDeep}
                                                       });
            }
        }

        public static string PathEditor { get { return Path.Combine(TransformProStyles.pathRoot, "Editor"); } }
        public static string PathEditorResources { get { return Path.Combine(TransformProStyles.PathEditor, "Resources"); } }
        public static string PathRoot { get { return TransformProStyles.pathRoot; } }

        public static GUIStyle Popup
        {
            get
            {
                return TransformProStyles.popup ??
                       (TransformProStyles.popup = new GUIStyle(EditorStyles.popup));
            }
        }

        public static GUIStyle PopupRight
        {
            get
            {
                if (EditorGUIUtility.isProSkin)
                {
                    return TransformProStyles.popupRightProfessional ??
                           (TransformProStyles.popupRightProfessional = new GUIStyle(TransformProStyles.Popup)
                                                                        {
                                                                            margin = {left = 0},
                                                                            fixedHeight = 16,
                                                                            normal = {background = TransformProStylesIcons.GetCustomIcon("PopupRight.pro")},
                                                                            active = {background = TransformProStylesIcons.GetCustomIcon("PopupRight-active.pro")},
                                                                            focused = {background = TransformProStylesIcons.GetCustomIcon("PopupRight-focus.pro")}
                                                                        });
                }

                return TransformProStyles.popupRightPersonal ??
                       (TransformProStyles.popupRightPersonal = new GUIStyle(TransformProStyles.Popup)
                                                                {
                                                                    margin = {left = 0},
                                                                    fixedHeight = 16,
                                                                    normal = {background = TransformProStylesIcons.GetCustomIcon("PopupRight")},
                                                                    active = {background = TransformProStylesIcons.GetCustomIcon("PopupRight-active")},
                                                                    focused = {background = TransformProStylesIcons.GetCustomIcon("PopupRight-focus")}
                                                                });
            }
        }

        public static GUISkin Skin
        {
            get
            {
                if (TransformProStyles.skin != null)
                {
                    return TransformProStyles.skin;
                }

                TransformProStyles.skin = Object.Instantiate(GUI.skin);
                TransformProStyles.skin.button = TransformProStyles.Buttons.Standard.Single;
                return TransformProStyles.skin;
            }
        }

        public static GUIStyle Toggle
        {
            get
            {
                return TransformProStyles.toggle ??
                       (TransformProStyles.toggle = new GUIStyle(EditorStyles.miniButton)
                                                    {
                                                        normal = {background = TransformProStylesIcons.GetCustomIcon("ToggleOff")},
                                                        active = {background = TransformProStylesIcons.GetCustomIcon("ToggleOff")},
                                                        focused = {background = TransformProStylesIcons.GetCustomIcon("ToggleOff")},
                                                        onNormal = {background = TransformProStylesIcons.GetCustomIcon("ToggleOn")},
                                                        onActive = {background = TransformProStylesIcons.GetCustomIcon("ToggleOn")},
                                                        onFocused = {background = TransformProStylesIcons.GetCustomIcon("ToggleOn")},
                                                        border = new RectOffset(12, 12, 4, 4),
                                                        padding = new RectOffset(0, 0, 0, 0),
                                                        margin = new RectOffset(0, 0, 0, 0),
                                                        overflow = new RectOffset(0, 0, 0, 0)
                                                    });
            }
        }

        public static void Initialise()
        {
            string[] guid = AssetDatabase.FindAssets("TransformProPreferences t:MonoScript");
            if (guid.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid.First());
                if (assetPath != null)
                {
                    assetPath = Path.GetDirectoryName(assetPath);
                    if (assetPath != null)
                    {
                        assetPath = Directory.GetParent(assetPath).ToString();
                        TransformProStyles.pathRoot = assetPath;
                        TransformProStyles.initialised = true;
                        return;
                    }
                }
            }

            Debug.LogError("[<color=red>TransformPro</color>] Could not detemine installation path. Further errors may occur.");
        }

        public static bool Load()
        {
            if (!TransformProStyles.initialised)
            {
                return false;
            }

            try
            {
                if (TransformProStyles.buttons == null)
                {
                    TransformProStyles.buttons = new TransformProStylesButtons();
                }

                if (TransformProStyles.icons == null)
                {
                    TransformProStyles.icons = new TransformProStylesIcons();
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
