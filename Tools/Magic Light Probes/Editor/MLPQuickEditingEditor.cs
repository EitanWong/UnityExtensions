#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MagicLightProbes
{
    [CustomEditor(typeof(MLPQuickEditing))]
    public class MLPQuickEditingEditor : Editor
    {
        private enum OptionToCheck
        {
            EquivalentFill,
            UnlitFill,
            FreeFill
        }

        public float storedEquivalentVolumeFillRate;
        public float storedUnlitVolumeFillRate;
        public float storedFreeVolumeFillRate;
        public Vector2 colorThresholdRange = new Vector2(0, 1);
        public float lastGizmoScale;
        public float lastDrawDistance;

        public override void OnInspectorGUI()
        {
            MLPQuickEditing quickEditing = (MLPQuickEditing)target;

            EventType currentEvent = Event.current.type;            

            if (quickEditing.parent.calculated)
            {
                EditorGUI.BeginChangeCheck();

                if (quickEditing.parent.workflow == MagicLightProbes.Workflow.Simple)
                {
                    quickEditing.parent.fillingMode = MagicLightProbes.FillingMode.SeparateFilling;
                }
                switch (quickEditing.parent.fillingMode)
                {
                    case MagicLightProbes.FillingMode.SeparateFilling:
                        GeneralMethods.InitStyles();
                        GUILayout.BeginVertical(GUI.skin.box);
                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Gizmo Scale", GUILayout.MinWidth(200));
                        quickEditing.gizmoScale = EditorGUILayout.Slider(quickEditing.gizmoScale, 0.01f, 10, GUILayout.MaxWidth(250));

                        if (lastGizmoScale != quickEditing.gizmoScale)
                        {
                            lastGizmoScale = quickEditing.gizmoScale;
                            SceneView.RepaintAll();
                        }

                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Draw Distance", GUILayout.MinWidth(200));
                        quickEditing.drawDistance = EditorGUILayout.Slider(quickEditing.drawDistance, 0.01f, 50, GUILayout.MaxWidth(250));

                        if (lastDrawDistance != quickEditing.drawDistance)
                        {
                            lastDrawDistance = quickEditing.drawDistance;
                            SceneView.RepaintAll();
                        }

                        GUILayout.EndHorizontal();

                        GUILayout.BeginVertical(GUI.skin.box);

                        GUILayout.Label("Color Threshold", GeneralMethods.captionStyle);

                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Range", GUILayout.MinWidth(200));

                        colorThresholdRange.x = EditorGUILayout.FloatField(colorThresholdRange.x, GUILayout.MaxWidth(125));
                        colorThresholdRange.y = EditorGUILayout.FloatField(colorThresholdRange.y, GUILayout.MaxWidth(125));

                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Threshold", GUILayout.MinWidth(200));
                        quickEditing.parent.colorTreshold = EditorGUILayout.Slider(quickEditing.parent.colorTreshold, colorThresholdRange.x, colorThresholdRange.y, GUILayout.MaxWidth(250));

                        if (currentEvent == EventType.MouseUp)
                        {
                            StartRecalulateColorThreshold(quickEditing);
                        }

                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                        GUILayout.Space(5);
                        GUILayout.BeginVertical(GUI.skin.box);

                        GUILayout.Label("Spacing In Corners", GeneralMethods.captionStyle);

                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Spacing", GUILayout.MinWidth(200));

                        float currentVolumeSpacing = 0;
                        float cornersDetectionThreshold = 0;

                        if (quickEditing.parent.useDynamicDensity)
                        {
                            currentVolumeSpacing = quickEditing.parent.volumeSpacingMax;
                            cornersDetectionThreshold = quickEditing.parent.cornersDetectionThresholdMax;
                        }
                        else
                        {
                            currentVolumeSpacing = quickEditing.parent.volumeSpacing;
                            cornersDetectionThreshold = quickEditing.parent.cornersDetectionThreshold;
                        }

                        EditorGUI.BeginChangeCheck();

                        quickEditing.parent.cornerProbesSpacing = EditorGUILayout.Slider(quickEditing.parent.cornerProbesSpacing, cornersDetectionThreshold, currentVolumeSpacing * 2, GUILayout.MaxWidth(250));
                                                
                        if (currentEvent == EventType.MouseUp)
                        {
                            StartRecalculateCornerProbeSpacing(quickEditing);
                        }
                        else if (currentEvent ==  EventType.KeyDown && EditorGUI.EndChangeCheck())
                        {
                            StartRecalculateCornerProbeSpacing(quickEditing);
                        }

                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();

                        if (quickEditing.parent.workflow == MagicLightProbes.Workflow.Advanced)
                        {
                            GUILayout.Space(5);
                            GUILayout.BeginVertical(GUI.skin.box);

                            GUILayout.Label("Equivalent Volume Filling", GeneralMethods.captionStyle);

                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Fill Equivalent Volume", GUILayout.MinWidth(200));
                            quickEditing.parent.fillEquivalentVolume = EditorGUILayout.Toggle(quickEditing.parent.fillEquivalentVolume, GUILayout.MaxWidth(250));

                            GUILayout.EndHorizontal();

                            if (quickEditing.parent.fillEquivalentVolume)
                            {
                                bool available = CheckIfOptionAvailable(quickEditing, OptionToCheck.EquivalentFill);

                                GUILayout.BeginVertical(GUI.skin.box);

                                if (available)
                                {
                                    GUILayout.BeginHorizontal();

                                    GUILayout.Label("Filling Rate", GUILayout.MinWidth(200));

                                    EditorGUI.BeginChangeCheck();

                                    quickEditing.parent.equivalentVolumeFillingRate = EditorGUILayout.Slider(quickEditing.parent.equivalentVolumeFillingRate, 0f, 1.0f, GUILayout.MaxWidth(250));

                                    if (currentEvent == EventType.MouseUp)
                                    {
                                        StartRecalulateEquivalentFillingRate(quickEditing);
                                    }
                                    else if (currentEvent == EventType.KeyDown && EditorGUI.EndChangeCheck())
                                    {
                                        StartRecalulateEquivalentFillingRate(quickEditing);
                                    }
                                    else
                                    {
                                        if (storedEquivalentVolumeFillRate != 0)
                                        {
                                            quickEditing.parent.equivalentVolumeFillingRate = storedEquivalentVolumeFillRate;
                                            storedEquivalentVolumeFillRate = 0;
                                            StartRecalulateEquivalentFillingRate(quickEditing);
                                        }
                                    }

                                    GUILayout.EndHorizontal();
                                }
                                else
                                {
                                    EditorGUILayout.HelpBox("This volume does not contain probes illuminated directly. Editing this option is not available.", MessageType.Warning);
                                }

                                GUILayout.EndVertical();

                            }
                            else
                            {
                                if (storedEquivalentVolumeFillRate == 0)
                                {
                                    storedEquivalentVolumeFillRate = quickEditing.parent.equivalentVolumeFillingRate;
                                }

                                quickEditing.parent.equivalentVolumeFillingRate = 0;
                                StartRecalulateEquivalentFillingRate(quickEditing);
                            }

                            GUILayout.EndVertical();
                            GUILayout.Space(5);
                            GUILayout.BeginVertical(GUI.skin.box);

                            GUILayout.Label("Unlit Volume Filling", GeneralMethods.captionStyle);

                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Fill Unlit Volume", GUILayout.MinWidth(200));
                            quickEditing.parent.fillUnlitVolume = EditorGUILayout.Toggle(quickEditing.parent.fillUnlitVolume, GUILayout.MaxWidth(250));

                            GUILayout.EndHorizontal();

                            if (quickEditing.parent.fillUnlitVolume)
                            {
                                bool available = CheckIfOptionAvailable(quickEditing, OptionToCheck.UnlitFill);

                                GUILayout.BeginVertical(GUI.skin.box);

                                if (available)
                                {
                                    GUILayout.BeginHorizontal();

                                    GUILayout.Label("Filling Rate", GUILayout.MinWidth(200));

                                    EditorGUI.BeginChangeCheck();

                                    quickEditing.parent.unlitVolumeFillingRate = EditorGUILayout.Slider(quickEditing.parent.unlitVolumeFillingRate, 0f, 1.0f, GUILayout.MaxWidth(250));

                                    if (currentEvent == EventType.MouseUp)
                                    {
                                        StartRecalulateUnlitFillingRate(quickEditing);
                                    }
                                    else if (currentEvent == EventType.KeyDown && EditorGUI.EndChangeCheck())
                                    {
                                        StartRecalulateUnlitFillingRate(quickEditing);
                                    }
                                    else
                                    {
                                        if (storedUnlitVolumeFillRate != 0)
                                        {
                                            quickEditing.parent.unlitVolumeFillingRate = storedUnlitVolumeFillRate;
                                            storedUnlitVolumeFillRate = 0;
                                            StartRecalulateUnlitFillingRate(quickEditing);
                                        }
                                    }

                                    GUILayout.EndHorizontal();
                                }
                                else
                                {
                                    EditorGUILayout.HelpBox("This volume does not contain unlit probes. Editing this option is not available.", MessageType.Warning);
                                }

                                GUILayout.EndVertical();
                            }
                            else
                            {
                                if (storedUnlitVolumeFillRate == 0)
                                {
                                    storedUnlitVolumeFillRate = quickEditing.parent.unlitVolumeFillingRate;
                                }

                                quickEditing.parent.unlitVolumeFillingRate = 0;
                                StartRecalulateUnlitFillingRate(quickEditing);
                            }

                            //GUILayout.BeginHorizontal();

                            //GUILayout.Label("Distance From Geometry", GUILayout.MinWidth(200));

                            //quickEditing.parent.distanceFromNearbyGeometry = EditorGUILayout.Slider(quickEditing.parent.distanceFromNearbyGeometry, 0.1f, 1.0f, GUILayout.MaxWidth(250));

                            //if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                            //{
                            //    if (lastDistanceFromGeometry != quickEditing.parent.distanceFromNearbyGeometry)
                            //    {
                            //    lastDistanceFromGeometry = quickEditing.parent.distanceFromNearbyGeometry;

                            //        quickEditing.parent.RecalculateDistanceFromGeometry();
                            //    }
                            //}

                            //GUILayout.EndHorizontal();
                            //GUILayout.EndVertical();
                        }
                        else
                        {
                            GUILayout.Space(5);
                            GUILayout.BeginVertical(GUI.skin.box);

                            GUILayout.Label("Free Volume Filling", GeneralMethods.captionStyle);

                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Fill Free Volume", GUILayout.MinWidth(200));
                            quickEditing.parent.fillFreeVolume = EditorGUILayout.Toggle(quickEditing.parent.fillFreeVolume, GUILayout.MaxWidth(250));

                            GUILayout.EndHorizontal();

                            if (quickEditing.parent.fillFreeVolume)
                            {
                                bool available = CheckIfOptionAvailable(quickEditing, OptionToCheck.FreeFill);

                                GUILayout.BeginVertical(GUI.skin.box);

                                if (available)
                                {
                                    GUILayout.BeginHorizontal();

                                    GUILayout.Label("Filling Rate", GUILayout.MinWidth(200));

                                    EditorGUI.BeginChangeCheck();

                                    quickEditing.parent.freeVolumeFillingRate = EditorGUILayout.Slider(quickEditing.parent.freeVolumeFillingRate, 0f, 1.0f, GUILayout.MaxWidth(250));

                                    if (currentEvent == EventType.MouseUp)
                                    {
                                        StartRecalulateFreeFillingRate(quickEditing);
                                    }
                                    else if (currentEvent == EventType.KeyDown && EditorGUI.EndChangeCheck())
                                    {
                                        StartRecalulateFreeFillingRate(quickEditing);
                                    }
                                    else
                                    {
                                        if (storedFreeVolumeFillRate != 0)
                                        {
                                            quickEditing.parent.freeVolumeFillingRate = storedFreeVolumeFillRate;
                                            storedFreeVolumeFillRate = 0;
                                            StartRecalulateFreeFillingRate(quickEditing);
                                        }
                                    }

                                    GUILayout.EndHorizontal();
                                }
                                else
                                {
                                    EditorGUILayout.HelpBox("This volume does not contain free probes. Editing this option is not available.", MessageType.Warning);
                                }

                                GUILayout.EndVertical();
                            }
                            else
                            {
                                if (storedFreeVolumeFillRate == 0)
                                {
                                    storedFreeVolumeFillRate = quickEditing.parent.freeVolumeFillingRate;
                                }

                                quickEditing.parent.freeVolumeFillingRate = 0;
                                StartRecalulateFreeFillingRate(quickEditing);
                            }
                        }

                        GUILayout.EndVertical();

                        GUILayout.Label("Changing Progress", GeneralMethods.captionStyle);

                        if (!quickEditing.parent.realtimeEditing)
                        {
                            EditorGUI.ProgressBar(EditorGUILayout.BeginVertical(), 100.0f, "No Tasks");
                        }
                        else
                        {
                            EditorGUI.ProgressBar(EditorGUILayout.BeginVertical(), quickEditing.parent.currentPassProgress / 100.0f, quickEditing.parent.currentPass + " - " + Mathf.RoundToInt(quickEditing.parent.currentPassProgress).ToString() + "%");
                        }

                        GUILayout.Space(20);
                        GUILayout.EndVertical();

                        EditorGUILayout.HelpBox("After changing the parameters, it is necessary to recombine the volumes. To do this, click on the component \"MLP Combined Volume\".", MessageType.Warning);

                        GUILayout.EndVertical();
                        break;
                    default:
                        EditorGUILayout.HelpBox("This feature is under development.", MessageType.Info);
                        break;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Options will become available after calculating the volume.", MessageType.Info);
            }
        }

        private void StartRecalculateCornerProbeSpacing(MLPQuickEditing quickEditing)
        {
            if (quickEditing.parent.lastCornerProbesSpacing != quickEditing.parent.cornerProbesSpacing)
            {
                quickEditing.parent.lastCornerProbesSpacing = quickEditing.parent.cornerProbesSpacing;
                quickEditing.parent.RecalculateCornerProbeSpacing();
            }
        }

        private void StartRecalulateColorThreshold(MLPQuickEditing quickEditing)
        {
            if (quickEditing.parent.lastColorThreshold != quickEditing.parent.colorTreshold)
            {
                quickEditing.parent.lastColorThreshold = quickEditing.parent.colorTreshold;
                quickEditing.parent.RecalculateColorThereshold();
            }
        }

        private void StartRecalulateEquivalentFillingRate (MLPQuickEditing quickEditing)
        {
            if (quickEditing.parent.lastEquivalentVolumeFillingRate != quickEditing.parent.equivalentVolumeFillingRate)
            {
                quickEditing.parent.lastEquivalentVolumeFillingRate = quickEditing.parent.equivalentVolumeFillingRate;
                quickEditing.parent.RecalculateEuivalentVolumeFilling();
            }
        }

        private void StartRecalulateUnlitFillingRate(MLPQuickEditing quickEditing)
        {
            if (quickEditing.parent.lastUnlitVolumeFillingRate != quickEditing.parent.unlitVolumeFillingRate)
            {
                quickEditing.parent.lastUnlitVolumeFillingRate = quickEditing.parent.unlitVolumeFillingRate;
                quickEditing.parent.RecalculateUnlitVolumeFilling();
            }
        }

        private void StartRecalulateFreeFillingRate(MLPQuickEditing quickEditing)
        {
            if (quickEditing.parent.lastFreeVolumeFillingRate != quickEditing.parent.freeVolumeFillingRate)
            {
                quickEditing.parent.lastFreeVolumeFillingRate = quickEditing.parent.freeVolumeFillingRate;
                quickEditing.parent.RecalculateFreeVolumeFilling();
            }
        }

        private bool CheckIfOptionAvailable (MLPQuickEditing quickEditing, OptionToCheck optionToCheck)
        {
            bool available = false;

            switch (optionToCheck)
            {
                case OptionToCheck.EquivalentFill:
                    if (quickEditing.parent.subVolumesDivided.Count > 0)
                    {
                        for (int i = 0; i < quickEditing.parent.subVolumesDivided.Count; i++)
                        {
                            if (quickEditing.parent.subVolumesDivided[i].GetComponent<MLPVolume>().localEquivalentPointsPositions.Count > 0)
                            {
                                available = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (quickEditing.parent.probesVolume.GetComponent<MLPVolume>().localEquivalentPointsPositions.Count > 0)
                        {
                            available = true;
                        }
                    }
                    break;
                case OptionToCheck.UnlitFill:
                    if (quickEditing.parent.subVolumesDivided.Count > 0)
                    {
                        for (int i = 0; i < quickEditing.parent.subVolumesDivided.Count; i++)
                        {
                            if (quickEditing.parent.subVolumesDivided[i].GetComponent<MLPVolume>().localUnlitPointsPositions.Count > 0)
                            {
                                available = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (quickEditing.parent.probesVolume.GetComponent<MLPVolume>().localUnlitPointsPositions.Count > 0)
                        {
                            available = true;
                        }
                    }
                    break;
                case OptionToCheck.FreeFill:
                    if (quickEditing.parent.subVolumesDivided.Count > 0)
                    {
                        for (int i = 0; i < quickEditing.parent.subVolumesDivided.Count; i++)
                        {
                            if (quickEditing.parent.subVolumesDivided[i].GetComponent<MLPVolume>().localFreePointsPositions.Count > 0)
                            {
                                available = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (quickEditing.parent.probesVolume.GetComponent<MLPVolume>().localFreePointsPositions.Count > 0)
                        {
                            available = true;
                        }
                    }
                    break;
            }            

            return available;
        }
    }
}
#endif