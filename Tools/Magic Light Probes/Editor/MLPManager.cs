using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections;

namespace MagicLightProbes
{
    public class MLPManager : EditorWindow
    {
        public static IEnumerator volumesGeneratinoIterator;

        private static List<MagicLightProbes> probeGroups = new List<MagicLightProbes>();
        private static Light[] lights;
        private static List<Light> directionalLights = new List<Light>();
        private static List<Light> pointLights = new List<Light>();
        private static List<Light> spotLights = new List<Light>();
        private static List<Light> areaLights = new List<Light>();
        private static List<MLPLight> customLights = new List<MLPLight>();
        public static List<GameObject> staticObjects = new List<GameObject>();
        public static bool saveSceneBeforeCalculation = true;

        private int selectedTab;
        private static bool cahnged;
        private int volumesCount;
        private int lastVolumesCount;
        public static bool volumesGenerationIteratorUpdate;

        public static MLPManager managerWindow;
        private static bool initialized;

        public static float currentGenerationProgress;
        public static int currentObject;
        public static int generationProgressFrameSkipper;

        public static float groupingThreshold = 10;

        [MenuItem("Tools/Magic Tools/Magic Light Probes/MLP Manager", priority = 0)]
        static void Init()
        {
            managerWindow = (MLPManager)GetWindow(typeof(MLPManager), false, "MLP Manager");
            managerWindow.minSize = new Vector2(300 * EditorGUIUtility.pixelsPerPoint, 150 * EditorGUIUtility.pixelsPerPoint);
            managerWindow.Show();

            lights = FindObjectsOfType<Light>();
            probeGroups.Clear();
            probeGroups.AddRange(FindObjectsOfType<MagicLightProbes>());

            //EditorSceneManager.sceneOpened += OnSceneOpened;
            //EditorSceneManager.sceneOpening += OnSceneOpening;
            //EditorApplication.update += CheckIfReloaded;

            initialized = true;
        }

