#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MagicLightProbes
{
    public static class GeneralMethods
    {       
        public static GUIStyle captionStyle;
        public static GUIStyle managerWindowCaptionStyle;
        public static GUIStyle managerWindowTopPanelButtonStyle;
        public static GUIStyle managerWindowLabelStyle;
        public static GUIStyle managerWindowLabelCenteredStyle;
        public static GUIStyle managerWindowBoldLabelStyle;
        public static GUIStyle managerWindowWarningLabelStyle;
        public static GUIStyle managerWindowCalculatedLabelStyle;
        public static GUIStyle managerWindowProcessLabelStyle;
        public static GUIStyle managerWindowBackgroundBoxStyle;
        public static GUIStyle managerWindowCenteredFoldoutLabel;
        public static GUIStyle greenLabelStyle;
        public static bool stylesInitialized;
        private static int maxDensityValue = 5;
        private static Rect VolumesScrollArea = new Rect();
        private static Rect LightsScrollArea = new Rect();
        private static Vector2 volumesScrollPos;
        private static Vector2 lightsScrollPos;
        public static bool listChanged;        
        private static int mainComponentSelectedTab;
        private static MagicLightProbes.BoundsDisplayMode globalBoundsDisplayMode;
        private static MagicLightProbes.BoundsDisplayMode lastGlobalBoundsDisplayMode;

        private static List<MagicLightProbes> defaultGlobal = new List<MagicLightProbes>();

        private static string[] options = new string[32];

        public static void InitStyles()
        {
            stylesInitialized = true;

            captionStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(),
                padding = new RectOffset(10, 10, 5, 5),
                fontSize = 11,
                fontStyle = FontStyle.Bold
            };

            managerWindowBackgroundBoxStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(5, 5, 5, 5),
                padding = new RectOffset(2, 2, 2, 2)
            };

            managerWindowCaptionStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(),
                padding = new RectOffset(10, 10, 5, 5),
                fontSize = 11,
                fontStyle = FontStyle.Bold
            };

            managerWindowTopPanelButtonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(),
                padding = new RectOffset(10, 10, 5, 5),
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                fixedWidth = 125
            };

            managerWindowLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(),
                padding = new RectOffset(0, 0, 5, 0),
                fontSize = 11
            };

            managerWindowLabelCenteredStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(),
                padding = new RectOffset(0, 0, 5, 0),
                fontSize = 11
            };

            managerWindowCenteredFoldoutLabel = new GUIStyle(EditorStyles.foldout)
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(),
                padding = new RectOffset(0, 0, 5, 0),
                fontSize = 11
            };

            managerWindowBoldLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(),
                padding = new RectOffset(0, 0, 3, 0),
                fontSize = 11,
                fontStyle = FontStyle.Bold
            };

            managerWindowWarningLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(),
                padding = new RectOffset(10, 10, 5, 5),
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                fixedWidth = 150
            };

            managerWindowCalculatedLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(),
                padding = new RectOffset(10, 10, 5, 5),
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                fixedWidth = 150
            };

            managerWindowProcessLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(),
                padding = new RectOffset(10, 10, 5, 5),
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                fixedWidth = 150
            };

            greenLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11
            };

            greenLabelStyle.normal.textColor = Color.green;
            managerWindowWarningLabelStyle.normal.textColor = Color.yellow;
            managerWindowCalculatedLabelStyle.normal.textColor = Color.green;
            managerWindowProcessLabelStyle.normal.textColor = Color.red;
        }

        public static void MLPMainComponentEditor(MagicLightProbes magicLightProbes)
        {
            bool lockCalculation = false;

            if (!magicLightProbes.calculatingVolume)
            {
                if (magicLightProbes.combinedVolumeError)
                {
                    EditorGUILayout.HelpBox("MLP Combined Volume is deactivated in your hierarchy. " +
                        "This component is necessary for the correct operation of the system. " +
                        "Please find the MLP Combined Volume component and activate it.", MessageType.Error, true);
                    return;
                }

                magicLightProbes.selectedTab = GUILayout.Toolbar(magicLightProbes.selectedTab, new string[] { "Basic Parameters", "Culling Options", "Debug" });

                for (int i = 0; i < 32; i++)
                {
                    options[i] = "";

                    if (!string.IsNullOrEmpty(LayerMask.LayerToName(i)))
                    {
                        options[i] = LayerMask.LayerToName(i);
                    }
                    else
                    {
                        options[i] = "-Not Set-";
                    }
                }

                if (magicLightProbes.useDynamicDensity)
                {
                    if (magicLightProbes.lastVolumeSpacingMin != magicLightProbes.volumeSpacingMin)
                    {
                        magicLightProbes.lastVolumeSpacingMin = magicLightProbes.volumeSpacingMin;
                        magicLightProbes.volumeSpacingChanged = true;
                    }
                }
                else
                {
                    if (magicLightProbes.lastVolumeSpacingMax != magicLightProbes.volumeSpacingMax)
                    {
                        magicLightProbes.lastVolumeSpacingMax = magicLightProbes.volumeSpacingMax;
                        magicLightProbes.volumeSpacingChanged = true;
                    }
                }

                if (EditorPrefs.GetBool("MLP_newVersionAvailable"))
                {
                    EditorGUILayout.HelpBox("New version (" + EditorPrefs.GetString("MLP_latestVersion") + ") " +
                        "available.\r\nGo to [Tools->Magic Light Probes->About MLP...] to download it.", MessageType.Warning, true);
                }

                switch (magicLightProbes.selectedTab)
                {
                    case 0:
                        GUILayout.BeginVertical(GUI.skin.box);
                        GUILayout.BeginHorizontal();

                        GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Workflow", MLPTooltipManager.MainComponent.Tabs.BasicParameters), GUILayout.MinWidth(150));
                        magicLightProbes.workflow = (MagicLightProbes.Workflow) EditorGUILayout.EnumPopup(magicLightProbes.workflow, GUILayout.MaxWidth(250));

                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();

                        switch (magicLightProbes.workflow)
                        {
                            case MagicLightProbes.Workflow.Simple:
                                EditorGUILayout.TextArea("In this workflow, the position of the " +
                                    "light sources and their properties are not taken into account. " +
                                    "This allows you to calculate faster and does not require additional " +
                                    "configuration.", GUI.skin.GetStyle("HelpBox"));
                                break;
                            case MagicLightProbes.Workflow.Advanced:
                                EditorGUILayout.TextArea("This workflow requires the addition of " +
                                    "the \"MLP Light\" component to all light sources that must be taken into " +
                                    "account during the calculation. This allows you to achieve more accurate results, " +
                                    "but also requires fine tuning of each light source.", GUI.skin.GetStyle("HelpBox"));
                                break;
                        }

                        GUILayout.EndHorizontal();
                        GUILayout.BeginVertical(GUI.skin.box);

                        GUILayout.Label("Volume Density", managerWindowLabelCenteredStyle);

                        GUILayout.Space(5);
                        GUILayout.BeginVertical(GUI.skin.box);
                        //GUILayout.BeginHorizontal();

                        //GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Use Dynamic Density", MLPTooltipManager.MainComponent.Tabs.BasicParameters), GUILayout.MinWidth(150));
                        //magicLightProbes.useDynamicDensity = EditorGUILayout.Toggle(magicLightProbes.useDynamicDensity, GUILayout.MaxWidth(250));

                        //GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();

                        GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Max Value", MLPTooltipManager.MainComponent.Tabs.BasicParameters), GUILayout.MinWidth(150));
                        maxDensityValue = EditorGUILayout.IntField(maxDensityValue, GUILayout.MaxWidth(250));

                        GUILayout.EndHorizontal();                                            

                        if (magicLightProbes.useDynamicDensity)
                        {
                            GUILayout.BeginHorizontal();

                            GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Probes Spacing", MLPTooltipManager.MainComponent.Tabs.BasicParameters), managerWindowLabelCenteredStyle);

                            GUILayout.EndHorizontal();
                            GUILayout.BeginVertical(GUI.skin.box);
                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Min", GUILayout.MinWidth(150));
                            magicLightProbes.volumeSpacingMin = EditorGUILayout.Slider(magicLightProbes.volumeSpacingMin, 0.1f, maxDensityValue, GUILayout.MaxWidth(250));
                            magicLightProbes.nearbyGeometryDetectionRadiusMin = magicLightProbes.volumeSpacingMin + (magicLightProbes.volumeSpacingMin / 2);

                            if (magicLightProbes.volumeSpacingMin > magicLightProbes.volumeSpacingMax)
                            {
                                magicLightProbes.volumeSpacingMax = magicLightProbes.volumeSpacingMin;
                            }

                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Max", GUILayout.MinWidth(150));

                            magicLightProbes.volumeSpacingMax = EditorGUILayout.Slider(magicLightProbes.volumeSpacingMax, 0.1f, maxDensityValue, GUILayout.MaxWidth(250));
                            magicLightProbes.nearbyGeometryDetectionRadiusMax = magicLightProbes.volumeSpacingMax + (magicLightProbes.volumeSpacingMax / 2);

                            if (magicLightProbes.volumeSpacingMax < magicLightProbes.volumeSpacingMin)
                            {
                                magicLightProbes.volumeSpacingMin = magicLightProbes.volumeSpacingMax;
                            }

                            GUILayout.EndHorizontal();
                            GUILayout.EndVertical();
                        }
                        else
                        {
                            GUILayout.BeginHorizontal();

                            GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Probes Spacing", MLPTooltipManager.MainComponent.Tabs.BasicParameters), GUILayout.MinWidth(150));

                            magicLightProbes.volumeSpacing = EditorGUILayout.Slider(magicLightProbes.volumeSpacing, 0.1f, maxDensityValue, GUILayout.MaxWidth(250));
                            magicLightProbes.nearbyGeometryDetectionRadius = magicLightProbes.volumeSpacing + (magicLightProbes.volumeSpacing / 2);
                            
                            GUILayout.EndHorizontal();
                        }
                        
                        GUILayout.BeginHorizontal();

                        GUILayout.Label("", GUILayout.MinWidth(150));

                        if (magicLightProbes.useDynamicDensity)
                        {
                            EditorGUILayout.TextArea("Try place probe every " + magicLightProbes.volumeSpacingMin + " - " + magicLightProbes.volumeSpacingMax + " meters", GUI.skin.GetStyle("HelpBox"), GUILayout.MaxWidth(250));
                        }
                        else
                        {
                            EditorGUILayout.TextArea("Try place probe every " + magicLightProbes.volumeSpacing + " meters", GUI.skin.GetStyle("HelpBox"), GUILayout.MaxWidth(250));
                        }

                        GUILayout.EndHorizontal();
                        GUILayout.Space(10);
                        GUILayout.BeginVertical(GUI.skin.box);

                        if (magicLightProbes.useDynamicDensity)
                        {
                            GUILayout.BeginHorizontal();

                            GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Corners Detection Threshold", MLPTooltipManager.MainComponent.Tabs.BasicParameters), managerWindowLabelCenteredStyle);

                            GUILayout.EndHorizontal();
                            GUILayout.BeginVertical(GUI.skin.box);
                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Min", GUILayout.MinWidth(200));

                            magicLightProbes.cornersDetectionThresholdMin = EditorGUILayout.Slider(magicLightProbes.cornersDetectionThresholdMin, 0.1f, magicLightProbes.volumeSpacingMin, GUILayout.MaxWidth(250));

                            if (magicLightProbes.cornersDetectionThresholdMin > magicLightProbes.cornersDetectionThresholdMax)
                            {
                                magicLightProbes.cornersDetectionThresholdMax = magicLightProbes.cornersDetectionThresholdMin;
                            }

                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Max", GUILayout.MinWidth(200));

                            magicLightProbes.cornersDetectionThresholdMax = EditorGUILayout.Slider(magicLightProbes.cornersDetectionThresholdMax, 0.1f, magicLightProbes.volumeSpacingMax, GUILayout.MaxWidth(250));

                            if (magicLightProbes.cornersDetectionThresholdMax < magicLightProbes.cornersDetectionThresholdMin)
                            {
                                magicLightProbes.cornersDetectionThresholdMin = magicLightProbes.cornersDetectionThresholdMax;
                            }

                            GUILayout.EndHorizontal();
                            GUILayout.EndVertical();
                            GUILayout.BeginHorizontal();

                            GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Corner Probe Spacing", MLPTooltipManager.MainComponent.Tabs.BasicParameters), GUILayout.MinWidth(200));

                            if (magicLightProbes.useDynamicDensity)
                            {
                                magicLightProbes.cornerProbesSpacing = EditorGUILayout.Slider(magicLightProbes.cornerProbesSpacing, magicLightProbes.cornersDetectionThresholdMin, magicLightProbes.volumeSpacingMin * 2, GUILayout.MaxWidth(250));
                            }
                            else
                            {
                                magicLightProbes.cornerProbesSpacing = EditorGUILayout.Slider(magicLightProbes.cornerProbesSpacing, magicLightProbes.cornersDetectionThreshold, magicLightProbes.volumeSpacing * 2, GUILayout.MaxWidth(250));
                            }

                            magicLightProbes.lastCornerProbesSpacing = magicLightProbes.cornerProbesSpacing;                            

                            GUILayout.EndHorizontal();
                        }
                        else
                        {
                            GUILayout.BeginHorizontal();

                            GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Corners Detection Threshold", MLPTooltipManager.MainComponent.Tabs.BasicParameters), GUILayout.MinWidth(200));
                            magicLightProbes.cornersDetectionThreshold = EditorGUILayout.Slider(magicLightProbes.cornersDetectionThreshold, 0.1f, maxDensityValue, GUILayout.MaxWidth(250));

                            if (magicLightProbes.cornersDetectionThreshold > magicLightProbes.volumeSpacing)
                            {
                                magicLightProbes.cornersDetectionThreshold = magicLightProbes.volumeSpacing;
                            }

                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();

                            GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Corner Probe Spacing", MLPTooltipManager.MainComponent.Tabs.BasicParameters), GUILayout.MinWidth(200));
                            magicLightProbes.cornerProbesSpacing = EditorGUILayout.Slider(magicLightProbes.cornerProbesSpacing, magicLightProbes.cornersDetectionThreshold, magicLightProbes.volumeSpacing * 2, GUILayout.MaxWidth(250));
                            magicLightProbes.lastCornerProbesSpacing = magicLightProbes.cornerProbesSpacing;

                            GUILayout.EndHorizontal();
                        }                        
                        
                        GUILayout.BeginHorizontal();

                        GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Distance From Geometry", MLPTooltipManager.MainComponent.Tabs.BasicParameters), GUILayout.MinWidth(200));
                        magicLightProbes.distanceFromNearbyGeometry = EditorGUILayout.Slider(magicLightProbes.distanceFromNearbyGeometry, 0.1f, 1.0f, GUILayout.MaxWidth(250));

                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                        GUILayout.BeginHorizontal();

                        GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Probes Count Limit", MLPTooltipManager.MainComponent.Tabs.BasicParameters), GUILayout.MinWidth(150));
                        magicLightProbes.maxProbesInVolume = EditorGUILayout.IntField(magicLightProbes.maxProbesInVolume, GUILayout.MaxWidth(250));

                        if (magicLightProbes.maxProbesInVolume < 10000)
                        {
                            magicLightProbes.maxProbesInVolume = 10000;
                        }

                        magicLightProbes.defaultMaxProbesCount = magicLightProbes.maxProbesInVolume;

                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                        GUILayout.Space(5);
                        GUILayout.BeginVertical(GUI.skin.box);

                        GUILayout.Label("Ground And Floor Objects", managerWindowLabelCenteredStyle);

                        GUILayout.Space(5);

                        GUILayout.BeginHorizontal();

                        GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Use Volume Bottom", MLPTooltipManager.MainComponent.Tabs.BasicParameters), GUILayout.MinWidth(150));
                        magicLightProbes.useVolumeBottom = EditorGUILayout.Toggle(magicLightProbes.useVolumeBottom, GUILayout.MaxWidth(250));

                        GUILayout.EndHorizontal();

                        string sourceVolumeName = "";

                        if (!magicLightProbes.useVolumeBottom)
                        {
                            bool globalGroundObjectsUsed = false;

                            if (!magicLightProbes.globalGroundObjects)
                            {
                                for (int i = 0; i < magicLightProbes.allVolumes.Count; i++)
                                {
                                    if (magicLightProbes.allVolumes[i] != magicLightProbes && magicLightProbes.allVolumes[i].globalGroundObjects)
                                    {
                                        magicLightProbes.storedGroundAndFloorKeywords = new List<string>(magicLightProbes.groundAndFloorKeywords);
                                        magicLightProbes.groundAndFloorKeywords = new List<string>(magicLightProbes.allVolumes[i].groundAndFloorKeywords);
                                        sourceVolumeName = magicLightProbes.allVolumes[i].name;
                                        globalGroundObjectsUsed = true;
                                        break;
                                    }
                                }
                            }

                            if (!globalGroundObjectsUsed)
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Selected Objects As Global", MLPTooltipManager.MainComponent.Tabs.BasicParameters), GUILayout.MinWidth(150));
                                    magicLightProbes.globalGroundObjects = EditorGUILayout.Toggle(magicLightProbes.globalGroundObjects, GUILayout.MaxWidth(250));
                                }

                                if (magicLightProbes.groundAndFloorKeywords.Count > 0)
                                {
                                    for (int i = 0; i < magicLightProbes.groundAndFloorKeywords.Count; i++)
                                    {
                                        GUILayout.BeginVertical(GUI.skin.box);
                                        GUILayout.BeginHorizontal();

                                        GUILayout.Label(magicLightProbes.groundAndFloorKeywords[i], GUILayout.MinWidth(200));

                                        if (GUILayout.Button("Remove Object"))
                                        {
                                            magicLightProbes.groundAndFloorKeywords.Remove(magicLightProbes.groundAndFloorKeywords[i]);
                                        }

                                        GUILayout.EndHorizontal();
                                        GUILayout.EndVertical();
                                    }
                                }
                                else
                                {
                                    GUILayout.BeginHorizontal();

                                    EditorGUILayout.HelpBox("" +
                                        "Before starting the calculation, " +
                                        "you must specify the objects that the algorithm " +
                                        "will use as floor or ground.Below these objects, the " +
                                        "probes will be forcibly removed.", MessageType.Error);

                                    GUILayout.EndHorizontal();
                                }

                                GUILayout.BeginHorizontal();

                                GUIStyle myStyle = GUI.skin.GetStyle("HelpBox");
                                myStyle.alignment = TextAnchor.MiddleCenter;

                                EditorGUILayout.TextArea("\r\n\r\nDrag & Drop Objects Here\r\n\r\n", myStyle);

                                Rect dragDropArea = GUILayoutUtility.GetLastRect();

                                if (dragDropArea.Contains(Event.current.mousePosition))
                                {
                                    if (Event.current.type == EventType.DragUpdated)
                                    {
                                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                                        Event.current.Use();
                                    }
                                    else if (Event.current.type == EventType.DragPerform)
                                    {
                                        for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                                        {
                                            GameObject newGroundObject = DragAndDrop.objectReferences[i] as GameObject;

                                            if (newGroundObject != null)
                                            {
                                                if (!magicLightProbes.groundAndFloorKeywords.Contains(newGroundObject.name))
                                                {
                                                    if (newGroundObject.GetComponent<MeshRenderer>() != null || newGroundObject.GetComponent<Collider>() != null)
                                                    {
                                                        string errorMessage = "";
                                                        bool isStatic = MLPUtilites.CheckIfStatic(newGroundObject, out errorMessage);

                                                        if (isStatic)
                                                        {
                                                            magicLightProbes.groundAndFloorKeywords.Add(newGroundObject.name);

                                                            Debug.LogFormat("<color=yellow>MLP:</color> An object named " +
                                                                "\"" + DragAndDrop.objectReferences[i].name + "\" has been added to the list.");
                                                        }
                                                        else
                                                        {
                                                            if (EditorUtility.DisplayDialog("Magic Light Probes", errorMessage, "OK"))
                                                            {
                                                                return;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        EditorUtility.DisplayDialog("Magic Light Probes", "The object you have selected does not have a Collider or Mesh Renderer " +
                                                        "and cannot be taken into account by the system. Add a Collider or Mesh Renderer (with mesh filter) to the object or " +
                                                        "select another object.", "OK");
                                                    }
                                                }
                                                else
                                                {
                                                    EditorUtility.DisplayDialog("Magic Light Probes", "This object has already " +
                                                                                            "been added to the list.", "OK");
                                                }
                                            }
                                        }
                                        Event.current.Use();
                                    }
                                }

                                GUILayout.EndHorizontal();
                            }
                            else
                            { 
                                EditorGUILayout.HelpBox("The global list of objects from " + sourceVolumeName + " is used.", MessageType.Info);
                            }
                        }

                        if (magicLightProbes.tooManySubVolumes)
                        {
                            GUILayout.BeginVertical(GUI.skin.box);
                            GUILayout.BeginHorizontal();

                            EditorGUILayout.HelpBox("The main volume contains too many sub-volumes. \r\n\r\n" +
                                    "What can you do: \r\n" +
                                    " - Increase the \"Probes Count Limit\"\r\n" +
                                    " - Change the spacing settings (\"Probe Spacing\" or \"Corners Detection Threshold\") \r\n" +
                                    " - Reduce volume sizes)", MessageType.Error);

                            GUILayout.EndHorizontal();
                            GUILayout.EndVertical();
                        }

                        GUILayout.EndVertical();
                        GUILayout.EndVertical();
                        break;
                    case 1:
                        GUILayout.BeginVertical(GUI.skin.box);

                        if (magicLightProbes.workflow == MagicLightProbes.Workflow.Advanced)
                        {
                            GUILayout.BeginVertical(GUI.skin.box);

                            GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Exclude Lights From Process", MLPTooltipManager.MainComponent.Tabs.CullingOptions), captionStyle);

                            for (int i = 0; i < magicLightProbes.excludedLights.Count; i++)
                            {
                                GUILayout.BeginHorizontal();

                                if (magicLightProbes.excludedLights[i] != null)
                                {
                                    GUILayout.Label(magicLightProbes.excludedLights[i].gameObject.name, GUILayout.MinWidth(200));
                                }
                                else
                                {
                                    GUILayout.Label("- Not Selected -", GUILayout.MinWidth(200));
                                }

                                magicLightProbes.excludedLights[i] = EditorGUILayout.ObjectField(magicLightProbes.excludedLights[i], typeof(MLPLight), true, GUILayout.MaxWidth(250)) as MLPLight;

                                if (GUILayout.Button("Remove Light"))
                                {
                                    magicLightProbes.excludedLights.Remove(magicLightProbes.excludedLights[i]);
                                }

                                GUILayout.EndHorizontal();
                            }

                            if (GUILayout.Button("Exclude Light..."))
                            {
                                magicLightProbes.excludedLights.Add(new MLPLight());
                            }

                            GUILayout.EndVertical();
                        }

                        GUILayout.BeginHorizontal();

                        GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Max Height Above Geometry", MLPTooltipManager.MainComponent.Tabs.CullingOptions), GUILayout.MinWidth(200));
                        magicLightProbes.maxHeightAboveGeometry = EditorGUILayout.FloatField(magicLightProbes.maxHeightAboveGeometry, GUILayout.MaxWidth(250));

                        if (magicLightProbes.lastMaxHeightAboveGeometry != 0 && magicLightProbes.lastMaxHeightAboveGeometry != magicLightProbes.maxHeightAboveGeometry)
                        {

                        }

                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();

                        GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Max Height Above Terrain", MLPTooltipManager.MainComponent.Tabs.CullingOptions), GUILayout.MinWidth(200));
                        magicLightProbes.maxHeightAboveTerrain = EditorGUILayout.FloatField(magicLightProbes.maxHeightAboveTerrain, GUILayout.MaxWidth(250));

                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();

                        GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Collision Detection Layers", MLPTooltipManager.MainComponent.Tabs.CullingOptions), GUILayout.MinWidth(200));
                        magicLightProbes.layerMask = EditorGUILayout.MaskField(magicLightProbes.layerMask, options, GUILayout.MaxWidth(250));

                        for (int i = 0; i < 32; i++)
                        {
                            if ((1 << LayerMask.NameToLayer(LayerMask.LayerToName(i)) & magicLightProbes.layerMask) != 0)
                            {
                                magicLightProbes.firstCollisionLayer = LayerMask.NameToLayer(LayerMask.LayerToName(i));
                                break;
                            }
                        }

                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();

                        GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Collision Detection Radius", MLPTooltipManager.MainComponent.Tabs.CullingOptions), GUILayout.MinWidth(200));
                        magicLightProbes.collisionDetectionRadius = EditorGUILayout.FloatField(magicLightProbes.collisionDetectionRadius, GUILayout.MaxWidth(250));

                        GUILayout.EndHorizontal();

                        if (magicLightProbes.workflow == MagicLightProbes.Workflow.Advanced)
                        {
                            GUILayout.BeginHorizontal();

                            GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Filling Mode", MLPTooltipManager.MainComponent.Tabs.CullingOptions), GUILayout.MinWidth(200));
                            magicLightProbes.fillingMode = (MagicLightProbes.FillingMode) EditorGUILayout.EnumPopup(magicLightProbes.fillingMode, GUILayout.MaxWidth(250));

                            GUILayout.EndHorizontal();

                            switch (magicLightProbes.fillingMode)
                            {
                                case MagicLightProbes.FillingMode.FullFilling:
                                    break;
                                case MagicLightProbes.FillingMode.SeparateFilling:
                                    GUILayout.BeginHorizontal();

                                    GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Consider Distance To Lights", MLPTooltipManager.MainComponent.Tabs.CullingOptions), GUILayout.MinWidth(200));
                                    magicLightProbes.considerDistanceToLights = EditorGUILayout.Toggle(magicLightProbes.considerDistanceToLights, GUILayout.MaxWidth(250));

                                    GUILayout.EndHorizontal();
                                    GUILayout.BeginHorizontal();

                                    GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Light Intensity Threshold", MLPTooltipManager.MainComponent.Tabs.CullingOptions), GUILayout.MinWidth(200));
                                    magicLightProbes.lightIntensityTreshold = EditorGUILayout.Slider(magicLightProbes.lightIntensityTreshold, 0.01f, 1f, GUILayout.MaxWidth(250));

                                    GUILayout.EndHorizontal();
                                    GUILayout.Space(5);
                                    GUILayout.BeginHorizontal();

                                    GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Cull By Color", MLPTooltipManager.MainComponent.Tabs.CullingOptions), GUILayout.MinWidth(200));
                                    magicLightProbes.cullByColor = EditorGUILayout.Toggle(magicLightProbes.cullByColor, GUILayout.MaxWidth(250));

                                    GUILayout.EndHorizontal();

                                    if (magicLightProbes.cullByColor)
                                    {
                                        GUILayout.BeginVertical(GUI.skin.box);
                                        GUILayout.BeginHorizontal();

                                        GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Color Threshold", MLPTooltipManager.MainComponent.Tabs.CullingOptions), GUILayout.MinWidth(200));
                                        magicLightProbes.colorTreshold = EditorGUILayout.Slider(magicLightProbes.colorTreshold, 0.001f, 1.0f, GUILayout.MaxWidth(250));
                                        magicLightProbes.lastColorThreshold = magicLightProbes.colorTreshold;

                                        GUILayout.EndHorizontal();
                                        GUILayout.BeginHorizontal();

                                        GUILayout.Label("", GUILayout.MinWidth(200));
                                        EditorGUILayout.TextArea("The larger this value - a larger range of colors will be considered equivalent.", GUI.skin.GetStyle("HelpBox"), GUILayout.MaxWidth(250));

                                        GUILayout.EndHorizontal();                                        
                                        GUILayout.EndVertical();
                                    }

                                    GUILayout.Space(5);
                                    GUILayout.BeginHorizontal();

                                    GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Fill Equivalent Volume", MLPTooltipManager.MainComponent.Tabs.CullingOptions), GUILayout.MinWidth(200));
                                    magicLightProbes.fillEquivalentVolume = EditorGUILayout.Toggle(magicLightProbes.fillEquivalentVolume, GUILayout.MaxWidth(250));

                                    GUILayout.EndHorizontal();

                                    if (magicLightProbes.fillEquivalentVolume)
                                    {
                                        GUILayout.BeginVertical(GUI.skin.box);
                                        GUILayout.BeginHorizontal();

                                        GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Filling Rate", MLPTooltipManager.MainComponent.Tabs.CullingOptions), GUILayout.MinWidth(200));
                                        magicLightProbes.equivalentVolumeFillingRate = EditorGUILayout.Slider(magicLightProbes.equivalentVolumeFillingRate, 0f, 1f, GUILayout.MaxWidth(250));
                                        magicLightProbes.lastEquivalentVolumeFillingRate = magicLightProbes.equivalentVolumeFillingRate;

                                        GUILayout.EndHorizontal();
                                        GUILayout.EndVertical();
                                    }

                                    GUILayout.BeginHorizontal();

                                    GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Fill Unlit Volume", MLPTooltipManager.MainComponent.Tabs.CullingOptions), GUILayout.MinWidth(200));
                                    magicLightProbes.fillUnlitVolume = EditorGUILayout.Toggle(magicLightProbes.fillUnlitVolume, GUILayout.MaxWidth(250));

                                    GUILayout.EndHorizontal();

                                    if (magicLightProbes.fillUnlitVolume)
                                    {
                                        GUILayout.BeginVertical(GUI.skin.box);
                                        GUILayout.BeginHorizontal();

                                        GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Filling Rate", MLPTooltipManager.MainComponent.Tabs.CullingOptions), GUILayout.MinWidth(200));
                                        magicLightProbes.unlitVolumeFillingRate = EditorGUILayout.Slider(magicLightProbes.unlitVolumeFillingRate, 0f, 1f, GUILayout.MaxWidth(250));
                                        magicLightProbes.lastUnlitVolumeFillingRate = magicLightProbes.unlitVolumeFillingRate;

                                        GUILayout.EndHorizontal();
                                        GUILayout.EndVertical();
                                    }
                                    break;
                                case MagicLightProbes.FillingMode.VerticalDublicating:
                                    GUILayout.BeginHorizontal();

                                    GUILayout.Label("Height", GUILayout.MinWidth(200));
                                    magicLightProbes.verticalDublicatingHeight = EditorGUILayout.FloatField(magicLightProbes.verticalDublicatingHeight, GUILayout.MaxWidth(250));

                                    GUILayout.EndHorizontal();
                                    GUILayout.BeginHorizontal();

                                    GUILayout.Label("Step", GUILayout.MinWidth(200));
                                    magicLightProbes.verticalDublicatingStep = EditorGUILayout.FloatField(magicLightProbes.verticalDublicatingStep, GUILayout.MaxWidth(250));

                                    GUILayout.EndHorizontal();
                                    break;
                            }
                        }
                        else
                        {
                            GUILayout.BeginHorizontal();

                            GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Color Threshold", MLPTooltipManager.MainComponent.Tabs.CullingOptions), GUILayout.MinWidth(200));
                            magicLightProbes.colorTreshold = EditorGUILayout.Slider(magicLightProbes.colorTreshold, 0.001f, 1.0f, GUILayout.MaxWidth(250));
                            magicLightProbes.lastColorThreshold = magicLightProbes.colorTreshold;

                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();

                            GUILayout.Label("", GUILayout.MinWidth(200));
                            EditorGUILayout.TextArea("The larger this value - a larger range of colors will be considered equivalent.", GUI.skin.GetStyle("HelpBox"), GUILayout.MaxWidth(250));

                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();

                            GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Save Probes On Shading Borders", MLPTooltipManager.MainComponent.Tabs.CullingOptions), GUILayout.MinWidth(200));
                            magicLightProbes.forceSaveProbesOnShadingBorders = EditorGUILayout.Toggle(magicLightProbes.forceSaveProbesOnShadingBorders, GUILayout.MaxWidth(250));

                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();

                            GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Save Probes On Geometry Edges", MLPTooltipManager.MainComponent.Tabs.CullingOptions), GUILayout.MinWidth(200));
                            magicLightProbes.placeProbesOnGeometryEdges = EditorGUILayout.Toggle(magicLightProbes.placeProbesOnGeometryEdges, GUILayout.MaxWidth(250));

                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();

                            GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Free Volume Filling Rate", MLPTooltipManager.MainComponent.Tabs.CullingOptions), GUILayout.MinWidth(200));
                            magicLightProbes.freeVolumeFillingRate = EditorGUILayout.Slider(magicLightProbes.freeVolumeFillingRate, 0f, 1f, GUILayout.MaxWidth(250));
                            magicLightProbes.lastFreeVolumeFillingRate = magicLightProbes.freeVolumeFillingRate;

                            GUILayout.EndHorizontal();
                        }

                        GUILayout.BeginHorizontal();

                        GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Optimize For Mixed Lighting", MLPTooltipManager.MainComponent.Tabs.CullingOptions), GUILayout.MinWidth(200));
                        magicLightProbes.optimizeForMixedLighting = EditorGUILayout.Toggle(magicLightProbes.optimizeForMixedLighting, GUILayout.MaxWidth(250));

                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();

                        GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Try Prevent Light Leakage", MLPTooltipManager.MainComponent.Tabs.CullingOptions), GUILayout.MinWidth(200));
                        magicLightProbes.preventLeakageThroughWalls = EditorGUILayout.Toggle(magicLightProbes.preventLeakageThroughWalls, GUILayout.MaxWidth(250));

                        GUILayout.EndHorizontal();
                        break;
                    case 2:
                        GUILayout.BeginVertical(GUI.skin.box);
                        GUILayout.BeginHorizontal();

                        GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Bounds Dispaly Mode", MLPTooltipManager.MainComponent.Tabs.Debug), GUILayout.MinWidth(150));
                        magicLightProbes.boundsDisplayMode = (MagicLightProbes.BoundsDisplayMode) EditorGUILayout.EnumPopup(magicLightProbes.boundsDisplayMode, GUILayout.MaxWidth(250));

                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();

                        GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Enable Debug Mode", MLPTooltipManager.MainComponent.Tabs.Debug), GUILayout.MinWidth(150));
                        magicLightProbes.debugMode = EditorGUILayout.Toggle(magicLightProbes.debugMode, GUILayout.MaxWidth(250));

                        GUILayout.EndHorizontal();

                        if (magicLightProbes.debugMode)
                        {
                            GUILayout.BeginHorizontal();

                            GUILayout.Label(MLPTooltipManager.MainComponent.GetParameter("Debug Object Scale", MLPTooltipManager.MainComponent.Tabs.Debug), GUILayout.MinWidth(150));
                            magicLightProbes.debugObjectScale = EditorGUILayout.FloatField(magicLightProbes.debugObjectScale, GUILayout.MaxWidth(250));

                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Debug Pass", EditorStyles.boldLabel, GUILayout.MinWidth(150));
                            magicLightProbes.debugPass = (MagicLightProbes.DebugPasses) EditorGUILayout.EnumPopup(magicLightProbes.debugPass, GUILayout.MaxWidth(250));

                            GUILayout.EndHorizontal();

                            switch (magicLightProbes.debugPass)
                            {
                                case MagicLightProbes.DebugPasses.MaximumHeight:
                                    magicLightProbes.debugShowLightIntensity = false;

                                    GUILayout.BeginVertical(GUI.skin.box);
                                    GUILayout.BeginHorizontal();

                                    GUILayout.Label("Max Height Above Geometry", GUILayout.MinWidth(150));
                                    magicLightProbes.maxHeightAboveGeometry = EditorGUILayout.FloatField(magicLightProbes.maxHeightAboveGeometry, GUILayout.MaxWidth(250));

                                    GUILayout.EndHorizontal();
                                    GUILayout.BeginHorizontal();

                                    GUILayout.Label("Max Height Above Terrain", GUILayout.MinWidth(150));
                                    magicLightProbes.maxHeightAboveTerrain = EditorGUILayout.FloatField(magicLightProbes.maxHeightAboveTerrain, GUILayout.MaxWidth(250));

                                    GUILayout.EndHorizontal();
                                    GUILayout.BeginHorizontal();

                                    GUILayout.Label("Draw Mode", EditorStyles.boldLabel, GUILayout.MinWidth(150));
                                    magicLightProbes.drawMode = (MagicLightProbes.DrawModes) EditorGUILayout.EnumPopup(magicLightProbes.drawMode, GUILayout.MaxWidth(250));

                                    GUILayout.EndHorizontal();
                                    GUILayout.EndVertical();
                                    break;
                                case MagicLightProbes.DebugPasses.GeometryCollision:
                                    magicLightProbes.debugShowLightIntensity = false;

                                    GUILayout.BeginVertical(GUI.skin.box);
                                    GUILayout.BeginHorizontal();

                                    GUILayout.Label("Collision Detection Radius", GUILayout.MinWidth(150));
                                    magicLightProbes.collisionDetectionRadius = EditorGUILayout.FloatField(magicLightProbes.collisionDetectionRadius, GUILayout.MaxWidth(250));

                                    GUILayout.EndHorizontal();
                                    GUILayout.BeginHorizontal();

                                    GUILayout.Label("Draw Mode", EditorStyles.boldLabel, GUILayout.MinWidth(150));
                                    magicLightProbes.drawMode = (MagicLightProbes.DrawModes) EditorGUILayout.EnumPopup(magicLightProbes.drawMode, GUILayout.MaxWidth(250));

                                    GUILayout.EndHorizontal();
                                    GUILayout.EndVertical();
                                    break;
                                case MagicLightProbes.DebugPasses.GeometryIntersections:
                                    GUILayout.BeginVertical(GUI.skin.box);
                                    GUILayout.BeginHorizontal();

                                    GUILayout.Label("Corners Detection Threshold", GUILayout.MinWidth(200));
                                    magicLightProbes.cornersDetectionThreshold = EditorGUILayout.Slider(magicLightProbes.cornersDetectionThreshold, 0.1f, maxDensityValue, GUILayout.MaxWidth(250));

                                    if (magicLightProbes.cornersDetectionThreshold > magicLightProbes.volumeSpacing)
                                    {
                                        magicLightProbes.cornersDetectionThreshold = magicLightProbes.volumeSpacing;
                                    }

                                    GUILayout.EndHorizontal();
                                    GUILayout.BeginHorizontal();

                                    GUILayout.Label("Corner Probes Spacing", GUILayout.MinWidth(200));
                                    magicLightProbes.cornerProbesSpacing = EditorGUILayout.Slider(magicLightProbes.cornerProbesSpacing, magicLightProbes.cornersDetectionThreshold, magicLightProbes.volumeSpacing * 2, GUILayout.MaxWidth(250));

                                    GUILayout.EndHorizontal();
                                    GUILayout.EndVertical();
                                    break;
                                case MagicLightProbes.DebugPasses.NearGeometry:
                                    GUILayout.BeginVertical(GUI.skin.box);
                                    GUILayout.BeginHorizontal();

                                    GUILayout.Label("Distance From Geometry", GUILayout.MinWidth(150));
                                    magicLightProbes.distanceFromNearbyGeometry = EditorGUILayout.Slider(magicLightProbes.distanceFromNearbyGeometry, 0.1f, 1.0f, GUILayout.MaxWidth(250));

                                    GUILayout.EndHorizontal();
                                    GUILayout.BeginHorizontal();

                                    GUILayout.Label("Cull By Color", GUILayout.MinWidth(150));
                                    magicLightProbes.cullByColor = EditorGUILayout.Toggle(magicLightProbes.cullByColor, GUILayout.MaxWidth(250));

                                    GUILayout.EndHorizontal();

                                    if (magicLightProbes.cullByColor)
                                    {
                                        GUILayout.BeginHorizontal();

                                        GUILayout.Label("Color Threshold", GUILayout.MinWidth(150));
                                        magicLightProbes.colorTreshold = EditorGUILayout.Slider(magicLightProbes.colorTreshold, 0.001f, 1.0f, GUILayout.MaxWidth(250));

                                        GUILayout.EndHorizontal();
                                    }

                                    GUILayout.EndVertical();
                                    break;
                                case MagicLightProbes.DebugPasses.OutOfRange:
                                    if (magicLightProbes.workflow == MagicLightProbes.Workflow.Advanced)
                                    {
                                        magicLightProbes.debugShowLightIntensity = false;

                                        GUILayout.BeginVertical(GUI.skin.box);
                                        GUILayout.BeginHorizontal();

                                        GUILayout.Label("Draw Mode", EditorStyles.boldLabel, GUILayout.MinWidth(150));
                                        magicLightProbes.drawMode = (MagicLightProbes.DrawModes) EditorGUILayout.EnumPopup(magicLightProbes.drawMode, GUILayout.MaxWidth(250));

                                        GUILayout.EndHorizontal();
                                        GUILayout.EndVertical();
                                    }
                                    else
                                    {
                                        lockCalculation = true;
                                        EditorGUILayout.HelpBox("This mode does not work in a \"Simple\" workflow..", MessageType.Info);
                                    }
                                    break;
                                case MagicLightProbes.DebugPasses.OutOfRangeBorders:
                                    if (magicLightProbes.workflow == MagicLightProbes.Workflow.Advanced)
                                    {
                                        GUILayout.BeginVertical(GUI.skin.box);
                                        GUILayout.BeginHorizontal();

                                        GUILayout.Label("Show Light Intensity", EditorStyles.boldLabel, GUILayout.MinWidth(150));
                                        magicLightProbes.debugShowLightIntensity = EditorGUILayout.Toggle(magicLightProbes.debugShowLightIntensity, GUILayout.MaxWidth(250));

                                        GUILayout.EndHorizontal();
                                        GUILayout.Space(5);

                                        EditorGUILayout.HelpBox("This pass exludes light sources with the calculation type \"Light Intensuty\".", MessageType.Info);

                                        GUILayout.EndVertical();
                                    }
                                    else
                                    {
                                        lockCalculation = true;
                                        EditorGUILayout.HelpBox("This mode does not work in a \"Simple\" workflow..", MessageType.Info);
                                    }
                                    break;
                                case MagicLightProbes.DebugPasses.ShadingBorders:
                                case MagicLightProbes.DebugPasses.ContrastAreas:
                                    if (magicLightProbes.workflow == MagicLightProbes.Workflow.Advanced)
                                    {
                                        GUILayout.BeginVertical(GUI.skin.box);
                                        GUILayout.BeginHorizontal();

                                        GUILayout.Label("Show Light Intensity", EditorStyles.boldLabel, GUILayout.MinWidth(150));
                                        magicLightProbes.debugShowLightIntensity = EditorGUILayout.Toggle(magicLightProbes.debugShowLightIntensity, GUILayout.MaxWidth(250));

                                        GUILayout.EndHorizontal();
                                        GUILayout.Space(5);

                                        EditorGUILayout.HelpBox("This pass exludes light sources with the calculation type \"Light Intensuty\".", MessageType.Info);

                                        GUILayout.EndVertical();
                                    }
                                    else
                                    {
                                        lockCalculation = true;
                                        EditorGUILayout.HelpBox("This mode does not work in a \"Simple\" workflow..", MessageType.Info);
                                    }
                                    break;
                                case MagicLightProbes.DebugPasses.NearLights:
                                    if (magicLightProbes.workflow == MagicLightProbes.Workflow.Advanced)
                                    {
                                        GUILayout.BeginVertical(GUI.skin.box);
                                        GUILayout.Space(5);
                                        EditorGUILayout.HelpBox("This pass exludes light sources in \"Mixed Mode\".", MessageType.Info);
                                        GUILayout.EndVertical();
                                    }
                                    else
                                    {
                                        lockCalculation = true;
                                        EditorGUILayout.HelpBox("This mode does not work in a \"Simple\" workflow..", MessageType.Info);
                                    }
                                    break;
                                case MagicLightProbes.DebugPasses.EqualProbes:
                                    if (magicLightProbes.workflow == MagicLightProbes.Workflow.Advanced)
                                    {
                                        GUILayout.BeginVertical(GUI.skin.box);
                                        GUILayout.BeginHorizontal();

                                        GUILayout.Label("Equivalent Intensity Accuracy", GUILayout.MinWidth(150));
                                        magicLightProbes.lightIntensityTreshold = EditorGUILayout.Slider(magicLightProbes.lightIntensityTreshold, 0.01f, 1f, GUILayout.MaxWidth(250));

                                        GUILayout.EndHorizontal();
                                        GUILayout.Space(5);
                                        GUILayout.BeginHorizontal();

                                        GUILayout.Label("Optimize Equivalent Probes", GUILayout.MinWidth(150));
                                        magicLightProbes.fillEquivalentVolume = EditorGUILayout.Toggle(magicLightProbes.fillEquivalentVolume, GUILayout.MaxWidth(250));

                                        GUILayout.EndHorizontal();

                                        if (magicLightProbes.fillEquivalentVolume)
                                        {
                                            GUILayout.BeginVertical(GUI.skin.box);
                                            GUILayout.BeginHorizontal();

                                            GUILayout.Label("Filling Density", GUILayout.MinWidth(150));
                                            magicLightProbes.equivalentVolumeFillingRate = EditorGUILayout.Slider(magicLightProbes.equivalentVolumeFillingRate, 0f, 1f, GUILayout.MaxWidth(250));

                                            GUILayout.EndHorizontal();
                                            GUILayout.EndVertical();
                                        }

                                        GUILayout.BeginHorizontal();

                                        GUILayout.Label("Show Light Intensity", EditorStyles.boldLabel, GUILayout.MinWidth(150));
                                        magicLightProbes.debugShowLightIntensity = EditorGUILayout.Toggle(magicLightProbes.debugShowLightIntensity, GUILayout.MaxWidth(250));

                                        GUILayout.EndHorizontal();

                                        GUILayout.EndVertical();
                                    }
                                    else
                                    {
                                        lockCalculation = true;
                                        EditorGUILayout.HelpBox("This mode does not work in a \"Simple\" workflow..", MessageType.Info);
                                    }
                                    break;
                                case MagicLightProbes.DebugPasses.EqualColor:
                                    GUILayout.BeginHorizontal();

                                    GUILayout.Label("Color Threshold", GUILayout.MinWidth(150));
                                    magicLightProbes.colorTreshold = EditorGUILayout.Slider(magicLightProbes.colorTreshold, 0.001f, 1.0f, GUILayout.MaxWidth(250));

                                    GUILayout.EndHorizontal();
                                    GUILayout.BeginHorizontal();

                                    GUILayout.Label("", GUILayout.MinWidth(150));
                                    EditorGUILayout.TextArea("The larger this value - a larger range of colors will be considered equivalent.", GUI.skin.GetStyle("HelpBox"), GUILayout.MaxWidth(250));

                                    GUILayout.EndHorizontal();
                                    break;
                            }
                        }
                        break;
                }

                GUILayout.Space(5);
                GUILayout.BeginVertical(GUI.skin.box);

                GUILayout.Label("Volume Editing", managerWindowLabelCenteredStyle);

                GUILayout.Space(5);

                if (magicLightProbes.calculated && !magicLightProbes.debugMode)
                {
                    if (GUILayout.Button("Go To Quick Editing"))
                    {
                        Selection.activeGameObject = magicLightProbes.quickEditingComponent.gameObject;
                    }
                }

                if (GUILayout.Button("Edit Volume Bounds"))
                {
                    Selection.activeGameObject = magicLightProbes.probesVolume.gameObject;
                }

                GUILayout.EndVertical();
                GUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("Settings are locked until the end of the calculation.", MessageType.Info);
            }            

            if (magicLightProbes.debugMode)
            {
                GUILayout.Label("Total Probes In Volume: In debug mode, counting is not performed", EditorStyles.boldLabel);
            }
            else
            {
                GUILayout.Label("Total Probes In Volume: " + magicLightProbes.totalProbesInVolume, EditorStyles.boldLabel);
            }

            if (magicLightProbes.calculatingVolume)
            {
                if (magicLightProbes.subVolumesDivided.Count > 0)
                {
                    GUILayout.Label("Total Progress", GeneralMethods.captionStyle);
                    EditorGUI.ProgressBar(EditorGUILayout.BeginVertical(), (float)magicLightProbes.currentVolumePart / (float) magicLightProbes.subVolumesDivided.Count, "Volume " + magicLightProbes.currentVolumePart.ToString() + " Of " + magicLightProbes.subVolumesDivided.Count.ToString());
                    GUILayout.Space(20);
                    GUILayout.EndVertical();

                    GUILayout.Label("Current Part Progress", GeneralMethods.captionStyle);
                    EditorGUI.ProgressBar(EditorGUILayout.BeginVertical(), magicLightProbes.totalProgress / 100.0f, Mathf.RoundToInt(magicLightProbes.totalProgress).ToString() + "%");
                    GUILayout.Space(20);
                    GUILayout.EndVertical();
                }
                else
                {
                    if (!magicLightProbes.debugMode)
                    {
                        GUILayout.Label("Total Progress", GeneralMethods.captionStyle);
                        EditorGUI.ProgressBar(EditorGUILayout.BeginVertical(), magicLightProbes.totalProgress / 100.0f, Mathf.RoundToInt(magicLightProbes.totalProgress).ToString() + "%");
                        GUILayout.Space(20);
                        GUILayout.EndVertical();
                    }
                }

                GUILayout.Label("Current Pass Progress", GeneralMethods.captionStyle);
                EditorGUI.ProgressBar(EditorGUILayout.BeginVertical(), magicLightProbes.currentPassProgress / 100.0f, magicLightProbes.currentPass + " - " + Mathf.RoundToInt(magicLightProbes.currentPassProgress).ToString() + "%");
                GUILayout.Space(20);

                GUILayout.EndVertical();
                GUILayout.BeginHorizontal();

                GUILayout.Label("ETA:", EditorStyles.boldLabel, GUILayout.MinWidth(15));

                var ts = TimeSpan.FromSeconds(magicLightProbes.eta);

                GUILayout.Label(string.Format("{0:00} min {1:00} sec", ts.TotalMinutes, ts.Seconds), EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
            }

            if (!magicLightProbes.calculatingVolume)
            {
                if (GUILayout.Button("Calculate Probes Volume"))
                {
                    if (!lockCalculation)
                    {
                        if (magicLightProbes.groundAndFloorKeywords.Count == 0 && !magicLightProbes.useVolumeBottom)
                        {
                            EditorUtility.DisplayDialog("Magic Light Probes", "Before starting the calculation, the parameter \"Ground And Floor Keywords\" must be configured.", "OK");
                            return;
                        }

                        if (MLPManager.saveSceneBeforeCalculation)
                        {
                            int dialogResult = EditorUtility.DisplayDialogComplex(
                            "Magic Light Probes",
                            "Before starting the calculation, it is recommended to save all the scenes.",
                            "OK",
                            "Cancel",
                            "Don't save and don't ask again");

                            if (dialogResult == 0)
                            {
                                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                            }
                            else if (dialogResult == 2)
                            {
                                MLPManager.saveSceneBeforeCalculation = false;
                            }
                        }

                        if (magicLightProbes.debugMode)
                        {
                            magicLightProbes.lightProbesVolumeCalculatingRoutine = magicLightProbes.DebugCalculateProbesVolume();
                        }
                        else
                        {
                            magicLightProbes.lightProbesVolumeCalculatingRoutine = magicLightProbes.CalculateProbesVolume();
                        }

                        EditorApplication.update += magicLightProbes.MainIteratorUpdate;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Magic Light Probes", "It is not possible to calculate in this mode, " +
                            "since light sources are not taken into account by the system. Switch to an \"Advanced\" " +
                            "workflow or choose another debug pass.", "OK");
                    }
                }

                if (GUILayout.Button("Clear Volume"))
                {
                    if (EditorUtility.DisplayDialog("Magic Light Probes", "All data for this volume will be deleted, including operational data to speed up recalculation.", "OK", "Cancel"))
                    {
                        if (magicLightProbes.lightProbesVolumeCalculatingRoutine != null)
                        {
                            EditorApplication.update -= magicLightProbes.SubPassIteratorUpdate;
                            EditorApplication.update -= magicLightProbes.MainIteratorUpdate;
                            magicLightProbes.lightProbesVolumeCalculatingSubRoutine = null;
                            magicLightProbes.lightProbesVolumeCalculatingRoutine = null;
                        }

                        magicLightProbes.calculatingVolume = false;
                        magicLightProbes.ClearScene();
                        magicLightProbes.ResetInternal();
                        magicLightProbes.ClearAllStoredData();
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Stop Calculating"))
                {
                    magicLightProbes.CancelCalculation();
                }

                //if (GUILayout.Button("Next Step"))
                //{
                //    magicLightProbes.nextStep = true;
                //}
            }
        }        

        public static void MLPManagerWindow(
            MLPManager manager,
            List<Light> directionalLights, 
            List<Light> pointLights, 
            List<Light> spotLights, 
            List<Light> areaLights, 
            List<MLPLight> customLights, 
            int selectedTab, 
            List<MagicLightProbes> allGroups)
        {
            foreach (var volume in allGroups)
            {
                if (volume == null)
                {
                    listChanged = true;
                }
            }

            if (allGroups.Count == 0 || listChanged)
            {
                allGroups.Clear();
                allGroups.AddRange(Object.FindObjectsOfType<MagicLightProbes>());
                listChanged = false;
            }

            switch (selectedTab)
            {
                case 0:
                    //GUILayout.Label("Automatic Placement", managerWindowLabelCenteredStyle);

                    //GUILayout.BeginVertical(GUI.skin.box);
                    //GUILayout.BeginHorizontal();

                    //GUILayout.Label("Grouping Threshold (meters)", GUILayout.MinWidth(150));
                    //MLPManager.groupingThreshold = EditorGUILayout.FloatField(MLPManager.groupingThreshold, GUILayout.MaxWidth(250));

                    //GUILayout.EndHorizontal();
                    //GUILayout.BeginHorizontal();
                    //GUILayout.FlexibleSpace();

                    //if (GUILayout.Button("Generate Volumes", GUILayout.Width(150)))
                    //{
                    //    MLPManager.volumesGeneratinoIterator = null;
                    //    MLPManager.volumesGeneratinoIterator = manager.GenerateVolumes();
                    //    EditorApplication.update += manager.VolumesGeneratinoIteratorUpdate;

                    //    manager.GenerateVolumes();
                    //}

                    //GUILayout.EndHorizontal();
                    //GUILayout.EndVertical();

                    GUILayout.Label("Manual Placement", managerWindowLabelCenteredStyle);

                    GUILayout.BeginVertical(GUI.skin.box);
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Add Volume...", GUILayout.Width(150)))
                    {
                        MLPManager.AddVolume(allGroups);
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();

                    if (MLPManager.volumesGenerationIteratorUpdate)
                    {
                        GUILayout.Label("Generation Progress", GeneralMethods.captionStyle);
                        EditorGUI.ProgressBar(EditorGUILayout.BeginVertical(), 
                            (float) MLPManager.currentObject / (float) MLPManager.staticObjects.Count, 
                            "Volumes Generation - " + 
                            Mathf.RoundToInt(((float) MLPManager.currentObject / (float) MLPManager.staticObjects.Count) * 100.0f) + "%");
                        GUILayout.Space(20);
                        GUILayout.EndVertical();
                    }
                    break;
                case 1:
                    GUILayout.BeginVertical(GUI.skin.box);
                    GUILayout.BeginHorizontal();

                    GUILayout.Label("Total Volumes: ");
                    GUILayout.Label(allGroups.Count.ToString(), managerWindowBoldLabelStyle);

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();

                    int totalProbes = 0;

                    for (int i = 0; i < allGroups.Count; i++)
                    {
                        totalProbes += allGroups[i].localFinishedPositions.Count;
                    }

                    GUILayout.Label("Total Probes: ");
                    GUILayout.Label(totalProbes.ToString(), managerWindowBoldLabelStyle);
                    GUILayout.FlexibleSpace();

                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("Calculate All Volumes"))
                    {
                        bool allow = true;
                        
                        for (int i = 0; i < allGroups.Count; i++)
                        {
                            if (!allGroups[i].useVolumeBottom && allGroups[i].groundAndFloorKeywords.Count == 0)
                            {
                                EditorUtility.DisplayDialog("Magic Light Probes", "Before starting the calculation, the parameter \"Ground And Floor Keywords\" of ["+ allGroups[i].name + "] must be configured.", "OK");
                                allow = false;
                                break;                                
                            }
                        }    
                        
                        if (!allow)
                        {
                            return;
                        }

                        if (EditorUtility.DisplayDialog("Magic Light Probes", "Before starting the calculation, it is recommended to save all the scenes.", "OK"))
                        {
                            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                        }

                        Selection.activeObject = GameObject.FindObjectOfType<MLPCombinedVolume>();
                        
                        for (int i = 0; i < allGroups.Count; i++)
                        {
                            if (i == 0)
                            {
                                if (allGroups[i].debugMode)
                                {
                                    allGroups[i].lightProbesVolumeCalculatingRoutine = allGroups[i].DebugCalculateProbesVolume();
                                }
                                else
                                {
                                    allGroups[i].lightProbesVolumeCalculatingRoutine = allGroups[i].CalculateProbesVolume();
                                }
                            }
                            else
                            {
                                allGroups[i].waitForPrevious = true;
                                allGroups[i].previousVolume = allGroups[i - 1];

                                if (allGroups[i].debugMode)
                                {                                    
                                    allGroups[i].lightProbesVolumeCalculatingRoutine = allGroups[i].DebugCalculateProbesVolume();
                                }
                                else
                                {
                                    allGroups[i].lightProbesVolumeCalculatingRoutine = allGroups[i].CalculateProbesVolume();
                                }
                            }                           

                            EditorApplication.update += allGroups[i].MainIteratorUpdate;
                        }
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();

                    GUILayout.Space(15);

                    volumesScrollPos = EditorGUILayout.BeginScrollView(volumesScrollPos);

                    GUILayout.BeginVertical(GUI.skin.box);
                    GUILayout.BeginHorizontal();

                    GUILayout.Label("Volumes List", managerWindowCaptionStyle);

                    GUILayout.EndHorizontal();

                    for (int i = 0; i < allGroups.Count; i++)
                    {
                        if (allGroups[i] != null)
                        {
                            if (i > 0)
                            {
                                GUILayout.Space(5);
                            }

                            GUILayout.BeginVertical(GUI.skin.box);
                            GUILayout.BeginHorizontal();

                            if (allGroups[i].debugMode)
                            {
                                GUILayout.Label("DEBUG MODE", managerWindowWarningLabelStyle);
                            }

                            if (allGroups[i].calculated)
                            {
                                GUILayout.Label("CALCULATED", managerWindowCalculatedLabelStyle);
                            }

                            if (allGroups[i].calculatingVolume)
                            {
                                GUILayout.BeginVertical();
                                GUILayout.BeginHorizontal();

                                GUILayout.FlexibleSpace();
                                GUILayout.Label("CALCULATING", managerWindowProcessLabelStyle);
                                GUILayout.FlexibleSpace();

                                if (GUILayout.Button("Cancel", managerWindowTopPanelButtonStyle))
                                {
                                    if (allGroups[i].lightProbesVolumeCalculatingRoutine != null)
                                    {
                                        EditorApplication.update -= allGroups[i].SubPassIteratorUpdate;
                                        //EditorApplication.update -= allGroups[i].MainIteratorUpdate;
                                        allGroups[i].lightProbesVolumeCalculatingSubRoutine = null;
                                        allGroups[i].lightProbesVolumeCalculatingRoutine = null;
                                    }

                                    allGroups[i].restored = false;
                                    allGroups[i].calculatingVolume = false;
                                }

                                GUILayout.EndHorizontal();
                                GUILayout.Space(5);

                                if (!allGroups[i].waitForPrevious)
                                {
                                    if (allGroups[i].subVolumesDivided.Count > 0)
                                    {
                                        GUILayout.Label("Total Progress", GeneralMethods.captionStyle);
                                        EditorGUI.ProgressBar(EditorGUILayout.BeginVertical(), (float) allGroups[i].currentVolumePart / (float) allGroups[i].subVolumesDivided.Count, "Volume " + allGroups[i].currentVolumePart.ToString() + " Of " + allGroups[i].subVolumesDivided.Count.ToString());
                                        GUILayout.Space(20);
                                        GUILayout.EndVertical();

                                        GUILayout.Label("Current Part Progress", GeneralMethods.captionStyle);
                                        EditorGUI.ProgressBar(EditorGUILayout.BeginVertical(), allGroups[i].totalProgress / 100.0f, Mathf.RoundToInt(allGroups[i].totalProgress).ToString() + "%");
                                        GUILayout.Space(20);
                                        GUILayout.EndVertical();
                                    }
                                    else
                                    {
                                        GUILayout.Label("Total Progress", GeneralMethods.captionStyle);
                                        EditorGUI.ProgressBar(EditorGUILayout.BeginVertical(), allGroups[i].totalProgress / 100.0f, Mathf.RoundToInt(allGroups[i].totalProgress).ToString() + "%");
                                        GUILayout.Space(20);
                                        GUILayout.EndVertical();
                                    }

                                    GUILayout.Label("Current Pass Progress", GeneralMethods.captionStyle);
                                    EditorGUI.ProgressBar(EditorGUILayout.BeginVertical(), allGroups[i].currentPassProgress / 100.0f, allGroups[i].currentPass + " - " + Mathf.RoundToInt(allGroups[i].currentPassProgress).ToString() + "%");
                                    GUILayout.Space(20);

                                    GUILayout.EndVertical();
                                }

                                GUILayout.EndVertical();
                            }
                            else
                            {
                                GUILayout.FlexibleSpace();
                                GUILayout.Label(allGroups[i].gameObject.name, managerWindowCaptionStyle);
                                GUILayout.FlexibleSpace();

                                if (GUILayout.Button("Remove", managerWindowTopPanelButtonStyle))
                                {
                                    if (EditorUtility.DisplayDialog("Remove Light Probe Group", "Delete a " + allGroups[i].gameObject.name + " from the manager and from the scene without the possibility of recovery?", "OK", "Cancel"))
                                    {
                                        allGroups[i].ClearAllStoredData();
                                        Object.DestroyImmediate(allGroups[i].gameObject);
                                        listChanged = true;
                                        return;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }

                                if (GUILayout.Button("Select On Scene", managerWindowTopPanelButtonStyle))
                                {
                                    Selection.activeGameObject = allGroups[i].gameObject;
                                }
                            }

                            GUILayout.EndHorizontal();
                            GUILayout.Space(5);

                            allGroups[i].showOptionsInManagerWindow = EditorGUILayout.Foldout(allGroups[i].showOptionsInManagerWindow, allGroups[i].name + " Options", true, EditorStyles.foldout);

                            if (allGroups[i].showOptionsInManagerWindow)
                            {
                                MLPMainComponentEditor(allGroups[i]);
                            }
                        }
                        else
                        {
                            listChanged = true;
                        }

                        GUILayout.EndVertical();
                    }

                    GUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();

                    VolumesScrollArea = GUILayoutUtility.GetLastRect();
                    break;
                case 2:
                    lightsScrollPos = EditorGUILayout.BeginScrollView(lightsScrollPos);
                    GUILayout.BeginVertical(GUI.skin.box);

                    DrawLightsGroup(directionalLights, "Directional Lights");
                    DrawLightsGroup(pointLights, "Point Lights");
                    DrawLightsGroup(spotLights, "Spot Lights");
                    DrawLightsGroup(areaLights, "Area Lights");
                    DrawLightsGroup(customLights, "Custom Lights");

                    GUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                    LightsScrollArea = GUILayoutUtility.GetLastRect();
                    break;
                case 3:
                    GUILayout.Label("These settings will be changed for all volumes on the scene", managerWindowLabelCenteredStyle);
                    GUILayout.Space(20);

                    GUILayout.BeginVertical(GUI.skin.box);
                    GUILayout.BeginHorizontal();

                    GUILayout.Label("Global Bounds Display Mode", GUILayout.MinWidth(150));
                    globalBoundsDisplayMode = (MagicLightProbes.BoundsDisplayMode)EditorGUILayout.EnumPopup(globalBoundsDisplayMode, GUILayout.MaxWidth(250));

                    if (lastGlobalBoundsDisplayMode != globalBoundsDisplayMode)
                    {
                        //if (defaultGlobal.Contains())

                        for (int i = 0; i < allGroups.Count; i++)
                        {
                            allGroups[i].boundsDisplayMode = globalBoundsDisplayMode;
                        }

                        lastGlobalBoundsDisplayMode = globalBoundsDisplayMode;
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();

                    GUILayout.Label("Save Scene Before Calculation", GUILayout.MinWidth(150));
                    MLPManager.saveSceneBeforeCalculation = EditorGUILayout.Toggle(MLPManager.saveSceneBeforeCalculation, GUILayout.MaxWidth(250));                    

                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    break;
            }
        }

        public static void MLPCombinedVolumeEditor (MLPCombinedVolume target)
        {
            if (!stylesInitialized)
            {
                InitStyles();
            }

            List<MagicLightProbes> volumes = new List<MagicLightProbes>();
            volumes.AddRange(Object.FindObjectsOfType<MagicLightProbes>());

            int totalProbes = 0;

            foreach (var volume in volumes)
            {
                totalProbes += volume.localFinishedPositions.Count;
            }

            EditorGUILayout.HelpBox("This script automatically combines all calculated volumes. Do not remove it from the scene.", MessageType.Info);

            GUILayout.BeginVertical(GUI.skin.box);           
            GUILayout.BeginHorizontal();

            target.pressed = GUILayout.Toggle(target.pressed, "Set Custom Probes", "Button"); 
            
            if (GUILayout.Button("Clear Custom Probes"))
            {
                target.customProbesToRemove.AddRange(target.customPositions);
                target.customPositions.Clear();
                target.combined = false;
            }

            GUILayout.EndHorizontal();

            if (target.pressed)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.BeginHorizontal();

                GUILayout.Label("Distance From Geometry: ", managerWindowBoldLabelStyle, GUILayout.Width(250));
                target.distanceFromGeometry = EditorGUILayout.FloatField(target.distanceFromGeometry, GUILayout.Width(150));

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }

            //if (GUILayout.Button("Render Probes"))
            //{
            //    Lightmapping.BakeLightProbesOnlyAsync();
            //}

            GUILayout.BeginHorizontal();

            GUILayout.Label("Total Volumes On Scene: ", managerWindowBoldLabelStyle, GUILayout.Width(250));
            GUILayout.Label(volumes.Count.ToString(), managerWindowBoldLabelStyle);            

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.Label("Total Probes On Scene: ", managerWindowBoldLabelStyle, GUILayout.Width(250));
            GUILayout.Label(target.GetComponent<LightProbeGroup>().probePositions.Length.ToString(), managerWindowBoldLabelStyle);

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            if (target.warningShow)
            {
                GUILayout.BeginVertical(GUI.skin.box);
            
                EditorGUILayout.HelpBox("This object is needed for the plugin to work properly", MessageType.Error); 
                
                GUILayout.EndVertical();
            }
        }

        private static void DrawLightsGroup (List<Light> inputList, string label)
        {
            if (inputList.Count > 0)
            {
                GUILayout.Label(label, managerWindowLabelCenteredStyle);

                List<Light> realtimeLights = new List<Light>();
                List<Light> bakedLights = new List<Light>();
                List<Light> mixedLights = new List<Light>();

                for (int i = 0; i < inputList.Count; i++)
                {
                    if (inputList[i].lightmapBakeType == LightmapBakeType.Realtime)
                    {
                        realtimeLights.Add(inputList[i]);
                    }

                    if (inputList[i].lightmapBakeType == LightmapBakeType.Baked)
                    {
                        bakedLights.Add(inputList[i]);
                    }

                    if (inputList[i].lightmapBakeType == LightmapBakeType.Mixed)
                    {
                        mixedLights.Add(inputList[i]);
                    }
                }

                if (realtimeLights.Count > 0)
                {
                    DrawSortedLightsGroup(realtimeLights, "Realtime Mode");
                }

                if (bakedLights.Count > 0)
                {
                    DrawSortedLightsGroup(bakedLights, "Baked Mode");
                }

                if (mixedLights.Count > 0)
                {
                    DrawSortedLightsGroup(mixedLights, "Mixed Mode");
                }
            }
        }

        private static void DrawLightsGroup(List<MLPLight> inputList, string label)
        {
            if (inputList.Count > 0)
            {
                GUILayout.Label(label, managerWindowLabelCenteredStyle);

                List<MLPLight> realtimeLights = new List<MLPLight>();
                List<MLPLight> bakedLights = new List<MLPLight>();
                List<MLPLight> mixedLights = new List<MLPLight>();

                for (int i = 0; i < inputList.Count; i++)
                {
                    if (inputList[i].lightMode == LightmapBakeType.Realtime)
                    {
                        realtimeLights.Add(inputList[i]);
                    }

                    if (inputList[i].lightMode == LightmapBakeType.Baked)
                    {
                        bakedLights.Add(inputList[i]);
                    }

                    if (inputList[i].lightMode == LightmapBakeType.Mixed)
                    {
                        mixedLights.Add(inputList[i]);
                    }
                }

                if (realtimeLights.Count > 0)
                {
                    DrawSortedLightsGroup(realtimeLights, "Realtime Mode");
                }

                if (bakedLights.Count > 0)
                {
                    DrawSortedLightsGroup(bakedLights, "Baked Mode");
                }

                if (mixedLights.Count > 0)
                {
                    DrawSortedLightsGroup(mixedLights, "Mixed Mode");
                }
            }
        }

        private static void DrawSortedLightsGroup(List<Light> inputList, string label)
        {
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Label(label, managerWindowLabelCenteredStyle);

            for (int i = 0; i < inputList.Count; i++)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label(inputList[i].name + ": ");

                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();

                inputList[i] = EditorGUILayout.ObjectField(inputList[i], typeof(Light), true, GUILayout.MaxWidth(300)) as Light;
                GUILayout.FlexibleSpace();

                MLPLight mlpLight = inputList[i].gameObject.GetComponent<MLPLight>();

                if (mlpLight == null)
                {
                    if (GUILayout.Button("Add \"MLP Light\" Component"))
                    {
                        inputList[i].gameObject.AddComponent<MLPLight>();
                    }

                    GUILayout.EndHorizontal();
                }
                else
                {
                    if (GUILayout.Button("Remove \"MLP Light\" Component"))
                    {
                        Object.DestroyImmediate(inputList[i].gameObject.GetComponent<MLPLight>());
                        return;
                    }

                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
        }

        private static void DrawSortedLightsGroup(List<MLPLight> inputList, string label)
        {
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Label(label, managerWindowLabelCenteredStyle);

            for (int i = 0; i < inputList.Count; i++)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label(inputList[i].name + ": ");

                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();

                inputList[i] = EditorGUILayout.ObjectField(inputList[i], typeof(MLPLight), true, GUILayout.MaxWidth(300)) as MLPLight;
                GUILayout.FlexibleSpace();

                MLPLight mlpLight = inputList[i].gameObject.GetComponent<MLPLight>();

                if (mlpLight == null)
                {
                    if (GUILayout.Button("Add \"MLP Light\" Component"))
                    {
                        inputList[i].gameObject.AddComponent<MLPLight>();
                    }

                    GUILayout.EndHorizontal();
                }
                else
                {
                    if (GUILayout.Button("Remove \"MLP Light\" Component"))
                    {
                        Object.DestroyImmediate(inputList[i].gameObject.GetComponent<MLPLight>());
                        return;
                    }

                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
        }
    }
}
#endif
