using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

namespace MagicLightProbes
{
    [CustomEditor(typeof(MLPLight)), CanEditMultipleObjects]
    public class MLPLightEditor : Editor
    {
        public SerializedProperty parentVolume;
        public SerializedProperty targetLight;
        public SerializedProperty lightMode;
        public SerializedProperty shadowmaskMode;
        public SerializedProperty lightType;
        public SerializedProperty lightTypeMA;
        public SerializedProperty lastLightType;
        public SerializedProperty accuracy;
        public SerializedProperty lastAccuracy;
        public SerializedProperty calculationMode;
        public SerializedProperty intensity;
        public SerializedProperty useSourceParameters;
        public SerializedProperty range;
        public SerializedProperty saveNearbyProbes;
        public SerializedProperty saveRadius;
        public SerializedProperty isDirectional;
        public SerializedProperty angle;
        public SerializedProperty reverseDirection;
        public SerializedProperty position;
        public SerializedProperty forward;
        public SerializedProperty parentGameObject;
        public SerializedProperty accurateTrace;
        public SerializedProperty tracePointSettingType;
        public SerializedProperty tracePointSize;
        public SerializedProperty lastTracePointSize;
        public SerializedProperty mainTracePoint;
        public SerializedProperty tracePoints;
        public SerializedProperty customTracePoints;
        public SerializedProperty tracePointsData;
        public SerializedProperty resetEditor;
        public SerializedProperty showLightOnScene;
        public SerializedProperty lastMesh;
        public SerializedProperty saveOnOutOfRange;
        public SerializedProperty isHDRP;
        public SerializedProperty hdrpAreaSize;

        bool bakeryMeshIsNull = true;
        bool initialized;

        private void InitSerializedProperties()
        {
            parentVolume = serializedObject.FindProperty("parentVolume");
            targetLight = serializedObject.FindProperty("targetLight");
            lightMode = serializedObject.FindProperty("lightMode");
            shadowmaskMode = serializedObject.FindProperty("shadowmaskMode");
            lightType = serializedObject.FindProperty("lightType");
            lightTypeMA = serializedObject.FindProperty("lightTypeMA");
            lastLightType = serializedObject.FindProperty("lastLightType");
            accuracy = serializedObject.FindProperty("accuracy");
            lastAccuracy = serializedObject.FindProperty("lastAccuracy");
            calculationMode = serializedObject.FindProperty("calculationMode");
            intensity = serializedObject.FindProperty("intensity");
            useSourceParameters = serializedObject.FindProperty("useSourceParameters");
            range = serializedObject.FindProperty("range");
            saveNearbyProbes = serializedObject.FindProperty("saveNearbyProbes");
            saveRadius = serializedObject.FindProperty("saveRadius");
            isDirectional = serializedObject.FindProperty("isDirectional");
            angle = serializedObject.FindProperty("angle");
            reverseDirection = serializedObject.FindProperty("reverseDirection");
            position = serializedObject.FindProperty("position");
            forward = serializedObject.FindProperty("forward");
            parentGameObject = serializedObject.FindProperty("parentGameObject");
            accurateTrace = serializedObject.FindProperty("accurateTrace");
            tracePointSettingType = serializedObject.FindProperty("tracePointSettingType");
            tracePointSize = serializedObject.FindProperty("tracePointSize");
            lastTracePointSize = serializedObject.FindProperty("lastTracePointSize");
            mainTracePoint = serializedObject.FindProperty("mainTracePoint");
            tracePoints = serializedObject.FindProperty("tracePoints");
            tracePointsData = serializedObject.FindProperty("tracePointsData");
            customTracePoints = serializedObject.FindProperty("customTracePoints");
            resetEditor = serializedObject.FindProperty("resetEditor");
            showLightOnScene = serializedObject.FindProperty("showLightOnScene");
            lastMesh = serializedObject.FindProperty("lastMesh");
            saveOnOutOfRange = serializedObject.FindProperty("saveOnOutOfRange");
            isHDRP = serializedObject.FindProperty("isHDRP");
            hdrpAreaSize = serializedObject.FindProperty("hdrpAreaSize");

            initialized = true;
        }

        private void OnEnable()
        {
            InitSerializedProperties();
        }

#if BAKERYPACKAGE_EXIST
        bool IsLightCompletelyBaked(bool bakeToIndirect, ftRenderLightmap.RenderMode rmode)
        {
            bool isBaked = ((rmode == ftRenderLightmap.RenderMode.FullLighting) ||
                            (rmode == ftRenderLightmap.RenderMode.Indirect && bakeToIndirect) ||
                            (rmode == ftRenderLightmap.RenderMode.Shadowmask && bakeToIndirect));
            return isBaked;
        }
#endif