        public void VolumesGeneratinoIteratorUpdate()
        {
            volumesGenerationIteratorUpdate = true;

            if (volumesGeneratinoIterator != null && volumesGeneratinoIterator.MoveNext())
            {
                return;
            }

            EditorApplication.update -= VolumesGeneratinoIteratorUpdate;
            volumesGenerationIteratorUpdate = false;
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        public bool UpdateProgress(int count, int period = 100)
        {
            currentGenerationProgress = ((float) currentObject / (float) count) * 100.0f;
            currentObject++;
            generationProgressFrameSkipper++;

            if (generationProgressFrameSkipper == period)
            {
                generationProgressFrameSkipper = 0;

                return true;
            }
            else
            {
                return false;
            }
        }

        void OnGUI()
        {            
            if (MagicLightProbes.operationalDataLost && !initialized)
            {
                Init();
            }
            
            selectedTab = GUILayout.Toolbar(selectedTab, new string[] { "Volumes Generation", "Volumes Overview", "Lights", "Global Settings"});

            if (!GeneralMethods.stylesInitialized)
            {
                GeneralMethods.InitStyles();
            }

            if (lights == null)
            {
                lights = FindObjectsOfType<Light>();
            }

            cahnged = true;

            if (lights.Length == 0 || cahnged)
            {
                lights = FindObjectsOfType<Light>();
                List<MLPLight> tempList = new List<MLPLight>(FindObjectsOfType<MLPLight>());

                directionalLights.Clear();
                pointLights.Clear();
                spotLights.Clear();
                areaLights.Clear();
                customLights.Clear();

                for (int i = 0; i < tempList.Count; i++)
                {
                    if (tempList[i].GetComponent<Light>() == null)
                    {
                        customLights.Add(tempList[i]);
                    }                            
                }

                for (int i = 0; i < lights.Length; i++)
                {
                    switch (lights[i].type)
                    {
                        case LightType.Directional:
                            directionalLights.Add(lights[i]);
                            break;
                        case LightType.Point:
                            pointLights.Add(lights[i]);
                            break;
                        case LightType.Spot:
                            spotLights.Add(lights[i]);
                            break;
                        case LightType.Area:
                            areaLights.Add(lights[i]);
                            break;
                    }
                }

                cahnged = false;
            }

            foreach (var light in lights)
            {
                if (light == null)
                {
                    cahnged = true;
                    break;
                }
            }

            volumesCount = FindObjectsOfType<MagicLightProbes>().Length;

            if (lastVolumesCount != volumesCount)
            {
                probeGroups.Clear();
                probeGroups.AddRange(FindObjectsOfType<MagicLightProbes>());
                lastVolumesCount = volumesCount;
            }

            GeneralMethods.MLPManagerWindow(this, directionalLights, pointLights, spotLights, areaLights, customLights, selectedTab, probeGroups);
        }

        public IEnumerator GenerateVolumes ()
        {
            currentGenerationProgress = 0;
            currentObject = 0;
            generationProgressFrameSkipper = 0;

            List<Vector3> staticObjectsPositions = new List<Vector3>();
            List<GameObject> tempGroupList = new List<GameObject>();
            List<Vector3> tempRemovePositionsList = new List<Vector3>();
            List<List<GameObject>> staticObjectsGroups = new List<List<GameObject>>();
            List<float> avaragedSisez = new List<float>();
            
            FindStaticObjects();

            if (staticObjects.Count > 0)
            {
                for (int i = 0; i < staticObjects.Count; i++)
                {
                    staticObjectsPositions.Add(staticObjects[i].transform.position);
                }

                for (int i = 0; i < staticObjectsPositions.Count; i++)
                {
                    float avaragedSize = 0;
                    int counter = 0;

                    for (int j = 0; j < staticObjectsPositions.Count; j++)
                    {
                        if (staticObjectsPositions[i] != staticObjectsPositions[j])
                        {
                            float diatance = Vector3.Distance(staticObjectsPositions[i], staticObjectsPositions[j]);

                            if (diatance < groupingThreshold)
                            {
                                avaragedSize += diatance;
                                counter++;

                                if (!tempGroupList.Contains(staticObjects[i]))
                                {
                                    tempGroupList.Add(staticObjects[i]);
                                    tempRemovePositionsList.Add(staticObjectsPositions[i]);
                                }

                                if (!tempGroupList.Contains(staticObjects[j]))
                                {
                                    tempGroupList.Add(staticObjects[j]);
                                    tempRemovePositionsList.Add(staticObjectsPositions[j]);
                                }
                            }
                        }
                    }

                    if (counter > 0)
                    {
                        avaragedSize /= counter;
                        avaragedSisez.Add(avaragedSize);
                        staticObjectsGroups.Add(new List<GameObject>(tempGroupList));

                        for (int j = 0; j < tempRemovePositionsList.Count; j++)
                        {
                            staticObjectsPositions.Remove(tempRemovePositionsList[j]);
                        }

                        for (int j = 0; j < tempGroupList.Count; j++)
                        {
                            staticObjects.Remove(tempGroupList[j]);
                        }
                    }

                    tempGroupList.Clear();
                    tempRemovePositionsList.Clear();

                    if (UpdateProgress(staticObjectsPositions.Count, 1))
                    {
                        yield return null;
                    }
                }

                Vector3 avaragedPosition = new Vector3();                

                for (int i = 0; i < staticObjectsGroups.Count; i++)
                {
                    avaragedPosition = Vector3.zero;

                    for (int j = 0; j < staticObjectsGroups[i].Count; j++)
                    {
                        avaragedPosition += staticObjectsGroups[i][j].transform.position;
                    }

                    avaragedPosition /= staticObjectsGroups[i].Count;

                    AddVolume(probeGroups, avaragedPosition, avaragedSisez[i]);
                }
            }
            else
            {
                
            }
        }

        public static void AddVolume(List<MagicLightProbes> allGroups, Vector3 position = new Vector3(), float avaragedSize = 0)
        {
            GameObject magicToolsObject = GameObject.Find("Magic Tools");

            if (magicToolsObject == null)
            {
                magicToolsObject = new GameObject("Magic Tools");
            }

            GameObject mlpGroupsObject;

            mlpGroupsObject = GameObject.Find("Magic Light Probes");

            if (mlpGroupsObject == null)
            {
                mlpGroupsObject = new GameObject("Magic Light Probes");
            }

            mlpGroupsObject.transform.parent = magicToolsObject.transform;

            GameObject newProbesVolume = new GameObject("MLP Group " + allGroups.Count, typeof(MagicLightProbes));

            newProbesVolume.transform.parent = mlpGroupsObject.transform;

            if (position != Vector3.zero)
            {
                newProbesVolume.transform.position = position;                
            }
            else
            {
                newProbesVolume.transform.position = SceneView.lastActiveSceneView.camera.transform.position + SceneView.lastActiveSceneView.camera.transform.forward * 5;
            }

            GameObject probeVolume = GameObject.CreatePrimitive(PrimitiveType.Cube);

            probeVolume.name = "MLP Group " + allGroups.Count + " Volume";
            probeVolume.AddComponent<MLPVolume>();
            probeVolume.transform.parent = newProbesVolume.transform;
            probeVolume.GetComponent<MeshRenderer>().lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            probeVolume.GetComponent<MeshRenderer>().reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            probeVolume.GetComponent<MeshRenderer>().enabled = false;
            probeVolume.transform.localPosition = Vector3.zero;

            if (avaragedSize > 0)
            {
                probeVolume.transform.localScale = new Vector3(avaragedSize, avaragedSize, avaragedSize);
            }

            Object.DestroyImmediate(probeVolume.GetComponent<Collider>());

            GameObject lightProbeGroup = new GameObject("Light Probe Group " + allGroups.Count, typeof(MLPQuickEditing));
            lightProbeGroup.transform.parent = newProbesVolume.transform;            

            MagicLightProbes mlp = newProbesVolume.GetComponent<MagicLightProbes>();
            MLPQuickEditing quickEditing = lightProbeGroup.GetComponent<MLPQuickEditing>();

            quickEditing.parent = mlp;
            mlp.quickEditingComponent = quickEditing;
            mlp.probesVolume = probeVolume;
            mlp.mainTargetVolume = probeVolume.GetComponent<MLPVolume>();
            mlp.mainTargetMeshRenderer = probeVolume.GetComponent<MeshRenderer>();
            allGroups.Add(mlp);

            if (MLPCombinedVolume.Instance == null)
            {
                MLPCombinedVolume.CreateCombinedVolumeObject();
            }

            mlp.combinedVolumeComponent = MLPCombinedVolume.Instance;
        }

        private void FindStaticObjects()
        {
            staticObjects.Clear();

            UnityEngine.Object[] All_GOs = FindObjectsOfType(typeof(GameObject));

            foreach (GameObject obj in All_GOs)
            {
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(obj);

#if UNITY_2019_2_OR_NEWER
                if ((flags & StaticEditorFlags.ContributeGI) != 0)
                {
                    staticObjects.Add(obj);
                }                
#else
                if ((flags & StaticEditorFlags.LightmapStatic) != 0)
                {
                    if (obj.GetComponents(typeof(Component)).Length > 1)
                    {
                        staticObjects.Add(obj);
                    }
                }                
#endif
            }
        }
    }
}