        void SetParentVolume(List<MagicLightProbes> checkList)
        {
            if (lightType.enumValueIndex == (int) MLPLight.MLPLightType.Directional)
            {
                parentVolume.objectReferenceValue = checkList[0];
                (parentGameObject.objectReferenceValue as GameObject).layer = checkList[0].firstCollisionLayer;
            }
            else
            {
                bool stop = false;

                foreach (var volume in checkList)
                {
                    if (stop)
                    {
                        break;
                    }

                    if (volume.gameObject.activeInHierarchy && volume.probesVolume.GetComponent<MeshRenderer>().bounds.Contains(position.vector3Value))
                    {
                        if (volume.innerVolumesObjects.Count > 0)
                        {
                            for (int i = 0; i < volume.innerVolumesObjects.Count; i++)
                            {
                                if (stop)
                                {
                                    break;
                                }

                                if (volume.innerVolumesObjects[i].gameObject.activeInHierarchy && volume.innerVolumesObjects[i].probesVolume.GetComponent<MeshRenderer>().bounds.Contains(position.vector3Value))
                                {
                                    stop = true;

                                    if (volume.innerVolumesObjects[i].innerVolumesObjects.Count > 0)
                                    {
                                        SetParentVolume(volume.innerVolumesObjects[i].innerVolumesObjects);
                                    }
                                    else
                                    {
                                        parentVolume.objectReferenceValue = volume.innerVolumesObjects[i];
                                        (parentGameObject.objectReferenceValue as GameObject).layer = volume.innerVolumesObjects[i].firstCollisionLayer;
                                    }
                                    break;
                                }
                            }

                            if (parentVolume.objectReferenceValue == null && !stop)
                            {
                                parentVolume.objectReferenceValue = volume;
                                (parentGameObject.objectReferenceValue as GameObject).layer = volume.firstCollisionLayer;
                                break;
                            }
                        }
                        else
                        {
                            parentVolume.objectReferenceValue = volume;
                            (parentGameObject.objectReferenceValue as GameObject).layer = volume.firstCollisionLayer;
                            stop = true;
                            break;
                        }
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            if (!initialized)
            {
                OnEnable();
            }

            serializedObject.Update();

            parentGameObject.objectReferenceValue = (serializedObject.targetObject as MLPLight).gameObject;

            List<MagicLightProbes> allVolumes = new List<MagicLightProbes>();
            allVolumes.AddRange(Resources.FindObjectsOfTypeAll<MagicLightProbes>());

            if (allVolumes.Count == 0 && PrefabStageUtility.GetCurrentPrefabStage() == null)
            {
                if (EditorUtility.DisplayDialog("Magic Light Probe component required", "First add \"Magic Light Probes\" object to the scene.", "Add"))
                {
                    MLPManager.AddVolume(allVolumes);
                    return;
                }

                return;
            }

            parentVolume.objectReferenceValue = null;

            SetParentVolume(allVolumes);

            if (parentVolume.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("None of the calculation volumes contains this light source.", MessageType.Info);
                return;
            }
            else
            {
                if (lightMode.intValue == (int) LightmapBakeType.Realtime)
                {
                    EditorGUILayout.HelpBox("Realtime light sources are not taken into account.", MessageType.Info);
                    return;
                }
            }

            if (lightType.enumValueIndex != lastLightType.enumValueIndex)
            {
                lastLightType.enumValueIndex = lightType.enumValueIndex;
                MLPLightResetInternal();
            }

            if (accuracy.intValue != lastAccuracy.intValue)
            {
                lastAccuracy.intValue = accuracy.intValue;
                MLPLightResetInternal();
            }

            if (!(parentVolume.objectReferenceValue as MagicLightProbes).calculatingVolume)
            {
                position.vector3Value = (serializedObject.targetObject as MLPLight).transform.position;
                forward.vector3Value = (serializedObject.targetObject as MLPLight).transform.forward;
                targetLight.objectReferenceValue = (serializedObject.targetObject as MLPLight).GetComponent<Light>();

#if BAKERYPACKAGE_EXIST  
                BakeryDirectLight bakeryDirect = (serializedObject.targetObject as MLPLight).GetComponent<BakeryDirectLight>();
                BakeryPointLight bakeryPoint = (serializedObject.targetObject as MLPLight).GetComponent<BakeryPointLight>();
                BakeryLightMesh bakeryMesh = (serializedObject.targetObject as MLPLight).GetComponent<BakeryLightMesh>();

                if (bakeryDirect != null || bakeryPoint != null || bakeryMesh != null)
                {
                    if (ftRenderLightmap.instance == null)
                    {
                        EditorGUILayout.HelpBox("Open the \"Bakery->Render lightmap...\" window. It is necessary for the script to work correctly.", MessageType.Info);
                        return;
                    }
                    else
                    {
                        if (targetLight.objectReferenceValue == null && bakeryMesh == null)
                        {
                            targetLight.objectReferenceValue = (serializedObject.targetObject as MLPLight).gameObject.AddComponent<Light>();
                        }
                    }
                }

                if (bakeryDirect != null)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label("Show Light On Scene", GUILayout.Width(152));
                    EditorGUILayout.PropertyField(showLightOnScene, GUIContent.none);

                    GUILayout.EndHorizontal();

                    if (showLightOnScene.boolValue)
                    {
                        (targetLight.objectReferenceValue as Light).enabled = true;
                    }
                    else
                    {
                        (targetLight.objectReferenceValue as Light).enabled = false;
                    }
                }

                if (bakeryMesh != null)
                {
                    //GUILayout.BeginHorizontal();

                    //GUILayout.Label("Light Type", GUILayout.Width(152));
                    //EditorGUILayout.PropertyField(lightTypeMA, GUIContent.none);

                    //GUILayout.EndHorizontal();

                    //if (lightTypeMA.intValue == 0)
                    //{
                    //    lightTypeMA.intValue = 3;
                    //}

                    //lightType.intValue = (int) MLPLight.MLPLightType.Mesh;
                    //lastLightType.intValue = lightTypeMA.intValue;

                    if (lightType.enumValueIndex != (int) MLPLight.MLPLightType.Area)
                    {
                        DestroyImmediate(targetLight.objectReferenceValue);
                    }
                    else
                    {
                        //if (targetLight.objectReferenceValue == null)
                        //{
                        //    targetLight.objectReferenceValue = (serializedObject.targetObject as MLPLight).gameObject.AddComponent<Light>();
                        //}

                        //(targetLight.objectReferenceValue as Light).type = (LightType) lightType.intValue;
                        //(targetLight.objectReferenceValue as Light).intensity = bakeryMesh.intensity;
                        //(targetLight.objectReferenceValue as Light).range = bakeryMesh.cutoff;                        

                        //if (IsLightCompletelyBaked(bakeryMesh.bakeToIndirect, ftRenderLightmap.instance.userRenderMode))
                        //{
                        //    (targetLight.objectReferenceValue as Light).lightmapBakeType = LightmapBakeType.Baked;
                        //    lightMode.intValue = (int) (targetLight.objectReferenceValue as Light).lightmapBakeType;
                        //}
                        //else
                        //{
                        //    (targetLight.objectReferenceValue as Light).lightmapBakeType = LightmapBakeType.Mixed;
                        //    lightMode.intValue = (int) (targetLight.objectReferenceValue as Light).lightmapBakeType;
                        //}
                    }
                }

                if (bakeryDirect != null)
                {
                    (targetLight.objectReferenceValue as Light).type = LightType.Directional;
                    (targetLight.objectReferenceValue as Light).intensity = bakeryDirect.intensity;

                    if (IsLightCompletelyBaked(bakeryDirect.bakeToIndirect, ftRenderLightmap.instance.userRenderMode))
                    {
                        (targetLight.objectReferenceValue as Light).lightmapBakeType = LightmapBakeType.Baked;
                    }
                    else
                    {
                        (targetLight.objectReferenceValue as Light).lightmapBakeType = LightmapBakeType.Mixed;
                    }
                }

                if (bakeryPoint != null)
                {
                    (targetLight.objectReferenceValue as Light).intensity = bakeryPoint.intensity;

                    switch (bakeryPoint.projMode)
                    {
                        case BakeryPointLight.ftLightProjectionMode.Omni:
                        case BakeryPointLight.ftLightProjectionMode.Cubemap:
                            (targetLight.objectReferenceValue as Light).type = LightType.Point;
                            break;
                        case BakeryPointLight.ftLightProjectionMode.Cone:
                        case BakeryPointLight.ftLightProjectionMode.Cookie:
                            (targetLight.objectReferenceValue as Light).type = LightType.Spot;
                            (targetLight.objectReferenceValue as Light).spotAngle = bakeryPoint.angle;
                            break;
                        case BakeryPointLight.ftLightProjectionMode.IES:
                            EditorGUILayout.HelpBox("This type of light source is not supported.", MessageType.Info);
                            return;

                    }

                    if (IsLightCompletelyBaked(bakeryPoint.bakeToIndirect, ftRenderLightmap.instance.userRenderMode))
                    {
                        (targetLight.objectReferenceValue as Light).lightmapBakeType = LightmapBakeType.Baked;
                    }
                    else
                    {
                        (targetLight.objectReferenceValue as Light).lightmapBakeType = LightmapBakeType.Mixed;
                    }
                }

                if (bakeryMesh == null)
                {
                    bakeryMeshIsNull = true;
                }
                else
                {
                    bakeryMeshIsNull = false;
                }
#endif

                if (targetLight.objectReferenceValue != null && bakeryMeshIsNull)
                {
                    //switch ((targetLight.objectReferenceValue as Light).type)
                    //{
                    //    case LightType.Spot:
                    //        lightType.enumValueIndex = (int) MLPLight.MLPLightType.Spot;
                    //        break;
                    //    case LightType.Directional:
                    //        lightType.enumValueIndex = (int) MLPLight.MLPLightType.Directional;
                    //        isSun = true;
                    //        break;
                    //    case LightType.Point:
                    //        lightType.enumValueIndex = (int) MLPLight.MLPLightType.Point;
                    //        break;
                    //    case LightType.Area:
                    //        lightType.enumValueIndex = (int) MLPLight.MLPLightType.Area;
                    //        break;
                    //}

                    lightMode.intValue = (int) (targetLight.objectReferenceValue as Light).lightmapBakeType;
                }
                else
                {
                    if (bakeryMeshIsNull)
                    {
                        lightType.enumValueIndex = (int) MLPLight.MLPLightType.Mesh;
                        //(targetLight.objectReferenceValue as Light).lightmapBakeType = LightmapBakeType.Baked;
                        lightMode.intValue = (int) LightmapBakeType.Baked;
                    }
                }

                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();

                GUILayout.Label("Light Type", GUILayout.Width(152));
                EditorGUILayout.PropertyField(lightType, GUIContent.none);

                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();

                GUILayout.Label("Calculation Mode", GUILayout.Width(152));
                EditorGUILayout.PropertyField(calculationMode, GUIContent.none);                

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.BeginVertical(GUI.skin.box);

                switch (lightType.enumValueIndex)
                {
                    case (int) MLPLight.MLPLightType.Directional:
                        intensity.floatValue = (targetLight.objectReferenceValue as Light).intensity;
                        break;
                    case (int) MLPLight.MLPLightType.Point:
                        intensity.floatValue = (targetLight.objectReferenceValue as Light).intensity;

                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Use Source Parametrs", GUILayout.MinWidth(148));
                        EditorGUILayout.PropertyField(useSourceParameters, GUIContent.none, GUILayout.Width(305), GUILayout.MinWidth(10));

                        GUILayout.EndHorizontal();
                        GUILayout.BeginVertical(GUI.skin.box);

                        if (useSourceParameters.boolValue)
                        {
                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Range: ", GUILayout.MinWidth(145));
                            GUILayout.Label((targetLight.objectReferenceValue as Light).range.ToString(), GUILayout.Width(300), GUILayout.MinWidth(10));

                            GUILayout.EndHorizontal();

                            range.floatValue = (targetLight.objectReferenceValue as Light).range;
                        }
                        else
                        {
                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Range: ", GUILayout.MinWidth(145));
                            EditorGUILayout.PropertyField(range, GUIContent.none, GUILayout.Width(300), GUILayout.MinWidth(10));

                            GUILayout.EndHorizontal();

                            if (range.floatValue < 0)
                            {
                                range.floatValue = 0;
                            }
                        }

                        GUILayout.EndVertical();
                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Save Nearby Probes", GUILayout.MinWidth(148));
                        EditorGUILayout.PropertyField(saveNearbyProbes, GUIContent.none, GUILayout.Width(305), GUILayout.MinWidth(10));

                        GUILayout.EndHorizontal();

                        if (saveNearbyProbes.boolValue)
                        {
                            GUILayout.BeginVertical(GUI.skin.box);
                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Save Radius", GUILayout.MinWidth(145));
                            EditorGUILayout.PropertyField(saveRadius, GUIContent.none, GUILayout.Width(300), GUILayout.MinWidth(10));

                            GUILayout.EndHorizontal();
                            GUILayout.EndVertical();

                            if (saveRadius.floatValue < 0)
                            {
                                saveRadius.floatValue = 0;
                            }

                            if (saveRadius.floatValue > range.floatValue)
                            {
                                saveRadius.floatValue = range.floatValue;
                            }
                        }

                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Save On Out Of Range", GUILayout.MinWidth(148));
                        EditorGUILayout.PropertyField(saveOnOutOfRange, GUIContent.none, GUILayout.Width(305), GUILayout.MinWidth(10));

                        GUILayout.EndHorizontal();
                        break;
                    case (int) MLPLight.MLPLightType.Spot:
                        isDirectional.boolValue = true;
                        intensity.floatValue = (targetLight.objectReferenceValue as Light).intensity;

                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Use Source Parametrs", GUILayout.MinWidth(148));
                        EditorGUILayout.PropertyField(useSourceParameters, GUIContent.none, GUILayout.Width(305), GUILayout.MinWidth(10));

                        GUILayout.EndHorizontal();
                        GUILayout.BeginVertical(GUI.skin.box);

                        if (useSourceParameters.boolValue)
                        {
                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Range: ", GUILayout.MinWidth(145));
                            GUILayout.Label((targetLight.objectReferenceValue as Light).range.ToString(), GUILayout.Width(300), GUILayout.MinWidth(10));

                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Angle: ", GUILayout.MinWidth(145));
                            GUILayout.Label((targetLight.objectReferenceValue as Light).spotAngle.ToString(), GUILayout.Width(300), GUILayout.MinWidth(10));

                            GUILayout.EndHorizontal();

                            range.floatValue = (targetLight.objectReferenceValue as Light).range;
                            angle.floatValue = (targetLight.objectReferenceValue as Light).spotAngle;
                        }
                        else
                        {
                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Range: ", GUILayout.MinWidth(145));
                            EditorGUILayout.PropertyField(range, GUIContent.none, GUILayout.Width(300), GUILayout.MinWidth(10));

                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Angle:", GUILayout.MinWidth(145));
                            EditorGUILayout.Slider(angle, 1.0f, 179.0f, GUIContent.none, GUILayout.Width(300), GUILayout.MinWidth(10));

                            if (angle.floatValue < 1)
                            {
                                angle.floatValue = 1;
                            }

                            GUILayout.EndHorizontal();

                            if (range.floatValue < 0)
                            {
                                range.floatValue = 0;
                            }
                        }

                        GUILayout.EndVertical();
                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Save Nearby Probes", GUILayout.MinWidth(148));
                        EditorGUILayout.PropertyField(saveNearbyProbes, GUIContent.none, GUILayout.Width(305), GUILayout.MinWidth(10));

                        GUILayout.EndHorizontal();

                        if (saveNearbyProbes.boolValue)
                        {
                            GUILayout.BeginVertical(GUI.skin.box);
                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Save Radius", GUILayout.MinWidth(145));
                            EditorGUILayout.PropertyField(saveRadius, GUIContent.none, GUILayout.Width(300), GUILayout.MinWidth(10));

                            GUILayout.EndHorizontal();
                            GUILayout.EndVertical();

                            if (saveRadius.floatValue < 0)
                            {
                                saveRadius.floatValue = 0;
                            }

                            if (saveRadius.floatValue > range.floatValue)
                            {
                                saveRadius.floatValue = range.floatValue;
                            }
                        }

                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Save On Out Of Range", GUILayout.MinWidth(148));
                        EditorGUILayout.PropertyField(saveOnOutOfRange, GUIContent.none, GUILayout.Width(305), GUILayout.MinWidth(10));

                        GUILayout.EndHorizontal();
                        break;
                    case (int) MLPLight.MLPLightType.Area:
                        isDirectional.boolValue = true;
                        //intensity.floatValue = (targetLight.objectReferenceValue as Light).intensity;

                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Reverse Direction", GUILayout.MinWidth(148));
                        EditorGUILayout.PropertyField(reverseDirection, GUIContent.none, GUILayout.Width(305), GUILayout.MinWidth(10));

                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Use Source Parametrs", GUILayout.MinWidth(148));
                        EditorGUILayout.PropertyField(useSourceParameters, GUIContent.none, GUILayout.Width(305), GUILayout.MinWidth(10));

                        GUILayout.EndHorizontal();
                        GUILayout.BeginVertical(GUI.skin.box);

                        if (useSourceParameters.boolValue)
                        {
                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Range: ", GUILayout.MinWidth(145));
                            GUILayout.Label((targetLight.objectReferenceValue as Light).range.ToString(), GUILayout.Width(300), GUILayout.MinWidth(10));

                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Angle: ", GUILayout.MinWidth(145));
                            GUILayout.Label("180", GUILayout.Width(300), GUILayout.MinWidth(10));

                            GUILayout.EndHorizontal();

                            range.floatValue = (targetLight.objectReferenceValue as Light).range;
                            angle.floatValue = 180.0f;
                        }
                        else
                        {
                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Range: ", GUILayout.MinWidth(145));
                            EditorGUILayout.PropertyField(range, GUIContent.none, GUILayout.Width(300), GUILayout.MinWidth(10));

                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Angle:", GUILayout.MinWidth(145));
                            EditorGUILayout.Slider(angle, 1.0f, 180.0f, GUIContent.none, GUILayout.Width(300), GUILayout.MinWidth(10));

                            if (angle.floatValue < 1)
                            {
                                angle.floatValue = 1;
                            }

                            GUILayout.EndHorizontal();

                            if (range.floatValue < 0)
                            {
                                range.floatValue = 0;
                            }
                        }

                        GUILayout.EndVertical();
                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Save Nearby Probes", GUILayout.MinWidth(148));
                        EditorGUILayout.PropertyField(saveNearbyProbes, GUIContent.none, GUILayout.Width(305), GUILayout.MinWidth(10));

                        GUILayout.EndHorizontal();

                        if (saveNearbyProbes.boolValue)
                        {
                            GUILayout.BeginVertical(GUI.skin.box);
                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Save Radius", GUILayout.MinWidth(145));
                            EditorGUILayout.PropertyField(saveRadius, GUIContent.none, GUILayout.Width(300), GUILayout.MinWidth(10));

                            GUILayout.EndHorizontal();
                            GUILayout.EndVertical();

                            if (saveRadius.floatValue < 0)
                            {
                                saveRadius.floatValue = 0;
                            }

                            if (saveRadius.floatValue > range.floatValue)
                            {
                                saveRadius.floatValue = range.floatValue;
                            }
                        }

                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Save On Out Of Range", GUILayout.MinWidth(148));
                        EditorGUILayout.PropertyField(saveOnOutOfRange, GUIContent.none, GUILayout.Width(305), GUILayout.MinWidth(10));

                        GUILayout.EndHorizontal();
                        break;
                    case (int) MLPLight.MLPLightType.Mesh:
                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Is Directional", GUILayout.MinWidth(148));
                        EditorGUILayout.PropertyField(isDirectional, GUIContent.none, GUILayout.Width(305), GUILayout.MinWidth(10));

                        GUILayout.EndHorizontal();

                        if (isDirectional.boolValue)
                        {
                            GUILayout.BeginVertical(GUI.skin.box);
                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Reverse Direction", GUILayout.MinWidth(145));
                            EditorGUILayout.PropertyField(reverseDirection, GUIContent.none, GUILayout.Width(300), GUILayout.MinWidth(10));

                            GUILayout.EndHorizontal();
                            GUILayout.EndVertical();
                        }

                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Range: ", GUILayout.MinWidth(148));
                        EditorGUILayout.PropertyField(range, GUIContent.none, GUILayout.Width(300), GUILayout.MinWidth(10));

                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Angle:", GUILayout.MinWidth(148));
                        EditorGUILayout.Slider(angle, 1.0f, 360.0f, GUIContent.none, GUILayout.Width(300), GUILayout.MinWidth(10));

                        if (angle.floatValue < 1)
                        {
                            angle.floatValue = 1;
                        }

                        GUILayout.EndHorizontal();

                        if (range.floatValue < 0)
                        {
                            range.floatValue = 0;
                        }

                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Intensity: ", GUILayout.MinWidth(148));
                        EditorGUILayout.PropertyField(intensity, GUIContent.none, GUILayout.Width(300), GUILayout.MinWidth(10));

                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Save Nearby Probes", GUILayout.MinWidth(148));
                        EditorGUILayout.PropertyField(saveNearbyProbes, GUIContent.none, GUILayout.Width(305), GUILayout.MinWidth(10));

                        GUILayout.EndHorizontal();

                        if (saveNearbyProbes.boolValue)
                        {
                            GUILayout.BeginVertical(GUI.skin.box);
                            GUILayout.BeginHorizontal();

                            GUILayout.Label("Save Radius", GUILayout.MinWidth(145));
                            EditorGUILayout.PropertyField(saveRadius, GUIContent.none, GUILayout.Width(300), GUILayout.MinWidth(10));

                            GUILayout.EndHorizontal();
                            GUILayout.EndVertical();

                            if (saveRadius.floatValue < 0)
                            {
                                saveRadius.floatValue = 0;
                            }

                            if (saveRadius.floatValue > range.floatValue)
                            {
                                saveRadius.floatValue = range.floatValue;
                            }
                        }

                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Save On Out Of Range", GUILayout.MinWidth(148));
                        EditorGUILayout.PropertyField(saveOnOutOfRange, GUIContent.none, GUILayout.Width(305), GUILayout.MinWidth(10));

                        GUILayout.EndHorizontal();

                        accurateTrace.boolValue = true;
                        break;
                }

                if (lightType.enumValueIndex != (int) MLPLight.MLPLightType.Directional)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label("Accurate Trace", GUILayout.MinWidth(148));
                    EditorGUILayout.PropertyField(accurateTrace, GUIContent.none, GUILayout.Width(305), GUILayout.MinWidth(10));

                    GUILayout.EndHorizontal();

                    if (accurateTrace.boolValue)
                    {
                        //BoxCollider colliderForTracing = (parentGameObject.objectReferenceValue as GameObject).GetComponent<BoxCollider>();

                        //if (colliderForTracing != null)
                        //{
                        //    DestroyImmediate(colliderForTracing);
                        //}

                        List<GameObject> childTracePoints = new List<GameObject>();

                        switch (lightType.intValue)
                        {
                            case (int) MLPLight.MLPLightType.Point:
                            case (int) MLPLight.MLPLightType.Spot:
                                MLPLightShowCustomTracePoints();
                                break;
                            case (int) MLPLight.MLPLightType.Area:
                                GUILayout.BeginHorizontal();

                                GUILayout.Label("Custom Trace Points", GUILayout.MinWidth(148));
                                EditorGUILayout.PropertyField(customTracePoints, GUIContent.none, GUILayout.Width(305), GUILayout.MinWidth(10));

                                GUILayout.EndHorizontal();

                                if (customTracePoints.boolValue)
                                {
                                    resetEditor.boolValue = false;
                                    MLPLightShowCustomTracePoints();
                                }
                                else
                                {
                                    if (!resetEditor.boolValue)
                                    {
                                        MLPLightResetInternal();
                                    }

                                    if (isHDRP.boolValue)
                                    {
                                        GUILayout.BeginHorizontal();

                                        float[] demensions = { (targetLight.objectReferenceValue as Light).areaSize.x, (targetLight.objectReferenceValue as Light).areaSize.y };

                                        GUILayout.Label("Accuracy", GUILayout.MinWidth(148));
                                        EditorGUILayout.IntSlider(accuracy, 0, 5, GUIContent.none, GUILayout.Width(305), GUILayout.MinWidth(10));

                                        tracePointSize.floatValue = Mathf.Min(demensions) / (accuracy.intValue + 1);

                                        GUILayout.EndHorizontal();
                                    }
                                    else
                                    {
                                        switch ((targetLight.objectReferenceValue as Light).type)
                                        {
                                            case LightType.Rectangle:
                                                GUILayout.BeginHorizontal();

                                                float[] demensions = { (targetLight.objectReferenceValue as Light).areaSize.x, (targetLight.objectReferenceValue as Light).areaSize.y };

                                                GUILayout.Label("Accuracy", GUILayout.MinWidth(148));
                                                EditorGUILayout.IntSlider(accuracy, 0, 5, GUIContent.none, GUILayout.Width(305), GUILayout.MinWidth(10));

                                                tracePointSize.floatValue = Mathf.Min(demensions) / (accuracy.intValue + 1);

                                                GUILayout.EndHorizontal();
                                                break;
                                            case LightType.Disc:
                                                GUILayout.BeginHorizontal();

                                                GUILayout.Label("Accuracy", GUILayout.MinWidth(148));
                                                EditorGUILayout.IntSlider(accuracy, 0, 5, GUIContent.none, GUILayout.Width(305), GUILayout.MinWidth(10));

                                                tracePointSize.floatValue = (targetLight.objectReferenceValue as Light).areaSize.x / (accuracy.intValue + 1);

                                                GUILayout.EndHorizontal();
                                                break;
                                        }
                                    }
                                }
                                break;
                            case (int) MLPLight.MLPLightType.Mesh:
                                GUILayout.BeginHorizontal();

                                GUILayout.Label("Trace Point Setting Type", GUILayout.Width(148));
                                EditorGUILayout.PropertyField(tracePointSettingType, GUIContent.none, GUILayout.Width(305), GUILayout.MinWidth(10));

                                GUILayout.EndHorizontal();

                                switch (tracePointSettingType.enumValueIndex)
                                {
                                    case (int) MLPLight.TracePointSettingMode.Auto:
                                        tracePointSize.floatValue = 0.1f;

                                        GUILayout.BeginHorizontal();

                                        GUILayout.Label("Accuracy", GUILayout.MinWidth(148));
                                        EditorGUILayout.IntSlider(accuracy, 1, 10, GUIContent.none, GUILayout.Width(305), GUILayout.MinWidth(10));

                                        GUILayout.EndHorizontal();

                                        if (accuracy.intValue < 1)
                                        {
                                            accuracy.intValue = 1;
                                        }
                                        else if (accuracy.intValue > 10)
                                        {
                                            accuracy.intValue = 10;
                                        }

                                        break;
                                    case (int) MLPLight.TracePointSettingMode.Custom:
                                        MLPLightShowCustomTracePoints();
                                        break;
                                }
                                break;
                        }

                        if (customTracePoints.boolValue)
                        {

                        }
                        else
                        {
                            if (lastTracePointSize.floatValue != tracePointSize.floatValue)
                            {
                                MLPLightResetInternal();

                                lastTracePointSize.floatValue = tracePointSize.floatValue;

                                List<Vector3> positions = new List<Vector3>();

                                switch (lightType.enumValueIndex)
                                {
                                    case (int) MLPLight.MLPLightType.Area:
                                        if (isHDRP.boolValue)
                                        {
                                            int xPointsCount = Mathf.RoundToInt(hdrpAreaSize.vector2Value.x / tracePointSize.floatValue) + 1;
                                            int yPointsCount = Mathf.RoundToInt(hdrpAreaSize.vector2Value.y / tracePointSize.floatValue);

                                            float positionAddX = 0;
                                            float positionAddY = 0;

                                            for (int px = 0; px < xPointsCount; px++)
                                            {
                                                if (px == 0)
                                                {
                                                    positionAddX = 0;
                                                }
                                                else
                                                {
                                                    positionAddX += tracePointSize.floatValue;
                                                }

                                                Vector3 pointX = new Vector3(
                                                    (0 - hdrpAreaSize.vector2Value.x / 2) + positionAddX,
                                                    0 - hdrpAreaSize.vector2Value.y / 2,
                                                    0);

                                                positions.Add(pointX);

                                                positionAddY = 0;

                                                for (int py = 0; py < yPointsCount; py++)
                                                {
                                                    positionAddY += tracePointSize.floatValue;

                                                    Vector3 pointY = new Vector3(
                                                        pointX.x,
                                                        pointX.y + positionAddY,
                                                        pointX.z);

                                                    positions.Add(pointY);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            switch ((targetLight.objectReferenceValue as Light).type)
                                            {
                                                case LightType.Rectangle:
                                                    int xPointsCount = Mathf.RoundToInt((targetLight.objectReferenceValue as Light).areaSize.x / tracePointSize.floatValue) + 1;
                                                    int yPointsCount = Mathf.RoundToInt((targetLight.objectReferenceValue as Light).areaSize.y / tracePointSize.floatValue);

                                                    float positionAddX = 0;
                                                    float positionAddY = 0;

                                                    for (int px = 0; px < xPointsCount; px++)
                                                    {
                                                        if (px == 0)
                                                        {
                                                            positionAddX = 0;
                                                        }
                                                        else
                                                        {
                                                            positionAddX += tracePointSize.floatValue;
                                                        }

                                                        Vector3 pointX = new Vector3(
                                                    (0 - (targetLight.objectReferenceValue as Light).areaSize.x / 2) + positionAddX,
                                                    0 - (targetLight.objectReferenceValue as Light).areaSize.y / 2,
                                                    0);

                                                        positions.Add(pointX);

                                                        positionAddY = 0;

                                                        for (int py = 0; py < yPointsCount; py++)
                                                        {
                                                            positionAddY += tracePointSize.floatValue;

                                                            Vector3 pointY = new Vector3(
                                                        pointX.x,
                                                        pointX.y + positionAddY,
                                                        pointX.z);

                                                            positions.Add(pointY);
                                                        }
                                                    }
                                                    break;
                                                case LightType.Disc:
                                                    int ringsCount = Mathf.RoundToInt((targetLight.objectReferenceValue as Light).areaSize.x / tracePointSize.floatValue) + 1;

                                                    float ringPosition = 0;

                                                    for (int r = 0; r < ringsCount; r++)
                                                    {
                                                        if (r == 0)
                                                        {
                                                            ringPosition = 0;
                                                        }
                                                        else
                                                        {
                                                            ringPosition += tracePointSize.floatValue;
                                                        }

                                                        Vector3 p1 = new Vector3(
                                                    ringPosition,
                                                    0,
                                                    0);

                                                        positions.Add(p1);

                                                        int p_counter = 0;
                                                        int pointsOnRingCount = Mathf.RoundToInt(Mathf.PI * (ringPosition * 2) / tracePointSize.floatValue);

                                                        if (pointsOnRingCount > 0)
                                                        {
                                                            for (int pr = 0; pr < 360; pr++)
                                                            {

                                                                if (p_counter == Mathf.RoundToInt(360 / pointsOnRingCount))
                                                                {
                                                                    Vector3 p2 = new Vector3(
                                                                ringPosition * Mathf.Cos(pr * Mathf.PI / 180),
                                                                ringPosition * Mathf.Sin(pr * Mathf.PI / 180),
                                                                0);

                                                                    positions.Add(p2);

                                                                    p_counter = 0;
                                                                }
                                                                p_counter++;
                                                            }
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                        break;
                                    case (int) MLPLight.MLPLightType.Mesh:  //Fucking slow
                                        if ((serializedObject.targetObject as MLPLight).GetComponent<MeshFilter>() == null)
                                        {
                                            EditorGUILayout.HelpBox("You must add the \"Light\" or \"Mesh Filter with Mesh Renderer\" components.", MessageType.Info);
                                            return;
                                        }

                                        Mesh lightMesh = (serializedObject.targetObject as MLPLight).GetComponent<MeshFilter>().sharedMesh;

                                        if (lastMesh.objectReferenceValue != lightMesh)
                                        {
                                            Vector3[] vertices = lightMesh.vertices;
                                            int[] triangles = lightMesh.triangles;
                                            List<Vector3> barycentric = new List<Vector3>();

                                            for (int i = 0; i < vertices.Length; i++)
                                            {
                                                if (!positions.Contains(vertices[i]))
                                                {
                                                    positions.Add(vertices[i]);
                                                }
                                            }

                                            List<Vector3> temp = new List<Vector3>();

                                            for (int i = 0; i < positions.Count; i++)
                                            {
                                                if (i == 0)
                                                {
                                                    temp.Add(positions[i]);
                                                }
                                                else
                                                {
                                                    int counter = 0;

                                                    for (int k = 0; k < temp.Count; k++)
                                                    {
                                                        if (Vector3.Distance(positions[i], temp[k]) > 1 - ((float) accuracy.intValue / 10))
                                                        {
                                                            counter++;
                                                        }
                                                    }

                                                    if (counter == temp.Count)
                                                    {
                                                        temp.Add(positions[i]);
                                                    }
                                                }
                                            }

                                            positions.Clear();
                                            positions.AddRange(temp);

                                            //if (accuracy.intValue > 0)
                                            //{
                                            //    for (int i = 0; i < vertices.Length; i++)
                                            //    {
                                            //        Vector3 p0 = vertices[triangles[i * 3 + 0]];
                                            //        Vector3 p1 = vertices[triangles[i * 3 + 1]];
                                            //        Vector3 p2 = vertices[triangles[i * 3 + 2]];
                                            //        Vector3 p3 = vertices[triangles[i * 3 + 3]];
                                            //        Vector3 p4 = vertices[triangles[i * 3 + 4]];
                                            //        Vector3 p5 = vertices[triangles[i * 3 + 5]];

                                            //        Vector3 barycentricPoint = (p0 + p1 + p2 + p3 + p4 + p5) / 6;

                                            //        if (!positions.Contains(barycentricPoint))
                                            //        {
                                            //            positions.Add(barycentricPoint);
                                            //            barycentric.Add(barycentricPoint);
                                            //        }
                                            //    }
                                            //}
                                        }
                                        break;
                                }

                                if (tracePointsData.arraySize == 0)
                                {
                                    foreach (var position in positions)
                                    {
                                        tracePoints.arraySize++;
                                        tracePoints.GetArrayElementAtIndex(tracePoints.arraySize - 1).objectReferenceValue = new GameObject((parentGameObject.objectReferenceValue as GameObject).name + " - Trace Point " + tracePoints.arraySize);
                                        (tracePoints.GetArrayElementAtIndex(tracePoints.arraySize - 1).objectReferenceValue as GameObject).transform.parent = (parentGameObject.objectReferenceValue as GameObject).transform;
                                        (tracePoints.GetArrayElementAtIndex(tracePoints.arraySize - 1).objectReferenceValue as GameObject).transform.position = (parentGameObject.objectReferenceValue as GameObject).transform.TransformPoint(position);
                                        (tracePoints.GetArrayElementAtIndex(tracePoints.arraySize - 1).objectReferenceValue as GameObject).transform.localRotation = Quaternion.identity;
                                        (tracePoints.GetArrayElementAtIndex(tracePoints.arraySize - 1).objectReferenceValue as GameObject).layer = (parentVolume.objectReferenceValue as MagicLightProbes).firstCollisionLayer;
                                        serializedObject.ApplyModifiedProperties();

                                        //if (lightType.enumValueIndex == (int)MLPLight.LPVBLightType.Mesh)
                                        //{
                                        //    (tracePoints.GetArrayElementAtIndex(tracePoints.arraySize - 1).objectReferenceValue as GameObject).AddComponent<BoxCollider>().size =
                                        //        new Vector3(
                                        //            tracePointSize.floatValue,
                                        //            tracePointSize.floatValue,
                                        //            tracePointSize.floatValue);
                                        //}
                                        //else
                                        //{
                                        //    (tracePoints.GetArrayElementAtIndex(tracePoints.arraySize - 1).objectReferenceValue as GameObject).AddComponent<BoxCollider>().size =
                                        //        new Vector3(
                                        //            0.1f,
                                        //            0.1f,
                                        //            0.01f);
                                        //}
                                    }
                                }
                            }
                        }

                        if (tracePointsData.arraySize == 0)
                        {
                            for (int i = 0; i < tracePoints.arraySize; i++)
                            {
                                MLPTracePoint current = new MLPTracePoint()
                                {
                                    position = (tracePoints.GetArrayElementAtIndex(i).objectReferenceValue as GameObject).transform.position,
                                    forward = (tracePoints.GetArrayElementAtIndex(i).objectReferenceValue as GameObject).transform.forward,
                                    name = (tracePoints.GetArrayElementAtIndex(i).objectReferenceValue as GameObject).name,
                                    pointGameObject = (tracePoints.GetArrayElementAtIndex(i).objectReferenceValue as GameObject)
                                };

                                MLPLight temp = (MLPLight)target;
                                List<MLPTracePoint> tempData = temp.tracePointsData;

                                tempData.Add(current);
                                serializedObject.CopyFromSerializedProperty(new SerializedObject(temp).FindProperty("tracePointsData"));
                            }
                        }
                    }
                    else
                    {
                        if (tracePoints.arraySize > 0)
                        {
                            if (EditorUtility.DisplayDialog("Magic Light Probes", "All points you set will be deleted.", "OK", "Cancel"))
                            {
                                MLPLightResetInternal();
                                tracePoints.ClearArray();
                            }
                            else
                            {
                                accurateTrace.boolValue = true;
                            }
                        }

                        MLPLight temp = (MLPLight)target;
                        MLPTracePoint tempData = temp.mainTracePoint;

                        if (tempData == null)
                        {
                            MLPTracePoint main = new MLPTracePoint()
                            {
                                position = (parentGameObject.objectReferenceValue as GameObject).transform.position,
                                forward = (parentGameObject.objectReferenceValue as GameObject).transform.forward,
                                name = (parentGameObject.objectReferenceValue as GameObject).transform.gameObject.name
                            };

                            tempData = main;
                            serializedObject.CopyFromSerializedProperty(new SerializedObject(temp).FindProperty("mainTracePoint"));
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Light settings are copied from the source.", MessageType.Info);
                }

                EditorGUILayout.HelpBox("Located in " + parentVolume.objectReferenceValue.name, MessageType.Info);

                GUILayout.EndVertical();

                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                EditorGUILayout.HelpBox("Settings are locked until the end of the calculation.", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void MLPLightResetInternal()
        {
            lastTracePointSize.floatValue = 0;
            range.floatValue = 0;
            angle.floatValue = 0;

            for (int i = 0; i < (parentGameObject.objectReferenceValue as GameObject).transform.childCount; i++)
            {
                GameObject tracePoint = (parentGameObject.objectReferenceValue as GameObject).transform.GetChild(i).gameObject;

                tracePoints.arraySize++;
                tracePoints.GetArrayElementAtIndex(tracePoints.arraySize - 1).objectReferenceValue = tracePoint;
            }

            for (int i = 0; i < tracePoints.arraySize; i++)
            {
                DestroyImmediate(tracePoints.GetArrayElementAtIndex(i).objectReferenceValue);
            }

            tracePoints.ClearArray();
            tracePointsData.ClearArray();
            resetEditor.boolValue = true;
            //serializedObject.ApplyModifiedProperties();
        }

        private void MLPLightShowCustomTracePoints()
        {
            GUILayout.BeginVertical(GUI.skin.box);

            if (GUILayout.Button("Add Point..."))
            {
                tracePoints.arraySize++;
                tracePoints.GetArrayElementAtIndex(tracePoints.arraySize - 1).objectReferenceValue = new GameObject((parentGameObject.objectReferenceValue as GameObject).name + " - Trace Point " + tracePoints.arraySize);
                (tracePoints.GetArrayElementAtIndex(tracePoints.arraySize - 1).objectReferenceValue as GameObject).transform.parent = (parentGameObject.objectReferenceValue as GameObject).transform;
                (tracePoints.GetArrayElementAtIndex(tracePoints.arraySize - 1).objectReferenceValue as GameObject).transform.localPosition = Vector3.zero;
                (tracePoints.GetArrayElementAtIndex(tracePoints.arraySize - 1).objectReferenceValue as GameObject).transform.localRotation = Quaternion.identity;
                (tracePoints.GetArrayElementAtIndex(tracePoints.arraySize - 1).objectReferenceValue as GameObject).layer = (parentVolume.objectReferenceValue as MagicLightProbes).firstCollisionLayer;
                return;
            }

            tracePoints.ClearArray();
            tracePointsData.ClearArray();

            for (int i = 0; i < (parentGameObject.objectReferenceValue as GameObject).transform.childCount; i++)
            {
                GameObject tracePoint = (parentGameObject.objectReferenceValue as GameObject).transform.GetChild(i).gameObject;

                tracePoints.arraySize++;
                tracePoints.GetArrayElementAtIndex(tracePoints.arraySize - 1).objectReferenceValue = tracePoint;

                MLPTracePoint current = new MLPTracePoint()
                {
                    position = (tracePoints.GetArrayElementAtIndex(i).objectReferenceValue as GameObject).transform.position,
                    forward = (tracePoints.GetArrayElementAtIndex(i).objectReferenceValue as GameObject).transform.forward,
                    name = (tracePoints.GetArrayElementAtIndex(i).objectReferenceValue as GameObject).name,
                    pointGameObject = (tracePoints.GetArrayElementAtIndex(i).objectReferenceValue as GameObject)
                };

                MLPLight temp = (MLPLight)target;
                List<MLPTracePoint> tempData = temp.tracePointsData;

                tempData.Add(current);
                serializedObject.CopyFromSerializedProperty(new SerializedObject(temp).FindProperty("tracePointsData"));
            }

            for (int i = 0; i < tracePoints.arraySize; i++)
            {
                GUILayout.BeginHorizontal();

                EditorGUILayout.PropertyField(tracePoints.GetArrayElementAtIndex(i), new GUIContent((tracePoints.GetArrayElementAtIndex(i).objectReferenceValue as GameObject).name));

                if (GUILayout.Button("Remove Point"))
                {
                    DestroyImmediate((tracePoints.GetArrayElementAtIndex(i).objectReferenceValue as GameObject));
                    tracePoints.DeleteArrayElementAtIndex(i);

                    List<GameObject> temp = new List<GameObject>();

                    for (int a = 0; a < tracePoints.arraySize; a++)
                    {
                        if (tracePoints.GetArrayElementAtIndex(i).objectReferenceValue != null)
                        {
                            temp.Add((tracePoints.GetArrayElementAtIndex(i).objectReferenceValue as GameObject));
                        }
                    }

                    tracePoints.ClearArray();

                    for (int b = 0; b < temp.Count; b++)
                    {
                        tracePoints.arraySize++;
                        tracePoints.GetArrayElementAtIndex(tracePoints.arraySize - 1).objectReferenceValue = temp[b];
                    }

                    return;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
    }
}
