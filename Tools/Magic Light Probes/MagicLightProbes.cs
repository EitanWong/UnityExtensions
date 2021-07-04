using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.IO;
using Application = UnityEngine.Application;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace MagicLightProbes
{
    [ExecuteInEditMode]
    [HelpURL("https://motion-games-studio.gitbook.io/magic-light-probes/system-components/main-component")]
    public class MagicLightProbes : MonoBehaviour
    {
        const string COMPUTE_SHADERS_FOLDER = "/Passes/Compute Shaders/";

        public IEnumerator colorThresholdRecalculationRoutine;

        public IEnumerator lightProbesVolumeCalculatingRoutine;
        public IEnumerator lightProbesVolumeCalculatingSubRoutine;
        public IEnumerator executingPassesRoutine;
        public IEnumerator volumeDivideingRoutine;

        public struct VolumeParameters
        {
            public int volumeIndex;
            public Vector3 position;
            public Vector3 demensions;

            public VolumeParameters(int _volumeIndex, Vector3 _position, Vector3 _demensions)
            {
                volumeIndex = _volumeIndex;
                position = _position;
                demensions = _demensions;
            }
        }

        #region Basic Parameters
        public List<string> groundAndFloorKeywords = new List<string>();
        public List<string> storedGroundAndFloorKeywords = new List<string>();
        public GameObject probesVolume;
        public bool useDynamicDensity;
        public float volumeSpacing = 0.4f;
        public float volumeSpacingMin = 0.4f;
        public float volumeSpacingMax = 5f;
        public float cornersDetectionThreshold = 0.4f;
        public float cornersDetectionThresholdMin = 0.4f;
        public float cornersDetectionThresholdMax = 5f;
        public float lastCornersDetectionThreshold;
        public float lastCornersDetectionThresholdMin;
        public float lastCornersDetectionThresholdMax;
        public int lastMaxProbesInVolume;
        public int maxProbesInVolume = 10000;
        public int defaultMaxProbesCount;
        public float lastVolumeSpacing;
        public float lastVolumeSpacingMin;
        public float lastVolumeSpacingMax;
        public bool volumeSpacingChanged;
        public bool tooManySubVolumes;
        #endregion

        #region Culling Options
        public enum FillingMode
        {
            VerticalDublicating,
            FullFilling,
            SeparateFilling
        }

        public enum Workflow
        {
            Simple,
            Advanced
        }

        public FillingMode fillingMode = FillingMode.SeparateFilling;
        public Workflow workflow = Workflow.Simple;
        public float maxHeightAboveGeometry = 3.5f;
        public float lastMaxHeightAboveGeometry = 0f;
        public float maxHeightAboveTerrain = 3.5f;
        public bool considerDistanceToLights = true;
        public float lightIntensityTreshold = 0.5f;
        public float colorTreshold = 0.01f;
        public float collisionDetectionRadius = 0.1f;
        public bool saveProbesNearbyGeometry = true;
        public float cornerProbesSpacing = 0.5f;
        public float nearbyGeometryDetectionRadius = 0.5f;
        public float nearbyGeometryDetectionRadiusMin;
        public float nearbyGeometryDetectionRadiusMax;
        public float distanceFromNearbyGeometry = 0.1f;
        public bool fillEquivalentVolume;
        public float equivalentVolumeFillingRate;
        public bool fillUnlitVolume;
        public bool fillFreeVolume = true;
        public float unlitVolumeFillingRate;
        public float freeVolumeFillingRate = 0.01f;
        public bool cullAcceptedVolume;
        public float acceptedVolumeFillingRate;
        public float nearbyGeometryVolumeFillingRate;
        public float verticalDublicatingHeight = 3.5f;
        public float verticalDublicatingStep = 1f;
        public LayerMask raycastFilter;
        public List<MLPLight> excludedLights = new List<MLPLight>();
        public LayerMask layerMask = 1 << 0;
        public int firstCollisionLayer;
        public bool useMultithreading = true;
        public GameObject previousSelection;
        public bool unloaded;
        public bool sceneChanging;
        public bool waitForPrevious;
        public MagicLightProbes previousVolume;
        public bool optimizeForMixedLighting;
        public bool lastOptimizeForMixedLightingValue;
        public bool lastUseDynamicDensityValue;
        public bool preventLeakageThroughWalls;
        public bool useVolumeBottom;
        public bool placeProbesOnGeometryEdges = true;

        public float lastColorThreshold;
        public float lastLightIntensityThreshold;
        public float lastEquivalentVolumeFillingRate;
        public float lastUnlitVolumeFillingRate;
        public float lastFreeVolumeFillingRate;
        public float lastCornerProbesSpacing;
        public float lastDistanceFromGeometry;
        #endregion

        #region Debug
        public enum BoundsDisplayMode
        {
            Always,
            OnSelection
        }

        public enum DebugPasses
        {
            MaximumHeight,
            GeometryCollision,
            GeometryIntersections,
            NearGeometry,
            OutOfRange,
            OutOfRangeBorders,
            ShadingBorders,
            ContrastAreas,
            NearLights,
            LightIntensity,
            UnlitProbes,
            EqualProbes,
            GeometryEdges,
            EqualColor
        };

        public enum DrawModes
        {
            Accepted,
            Culled,
            Both
        }

        public bool debugMode;
        public float debugObjectScale = 0.1f;
        public BoundsDisplayMode boundsDisplayMode;
        public DebugPasses debugPass;
        public DrawModes drawMode;
        public bool debugShowLightIntensity;
        public bool showPreviewGrid;
        public bool nextStep;
        public bool cullByColor = true;
        public bool forceSaveProbesOnShadingBorders = true;
        #endregion

        #region General

#if UNITY_EDITOR
        public MLPCombinedVolume combinedVolumeComponent;
        public List<MagicLightProbes> allVolumes = new List<MagicLightProbes>();
#endif        
        public enum CalculationTarget
        {
            GeometryEdges,
            GeneralCalculation
        }

        [System.Serializable]
        private struct TempPointData
        {
            public float xPos;
            public float yPos;
            public float zPos;

            public TempPointData(Vector3 _position)
            {
                xPos = _position.x;
                yPos = _position.y;
                zPos = _position.z;
            }
        }

        [System.Serializable]
        public class WorkPathFoundEvent : UnityEvent<string>
        {
        }

        public string dataPath;
        public string workPath;
        public bool workPathFound;
        public List<Vector3> localFinishedPositions = new List<Vector3>();
        public ComputeShader calculateVolumeFilling;
        public ComputeShader calculateProbeSpacing;
        public ComputeShader calculateDistanceFromGeometry;
        public MLPVolume currentVolume;
        public MLPQuickEditing quickEditingComponent;

        public bool recalculationRequired;

        private List<Collider> lightColliders = new List<Collider>();
        private List<Collider> objectColliders = new List<Collider>();
        private List<MLPPointData> finalDebugAcceptedPoints = new List<MLPPointData>();
        private List<MLPPointData> finalDebugCulledPoints = new List<MLPPointData>();
        private List<GameObject> tempObjects = new List<GameObject>();
        private List<GameObject> temporarilyDisabledDynamicObjects = new List<GameObject>();
        private List<GameObject> staticObjectsWithoutCollider = new List<GameObject>();
        private GameObject combinedVolumeObject;
        private VolumeParameters currentEditingVolume;
        private Vector3 currentSelectedObjectLastPosition;
        private GameObject lastSelectedObject;
        public bool cancelCombination;

        public List<IEnumerator> passesToExecute = new List<IEnumerator>();
        public List<GameObject> staticObjects = new List<GameObject>();
        public List<MLPLight> lights = new List<MLPLight>();
        public List<MLPPointData> tmpSharedPointsArray = new List<MLPPointData>();
        public List<MLPPointData> tmpOutOfRangePoints = new List<MLPPointData>();
        public List<MLPPointData> tmpOutOfMaxHeightPoints = new List<MLPPointData>();
        public List<MLPPointData> tmpGeometryCollisionPoints = new List<MLPPointData>();
        public List<MLPPointData> tmpContrastOnOutOfRangePoints = new List<MLPPointData>();
        public List<MLPPointData> tmpContrastShadingBordersPoints = new List<MLPPointData>();
        public List<MLPPointData> tmpUnlitPoints = new List<MLPPointData>();
        public List<MLPPointData> tmpFreePoints = new List<MLPPointData>();
        public List<MLPPointData> tmpNearbyGeometryPoints = new List<MLPPointData>();
        public List<MLPPointData> tmpPointsNearGeometryIntersections = new List<MLPPointData>();
        public List<MLPPointData> tmpNearbyLightsPoints = new List<MLPPointData>();
        public List<MLPPointData> tmpEqualPoints = new List<MLPPointData>();
        public List<MLPPointData> debugCulledPoints = new List<MLPPointData>();
        public List<MLPPointData> debugAcceptedPoints = new List<MLPPointData>();
        public List<GameObject> subVolumesDivided = new List<GameObject>();
        public List<Vector3> points = new List<Vector3>();
        public List<VolumeParameters> innerVolumes = new List<VolumeParameters>();
        public List<VolumeParameters> subVolumesParameters = new List<VolumeParameters>();
        public List<MagicLightProbes> innerVolumesObjects = new List<MagicLightProbes>();
        public List<LayerMask> layerMasks = new List<LayerMask>();
        public List<Vector3> transformedPoints = new List<Vector3>();
        public string assetEditorPath;

        public MagicLightProbes parentVolume;
        public static bool operationalDataLost = true;
        public bool localOperationalDataLost = true;
        public bool recombinationNeeded;
        public bool isInBackground;
        public bool realtimeEditing;
        public bool calculated;
        public bool calculatingError;
        public int xPointsCount;
        public int yPointsCount;
        public int zPointsCount;
        public float prevVolumeScaleX;
        public float prevVolumeScaleY;
        public float prevVolumeScaleZ;
        public int totalProbes;
        public int totalProbesInSubVolume;
        public int totalProbesInVolume;
        public bool calculatingVolume = false;
        public bool calculatingVolumeSubPass = false;
        public string currentPass;
        public int currentPassProgressCounter = 0;
        public int currentPassProgressFrameSkipper = 0;
        public float totalProgress;
        public float currentPassProgress;
        public int selectedTab;
        public bool showOptionsInManagerWindow;
        public bool restored = true;
        public int currentVolumePart = 0;
        public float eta = 0;
        public bool changed;
        public bool redivideParts;
        public bool combinedVolumeError;

        private bool passesExecuting = false;
        private int totalProgressCounter = 0;
        private int totalProgressFrameSkipper = 0;
        private float startTime;
        private float endTime;
        private bool scenePreparing;
        #endregion

        public void CheckForNearContrast(MLPPointData pointForCheck)
        {
            bool chekForAdd = true;
            List<MLPPointData> checkList = new List<MLPPointData>();

            foreach (var point in tmpSharedPointsArray)
            {
                if (Vector3.Distance(point.position, pointForCheck.position) <= 2.0f)
                {
                    checkList.Add(point);
                }
            }

            foreach (var point in checkList)
            {
                if (!point.contrastOnOutOfRangeArea && !point.contrastOnShadingArea)
                {
                    chekForAdd = false;
                    break;
                }
            }

            if (chekForAdd)
            {
                tmpSharedPointsArray.Add(pointForCheck);
            }
        }

        public bool UpdateProgress(int count, int period = 100)
        {
            currentPassProgress = ((float) currentPassProgressCounter / (float) count) * 100.0f;
            currentPassProgressCounter++;
            currentPassProgressFrameSkipper++;

            if (currentPassProgressFrameSkipper == period)
            {
                currentPassProgressFrameSkipper = 0;

                return true;
            }
            else
            {
                return false;
            }
        }

        private bool UpdateTotalProgress(int count, int period = 100)
        {
            totalProgress = ((float) totalProgressCounter / (float) count) * 100.0f;
            totalProgressCounter++;
            totalProgressFrameSkipper++;

            if (totalProgressFrameSkipper == period)
            {
                totalProgressFrameSkipper = 0;

                return true;
            }
            else
            {
                return false;
            }
        }

#if UNITY_EDITOR
        private void GetTagManager()
        {
            tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            tagsProp = tagManager.FindProperty("tags");
        }

        private void FindAllVolumes()
        {
            allVolumes = new List<MagicLightProbes>(FindObjectsOfType<MagicLightProbes>());

            for (int i = 0; i < allVolumes.Count; i++)
            {
                if (allVolumes[i] != this)
                {
                    allVolumes[i].allVolumes.Add(this);
                }
            }
        }

        private void OnEnable()
        {
            FindAllVolumes();

            dataPath = Application.dataPath;
            unloaded = false;
            EditorApplication.update -= CheckState;
            EditorApplication.update += CheckState;
            EditorSceneManager.sceneSaving -= StopCheckingState;
            EditorSceneManager.sceneSaving += StopCheckingState;
            EditorSceneManager.sceneClosing -= SceneChanging;
            EditorSceneManager.sceneClosing += SceneChanging;
            EditorSceneManager.sceneOpened += SceneChanging;

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui  -= OnScene;
            SceneView.duringSceneGui  += OnScene;
#else
            SceneView.onSceneGUIDelegate -= OnScene;
            SceneView.onSceneGUIDelegate += OnScene;
#endif

            GetTagManager();
            GetWorkPath(dataPath);
        }

        private void OnDisable()
        {
            EditorApplication.update -= CheckState;

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui  -= OnScene;
#else
            SceneView.onSceneGUIDelegate -= OnScene;
#endif
        }

        private void OnDestroy()
        {
            for (int i = 0; i < allVolumes.Count; i++)
            {
                if (allVolumes[i] != this)
                {
                    allVolumes[i].allVolumes.Remove(this);
                }
            }

            EditorApplication.update -= CheckState;

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui  -= OnScene;
#else
            SceneView.onSceneGUIDelegate -= OnScene;
#endif

            if (!sceneChanging)
            {
                ClearAllStoredData();
            }
        }

        public bool colorThresholdIteratorUpdate;

        public bool mainIteratorUpdate;
        public bool subPassIteratorUpdate;
        public bool executePassesIteratorUpdate;
        public SerializedObject tagManager;
        public SerializedProperty tagsProp;
        public bool globalGroundObjects;
        public MLPVolume mainTargetVolume;
        public MeshRenderer mainTargetMeshRenderer;
        public List<MLPVolume> subdevidedTargetVolumes = new List<MLPVolume>();

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            operationalDataLost = true;
        }

        public bool CheckIfTagExist(string tag)
        {
            bool result = false;

            if (tagsProp == null)
            {
                GetTagManager();
            }

            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);

                if (t.stringValue.Equals(tag))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public void ColorThresholdRecalculationIteratorUpdate()
        {
            colorThresholdIteratorUpdate = true;

            if (colorThresholdRecalculationRoutine != null && colorThresholdRecalculationRoutine.MoveNext())
            {
                return;
            }

            EditorApplication.update -= ColorThresholdRecalculationIteratorUpdate;
            colorThresholdIteratorUpdate = false;
        }

        public void MainIteratorUpdate()
        {
            mainIteratorUpdate = true;

            if (lightProbesVolumeCalculatingRoutine != null && lightProbesVolumeCalculatingRoutine.MoveNext())
            {
                return;
            }

            EditorApplication.update -= MainIteratorUpdate;
            mainIteratorUpdate = false;
        }

        public void SubPassIteratorUpdate()
        {
            subPassIteratorUpdate = true;

            if (lightProbesVolumeCalculatingSubRoutine != null && lightProbesVolumeCalculatingSubRoutine.MoveNext())
            {
                return;
            }

            EditorApplication.update -= SubPassIteratorUpdate;
            subPassIteratorUpdate = false;
        }

        public void ExecutePassesUpdate()
        {
            executePassesIteratorUpdate = true;

            if (executingPassesRoutine != null && executingPassesRoutine.MoveNext())
            {
                return;
            }

            EditorApplication.update -= ExecutePassesUpdate;
            executePassesIteratorUpdate = false;
        }

        private void Divide(Vector3 volume, Vector3[] positions, bool subVolumesDividing = false)
        {
            float[] dimensions =
                {
                    volume.x,
                    volume.y,
                    volume.z
                };

            float longest = Mathf.Max(dimensions);

            for (int i = 0; i < 3; i++)
            {
                if (volume[i] == longest)
                {
                    dimensions[i] = volume[i] / 2;
                    positions[0][i] += dimensions[i] / 2;
                    positions[1][i] -= dimensions[i] / 2;
                    break;
                }
            }

            int xPointsCount = 0;
            int yPointsCount = 0;
            int zPointsCount = 0;

            if (useDynamicDensity)
            {
                if (subVolumesDividing)
                {
                    xPointsCount = Mathf.RoundToInt(dimensions[0] / cornersDetectionThresholdMin);
                    yPointsCount = Mathf.RoundToInt(dimensions[1] / cornersDetectionThresholdMin);
                    zPointsCount = Mathf.RoundToInt(dimensions[2] / cornersDetectionThresholdMin);
                }
                else
                {
                    xPointsCount = Mathf.RoundToInt(dimensions[0] / cornersDetectionThresholdMax);
                    yPointsCount = Mathf.RoundToInt(dimensions[1] / cornersDetectionThresholdMax);
                    zPointsCount = Mathf.RoundToInt(dimensions[2] / cornersDetectionThresholdMax);
                }
            }
            else
            {
                xPointsCount = Mathf.RoundToInt(dimensions[0] / cornersDetectionThreshold);
                yPointsCount = Mathf.RoundToInt(dimensions[1] / cornersDetectionThreshold);
                zPointsCount = Mathf.RoundToInt(dimensions[2] / cornersDetectionThreshold);
            }

            int totalCount = xPointsCount * yPointsCount * zPointsCount;

            if (totalCount > maxProbesInVolume)
            {
                Vector3 halfVolume = new Vector3(dimensions[0], dimensions[1], dimensions[2]);

                for (int i = 0; i < 2; i++)
                {
                    Vector3[] newPositions =
                        {
                            positions[i],
                            positions[i]
                        };

                    if (subVolumesDividing)
                    {
                        Divide(halfVolume, newPositions, true);
                    }
                    else
                    {
                        Divide(halfVolume, newPositions);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    if (optimizeForMixedLighting)
                    {
                        lastOptimizeForMixedLightingValue = true;

                        for (int j = 0; j < staticObjects.Count; j++)
                        {
                            Collider[] obj = Physics.OverlapBox(positions[i],
                                new Vector3(dimensions[0] * 0.5f,
                                            dimensions[1] * 0.5f,
                                            dimensions[2] * 0.5f));

                            if (obj.Length > 0)
                            {
                                GameObject newVolume = new GameObject();

                                newVolume.name = gameObject.name + " Volume Part " + subVolumesDivided.Count;
                                newVolume.transform.parent = probesVolume.transform;
                                newVolume.transform.localScale = new Vector3(dimensions[0], dimensions[1], dimensions[2]);
                                newVolume.transform.position = positions[i];

                                MLPVolume volumePart = newVolume.AddComponent<MLPVolume>();

                                volumePart.id = subVolumesDivided.Count;
                                volumePart.isPartVolume = true;
                                volumePart.parentRootComponent = this;
                                volumePart.objectsInside++;

                                if (subVolumesDividing)
                                {
                                    volumePart.isSubdividedPart = true;
                                }

                                subVolumesDivided.Add(newVolume);
                                subVolumesParameters.Add(new VolumeParameters(subVolumesDivided.Count, newVolume.transform.position, newVolume.transform.localScale));

                                break;
                            }
                        }
                    }
                    else
                    {
                        lastOptimizeForMixedLightingValue = false;

                        GameObject newVolume = new GameObject();

                        newVolume.name = gameObject.name + " Volume Part " + subVolumesDivided.Count;
                        newVolume.transform.parent = probesVolume.transform;
                        newVolume.transform.localScale = new Vector3(dimensions[0], dimensions[1], dimensions[2]);
                        newVolume.transform.position = positions[i];

                        MLPVolume volumePart = newVolume.AddComponent<MLPVolume>();

                        volumePart.id = subVolumesDivided.Count;
                        volumePart.isPartVolume = true;
                        volumePart.parentRootComponent = this;

                        subVolumesDivided.Add(newVolume);
                    }
                }
            }
        }

        private void StopCheckingState(Scene scene, string test)
        {
            unloaded = true;
        }

        private void SceneChanging(Scene scene, bool removing)
        {
            sceneChanging = true;
        }

        private void SceneChanging(Scene scene, OpenSceneMode mode)
        {
            sceneChanging = true;
        }

        private ComputeShader FindShader(string name, string workPatch)
        {
            ComputeShader resultShader = null;

            if (File.Exists(workPatch + COMPUTE_SHADERS_FOLDER + name + ".compute"))
            {
                int subIndex = workPatch.IndexOf("Assets");
                string finalPath = workPatch.Substring(subIndex + "Assets".Length + 1);
                resultShader = AssetDatabase.LoadAssetAtPath<ComputeShader>( "Assets/" + finalPath + COMPUTE_SHADERS_FOLDER + name + ".compute");
            }
            else
            {
                Debug.LogFormat("<color=yellow>MLP:</color> Compute shader \"" + name + ".compute\" does not found.");
            }

            return resultShader;
        }

        private void LoadComputeShaders(string workPath)
        {
            if (calculateVolumeFilling == null)
            {
                calculateVolumeFilling = FindShader("RecalculateVolumeFilling", workPath);
            }

            if (calculateProbeSpacing == null)
            {
                calculateProbeSpacing = FindShader("CalculateProbeSpacing", workPath);
            }

            if (calculateDistanceFromGeometry == null)
            {
                calculateDistanceFromGeometry = FindShader("CalculateDistanceFromGeometry", workPath);
            }

            if (string.IsNullOrEmpty(assetEditorPath))
            {
                assetEditorPath = workPath + "/Editor/";
            }
            else if (!AssetDatabase.IsValidFolder(assetEditorPath))
            {
                assetEditorPath = workPath + "/Editor/";
            }
        }

        private void GetWorkPath(string dataPath)
        {
            Thread findShader = new Thread(new ThreadStart(() =>
            {
                string[] directories = Directory.GetDirectories(dataPath, "Magic Light Probes", SearchOption.AllDirectories);
                workPath = directories[0];
                workPathFound = true;
            }));

            findShader.Start();
        }

        public void ClearAllStoredData()
        {
            string scenePath = assetEditorPath + "/Scene Data/" + SceneManager.GetActiveScene().name;
            string selfPath = scenePath + "/" + gameObject.name;
            string metaFilePath = scenePath + "/" + gameObject.name + ".meta";

            if (Directory.Exists(selfPath) && File.Exists(metaFilePath))
            {
                Directory.Delete(selfPath, true);
                File.Delete(metaFilePath);
            }
        }

        void OnScene(SceneView scene)
        {
            if (unloaded)
            {
                return;
            }

            if (!calculatingVolume)
            {
                #region Volume Size Limit                

                if (
                    lastCornersDetectionThreshold != cornersDetectionThreshold ||
                    lastVolumeSpacingMin != volumeSpacingMin ||
                    lastVolumeSpacingMax != volumeSpacingMax ||
                    lastCornersDetectionThresholdMin != cornersDetectionThresholdMin ||
                    lastCornersDetectionThresholdMax != cornersDetectionThresholdMax ||
                    lastOptimizeForMixedLightingValue != optimizeForMixedLighting ||
                    lastUseDynamicDensityValue != useDynamicDensity ||
                    prevVolumeScaleX != probesVolume.transform.localScale.x ||
                    prevVolumeScaleY != probesVolume.transform.localScale.y ||
                    prevVolumeScaleZ != probesVolume.transform.localScale.z ||
                    lastMaxProbesInVolume != maxProbesInVolume)
                {
                    ClearAllStoredData();

                    if (staticObjects.Count == 0)
                    {
                        FindStaticObjects();
                    }

                    for (int i = 0; i < subVolumesDivided.Count; i++)
                    {
                        DestroyImmediate(subVolumesDivided[i].gameObject);
                    }

                    lastVolumeSpacing = volumeSpacing;
                    lastVolumeSpacingMin = volumeSpacingMin;
                    lastVolumeSpacingMax = volumeSpacingMax;
                    lastCornersDetectionThreshold = cornersDetectionThreshold;
                    lastCornersDetectionThresholdMin = cornersDetectionThresholdMin;
                    lastCornersDetectionThresholdMax = cornersDetectionThresholdMax;
                    lastUseDynamicDensityValue = useDynamicDensity;
                    lastMaxProbesInVolume = maxProbesInVolume;
                    prevVolumeScaleX = probesVolume.transform.localScale.x;
                    prevVolumeScaleY = probesVolume.transform.localScale.y;
                    prevVolumeScaleZ = probesVolume.transform.localScale.z;

                    subVolumesDivided.Clear();
                    subdevidedTargetVolumes.Clear();
                }

                if (subVolumesDivided.Count == 0)
                {
                    bool update = false;
                    redivideParts = true;

                    if (Selection.activeGameObject == probesVolume)
                    {
                        if (changed)
                        {
                            changed = false;
                            update = true;
                        }
                    }
                    else
                    {
                        update = true;
                    }

                    if (update)
                    {
                        if (useDynamicDensity)
                        {
                            Vector3[] positions =
                                {
                                    probesVolume.transform.position,
                                    probesVolume.transform.position
                                };

                            Divide(probesVolume.transform.localScale, positions);
                        }
                        else
                        {
                            xPointsCount = Mathf.RoundToInt(probesVolume.transform.localScale.x / cornersDetectionThreshold);
                            yPointsCount = Mathf.RoundToInt(probesVolume.transform.localScale.y / cornersDetectionThreshold);
                            zPointsCount = Mathf.RoundToInt(probesVolume.transform.localScale.z / cornersDetectionThreshold);

                            int totalCount = xPointsCount * yPointsCount * zPointsCount;

                            if (totalCount / maxProbesInVolume > 1000)
                            {
                                tooManySubVolumes = true;
                                return;
                            }
                            else
                            {
                                tooManySubVolumes = false;
                            }

                            if (totalCount > maxProbesInVolume)
                            {
                                Vector3[] positions =
                                {
                                    probesVolume.transform.position,
                                    probesVolume.transform.position
                                };

                                Divide(probesVolume.transform.localScale, positions);
                            }
                        }
                    }
                }
                else
                {
                    if (useDynamicDensity && redivideParts)
                    {
                        redivideParts = false;

                        List<GameObject> tempRemoveList = new List<GameObject>();

                        for (int i = 0; i < subVolumesDivided.Count; i++)
                        {
                            Collider[] obj = Physics.OverlapBox(subVolumesDivided[i].transform.position,
                                new Vector3(subVolumesDivided[i].transform.localScale.x * 0.5f,
                                            subVolumesDivided[i].transform.localScale.y * 0.5f,
                                            subVolumesDivided[i].transform.localScale.z * 0.5f));

                            if (obj.Length > 1)
                            {
                                tempRemoveList.Add(subVolumesDivided[i]);

                                xPointsCount = Mathf.RoundToInt(subVolumesDivided[i].transform.localScale.x / cornersDetectionThresholdMin);
                                yPointsCount = Mathf.RoundToInt(subVolumesDivided[i].transform.localScale.y / cornersDetectionThresholdMin);
                                zPointsCount = Mathf.RoundToInt(subVolumesDivided[i].transform.localScale.z / cornersDetectionThresholdMin);

                                Vector3[] positions =
                                {
                                    subVolumesDivided[i].transform.position,
                                    subVolumesDivided[i].transform.position
                                };

                                Divide(subVolumesDivided[i].transform.localScale, positions, true);
                            }
                        }

                        for (int i = 0; i < tempRemoveList.Count; i++)
                        {
                            subVolumesDivided.Remove(tempRemoveList[i]);
                            //subVolumesParameters.RemoveAt(i);

                            DestroyImmediate(tempRemoveList[i].gameObject);
                        }

                        tempRemoveList.Clear();
                    }
                }
                #endregion

                #region CheckToRecalculateOfPart

                //GameObject activeGameOject = Selection.activeGameObject;

                //if (activeGameOject != null)
                //{
                //    Vector3 activeGameOjectPosition = activeGameOject.transform.position;

                //    if (Selection.activeGameObject.GetComponent<MagicLightProbes>() == null &&
                //        Selection.activeGameObject.GetComponent<MLPVolume>() == null &&
                //        Selection.activeGameObject.GetComponent<MLPQuickEditing>() == null)
                //    {
                //        if (lastSelectedObject != activeGameOject || currentSelectedObjectLastPosition != activeGameOject.transform.position)
                //        {
                //            Parallel.For(0, subVolumesParameters.Count, (i, state) =>
                //            {
                //                if (CheckIfInside(subVolumesParameters[i], activeGameOjectPosition))
                //                {
                //                    currentEditingVolume = subVolumesParameters[i];
                //                    lastSelectedObject = activeGameOject;
                //                    recalculateOnePart = true;

                //                    if (lightProbesVolumeCalculatingRoutine == null)
                //                    {
                //                        lightProbesVolumeCalculatingRoutine = CalculateProbesVolume();
                //                        EditorApplication.update += MainIteratorUpdate;
                //                    }
                //            }
                //            });

                //            currentSelectedObjectLastPosition = Selection.activeGameObject.transform.position;
                //        }
                //    }
                //}

                #endregion

                //    #region Scene View Controls
                //    if (Selection.activeGameObject == gameObject)
                //    {
                //        Handles.BeginGUI();

                //        if (GUILayout.Button("Control for " + gameObject.name))
                //        {
                //            Debug.Log("Got it to work.");
                //        }

                //        Handles.EndGUI();
                //    }
                //    #endregion
            }
        }

        public void CheckState()
        {
            if (!Application.runInBackground)
            {
                Application.runInBackground = true;
            }

            if (!UnityEditorInternal.InternalEditorUtility.isApplicationActive)
            {
                isInBackground = true;
            }
            else
            {
                isInBackground = false;
            }

            if (unloaded)
            {
                return;
            }

            if (probesVolume == null)
            {
                probesVolume = gameObject.transform.GetChild(0).gameObject;
            }

            if (mainTargetVolume == null)
            {
                mainTargetVolume = probesVolume.GetComponent<MLPVolume>();
            }

            if (mainTargetMeshRenderer == null)
            {
                mainTargetMeshRenderer = probesVolume.GetComponent<MeshRenderer>();
            }

            #region Volume Bounds Display Mode
            if (subVolumesDivided.Count > 0)
            {
                if (subdevidedTargetVolumes.Count == 0)
                {
                    for (int i = 0; i < subVolumesDivided.Count; i++)
                    {
                        subdevidedTargetVolumes.Add(subVolumesDivided[i].GetComponent<MLPVolume>());
                    }
                }
            }

            switch (boundsDisplayMode)
            {
                case BoundsDisplayMode.Always:
                    if (Selection.activeGameObject != null)
                    {
                        if (Selection.activeGameObject == probesVolume)
                        {
                            mainTargetVolume.showGizmo = false;

                            if (subdevidedTargetVolumes.Count > 0)
                            { 
                                mainTargetVolume.showGizmoSelected = false;

                                for (int i = 0; i < subdevidedTargetVolumes.Count; i++)
                                {
                                    subdevidedTargetVolumes[i].showGizmo = false;
                                    subdevidedTargetVolumes[i].showGizmoSelected = true;
                                }
                            }
                            else
                            {
                                mainTargetVolume.showGizmoSelected = true;
                            }
                        }
                        else if (Selection.activeGameObject == gameObject)
                        {
                            mainTargetVolume.showGizmoSelected = true;

                            if (subdevidedTargetVolumes.Count > 0)
                            {
                                for (int i = 0; i < subdevidedTargetVolumes.Count; i++)
                                {
                                    subdevidedTargetVolumes[i].showGizmo = false;
                                    subdevidedTargetVolumes[i].showGizmoSelected = false;
                                }
                            }
                        }
                        else
                        {
                            mainTargetVolume.showGizmo = true;

                            if (subdevidedTargetVolumes.Count > 0)
                            {
                                for (int i = 0; i < subdevidedTargetVolumes.Count; i++)
                                {
                                    if (subdevidedTargetVolumes[i] == null)
                                    {
                                        subdevidedTargetVolumes.RemoveAt(i);
                                        break;
                                    }

                                    subdevidedTargetVolumes[i].showGizmo = false;
                                    subdevidedTargetVolumes[i].showGizmoSelected = false;
                                }
                            }
                        }
                    }
                    break;
                case BoundsDisplayMode.OnSelection:
                    if (Selection.activeGameObject != null)
                    {
                        if (Selection.activeGameObject == gameObject)
                        {
                            mainTargetVolume.showGizmo = false;
                            mainTargetVolume.showGizmoSelected = true;

                            if (subdevidedTargetVolumes.Count > 0)
                            {
                                for (int i = 0; i < subdevidedTargetVolumes.Count; i++)
                                {
                                    subdevidedTargetVolumes[i].showGizmo = false;
                                    subdevidedTargetVolumes[i].showGizmoSelected = false;
                                }
                            }
                        }
                        else if (Selection.activeGameObject == probesVolume.gameObject)
                        {
                            mainTargetVolume.showGizmo = false;

                            if (subdevidedTargetVolumes.Count > 0)
                            {
                                mainTargetVolume.showGizmoSelected = false;

                                for (int i = 0; i < subdevidedTargetVolumes.Count; i++)
                                {
                                    subdevidedTargetVolumes[i].showGizmoSelected = true;
                                }
                            }
                            else
                            {
                                mainTargetVolume.showGizmoSelected = true;
                            }
                        }
                        else
                        {
                            mainTargetVolume.showGizmo = false;
                            mainTargetVolume.showGizmoSelected = false;

                            if (subdevidedTargetVolumes.Count > 0)
                            {
                                for (int i = 0; i < subdevidedTargetVolumes.Count; i++)
                                {
                                    subdevidedTargetVolumes[i].showGizmo = false;
                                    subdevidedTargetVolumes[i].showGizmoSelected = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        mainTargetVolume.showGizmo = false;

                        if (subdevidedTargetVolumes.Count > 0)
                        {
                            for (int i = 0; i < subdevidedTargetVolumes.Count; i++)
                            {
                                subdevidedTargetVolumes[i].showGizmo = false;
                            }
                        }
                    }
                    break;
            }
            #endregion

            if (!calculatingVolume)
            {
                #region Inner Volumes Detection                
                innerVolumes.Clear();
                innerVolumesObjects.Clear();

                if (allVolumes.Count == 0 || allVolumes[0] == null)
                {
                    FindAllVolumes();
                }

                foreach (var volume in allVolumes)
                {
                    if (volume != this)
                    {
                        float thisVolumeDemesions =
                            probesVolume.transform.localScale.x *
                            probesVolume.transform.localScale.y *
                            probesVolume.transform.localScale.z;

                        float checkVolumeDemesions =
                            volume.probesVolume.transform.localScale.x *
                            volume.probesVolume.transform.localScale.y *
                            volume.probesVolume.transform.localScale.z;

                        Vector3[] boundPoints =
                            {
                            new Vector3(
                                volume.probesVolume.transform.position.x - (volume.probesVolume.transform.localScale.x / 2),
                                volume.probesVolume.transform.position.y,
                                volume.probesVolume.transform.position.z),
                            new Vector3(
                                volume.probesVolume.transform.position.x + (volume.probesVolume.transform.localScale.x / 2),
                                volume.probesVolume.transform.position.y,
                                volume.probesVolume.transform.position.z),
                            new Vector3(
                                volume.probesVolume.transform.position.x,
                                volume.probesVolume.transform.position.y - (volume.probesVolume.transform.localScale.y / 2),
                                volume.probesVolume.transform.position.z),
                            new Vector3(
                                volume.probesVolume.transform.position.x,
                                volume.probesVolume.transform.position.y + (volume.probesVolume.transform.localScale.y / 2),
                                volume.probesVolume.transform.position.z),
                            new Vector3(
                                volume.probesVolume.transform.position.x,
                                volume.probesVolume.transform.position.y,
                                volume.probesVolume.transform.position.z - (volume.probesVolume.transform.localScale.z / 2)),
                            new Vector3(
                                volume.probesVolume.transform.position.x,
                                volume.probesVolume.transform.position.y,
                                volume.probesVolume.transform.position.z + (volume.probesVolume.transform.localScale.z / 2))
                    };

                        bool volumeInside = false;

                        foreach (var bp in boundPoints)
                        {
                            if (mainTargetMeshRenderer.bounds.Contains(bp))
                            {
                                volumeInside = true;
                                break;
                            }
                        }

                        if (volumeInside)
                        {
                            if (checkVolumeDemesions < thisVolumeDemesions)
                            {
                                VolumeParameters innerVolume = new VolumeParameters(innerVolumes.Count, volume.probesVolume.transform.position, volume.probesVolume.transform.localScale);
                                innerVolumes.Add(innerVolume);
                                innerVolumesObjects.Add(volume);
                                volume.parentVolume = this;
                            }
                        }
                    }
                }
                #endregion

                #region Target Light Probe Group Control
                //if (gameObject.activeInHierarchy && Selection.activeGameObject != null)
                //{
                //    if (Selection.activeGameObject == gameObject || Selection.activeGameObject == targetProbesGroup.gameObject)
                //    {
                //        if (!targetProbesGroup.gameObject.activeInHierarchy || !targetProbesGroup.enabled)
                //        {
                //            if (previousSelection == Selection.activeGameObject && previousSelection != null)
                //            {
                //                if (!waitForPrevious && !debugMode)
                //                {
                //                    //EditorUtility.DisplayDialog("Magic Light Probes", "This component will be automatically disabled when baking.", "OK");
                //                }
                //            }

                //            targetProbesGroup.gameObject.SetActive(true);
                //            targetProbesGroup.enabled = true;
                //        }

                //        previousSelection = Selection.activeGameObject;
                //    }
                //    else
                //    {
                //        previousSelection = Selection.activeGameObject;
                //        targetProbesGroup.gameObject.SetActive(false);
                //    }
                //}
                //else
                //{
                //    previousSelection = null;
                //    targetProbesGroup.gameObject.SetActive(false);
                //}
                #endregion

                #region Combined Volume Object Control

                //if (combinedVolumeObject == null)
                //{
                //    MLPCombinedVolume[] target = Resources.FindObjectsOfTypeAll<MLPCombinedVolume>();

                //    combinedVolumeObject = new GameObject("-- MLP Combined Volume --", typeof(LightProbeGroup), typeof(MLPCombinedVolume));
                //    combinedVolumeComponent = combinedVolumeObject.GetComponent<MLPCombinedVolume>();
                //    combinedVolumeComponent.magicLightProbes = this;
                //    combinedVolumeComponent.targetProbeGroup = combinedVolumeObject.GetComponent<LightProbeGroup>();
                //    combinedVolumeObject.transform.parent = GameObject.Find("Magic Light Probes").transform;
                //}
                //else
                //{
                //    if (!combinedVolumeObject.activeInHierarchy)
                //    {
                //        if (combinedVolumeObject.transform.parent == null)
                //        {
                //            combinedVolumeObject.gameObject.SetActive(true);
                //            EditorUtility.DisplayDialog("Magic Light Probes", "This object is needed for the plugin to work properly.", "OK");
                //        }
                //        else
                //        {
                //            combinedVolumeError = true;
                //        }
                //    }
                //    else
                //    {
                //        combinedVolumeError = false;
                //    }

                //    //if (combinedVolumeComponent == null)
                //    //{
                //    //    combinedVolumeComponent = target[0];
                //    //    combinedVolumeComponent.magicLightProbes = this;
                //    //}
                //}
                #endregion

                #region CheckColorThresholdData
                //List<GameObject> volumeParts = new List<GameObject>();

                //if (subVolumesDivided.Count > 0)
                //{
                //    volumeParts.Add(subVolumesDivided[0]);
                //}
                //else
                //{
                //    volumeParts.Add(probesVolume);
                //}

                //if (volumeParts[0].GetComponent<MLPVolume>().localColorThresholdEditingPoints.Count == 0)
                //{
                //    localOperationalDataLost = true;
                //}
                //else
                //{

                //}
                #endregion

                if (workPathFound)
                {
                    LoadComputeShaders(workPath);
                }

                if (!restored)
                {
                    RestoreScene();
                }
            }

        }

        private List<GameObject> PrioritizeVolumeList(List<GameObject> volumes)
        {
            List<GameObject> prioritizedParts = new List<GameObject>();
            List<GameObject> nonPioritizedParts = new List<GameObject>();
            List<GameObject> sortedVolumeParts = new List<GameObject>();

            for (int i = 0; i < volumes.Count; i++)
            {
                if (volumes[i].transform.MLP_IsVisibleFrom(SceneView.lastActiveSceneView.camera))
                {
                    prioritizedParts.Add(volumes[i]);
                }
                else
                {
                    nonPioritizedParts.Add(volumes[i]);
                }
            }

            sortedVolumeParts.AddRange(prioritizedParts);
            sortedVolumeParts.AddRange(nonPioritizedParts);

            return sortedVolumeParts;
        }

        #region Rrealtime Recalculating
        #region Recalculate Calls

        public void RecalculateColorThereshold()
        {
            CullingByEquivalentColor cullingByEquivalentColor = new CullingByEquivalentColor();

            passesToExecute.Clear();
            points.Clear();
            localFinishedPositions.Clear();
            colorThresholdRecalculationRoutine = null;
            colorThresholdRecalculationRoutine = RecalculateColorThresholdRealtime(cullingByEquivalentColor, colorTreshold);

            EditorApplication.update -= ColorThresholdRecalculationIteratorUpdate;
            EditorApplication.update += ColorThresholdRecalculationIteratorUpdate;
        }

        public void RecalculateEuivalentVolumeFilling()
        {
            PartialVolumeFilling equivalentProbesOptimization = new PartialVolumeFilling();

            passesToExecute.Clear();
            points.Clear();
            localFinishedPositions.Clear();
            lightProbesVolumeCalculatingRoutine = null;
            lightProbesVolumeCalculatingRoutine = RecalculateEquivalentFillingRateRealtime(equivalentProbesOptimization, equivalentVolumeFillingRate);

            EditorApplication.update -= MainIteratorUpdate;
            EditorApplication.update += MainIteratorUpdate;
        }

        public void RecalculateUnlitVolumeFilling()
        {
            PartialVolumeFilling unlitProbesOptimization = new PartialVolumeFilling();

            passesToExecute.Clear();
            points.Clear();
            localFinishedPositions.Clear();
            lightProbesVolumeCalculatingRoutine = null;
            lightProbesVolumeCalculatingRoutine = RecalculateUnlitFillingRateRealtime(unlitProbesOptimization, unlitVolumeFillingRate);

            EditorApplication.update -= MainIteratorUpdate;
            EditorApplication.update += MainIteratorUpdate;
        }

        public void RecalculateFreeVolumeFilling()
        {
            PartialVolumeFilling freeProbesOptimization = new PartialVolumeFilling();

            passesToExecute.Clear();
            points.Clear();
            localFinishedPositions.Clear();
            lightProbesVolumeCalculatingRoutine = null;
            lightProbesVolumeCalculatingRoutine = RecalculateFreeFillingRateRealtime(freeProbesOptimization, freeVolumeFillingRate);

            EditorApplication.update -= MainIteratorUpdate;
            EditorApplication.update += MainIteratorUpdate;
        }

        public void RecalculateCornerProbeSpacing()
        {
            FindGeometryIntersections findGeometryIntersections = new FindGeometryIntersections();

            passesToExecute.Clear();
            points.Clear();
            localFinishedPositions.Clear();
            lightProbesVolumeCalculatingRoutine = null;
            lightProbesVolumeCalculatingRoutine = RecalculateCornerProbeSpacingRealtime(findGeometryIntersections, cornerProbesSpacing);

            EditorApplication.update -= MainIteratorUpdate;
            EditorApplication.update += MainIteratorUpdate;
        }

        public void RecalculateDistanceFromGeometry()
        {
            SetDistanceFromGeometry setDistanceFromGeometry = new SetDistanceFromGeometry();

            passesToExecute.Clear();
            points.Clear();
            localFinishedPositions.Clear();
            lightProbesVolumeCalculatingRoutine = null;
            lightProbesVolumeCalculatingRoutine = RecalculateDistanceFromGeometryRealtime(setDistanceFromGeometry, distanceFromNearbyGeometry);

            EditorApplication.update -= MainIteratorUpdate;
            EditorApplication.update += MainIteratorUpdate;
        }

        public void RecalculateLightIntensityThereshold()
        {
            CullByLightIntensity cullByLightIntensity = new CullByLightIntensity();

            passesToExecute.Clear();
            points.Clear();
            localFinishedPositions.Clear();
            lightProbesVolumeCalculatingRoutine = null;
            lightProbesVolumeCalculatingRoutine = RecalculateLightIntensityRealtime(cullByLightIntensity, lightIntensityTreshold);

            EditorApplication.update -= MainIteratorUpdate;
            EditorApplication.update += MainIteratorUpdate;
        }
        #endregion

        #region Recalculate Enumerators

        private IEnumerator RecalculateLightIntensityRealtime(CullByLightIntensity cullByLightIntensity, float newValue)
        {
            realtimeEditing = true;

            if (newValue != lightIntensityTreshold)
            {
                yield break;
            }

            passesExecuting = true;
            executingPassesRoutine = null;
            executingPassesRoutine = ExecuteAllPasses();
            EditorApplication.update += ExecutePassesUpdate;

            while (passesExecuting)
            {
                yield return null;
            }

            passesToExecute.Clear();
            points.Clear();
            localFinishedPositions.Clear();
            calculatingVolumeSubPass = true;
            lightProbesVolumeCalculatingSubRoutine = null;
            //lightProbesVolumeCalculatingSubRoutine = SetProbesForQuickEditing(null, currentPart);
            EditorApplication.update += SubPassIteratorUpdate;

            while (calculatingVolumeSubPass)
            {
                yield return null;
            }

            realtimeEditing = false;
            combinedVolumeComponent.combined = false;
        }

        private IEnumerator RecalculateEquivalentFillingRateRealtime(PartialVolumeFilling equivalentProbesOptimization, float newValue)
        {
            realtimeEditing = true;

            if (newValue != equivalentVolumeFillingRate)
            {
                yield break;
            }

            List<GameObject> sortedVolumeParts = new List<GameObject>();

            if (subVolumesDivided.Count > 0)
            {
                sortedVolumeParts = PrioritizeVolumeList(subVolumesDivided);
            }
            else
            {
                sortedVolumeParts.Add(probesVolume);
            }

            localFinishedPositions.Clear();

            for (int i = 0; i < sortedVolumeParts.Count; i++)
            {
                MLPVolume currentVolume = sortedVolumeParts[i].GetComponent<MLPVolume>();

                if (currentVolume.localEquivalentPointsPositions.Count > 0)
                {
                    passesToExecute.Add(equivalentProbesOptimization.ExecutePass(this, PartialVolumeFilling.TargetPoint.Equivalent, currentVolume, true));
                    passesExecuting = true;
                    executingPassesRoutine = null;
                    executingPassesRoutine = ExecuteAllPasses();
                    EditorApplication.update += ExecutePassesUpdate;

                    while (passesExecuting)
                    {
                        yield return null;
                    }

                    passesToExecute.Clear();
                    points.Clear();

                    calculatingVolumeSubPass = true;
                    lightProbesVolumeCalculatingSubRoutine = null;
                    lightProbesVolumeCalculatingSubRoutine = SetProbesForQuickEditing(sortedVolumeParts[i].GetComponent<MLPVolume>(), currentVolume.id);
                    EditorApplication.update += SubPassIteratorUpdate;

                    while (calculatingVolumeSubPass)
                    {
                        yield return null;
                    }
                }
            }

            realtimeEditing = false;
            combinedVolumeComponent.combined = false;
        }

        private IEnumerator RecalculateUnlitFillingRateRealtime(PartialVolumeFilling unlitProbesOptimization, float newValue)
        {
            realtimeEditing = true;

            if (newValue != unlitVolumeFillingRate)
            {
                yield break;
            }

            List<GameObject> sortedVolumeParts = new List<GameObject>();

            if (subVolumesDivided.Count > 0)
            {
                sortedVolumeParts = PrioritizeVolumeList(subVolumesDivided);
            }
            else
            {
                sortedVolumeParts.Add(probesVolume);
            }

            localFinishedPositions.Clear();

            for (int i = 0; i < sortedVolumeParts.Count; i++)
            {
                MLPVolume currentVolume = sortedVolumeParts[i].GetComponent<MLPVolume>();

                if (currentVolume.localUnlitPointsPositions.Count > 0)
                {
                    passesToExecute.Add(unlitProbesOptimization.ExecutePass(this, PartialVolumeFilling.TargetPoint.Unlit, currentVolume, true));
                    passesExecuting = true;
                    executingPassesRoutine = null;
                    executingPassesRoutine = ExecuteAllPasses();
                    EditorApplication.update += ExecutePassesUpdate;

                    while (passesExecuting)
                    {
                        yield return null;
                    }

                    passesToExecute.Clear();
                    points.Clear();

                    calculatingVolumeSubPass = true;
                    lightProbesVolumeCalculatingSubRoutine = null;
                    lightProbesVolumeCalculatingSubRoutine = SetProbesForQuickEditing(currentVolume, currentVolume.id);
                    EditorApplication.update += SubPassIteratorUpdate;

                    while (calculatingVolumeSubPass)
                    {
                        yield return null;
                    }
                }
            }

            realtimeEditing = false;
            combinedVolumeComponent.combined = false;
        }

        private IEnumerator RecalculateFreeFillingRateRealtime(PartialVolumeFilling freeProbesOptimization, float newValue)
        {
            realtimeEditing = true;

            if (newValue != freeVolumeFillingRate)
            {
                yield break;
            }

            List<GameObject> sortedVolumeParts = new List<GameObject>();

            if (subVolumesDivided.Count > 0)
            {
                sortedVolumeParts = PrioritizeVolumeList(subVolumesDivided);
            }
            else
            {
                sortedVolumeParts.Add(probesVolume);
            }

            localFinishedPositions.Clear();

            for (int i = 0; i < sortedVolumeParts.Count; i++)
            {
                MLPVolume currentVolume = sortedVolumeParts[i].GetComponent<MLPVolume>();

                if (currentVolume.localFreePointsPositions.Count > 0)
                {                    
                    passesToExecute.Add(freeProbesOptimization.ExecutePass(this, PartialVolumeFilling.TargetPoint.Free, currentVolume, true));
                    passesExecuting = true;
                    executingPassesRoutine = null;
                    executingPassesRoutine = ExecuteAllPasses();
                    EditorApplication.update += ExecutePassesUpdate;

                    while (passesExecuting)
                    {
                        yield return null;
                    }

                    passesToExecute.Clear();
                    points.Clear();

                    calculatingVolumeSubPass = true;
                    lightProbesVolumeCalculatingSubRoutine = null;
                    lightProbesVolumeCalculatingSubRoutine = SetProbesForQuickEditing(currentVolume, currentVolume.id);
                    EditorApplication.update += SubPassIteratorUpdate;

                    while (calculatingVolumeSubPass)
                    {
                        yield return null;
                    }
                }
            }

            realtimeEditing = false;
            combinedVolumeComponent.combined = false;
        }

        private IEnumerator RecalculateCornerProbeSpacingRealtime(FindGeometryIntersections findGeometryIntersections, float newValue)
        {
            realtimeEditing = true;

            if (newValue != cornerProbesSpacing)
            {
                yield break;
            }

            List<GameObject> sortedVolumeParts = new List<GameObject>();

            if (subVolumesDivided.Count > 0)
            {
                sortedVolumeParts = PrioritizeVolumeList(subVolumesDivided);
            }
            else
            {
                sortedVolumeParts.Add(probesVolume);
            }

            localFinishedPositions.Clear();

            for (int i = 0; i < sortedVolumeParts.Count; i++)
            {
                MLPVolume currentVolume = sortedVolumeParts[i].GetComponent<MLPVolume>();

                float currentCornersDetectionThreshold = 0;

                if (useDynamicDensity)
                {
                    if (currentVolume.isSubdividedPart)
                    {
                        currentCornersDetectionThreshold = cornersDetectionThresholdMin;
                    }
                    else
                    {
                        currentCornersDetectionThreshold = cornersDetectionThresholdMax;
                    }
                }
                else
                {
                    currentCornersDetectionThreshold = cornersDetectionThreshold;
                }

                if (currentVolume.resultLocalCornerPointsPositions.Count > 0)
                {
                    passesToExecute.Add(findGeometryIntersections.ExecutePass(this, currentVolume.id, currentCornersDetectionThreshold, currentVolume, true));                    
                    passesExecuting = true;
                    executingPassesRoutine = null;
                    executingPassesRoutine = ExecuteAllPasses();
                    EditorApplication.update += ExecutePassesUpdate;

                    while (passesExecuting)
                    {
                        yield return null;
                    }

                    passesToExecute.Clear();
                    points.Clear();

                    calculatingVolumeSubPass = true;
                    lightProbesVolumeCalculatingSubRoutine = null;
                    lightProbesVolumeCalculatingSubRoutine = SetProbesForQuickEditing(currentVolume, currentVolume.id);
                    EditorApplication.update += SubPassIteratorUpdate;

                    while (calculatingVolumeSubPass)
                    {
                        yield return null;
                    }
                }
            }

            realtimeEditing = false;

            RecalculateColorThereshold();

            while (colorThresholdIteratorUpdate)
            {
                yield return null;
            }

            combinedVolumeComponent.combined = false;
        }

        private IEnumerator RecalculateDistanceFromGeometryRealtime(SetDistanceFromGeometry setDistanceFromGeometry, float newValue)
        {
            realtimeEditing = true;

            if (newValue != distanceFromNearbyGeometry)
            {
                yield break;
            }

            List<GameObject> volumeParts = new List<GameObject>();

            if (subVolumesDivided.Count > 0)
            {
                volumeParts.AddRange(subVolumesDivided);
            }
            else
            {
                volumeParts.Add(probesVolume);
            }

            localFinishedPositions.Clear();
            int currentPart = 0;

            for (int i = 0; i < volumeParts.Count; i++)
            {
                MLPVolume currentVolume = volumeParts[i].GetComponent<MLPVolume>();

                if (currentVolume.localUnlitPointsPositions.Count > 0)
                {
                    passesToExecute.Add(setDistanceFromGeometry.ExecutePass(this, currentVolume));
                    passesExecuting = true;
                    executingPassesRoutine = null;
                    executingPassesRoutine = ExecuteAllPasses();
                    EditorApplication.update += ExecutePassesUpdate;

                    while (passesExecuting)
                    {
                        yield return null;
                    }

                    passesToExecute.Clear();
                    points.Clear();

                    calculatingVolumeSubPass = true;
                    lightProbesVolumeCalculatingSubRoutine = null;
                    lightProbesVolumeCalculatingSubRoutine = SetProbesForQuickEditing(currentVolume, currentPart);
                    EditorApplication.update += SubPassIteratorUpdate;

                    while (calculatingVolumeSubPass)
                    {
                        yield return null;
                    }
                }

                currentPart++;
            }

            realtimeEditing = false;
            combinedVolumeComponent.combined = false;
        }

        private IEnumerator RecalculateColorThresholdRealtime(CullingByEquivalentColor cullingByEquivalentColorPass, float newValue)
        {
            realtimeEditing = true;

            if (newValue != colorTreshold)
            {
                yield break;
            }

            List<GameObject> sortedVolumeParts = new List<GameObject>();

            if (subVolumesDivided.Count > 0)
            {
                sortedVolumeParts = PrioritizeVolumeList(subVolumesDivided);
            }
            else
            {
                sortedVolumeParts.Add(probesVolume);
            }

            localFinishedPositions.Clear();

            for (int i = 0; i < sortedVolumeParts.Count; i++)
            {
                MLPVolume currentSubVolume = sortedVolumeParts[i].GetComponent<MLPVolume>();

                passesToExecute.Add(cullingByEquivalentColorPass.ExecutePass(this, currentSubVolume.id, currentSubVolume, true));

                passesExecuting = true;
                executingPassesRoutine = null;
                executingPassesRoutine = ExecuteAllPasses();
                EditorApplication.update += ExecutePassesUpdate;

                while (passesExecuting)
                {
                    yield return null;
                }

                passesToExecute.Clear();
                points.Clear();

                calculatingVolumeSubPass = true;
                lightProbesVolumeCalculatingSubRoutine = null;
                lightProbesVolumeCalculatingSubRoutine = SetProbesForQuickEditing(sortedVolumeParts[i].GetComponent<MLPVolume>(), currentSubVolume.id);
                EditorApplication.update += SubPassIteratorUpdate;

                while (calculatingVolumeSubPass)
                {
                    yield return null;
                }
            }

            realtimeEditing = false;
            combinedVolumeComponent.combined = false;
        }

        #endregion

        #endregion

        public IEnumerator DebugCalculateProbesVolume()
        {
            restored = false;
            calculatingVolume = true;
            calculated = false;
            currentVolumePart = 0;

            ResetProgress();
            RestoreScene();

            if (waitForPrevious)
            {
                while (previousVolume.calculatingVolume)
                {
                    yield return null;
                }
            }

            FillVolume fillVolumePass;
            CullByHeight cullByHeightPass;
            CullGeometryCollisions cullGeometryCollisions;
            FindGeometryIntersections findGeometryIntersections;
            CheckNearbyGeometry checkNearbyGeometry;
            FindClosestPoints findClosestPoints;
            CheckVisibility checkVisibility;
            FindOutOfRangePoins findOutOfRangePoins;
            FindOutOfRangeAreas findOutOfRangeAreas;
            FindAreasAtShadingBorders findAreasAtShadingBorders;
            StoreNearLights storeNearLights;
            CalculateLightIntensity calculateLightIntensity;
            CullByLightIntensity cullByLightIntensity;
            FindGeometryEdges findGeometryEdges;
            CullingByEquivalentColor cullingByEquivalentColor;

            List<GameObject> volumeParts = new List<GameObject>();
            points.Clear();
            finalDebugAcceptedPoints.Clear();
            finalDebugCulledPoints.Clear();
            startTime = Time.realtimeSinceStartup;

            if (subVolumesDivided.Count > 0)
            {
                volumeParts.AddRange(subVolumesDivided);
            }
            else
            {
                volumeParts.Add(probesVolume);
            }

            passesToExecute.Add(PrepareScene());

            passesExecuting = true;
            executingPassesRoutine = ExecuteAllPasses();
            EditorApplication.update += ExecutePassesUpdate;

            while (passesExecuting)
            {
                yield return null;
            }

            passesToExecute.Clear();


            while (scenePreparing)
            {
                yield return null;
            }

            float startPartTime = 0;
            float endPartTime = 0;
            eta = 0;

            foreach (var volumePart in volumeParts)
            {
                startPartTime = Time.realtimeSinceStartup;

                currentVolume = volumePart.GetComponent<MLPVolume>();

                float currentVolumeSpacing = 0;
                float currentCornersDetectionThreshold = 0;

                if (useDynamicDensity)
                {
                    if (currentVolume.isSubdividedPart)
                    {
                        currentVolumeSpacing = volumeSpacingMin;
                        currentCornersDetectionThreshold = cornersDetectionThresholdMin;
                    }
                    else
                    {
                        currentVolumeSpacing = volumeSpacingMax;
                        currentCornersDetectionThreshold = cornersDetectionThresholdMax;
                    }
                }
                else
                {
                    currentVolumeSpacing = volumeSpacing;
                    currentCornersDetectionThreshold = cornersDetectionThreshold;
                }

                VolumeParameters volumePartParameters = new VolumeParameters(currentVolume.id, volumePart.transform.position, volumePart.transform.localScale);
                volumePart.GetComponent<MLPVolume>().isInProcess = true;
                volumePart.GetComponent<MLPVolume>().isCalculated = false;

                ResetInternal(currentVolume);
                ClearScene();

                switch (debugPass)
                {
                    case DebugPasses.MaximumHeight:
                        fillVolumePass = new FillVolume();
                        cullByHeightPass = new CullByHeight();

                        passesToExecute.Add(fillVolumePass.ExecutePass(this, currentVolumeSpacing, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(cullByHeightPass.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        break;
                    case DebugPasses.GeometryCollision:
                        fillVolumePass = new FillVolume();
                        cullByHeightPass = new CullByHeight();
                        cullGeometryCollisions = new CullGeometryCollisions();

                        passesToExecute.Add(fillVolumePass.ExecutePass(this, currentVolumeSpacing, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(cullByHeightPass.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(cullGeometryCollisions.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        break;
                    case DebugPasses.GeometryIntersections:
                        fillVolumePass = new FillVolume();
                        cullByHeightPass = new CullByHeight();
                        cullGeometryCollisions = new CullGeometryCollisions();
                        findGeometryIntersections = new FindGeometryIntersections();

                        passesToExecute.Add(fillVolumePass.ExecutePass(this, currentCornersDetectionThreshold, volumePartParameters, CalculationTarget.GeometryEdges));
                        passesToExecute.Add(cullByHeightPass.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(cullGeometryCollisions.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(findGeometryIntersections.ExecutePass(this, currentVolume.id, currentCornersDetectionThreshold, currentVolume));
                        break;
                    case DebugPasses.NearGeometry:
                        fillVolumePass = new FillVolume();
                        cullByHeightPass = new CullByHeight();
                        cullGeometryCollisions = new CullGeometryCollisions();
                        checkNearbyGeometry = new CheckNearbyGeometry();
                        checkVisibility = new CheckVisibility();

                        passesToExecute.Add(fillVolumePass.ExecutePass(this, currentVolumeSpacing, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(cullByHeightPass.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(cullGeometryCollisions.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(checkNearbyGeometry.ExecutePass(this, currentVolumeSpacing, currentCornersDetectionThreshold, currentVolume));

                        if (workflow == Workflow.Advanced)
                        {
                            passesToExecute.Add(checkVisibility.ExecutePass(this));
                        }
                        break;
                    case DebugPasses.OutOfRange:
                        fillVolumePass = new FillVolume();
                        cullByHeightPass = new CullByHeight();
                        cullGeometryCollisions = new CullGeometryCollisions();
                        checkNearbyGeometry = new CheckNearbyGeometry();
                        checkVisibility = new CheckVisibility();
                        findClosestPoints = new FindClosestPoints();
                        findOutOfRangePoins = new FindOutOfRangePoins();

                        passesToExecute.Add(fillVolumePass.ExecutePass(this, currentVolumeSpacing, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(cullByHeightPass.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(cullGeometryCollisions.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(checkNearbyGeometry.ExecutePass(this, currentVolumeSpacing, currentCornersDetectionThreshold, currentVolume));
                        passesToExecute.Add(checkVisibility.ExecutePass(this));
                        passesToExecute.Add(findClosestPoints.ExecutePass(this, debugAcceptedPoints));
                        passesToExecute.Add(findOutOfRangePoins.ExecutePass(this));
                        break;
                    case DebugPasses.OutOfRangeBorders:
                        fillVolumePass = new FillVolume();
                        cullByHeightPass = new CullByHeight();
                        cullGeometryCollisions = new CullGeometryCollisions();
                        checkNearbyGeometry = new CheckNearbyGeometry();
                        checkVisibility = new CheckVisibility();
                        findClosestPoints = new FindClosestPoints();
                        findOutOfRangePoins = new FindOutOfRangePoins();
                        findOutOfRangeAreas = new FindOutOfRangeAreas();

                        passesToExecute.Add(fillVolumePass.ExecutePass(this, currentVolumeSpacing, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(cullByHeightPass.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(cullGeometryCollisions.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(checkNearbyGeometry.ExecutePass(this, currentVolumeSpacing, currentCornersDetectionThreshold, currentVolume));
                        passesToExecute.Add(checkVisibility.ExecutePass(this));
                        passesToExecute.Add(findClosestPoints.ExecutePass(this, debugAcceptedPoints));
                        passesToExecute.Add(findOutOfRangePoins.ExecutePass(this));
                        passesToExecute.Add(findOutOfRangeAreas.ExecutePass(this));
                        break;
                    case DebugPasses.ShadingBorders:
                    case DebugPasses.ContrastAreas:
                        fillVolumePass = new FillVolume();
                        cullByHeightPass = new CullByHeight();
                        cullGeometryCollisions = new CullGeometryCollisions();
                        checkNearbyGeometry = new CheckNearbyGeometry();
                        checkVisibility = new CheckVisibility();
                        findClosestPoints = new FindClosestPoints();
                        findOutOfRangePoins = new FindOutOfRangePoins();
                        findOutOfRangeAreas = new FindOutOfRangeAreas();
                        findAreasAtShadingBorders = new FindAreasAtShadingBorders();

                        passesToExecute.Add(fillVolumePass.ExecutePass(this, currentVolumeSpacing, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(cullByHeightPass.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(cullGeometryCollisions.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(checkNearbyGeometry.ExecutePass(this, currentVolumeSpacing, currentCornersDetectionThreshold, currentVolume));
                        passesToExecute.Add(checkVisibility.ExecutePass(this));
                        passesToExecute.Add(findClosestPoints.ExecutePass(this, debugAcceptedPoints));
                        passesToExecute.Add(findOutOfRangePoins.ExecutePass(this));
                        passesToExecute.Add(findOutOfRangeAreas.ExecutePass(this));
                        passesToExecute.Add(findAreasAtShadingBorders.ExecutePass(this));
                        break;
                    case DebugPasses.NearLights:
                        fillVolumePass = new FillVolume();
                        cullByHeightPass = new CullByHeight();
                        cullGeometryCollisions = new CullGeometryCollisions();
                        checkNearbyGeometry = new CheckNearbyGeometry();
                        checkVisibility = new CheckVisibility();
                        findClosestPoints = new FindClosestPoints();
                        findOutOfRangePoins = new FindOutOfRangePoins();
                        storeNearLights = new StoreNearLights();

                        passesToExecute.Add(fillVolumePass.ExecutePass(this, currentVolumeSpacing, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(cullByHeightPass.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(cullGeometryCollisions.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(checkNearbyGeometry.ExecutePass(this, currentVolumeSpacing, currentCornersDetectionThreshold, currentVolume));
                        passesToExecute.Add(checkVisibility.ExecutePass(this));
                        passesToExecute.Add(findClosestPoints.ExecutePass(this, debugAcceptedPoints));
                        passesToExecute.Add(findOutOfRangePoins.ExecutePass(this));
                        passesToExecute.Add(storeNearLights.ExecutePass(this, currentVolumeSpacing));
                        break;
                    case DebugPasses.LightIntensity:
                    case DebugPasses.UnlitProbes:
                        fillVolumePass = new FillVolume();
                        cullByHeightPass = new CullByHeight();
                        cullGeometryCollisions = new CullGeometryCollisions();
                        checkNearbyGeometry = new CheckNearbyGeometry();
                        checkVisibility = new CheckVisibility();
                        findClosestPoints = new FindClosestPoints();
                        findOutOfRangePoins = new FindOutOfRangePoins();
                        findOutOfRangeAreas = new FindOutOfRangeAreas();
                        calculateLightIntensity = new CalculateLightIntensity();

                        passesToExecute.Add(fillVolumePass.ExecutePass(this, currentVolumeSpacing, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(cullByHeightPass.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(cullGeometryCollisions.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(checkNearbyGeometry.ExecutePass(this, currentVolumeSpacing, currentCornersDetectionThreshold, currentVolume));
                        passesToExecute.Add(checkVisibility.ExecutePass(this));
                        passesToExecute.Add(findClosestPoints.ExecutePass(this, debugAcceptedPoints));
                        passesToExecute.Add(findOutOfRangePoins.ExecutePass(this));
                        passesToExecute.Add(findOutOfRangeAreas.ExecutePass(this));
                        passesToExecute.Add(calculateLightIntensity.ExecutePass(this, currentVolume));
                        break;
                    case DebugPasses.EqualProbes:
                        fillVolumePass = new FillVolume();
                        cullByHeightPass = new CullByHeight();
                        cullGeometryCollisions = new CullGeometryCollisions();
                        checkNearbyGeometry = new CheckNearbyGeometry();
                        checkVisibility = new CheckVisibility();
                        findClosestPoints = new FindClosestPoints();
                        findOutOfRangePoins = new FindOutOfRangePoins();
                        cullByLightIntensity = new CullByLightIntensity();
                        calculateLightIntensity = new CalculateLightIntensity();

                        passesToExecute.Add(fillVolumePass.ExecutePass(this, currentVolumeSpacing, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(cullByHeightPass.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(cullGeometryCollisions.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(checkNearbyGeometry.ExecutePass(this, currentVolumeSpacing, currentCornersDetectionThreshold, currentVolume));
                        passesToExecute.Add(findOutOfRangePoins.ExecutePass(this));
                        passesToExecute.Add(checkVisibility.ExecutePass(this));
                        passesToExecute.Add(findClosestPoints.ExecutePass(this, debugAcceptedPoints));
                        passesToExecute.Add(calculateLightIntensity.ExecutePass(this, currentVolume));
                        passesToExecute.Add(cullByLightIntensity.ExecutePass(this, currentVolume));
                        break;
                    case DebugPasses.GeometryEdges:
                        fillVolumePass = new FillVolume();
                        cullByHeightPass = new CullByHeight();
                        cullGeometryCollisions = new CullGeometryCollisions();
                        checkNearbyGeometry = new CheckNearbyGeometry();
                        findClosestPoints = new FindClosestPoints();
                        findGeometryEdges = new FindGeometryEdges();

                        passesToExecute.Add(fillVolumePass.ExecutePass(this, currentCornersDetectionThreshold, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(cullByHeightPass.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(cullGeometryCollisions.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(checkNearbyGeometry.ExecutePass(this, currentVolumeSpacing, currentCornersDetectionThreshold, currentVolume));
                        passesToExecute.Add(findGeometryEdges.ExecutePass(this, currentVolumeSpacing));
                        break;
                    case DebugPasses.EqualColor:
                        fillVolumePass = new FillVolume();
                        cullByHeightPass = new CullByHeight();
                        cullGeometryCollisions = new CullGeometryCollisions();
                        checkNearbyGeometry = new CheckNearbyGeometry();
                        findClosestPoints = new FindClosestPoints();
                        cullingByEquivalentColor = new CullingByEquivalentColor();

                        passesToExecute.Add(fillVolumePass.ExecutePass(this, currentVolumeSpacing, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(cullByHeightPass.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(cullGeometryCollisions.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                        passesToExecute.Add(checkNearbyGeometry.ExecutePass(this, currentVolumeSpacing, currentCornersDetectionThreshold, currentVolume));
                        passesToExecute.Add(findClosestPoints.ExecutePass(this, debugAcceptedPoints));
                        passesToExecute.Add(cullingByEquivalentColor.ExecutePass(this, currentVolume.id, currentVolume));
                        break;
                }

                passesExecuting = true;
                executingPassesRoutine = ExecuteAllPasses();
                EditorApplication.update += ExecutePassesUpdate;

                while (passesExecuting)
                {
                    yield return null;
                }

                lightProbesVolumeCalculatingSubRoutine = null;
                calculatingVolumeSubPass = true;
                lightProbesVolumeCalculatingSubRoutine = SetProbes();
                EditorApplication.update += SubPassIteratorUpdate;

                while (calculatingVolumeSubPass)
                {
                    yield return null;
                }

                volumePart.GetComponent<MLPVolume>().isCalculated = true;
                volumePart.GetComponent<MLPVolume>().isInProcess = false;

                currentVolumePart++;

                endPartTime = Time.realtimeSinceStartup;

                if (eta == 0)
                {
                    eta = (endPartTime - startPartTime) * volumeParts.Count;
                }
                else
                {
                    if (eta > 0)
                    {
                        eta -= (endPartTime - startPartTime);
                    }
                    else
                    {
                        eta = 0;
                    }
                }
            }

            endTime = Time.realtimeSinceStartup;

            var elapsedTime = TimeSpan.FromSeconds(endTime - startTime);
            Debug.LogFormat("<color=yellow>MLP:</color> Calculation of " + gameObject.name + " completed in " + string.Format("{0:00} min {1:00} sec.", elapsedTime.TotalMinutes, elapsedTime.Seconds) + " <color=yellow>[DEBUG MODE]</color>");

            calculated = true;
            calculatingVolume = false;

            MagicLightProbes[] volumes = FindObjectsOfType<MagicLightProbes>();

            int calculatedVolumes = 0;

            foreach (var volume in volumes)
            {
                if (volume.calculated)
                {
                    calculatedVolumes++;
                }
            }

            if (calculatedVolumes == volumes.Length)
            {
                RestoreScene();
                CombineVolumes(volumes);
            }
            else
            {
                if (!waitForPrevious)
                {
                    RestoreScene();
                }
            }
        }

        public IEnumerator CalculateProbesVolume()
        {
            restored = false;
            calculatingVolume = true;
            calculated = false;
            currentVolumePart = 0;

            ResetProgress();
            RestoreScene();

            if (waitForPrevious)
            {
                while (previousVolume.calculatingVolume)
                {
                    yield return null;
                }
            }

            FillVolume fillVolumePass;
            CullByHeight cullByHeightPass;
            CullGeometryCollisions cullGeometryCollisions;
            FindGeometryIntersections findGeometryIntersections;
            CheckNearbyGeometry checkNearbyGeometry;
            FindClosestPoints findClosestPoints;
            CheckVisibility checkVisibility;
            FindOutOfRangePoins findOutOfRangePoins;
            FindOutOfRangeAreas findOutOfRangeAreas;
            FindAreasAtShadingBorders findAreasAtShadingBorders;
            StoreNearLights storeNearLights;
            CalculateLightIntensity calculateLightIntensity;
            CullByLightIntensity cullByLightIntensity;
            FindGeometryEdges findGeometryEdges;
            CheckForLightLeakAreas checkForLightLeakAreas;
            CullingByEquivalentColor cullingByEquivalentColor;
            RemoveUnusedPoints removeUnusedPoints;
            PartialVolumeFilling equivalentProbesOptimization;
            PartialVolumeFilling unlitProbesOptimization;
            PartialVolumeFilling freeProbesOptimization;
            DublicateVertical dublicateVertical;

            List<GameObject> volumeParts = new List<GameObject>();
            points.Clear();
            localFinishedPositions.Clear();
            startTime = Time.realtimeSinceStartup;

            if (subVolumesDivided.Count > 0)
            {
                volumeParts.AddRange(subVolumesDivided);
            }
            else
            {
                volumeParts.Add(probesVolume);
            }

            passesToExecute.Add(PrepareScene());

            passesExecuting = true;
            executingPassesRoutine = ExecuteAllPasses();
            EditorApplication.update += ExecutePassesUpdate;

            while (passesExecuting)
            {
                yield return null;
            }

            passesToExecute.Clear();

            List<float> avaragedTime = new List<float>();
            float startPartTime = 0;
            float endPartTime = 0;
            eta = 0;

            int currentPart = 0;

            foreach (var volumePart in volumeParts)
            {
                currentVolume = volumePart.GetComponent<MLPVolume>();

                float currentVolumeSpacing = 0;
                float currentCornersDetectionThreshold = 0;

                if (useDynamicDensity)
                {
                    if (currentVolume.isSubdividedPart)
                    {
                        currentVolumeSpacing = volumeSpacingMin;
                        currentCornersDetectionThreshold = cornersDetectionThresholdMin;
                    }
                    else
                    {
                        currentVolumeSpacing = volumeSpacingMax;
                        currentCornersDetectionThreshold = cornersDetectionThresholdMax;
                    }
                }
                else
                {
                    currentVolumeSpacing = volumeSpacing;
                    currentCornersDetectionThreshold = cornersDetectionThreshold;
                }

                EditorProgressBar.ShowProgressBar("Calculation... " + currentPart + "/" + volumeParts.Count, (float) currentPart / (float) volumeParts.Count);

                startPartTime = Time.realtimeSinceStartup;

                VolumeParameters volumePartParameters = new VolumeParameters(currentVolume.id, volumePart.transform.position, volumePart.transform.localScale);
                volumePart.GetComponent<MLPVolume>().isInProcess = true;
                volumePart.GetComponent<MLPVolume>().isCalculated = false;

                ResetInternal(currentVolume);
                ClearScene();

                fillVolumePass = new FillVolume();
                cullByHeightPass = new CullByHeight();
                cullGeometryCollisions = new CullGeometryCollisions();
                findGeometryIntersections = new FindGeometryIntersections();

                passesToExecute.Add(fillVolumePass.ExecutePass(this, currentCornersDetectionThreshold, volumePartParameters, CalculationTarget.GeometryEdges));
                passesToExecute.Add(cullByHeightPass.ExecutePass(this, volumePartParameters, CalculationTarget.GeometryEdges));
                passesToExecute.Add(cullGeometryCollisions.ExecutePass(this, volumePartParameters, CalculationTarget.GeometryEdges));
                passesToExecute.Add(findGeometryIntersections.ExecutePass(this, currentVolume.id, currentCornersDetectionThreshold, currentVolume));

                passesExecuting = true;
                executingPassesRoutine = ExecuteAllPasses();
                EditorApplication.update += ExecutePassesUpdate;

                while (passesExecuting)
                {
                    yield return null;
                }

                passesToExecute.Clear();
                tmpSharedPointsArray.Clear();

                fillVolumePass = new FillVolume();
                cullByHeightPass = new CullByHeight();
                cullGeometryCollisions = new CullGeometryCollisions();
                checkNearbyGeometry = new CheckNearbyGeometry();
                checkVisibility = new CheckVisibility();
                findClosestPoints = new FindClosestPoints();
                findOutOfRangePoins = new FindOutOfRangePoins();
                findOutOfRangeAreas = new FindOutOfRangeAreas();
                findAreasAtShadingBorders = new FindAreasAtShadingBorders();
                storeNearLights = new StoreNearLights();
                calculateLightIntensity = new CalculateLightIntensity();
                findGeometryEdges = new FindGeometryEdges();
                checkForLightLeakAreas = new CheckForLightLeakAreas();
                cullingByEquivalentColor = new CullingByEquivalentColor();
                cullByLightIntensity = new CullByLightIntensity();
                removeUnusedPoints = new RemoveUnusedPoints();
                equivalentProbesOptimization = new PartialVolumeFilling();
                unlitProbesOptimization = new PartialVolumeFilling();
                freeProbesOptimization = new PartialVolumeFilling();
                dublicateVertical = new DublicateVertical();

                passesToExecute.Add(fillVolumePass.ExecutePass(this, currentVolumeSpacing, volumePartParameters, CalculationTarget.GeneralCalculation));
                passesToExecute.Add(cullByHeightPass.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));
                passesToExecute.Add(cullGeometryCollisions.ExecutePass(this, volumePartParameters, CalculationTarget.GeneralCalculation));

                if (workflow == Workflow.Advanced)
                {
                    switch (fillingMode)
                    {
                        case FillingMode.FullFilling:
                            passesToExecute.Add(checkNearbyGeometry.ExecutePass(this, currentVolumeSpacing, currentCornersDetectionThreshold, currentVolume));

                            passesExecuting = true;
                            executingPassesRoutine = ExecuteAllPasses();
                            EditorApplication.update += ExecutePassesUpdate;

                            while (passesExecuting)
                            {
                                yield return null;
                            }

                            tmpSharedPointsArray.AddRange(tmpPointsNearGeometryIntersections);
                            break;
                        case FillingMode.SeparateFilling:
                            passesToExecute.Add(checkNearbyGeometry.ExecutePass(this, currentVolumeSpacing, currentCornersDetectionThreshold, currentVolume));
                            passesToExecute.Add(findClosestPoints.ExecutePass(this, tmpNearbyGeometryPoints));
                            passesToExecute.Add(findGeometryEdges.ExecutePass(this, currentVolumeSpacing));

                            if (preventLeakageThroughWalls)
                            {
                                passesToExecute.Add(checkForLightLeakAreas.ExecutePass(this));
                            }

                            findClosestPoints = new FindClosestPoints();

                            if (placeProbesOnGeometryEdges)
                            {
                                passesToExecute.Add(findClosestPoints.ExecutePass(this, tmpSharedPointsArray));
                            }

                            passesToExecute.Add(checkVisibility.ExecutePass(this));
                            passesToExecute.Add(findOutOfRangePoins.ExecutePass(this));
                            passesToExecute.Add(findOutOfRangeAreas.ExecutePass(this));
                            passesToExecute.Add(findAreasAtShadingBorders.ExecutePass(this, currentVolume));
                            passesToExecute.Add(storeNearLights.ExecutePass(this, currentVolumeSpacing));
                            passesToExecute.Add(calculateLightIntensity.ExecutePass(this, currentVolume));
                            passesToExecute.Add(cullByLightIntensity.ExecutePass(this, currentVolume));

                            if (cullByColor)
                            {
                                passesToExecute.Add(cullingByEquivalentColor.ExecutePass(this, currentVolume.id, currentVolume));
                            }

                            if (fillEquivalentVolume && equivalentVolumeFillingRate > 0)
                            {
                                passesToExecute.Add(equivalentProbesOptimization.ExecutePass(this, PartialVolumeFilling.TargetPoint.Equivalent, currentVolume));
                            }

                            if (fillUnlitVolume && unlitVolumeFillingRate > 0)
                            {
                                passesToExecute.Add(unlitProbesOptimization.ExecutePass(this, PartialVolumeFilling.TargetPoint.Unlit, currentVolume));
                            }

                            passesToExecute.Add(removeUnusedPoints.ExecutePass(this));

                            passesExecuting = true;
                            executingPassesRoutine = ExecuteAllPasses();
                            EditorApplication.update += ExecutePassesUpdate;

                            while (passesExecuting)
                            {
                                yield return null;
                            }
                            break;
                        case FillingMode.VerticalDublicating:
                            passesToExecute.Add(checkNearbyGeometry.ExecutePass(this, currentVolumeSpacing, currentCornersDetectionThreshold, currentVolume));
                            passesToExecute.Add(dublicateVertical.ExecutePass(this));

                            passesExecuting = true;
                            executingPassesRoutine = ExecuteAllPasses();
                            EditorApplication.update += ExecutePassesUpdate;

                            while (passesExecuting)
                            {
                                yield return null;
                            }
                            break;
                    }
                }
                else
                {
                    passesToExecute.Add(checkNearbyGeometry.ExecutePass(this, currentVolumeSpacing, currentCornersDetectionThreshold, currentVolume));
                    passesToExecute.Add(findClosestPoints.ExecutePass(this, tmpNearbyGeometryPoints));

                    if (placeProbesOnGeometryEdges)
                    {
                        passesToExecute.Add(findGeometryEdges.ExecutePass(this, currentVolumeSpacing));
                    }

                    if (preventLeakageThroughWalls)
                    {
                        passesToExecute.Add(checkForLightLeakAreas.ExecutePass(this));
                    }

                    findClosestPoints = new FindClosestPoints();

                    passesToExecute.Add(findClosestPoints.ExecutePass(this, tmpSharedPointsArray));
                    passesToExecute.Add(cullingByEquivalentColor.ExecutePass(this, currentVolume.id, currentVolume));
                    passesToExecute.Add(freeProbesOptimization.ExecutePass(this, PartialVolumeFilling.TargetPoint.Free, currentVolume));
                    passesToExecute.Add(removeUnusedPoints.ExecutePass(this));

                    passesExecuting = true;
                    executingPassesRoutine = ExecuteAllPasses();
                    EditorApplication.update += ExecutePassesUpdate;

                    while (passesExecuting)
                    {
                        yield return null;
                    }
                }

                lightProbesVolumeCalculatingSubRoutine = null;
                calculatingVolumeSubPass = true;
                lightProbesVolumeCalculatingSubRoutine = SetProbes(currentVolume.id);
                EditorApplication.update += SubPassIteratorUpdate;

                while (calculatingVolumeSubPass)
                {
                    yield return null;
                }

                if (!isInBackground)
                {
                    if (UpdateTotalProgress(passesToExecute.Count + 1, 1))
                    {
                        yield return null;
                    }
                }

                volumePart.GetComponent<MLPVolume>().isCalculated = true;
                volumePart.GetComponent<MLPVolume>().isInProcess = false;

                currentVolumePart++;

                endPartTime = Time.realtimeSinceStartup;
                avaragedTime.Add(endPartTime - startPartTime);

                if (eta == 0)
                {
                    eta = avaragedTime[0] * volumeParts.Count;
                }
                else
                {
                    if (eta > 0)
                    {
                        eta -= (avaragedTime[avaragedTime.Count - 1] + avaragedTime[avaragedTime.Count - 2]) / 2;
                    }
                    else
                    {
                        eta = 0;
                    }
                }

                currentPart++;
            }

            EditorProgressBar.ClearProgressBar();

            endTime = Time.realtimeSinceStartup;

            var elapsedTime = TimeSpan.FromSeconds(endTime - startTime);
            Debug.LogFormat("<color=yellow>MLP:</color> Calculation of " + gameObject.name + " completed in " + string.Format("{0:00} min {1:00} sec.", elapsedTime.TotalMinutes, elapsedTime.Seconds));

            calculated = true;
            calculatingVolume = false;
            operationalDataLost = false;

            MagicLightProbes[] volumes = FindObjectsOfType<MagicLightProbes>();

            int calculatedVolumes = 0;

            foreach (var volume in volumes)
            {
                if (volume.calculated)
                {
                    calculatedVolumes++;
                }
            }

            if (calculatedVolumes == volumes.Length)
            {
                RestoreScene();
                CombineVolumes(volumes);
            }
            else
            {
                if (!waitForPrevious)
                {
                    RestoreScene();
                }
            }
        }

        private IEnumerator ExecuteAllPasses()
        {
            recombinationNeeded = true;

            for (int i = 0; i < passesToExecute.Count; i++)
            {
                lightProbesVolumeCalculatingSubRoutine = null;
                calculatingVolumeSubPass = true;
                lightProbesVolumeCalculatingSubRoutine = passesToExecute[i];
                EditorApplication.update += SubPassIteratorUpdate;

                while (calculatingVolumeSubPass)
                {
                    yield return null;
                }

                if (!isInBackground)
                {
                    if (UpdateTotalProgress(passesToExecute.Count, 0))
                    {
                        yield return null;
                    }
                }
            }

            passesExecuting = false;
        }

        public void RestoreScene()
        {
            EditorUtilityDisplayProgressBar.HideProgress();
            EditorProgressBar.ClearProgressBar();

            if (layerMasks.Count > 0)
            {
                for (int j = 0; j < staticObjects.Count; j++)
                {
                    staticObjects[j].layer = layerMasks[j];
                }
            }

            if (objectColliders != null && objectColliders.Count > 0)
            {
                foreach (var obj in objectColliders)
                {
                    DestroyImmediate(obj);
                }
            }

            if (lightColliders != null && lightColliders.Count > 0)
            {
                foreach (var obj in lightColliders)
                {
                    DestroyImmediate(obj);
                }
            }

            if (temporarilyDisabledDynamicObjects != null && temporarilyDisabledDynamicObjects.Count > 0)
            {
                foreach (var obj in temporarilyDisabledDynamicObjects)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);
                    }
                }
            }

            passesToExecute.Clear();
            layerMasks.Clear();
            lightColliders.Clear();
            objectColliders.Clear();
            staticObjects.Clear();
            temporarilyDisabledDynamicObjects.Clear();
            staticObjectsWithoutCollider.Clear();

            restored = true;
        }

        public void ClearScene()
        {
            GameObject[] tempArray = tempObjects.ToArray();

            for (int i = 0; i < tempArray.Length; i++)
            {
                DestroyImmediate(tempArray[i]);
            }

            tempObjects.Clear();
            tempArray = null;
        }

        private void FindStaticObjects()
        {
            UnityEngine.Object[] All_GOs = FindObjectsOfType(typeof(GameObject));

            foreach (GameObject obj in All_GOs)
            {
                if (obj.transform.childCount > 0)
                {
                    continue;
                }

                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(obj);

                bool isStatic = false;

#if UNITY_2019_2_OR_NEWER
                if ((flags & StaticEditorFlags.ContributeGI) != 0)
                {
                    isStatic = true;
                }                
#else
                if ((flags & StaticEditorFlags.LightmapStatic) != 0)
                {
                    isStatic = true;
                }
#endif

                if (isStatic)
                {
                    if (obj.GetComponents(typeof(Component)).Length > 1)
                    {
                        staticObjects.Add(obj);
                    }
                }
                else
                {
                    if (calculatingVolume)
                    {
                        if (
                            obj.GetComponent<Light>() == null &&
                            obj.GetComponent<MLPCombinedVolume>() == null &&
                            obj.GetComponent<MagicLightProbes>() == null &&
                            obj.GetComponent<MLPLight>() == null &&
                            obj.GetComponentInParent<MLPLight>() == null &&
                            obj.GetComponent<MLPVolume>() == null &&
                            obj.name != "!ftraceLightmaps")
                        {
                            if (obj.GetComponents(typeof(Component)).Length > 1)
                            {
                                temporarilyDisabledDynamicObjects.Add(obj);
                                obj.SetActive(false);
                            }
                        }
                    }
                }
            }
        }

        private IEnumerator PrepareScene()
        {
            scenePreparing = true;
            waitForPrevious = false;

            if (workflow == Workflow.Advanced)
            {
                List<MLPLight> checkList = new List<MLPLight>();
                checkList.AddRange(FindObjectsOfType<MLPLight>());

                if (checkList.Count == 0)
                {
                    EditorUtility.DisplayDialog("Magic Light Probes", "No light source affects the volume. " +
                        "Be sure to add the “MLP Light” component to all the light sources that you want to use in the calculation.", "OK");

                    CancelCalculation();
                }

                lights = new List<MLPLight>();

                currentPass = "Getting Lights...";
                currentPassProgressCounter = 0;
                currentPassProgressFrameSkipper = 0;

                for (int i = 0; i < checkList.Count; i++)
                {
                    if (probesVolume.GetComponent<MeshRenderer>().bounds.Contains(checkList[i].transform.position) ||
                        checkList[i].lightType == MLPLight.MLPLightType.Directional)
                    {
                        lights.Add(checkList[i]);
                    }

                    if (UpdateProgress(checkList.Count, 0))
                    {
                        yield return null;
                    }
                }

                currentPass = "Setting Lights...";
                currentPassProgressCounter = 0;
                currentPassProgressFrameSkipper = 0;

                foreach (var light in lights)
                {
                    light.position = light.transform.position;
                    light.forward = light.transform.forward;

                    if (light.accurateTrace)
                    {
                        light.tracePointsData.Clear();

                        foreach (var tracePoint in light.tracePoints)
                        {
                            light.tracePointsData.Add(
                                new MLPTracePoint()
                                {
                                    position = tracePoint.transform.position,
                                    forward = tracePoint.transform.forward,
                                    name = tracePoint.gameObject.name,
                                    pointGameObject = tracePoint.gameObject
                                }
                            );

                            if (tracePoint.GetComponent<SphereCollider>() == null)
                            {
                                SphereCollider tempTracePointCollider = tracePoint.AddComponent<SphereCollider>();
                                tempTracePointCollider.radius = 0.01f;
                                lightColliders.Add(tempTracePointCollider);
                            }
                        }
                    }
                    else
                    {
                        light.mainTracePoint = new MLPTracePoint()
                        {
                            position = light.transform.position,
                            forward = light.transform.forward,
                            name = light.gameObject.name,
                            pointGameObject = light.gameObject
                        };

                        if (light.lightType != MLPLight.MLPLightType.Directional)
                        {
                            if (light.gameObject.GetComponent<SphereCollider>() == null)
                            {
                                SphereCollider tempLightCollider = light.gameObject.AddComponent<SphereCollider>();
                                tempLightCollider.radius = 0.01f;
                                lightColliders.Add(tempLightCollider);
                            }
                        }
                    }

                    if (UpdateProgress(checkList.Count, 0))
                    {
                        yield return null;
                    }
                }

                for (int i = 0; i < excludedLights.Count; i++)
                {
                    if (lights.Contains(excludedLights[i]))
                    {
                        lights.Remove(excludedLights[i]);
                    }
                }
            }

            currentPass = "Creating List Of Static Objects...";
            currentPassProgressCounter = 0;
            currentPassProgressFrameSkipper = 0;

            UnityEngine.Object[] All_GOs = FindObjectsOfType(typeof(GameObject));

            foreach (GameObject obj in All_GOs)
            {
                if (obj.name == "[generated-meshes]" || obj.name == "[generated-collider-mesh]")
                {
                    staticObjects.Add(obj);
                    //layerMask |= (1 << obj.layer);
                    continue;
                }

                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(obj);

#if UNITY_2019_2_OR_NEWER
                if ((flags & StaticEditorFlags.ContributeGI) != 0)
                {
                    staticObjects.Add(obj);
                }
                else
                {
                    if (calculatingVolume)
                    {
                        if (
                            obj.GetComponent<Light>() == null &&
                            obj.GetComponent<MLPCombinedVolume>() == null &&
                            obj.GetComponent<MagicLightProbes>() == null &&
                            obj.GetComponent<MLPLight>() == null &&
                            obj.GetComponentInParent<MLPLight>() == null &&
                            obj.GetComponent<MLPVolume>() == null &&
                            obj.name != "!ftraceLightmaps")
                        {
                            if (obj.GetComponents(typeof(Component)).Length > 1)
                            {
                                temporarilyDisabledDynamicObjects.Add(obj);
                                obj.SetActive(false);
                            }
                        }
                    }
                }
#else
                if ((flags & StaticEditorFlags.LightmapStatic) != 0)
                {
                    if (obj.GetComponents(typeof(Component)).Length > 1)
                    {
                        staticObjects.Add(obj);
                    }
                }
                else
                {
                    if (calculatingVolume)
                    {
                        if (
                            obj.GetComponent<Light>() == null &&
                            obj.GetComponent<MLPCombinedVolume>() == null &&
                            obj.GetComponent<MagicLightProbes>() == null &&
                            obj.GetComponent<MLPLight>() == null &&
                            obj.GetComponentInParent<MLPLight>() == null &&
                            obj.GetComponent<MLPVolume>() == null &&
                            obj.name != "!ftraceLightmaps")
                        {
                            if (obj.GetComponents(typeof(Component)).Length > 1)
                            {
                                temporarilyDisabledDynamicObjects.Add(obj);
                                obj.SetActive(false);
                            }
                        }
                    }
                }
#endif
                if (UpdateProgress(All_GOs.Length, 0))
                {
                    yield return null;
                }
            }

            currentPass = "Collider Preparation...";
            currentPassProgressCounter = 0;
            currentPassProgressFrameSkipper = 0;

            foreach (var obj in staticObjects)
            {
                if (obj.GetComponent<MLPLight>() == null && obj.GetComponent<MeshFilter>() != null)
                {
                    if (obj.GetComponent<Collider>() == null)
                    {
                        staticObjectsWithoutCollider.Add(obj);
                    }
                }
                else if (obj.GetComponent<MLPLight>() != null && obj.GetComponent<MLPLight>().lightType == MLPLight.MLPLightType.Mesh)
                {
                    if (obj.GetComponent<Collider>() == null)
                    {
                        staticObjectsWithoutCollider.Add(obj);
                    }
                }

                if (UpdateProgress(staticObjects.Count))
                {
                    yield return null;
                }
            }

            currentPassProgressCounter = 0;
            currentPassProgressFrameSkipper = 0;

            if (staticObjectsWithoutCollider.Count > 0)
            {
                foreach (var obj in staticObjectsWithoutCollider)
                {
                    if (obj.transform.parent != null && obj.transform.parent.GetComponent<LODGroup>() != null)
                    {
                        if (obj.name.Contains("LOD0"))
                        {
                            objectColliders.Add(obj.AddComponent<MeshCollider>());
                            obj.GetComponent<MeshCollider>().convex = false;
                        }
                    }
                    else
                    {
                        objectColliders.Add(obj.AddComponent<MeshCollider>());
                    }

                    if (UpdateProgress(staticObjectsWithoutCollider.Count))
                    {
                        yield return null;
                    }
                }
            }

            scenePreparing = false;
            passesExecuting = false;
        }

        public void ResetInternal(MLPVolume currentVolume = null)
        {
            calculated = false;
            //recalculateOnePart = false;

            tmpNearbyLightsPoints = new List<MLPPointData>();

            layerMasks.Clear();
            passesToExecute.Clear();
            tmpSharedPointsArray.Clear();
            tmpOutOfRangePoints.Clear();
            tmpOutOfMaxHeightPoints.Clear();
            tmpGeometryCollisionPoints.Clear();
            tmpContrastOnOutOfRangePoints.Clear();
            tmpContrastShadingBordersPoints.Clear();
            tmpUnlitPoints.Clear();
            tmpEqualPoints.Clear();
            tmpFreePoints.Clear();
            tmpNearbyGeometryPoints.Clear();
            tmpPointsNearGeometryIntersections.Clear();
            tmpNearbyLightsPoints.Clear();

            if (currentVolume != null)
            {
                currentVolume.localNearbyGeometryPoints.Clear();
                currentVolume.localCornerPointsPositions.Clear();
                currentVolume.localCornerPoints.Clear();
                currentVolume.localEquivalentPointsPositions.Clear();
                currentVolume.localUnlitPointsPositions.Clear();
                currentVolume.localFreePointsPositions.Clear();
                currentVolume.localAvaragedDirections.Clear();
                currentVolume.localColorThresholdEditingPoints.Clear();
                currentVolume.localContrastPoints.Clear();
                currentVolume.resultLocalCornerPointsPositions.Clear();
                currentVolume.resultLocalEquivalentPointsPositions.Clear();
                currentVolume.resultLocalUnlitPointsPositions.Clear();
                currentVolume.resultNearbyGeometryPointsPositions.Clear();
                currentVolume.resultLocalFreePointsPositions.Clear();
            }

            localFinishedPositions.Clear();
            debugAcceptedPoints.Clear();
            debugCulledPoints.Clear();

            ResetProgress();

            endTime = 0;
        }

        public void ResetProgress()
        {
            totalProbes = 0;
            currentPassProgress = 0.0f;
            totalProgress = 0.0f;
            currentPassProgressCounter = 0;
            currentPassProgressFrameSkipper = 0;
            totalProgressCounter = 0;
            totalProgressFrameSkipper = 0;
            totalProbesInSubVolume = 0;
            totalProbesInVolume = 0;
        }

        public bool CheckIfOutOfRange(MLPLight rootLight, MLPTracePoint tracePoint, MLPPointData point)
        {
            bool result = false;
            float distance = Vector3.Distance(tracePoint.position, point.position);

            switch (rootLight.lightType)
            {
                case MLPLight.MLPLightType.Point:
                    if (considerDistanceToLights)
                    {
                        if (Vector3.Distance(tracePoint.position, point.position) < rootLight.range)
                        {
                            result = false;

                            lock (point.inRangeForLights)
                            {
                                if (!point.inRangeForLights.Contains(rootLight))
                                {
                                    //point.lockForCull = true;
                                    point.inRangeForLights.Add(rootLight);
                                }
                            }
                        }
                        else
                        {
                            result = true;

                            lock (point.inRangeForLights)
                            {
                                if (point.inRangeForLights.Contains(rootLight))
                                {
                                    point.inRangeForLights.Remove(rootLight);
                                }
                            }
                        }
                    }
                    else
                    {
                        result = false;

                        lock (point.inRangeForLights)
                        {
                            if (!point.inRangeForLights.Contains(rootLight))
                            {
                                //point.lockForCull = true;
                                point.inRangeForLights.Add(rootLight);
                            }
                        }
                    }
                    break;
                case MLPLight.MLPLightType.Spot:
                    Vector3 direction = (point.position - tracePoint.position).normalized;

                    float angle = Vector3.Angle(direction, tracePoint.forward);

                    point.angleToLight = angle;

                    if (angle > rootLight.angle / 2)
                    {
                        result = true;

                        lock (point.inRangeForLights)
                        {
                            if (point.inRangeForLights.Contains(rootLight))
                            {
                                point.inRangeForLights.Remove(rootLight);
                            }
                        }
                    }
                    else
                    {
                        if (considerDistanceToLights)
                        {
                            if (distance <= rootLight.range)
                            {
                                result = false;

                                lock (point.inRangeForLights)
                                {
                                    if (!point.inRangeForLights.Contains(rootLight))
                                    {
                                        point.lockForCull = true;
                                        point.inRangeForLights.Add(rootLight);
                                    }
                                }
                            }
                            else
                            {
                                result = true;

                                lock (point.inRangeForLights)
                                {
                                    if (point.inRangeForLights.Contains(rootLight))
                                    {
                                        point.inRangeForLights.Remove(rootLight);
                                    }
                                }
                            }
                        }
                        else
                        {
                            result = false;

                            lock (point.inRangeForLights)
                            {
                                if (!point.inRangeForLights.Contains(rootLight))
                                {
                                    point.lockForCull = true;
                                    point.inRangeForLights.Add(rootLight);
                                }
                            }
                        }
                    }
                    break;
                case MLPLight.MLPLightType.Area:
                case MLPLight.MLPLightType.Mesh:
                    if (rootLight.isDirectional)
                    {
                        Vector3 directionToPoint = (point.position - tracePoint.position).normalized;
                        float angleToPoint;

                        if (rootLight.reverseDirection)
                        {
                            angleToPoint = Vector3.Angle(directionToPoint, -tracePoint.forward);
                        }
                        else
                        {
                            angleToPoint = Vector3.Angle(directionToPoint, tracePoint.forward);
                        }

                        point.angleToLight = angleToPoint;

                        if (angleToPoint > rootLight.angle / 2)
                        {
                            result = true;

                            lock (point.inRangeForLights)
                            {
                                if (point.inRangeForLights.Contains(rootLight))
                                {
                                    point.inRangeForLights.Remove(rootLight);
                                }
                            }
                        }
                        else
                        {
                            if (considerDistanceToLights)
                            {
                                if (Vector3.Distance(tracePoint.position, point.position) <= rootLight.range)
                                {
                                    result = false;

                                    lock (point.inRangeForLights)
                                    {
                                        if (!point.inRangeForLights.Contains(rootLight))
                                        {
                                            point.lockForCull = true;
                                            point.inRangeForLights.Add(rootLight);
                                        }
                                    }
                                }
                                else
                                {
                                    result = true;

                                    lock (point.inRangeForLights)
                                    {
                                        if (point.inRangeForLights.Contains(rootLight))
                                        {
                                            point.inRangeForLights.Remove(rootLight);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                result = false;

                                lock (point.inRangeForLights)
                                {
                                    if (!point.inRangeForLights.Contains(rootLight))
                                    {
                                        point.lockForCull = true;
                                        point.inRangeForLights.Add(rootLight);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (considerDistanceToLights)
                        {
                            if (Vector3.Distance(tracePoint.position, point.position) <= rootLight.range)
                            {
                                result = false;

                                lock (point.inRangeForLights)
                                {
                                    if (!point.inRangeForLights.Contains(rootLight))
                                    {
                                        point.lockForCull = true;
                                        point.inRangeForLights.Add(rootLight);
                                    }
                                }
                            }
                            else
                            {
                                result = true;

                                lock (point.inRangeForLights)
                                {
                                    if (point.inRangeForLights.Contains(rootLight))
                                    {
                                        point.inRangeForLights.Remove(rootLight);
                                    }
                                }
                            }
                        }
                        else
                        {
                            result = false;

                            lock (point.inRangeForLights)
                            {
                                if (!point.inRangeForLights.Contains(rootLight))
                                {
                                    point.lockForCull = true;
                                    point.inRangeForLights.Add(rootLight);
                                }
                            }
                        }
                    }
                    break;
            }

            return result;
        }

        public bool CheckIfInShadow(MLPLight light, MLPTracePoint tracePoint, MLPPointData point)
        {
            bool result = false;
            tracePoint.pointGameObject.SetActive(true);

            RaycastHit hitInfo;
            Ray rayToLight;

            switch (light.lightType)
            {
                case MLPLight.MLPLightType.Directional:
                    rayToLight = new Ray(point.position, -light.transform.forward);

                    if (!point.inRangeForLights.Contains(light))
                    {
                        point.inRangeForLights.Add(light);
                    }

                    if (Physics.Raycast(rayToLight, out hitInfo, Mathf.Infinity, layerMask))
                    {
                        if (CheckIfStatic(hitInfo.collider.gameObject))
                        {
                            result = true;

                            if (!point.inShadowForLights.Contains(light))
                            {
                                point.inShadowForLights.Add(light);
                            }
                        }
                    }
                    else
                    {
                        result = false;

                        if (!point.inRangeForLights.Contains(light))
                        {
                            point.inRangeForLights.Add(light);
                        }
                    }
                    break;
                default:
                    rayToLight = new Ray(point.position, (tracePoint.position - point.position).normalized);

                    if (Physics.Raycast(rayToLight, out hitInfo, Mathf.Infinity, layerMask))
                    {
                        if (CheckIfStatic(hitInfo.collider.gameObject) || hitInfo.collider.gameObject == tracePoint.pointGameObject)
                        {
                            if (hitInfo.collider.name != tracePoint.name)
                            {
                                tracePoint.pointGameObject.SetActive(false);
                                result = true;

                                if (!point.inShadowForLights.Contains(light))
                                {
                                    point.inShadowForLights.Add(light);
                                }
                            }
                            else
                            {
                                tracePoint.pointGameObject.SetActive(false);
                                result = false;

                                if (!point.inRangeForLights.Contains(light))
                                {
                                    //point.inRangeForLights.Add(light);
                                }
                            }
                        }
                    }
                    else
                    {
                        tracePoint.pointGameObject.SetActive(false);
                        result = false;

                        if (!point.inRangeForLights.Contains(light))
                        {
                            point.inRangeForLights.Add(light);
                        }
                    }
                    break;
            }

            return result;
        }

        public bool CheckIfStatic(GameObject gameObject)
        {
            if (gameObject.GetComponent<Collider>().isTrigger)
            {
                return false;
            }
            
            bool isStatic = false;
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(gameObject);

#if UNITY_2019_2_OR_NEWER
            if ((flags & StaticEditorFlags.ContributeGI) != 0)
            {
                isStatic = true;
            }
#else
            if ((flags & StaticEditorFlags.LightmapStatic) != 0)
            {
                isStatic = true;
            }
#endif

            return isStatic;
        }

        IEnumerator SetProbes(int subVolumeIndex = 0)
        {
            currentPass = "Setting Probes...";
            currentPassProgressCounter = 0;
            currentPassProgressFrameSkipper = 0;

            bool finished = false;

            if (subVolumesDivided.Count > 0)
            {
                if (currentVolumePart == subVolumesDivided.Count - 1)
                {
                    finished = true;
                }
            }
            else
            {
                if (currentVolumePart == subVolumesDivided.Count)
                {
                    finished = true;
                }
            }

            if (debugMode)
            {
                finalDebugAcceptedPoints.AddRange(debugAcceptedPoints);
                finalDebugCulledPoints.AddRange(debugCulledPoints);

                if (finished)
                {
                    Material accepted = new Material(Shader.Find("MagicLightProbes/VolumeBounds"));
                    Material culled = new Material(Shader.Find("MagicLightProbes/VolumeBounds"));

                    accepted.SetColor("_MainColor", Color.green);
                    culled.SetColor("_MainColor", Color.red);

                    if (debugPass == DebugPasses.GeometryIntersections ||
                        debugPass == DebugPasses.NearGeometry ||
                        debugPass == DebugPasses.OutOfRangeBorders ||
                        debugPass == DebugPasses.ShadingBorders ||
                        debugPass == DebugPasses.ContrastAreas ||
                        debugPass == DebugPasses.NearLights ||
                        debugPass == DebugPasses.LightIntensity ||
                        debugPass == DebugPasses.EqualProbes ||
                        debugPass == DebugPasses.EqualColor)
                    {
                        drawMode = DrawModes.Accepted;
                    }

                    if (debugPass == DebugPasses.UnlitProbes)
                    {
                        drawMode = DrawModes.Culled;
                    }

                    switch (drawMode)
                    {
                        case DrawModes.Accepted:
                            for (int i = 0; i < finalDebugAcceptedPoints.Count; i++)
                            {
                                DrawAccepted(i);

                                if (UpdateProgress(finalDebugAcceptedPoints.Count, 1000))
                                {
                                    yield return null;
                                }
                            }
                            break;
                        case DrawModes.Culled:
                            for (int i = 0; i < finalDebugCulledPoints.Count; i++)
                            {
                                DrawCulled(i);

                                if (UpdateProgress(finalDebugCulledPoints.Count))
                                {
                                    yield return null;
                                }
                            }
                            break;
                        case DrawModes.Both:
                            for (int i = 0; i < finalDebugAcceptedPoints.Count; i++)
                            {
                                DrawAccepted(i);

                                if (UpdateProgress(finalDebugAcceptedPoints.Count))
                                {
                                    yield return null;
                                }
                            }

                            currentPassProgressCounter = 0;
                            currentPassProgressFrameSkipper = 0;

                            for (int i = 0; i < finalDebugCulledPoints.Count; i++)
                            {
                                DrawCulled(i);

                                if (UpdateProgress(finalDebugCulledPoints.Count))
                                {
                                    yield return null;
                                }
                            }
                            break;
                    }
                }
            }
            else
            {
                if (workflow == Workflow.Advanced)
                {
                    string dirPath = assetEditorPath + "/Scene Data/" + SceneManager.GetActiveScene().name + "/" + name + "/ContrastPointsData";
                    string fullFilePath = dirPath + "/" + name + "_" + "vol_" + subVolumeIndex + "_ContrastPointsData.mlpdat";

                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }

                    List<TempPointData> tempPointDatas = new List<TempPointData>();


                    switch (fillingMode)
                    {
                        case FillingMode.SeparateFilling:
                            tmpSharedPointsArray.AddRange(tmpContrastShadingBordersPoints);
                            tmpSharedPointsArray.AddRange(tmpContrastOnOutOfRangePoints);
                            tmpSharedPointsArray.AddRange(tmpNearbyGeometryPoints);
                            tmpSharedPointsArray.AddRange(tmpPointsNearGeometryIntersections);

                            currentVolume.localContrastPoints.AddRange(tmpContrastShadingBordersPoints);
                            currentVolume.localContrastPoints.AddRange(tmpContrastOnOutOfRangePoints);
                            currentVolume.localNearbyGeometryPoints.AddRange(tmpNearbyGeometryPoints);
                            break;
                        case FillingMode.FullFilling:
                            tmpSharedPointsArray.AddRange(tmpNearbyGeometryPoints);
                            break;
                    }

                    for (int i = 0; i < currentVolume.localContrastPoints.Count; i++)
                    {
                        tempPointDatas.Add(new TempPointData(currentVolume.localContrastPoints[i].position));
                    }

                    MLPDataSaver.SaveData(tempPointDatas, fullFilePath);
                    tempPointDatas.Clear();

                    for (int i = 0; i < tmpSharedPointsArray.Count; i++)
                    {
                        points.Add(tmpSharedPointsArray[i].position);

                        if (!isInBackground)
                        {
                            if (UpdateProgress(tmpSharedPointsArray.Count, 1000))
                            {
                                yield return null;
                            }
                        }
                    }
                }
                else
                {
                    List<Vector3> resultPoints = new List<Vector3>();

                    resultPoints.AddRange(currentVolume.resultNearbyGeometryPointsPositions);
                    resultPoints.AddRange(currentVolume.resultLocalCornerPointsPositions);
                    resultPoints.AddRange(currentVolume.resultLocalFreePointsPositions);

                    for (int i = 0; i < resultPoints.Count; i++)
                    {
                        points.Add(resultPoints[i]);

                        if (!isInBackground)
                        {
                            if (UpdateProgress(resultPoints.Count, 1000))
                            {
                                yield return null;
                            }
                        }
                    }
                }

                if (finished)
                {
                    localFinishedPositions.AddRange(points);
                    totalProbesInVolume = localFinishedPositions.Count;
                }
            }

            calculatingVolumeSubPass = false;
        }

        IEnumerator SetProbesForQuickEditing(MLPVolume currentSubVolume = null, int subVolumeIndex = 0)
        {
            currentSubVolume.localAcceptedPoints.Clear();

            if (currentSubVolume.localContrastPoints.Count == 0)
            {
                string dirPath = assetEditorPath + "/Scene Data/" + SceneManager.GetActiveScene().name + "/" + name + "/ContrastPointsData";
                string fullFilePath = dirPath + "/" + name + "_" + "vol_" + subVolumeIndex + "_ContrastPointsData.mlpdat";

                List<TempPointData> tempPointDatas = new List<TempPointData>();

                tempPointDatas = MLPDataSaver.LoadData(tempPointDatas, fullFilePath);

                if (tempPointDatas.Count > 0)
                {
                    for (int i = 0; i < tempPointDatas.Count; i++)
                    {
                        MLPPointData pointData = new MLPPointData();

                        pointData.position = new Vector3(tempPointDatas[i].xPos, tempPointDatas[i].yPos, tempPointDatas[i].zPos);
                        currentSubVolume.localContrastPoints.Add(pointData);
                    }
                }
            }

            currentSubVolume.localAcceptedPoints.AddRange(currentSubVolume.localContrastPoints);
            currentSubVolume.localAcceptedPoints.AddRange(currentSubVolume.localNearbyGeometryPoints);

            currentPass = "Setting Probes...";
            currentPassProgressCounter = 0;
            currentPassProgressFrameSkipper = 0;

            for (int i = 0; i < currentSubVolume.localAcceptedPoints.Count; i++)
            {
                points.Add(currentSubVolume.localAcceptedPoints[i].position);

                if (UpdateProgress(currentSubVolume.localAcceptedPoints.Count, 1000))
                {
                    yield return null;
                }
            }

            currentPassProgressCounter = 0;
            currentPassProgressFrameSkipper = 0;

            for (int i = 0; i < currentSubVolume.resultLocalEquivalentPointsPositions.Count; i++)
            {
                points.Add(currentSubVolume.resultLocalEquivalentPointsPositions[i]);

                if (UpdateProgress(currentSubVolume.resultLocalEquivalentPointsPositions.Count, 1000))
                {
                    yield return null;
                }
            }

            currentPassProgressCounter = 0;
            currentPassProgressFrameSkipper = 0;

            for (int i = 0; i < currentSubVolume.resultLocalUnlitPointsPositions.Count; i++)
            {
                points.Add(currentSubVolume.resultLocalUnlitPointsPositions[i]);

                if (UpdateProgress(currentSubVolume.resultLocalUnlitPointsPositions.Count, 1000))
                {
                    yield return null;
                }
            }

            currentPassProgressCounter = 0;
            currentPassProgressFrameSkipper = 0;

            for (int i = 0; i < currentSubVolume.resultLocalCornerPointsPositions.Count; i++)
            {
                points.Add(currentSubVolume.resultLocalCornerPointsPositions[i]);

                if (UpdateProgress(currentSubVolume.resultLocalCornerPointsPositions.Count, 1000))
                {
                    yield return null;
                }
            }

            currentPassProgressCounter = 0;
            currentPassProgressFrameSkipper = 0;

            for (int i = 0; i < currentSubVolume.resultLocalFreePointsPositions.Count; i++)
            {
                points.Add(currentSubVolume.resultLocalFreePointsPositions[i]);

                if (UpdateProgress(currentSubVolume.resultLocalFreePointsPositions.Count, 1000))
                {
                    yield return null;
                }
            }

            int[] indeces = new int[points.Count];
            Vector3[] positions = new Vector3[points.Count];
            Lightmapping.Tetrahedralize(points.ToArray(), out indeces, out positions);

            List<Vector3> allPositions = new List<Vector3>();
            allPositions.AddRange(localFinishedPositions);
            allPositions.AddRange(positions);

            localFinishedPositions.Clear();
            localFinishedPositions.AddRange(allPositions);
            totalProbesInVolume = localFinishedPositions.Count;
            calculatingVolumeSubPass = false;
            SceneView.RepaintAll();
        }

        private void DrawAccepted(int i)
        {
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            point.AddComponent<MLPPoint>();
            point.GetComponent<Collider>().enabled = false;
            point.name = "Probe " + currentPassProgressCounter;
            point.transform.localScale = Vector3.one * debugObjectScale;
            point.transform.position = finalDebugAcceptedPoints[i].position;
            point.transform.parent = probesVolume.transform;

            Material currentMaterial = new Material(Shader.Find("MagicLightProbes/VolumeBounds"));

            switch (debugPass)
            {
                case DebugPasses.LightIntensity:
                    currentMaterial.SetColor("_MainColor", Color.Lerp(Color.red, Color.green, finalDebugAcceptedPoints[i].lightIntensity) / 1f);
                    break;
                case DebugPasses.MaximumHeight:
                case DebugPasses.OutOfRangeBorders:
                case DebugPasses.ShadingBorders:
                case DebugPasses.ContrastAreas:
                case DebugPasses.OutOfRange:
                case DebugPasses.EqualProbes:
                case DebugPasses.NearLights:
                    if (debugShowLightIntensity)
                    {
                        currentMaterial.SetColor("_MainColor", Color.Lerp(Color.red, Color.green, finalDebugAcceptedPoints[i].lightIntensity) / 1f);
                    }
                    else
                    {
                        currentMaterial.SetColor("_MainColor", Color.green);
                    }
                    break;
                case DebugPasses.GeometryIntersections:
                case DebugPasses.NearGeometry:
                    if (finalDebugAcceptedPoints[i].inCorner)
                    {
                        currentMaterial.SetColor("_MainColor", Color.blue);
                    }
                    else
                    {
                        currentMaterial.SetColor("_MainColor", Color.green);
                    }
                    break;
                case DebugPasses.EqualColor:
                    if (finalDebugAcceptedPoints[i].equalColor)
                    {
                        currentMaterial.SetColor("_MainColor", Color.red);
                    }
                    else
                    {
                        currentMaterial.SetColor("_MainColor", Color.green);
                    }
                    break;
            }

            point.GetComponent<MeshRenderer>().material = currentMaterial;

            MLPPoint litPointObject = point.GetComponent<MLPPoint>();

            litPointObject.col = finalDebugAcceptedPoints[i].col;
            litPointObject.row = finalDebugAcceptedPoints[i].row;
            litPointObject.depth = finalDebugAcceptedPoints[i].depth;
            litPointObject.position = finalDebugAcceptedPoints[i].position;
            litPointObject.lightIntencity = finalDebugAcceptedPoints[i].lightIntensity;
            litPointObject.equivalentIntensity = finalDebugAcceptedPoints[i].equalIntensity;
            litPointObject.outOfRange = finalDebugAcceptedPoints[i].outOfRange;
            litPointObject.contrastOnShadingArea = finalDebugAcceptedPoints[i].contrastOnShadingArea;
            litPointObject.contrastOnOutOfRangeArea = finalDebugAcceptedPoints[i].contrastOnOutOfRangeArea;
            litPointObject.savedNearGeometry = finalDebugAcceptedPoints[i].savedNearGeometry;
            litPointObject.savedOnGeometryIntersection = finalDebugAcceptedPoints[i].inCorner;
            litPointObject.inRangeForLights = finalDebugAcceptedPoints[i].inRangeForLights;
            litPointObject.inShadowForLight = finalDebugAcceptedPoints[i].inShadowForLights;
            litPointObject.nearbyPoints = finalDebugAcceptedPoints[i].nearbyPoints;
            litPointObject.saveOnGeometryEdge = finalDebugAcceptedPoints[i].onGeometryEdge;
            litPointObject.xStartPoint = finalDebugAcceptedPoints[i].xStartPoint;
            litPointObject.yStartPoint = finalDebugAcceptedPoints[i].yStartPoint;
            litPointObject.zStartPoint = finalDebugAcceptedPoints[i].zStartPoint;
            litPointObject.xEndPoint = finalDebugAcceptedPoints[i].xEndPoint;
            litPointObject.yEndPoint = finalDebugAcceptedPoints[i].yEndPoint;
            litPointObject.zEndPoint = finalDebugAcceptedPoints[i].zEndPoint;

            tempObjects.Add(point);
        }

        private void DrawCulled(int i)
        {
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            point.AddComponent<MLPPoint>();
            point.GetComponent<Collider>().enabled = false;
            point.name = "Probe " + currentPassProgressCounter;
            point.transform.localScale = Vector3.one * debugObjectScale;
            point.transform.position = finalDebugCulledPoints[i].position;
            point.transform.parent = probesVolume.transform;

            Material currentMaterial = new Material(Shader.Find("MagicLightProbes/VolumeBounds"));
            currentMaterial.SetColor("_MainColor", Color.red);
            point.GetComponent<MeshRenderer>().material = currentMaterial;

            MLPPoint litPointObject = point.GetComponent<MLPPoint>();

            litPointObject.position = finalDebugCulledPoints[i].position;
            litPointObject.lightIntencity = finalDebugCulledPoints[i].lightIntensity;
            litPointObject.outOfRange = finalDebugCulledPoints[i].outOfRange;
            litPointObject.contrastOnShadingArea = finalDebugCulledPoints[i].contrastOnShadingArea;
            litPointObject.contrastOnOutOfRangeArea = finalDebugCulledPoints[i].contrastOnOutOfRangeArea;

            tempObjects.Add(point);
        }

        public void CombineVolumes(MagicLightProbes[] volumes)
        {
            if (combinedVolumeObject == null)
            {
                combinedVolumeObject = GameObject.Find("-- MLP Combined Volume --");
                combinedVolumeComponent = combinedVolumeObject.GetComponent<MLPCombinedVolume>();
            }

            //if (combinedVolumeObject == null)
            //{
            //    combinedVolumeObject = new GameObject("-- MLP Combined Volume --", typeof(LightProbeGroup));
            //    combinedVolumeComponent = combinedVolumeObject.AddComponent<MLPCombinedVolume>();
            //}

            LightProbeGroup finalProbeGroup;
            finalProbeGroup = combinedVolumeComponent.GetComponent<LightProbeGroup>();

            List<Vector3> positions = new List<Vector3>();
            List<Vector3> candidates = new List<Vector3>();

            finalProbeGroup.probePositions = null;

            if (combinedVolumeComponent.customPositions.Count > 0)
            {
                volumes[0].localFinishedPositions.AddRange(combinedVolumeComponent.customPositions);
            }
            else
            {
                for (int i = 0; i < volumes[0].localFinishedPositions.Count; i++)
                {
                    for (int j = 0; j < combinedVolumeComponent.customProbesToRemove.Count; j++)
                    {
                        if (volumes[0].localFinishedPositions[i] == combinedVolumeComponent.customProbesToRemove[j])
                        {
                            volumes[0].localFinishedPositions.RemoveAt(i);
                        }
                    }                    
                }

                combinedVolumeComponent.customProbesToRemove.Clear();
            }

            int counter = 0;

            foreach (var volume in volumes)
            {
                if (cancelCombination)
                {
                    cancelCombination = false;
                    break;
                }

                int p_counter = 0;

                if (volume.recombinationNeeded)
                {
                    volume.transformedPoints.Clear();                                     

                    foreach (var point in volume.localFinishedPositions)
                    {
                        if (cancelCombination)
                        {
                            cancelCombination = false;
                            break;
                        }

                        Vector3 transformedPoint = finalProbeGroup.transform.InverseTransformPoint(point);
                        //Vector3 transformedPoint = point;

                        positions.Add(transformedPoint);
                        volume.transformedPoints.Add(new Vector3(transformedPoint.x, transformedPoint.y, transformedPoint.z));

                        EditorUtilityDisplayProgressBar.ShowCancelableProgressForCombination(this,
                            "Transform Points For Volume " + volume.name + " ... " + p_counter + "/" + volume.localFinishedPositions.Count,
                            p_counter, volume.localFinishedPositions.Count);

                        p_counter++;
                    }

                    volume.recombinationNeeded = false;
                }
                else
                {
                    positions.AddRange(volume.transformedPoints);
                }

                EditorUtilityDisplayProgressBar.ShowCancelableProgressForCombination(this, "Combine Volumes... " + counter + "/" + volumes.Length, counter, volumes.Length);
                counter++;
            }

            //counter = 0;

            //for (int i = 0; i < positions.Count; i++)
            //{
            //    int approved = 0;

            //    for (int j = 0; j < positions.Count; j++)
            //    {
            //        if (positions[j] != positions[i])
            //        {
            //            if (Vector3.Distance(positions[i], positions[j]) >= Mathf.Min(cornersDetectionThresholds.ToArray()) * 0.5f)
            //            {
            //                approved++;
            //            }
            //            else
            //            {
            //                positions.RemoveAt(i);
            //            }
            //        }
            //        else
            //        {
            //            approved++;
            //        }
            //    }

            //    if (approved == positions.Count)
            //    {
            //        candidates.Add(positions[i]);
            //    }

            //    EditorUtilityDisplayProgressBar.ShowProgress("Optimize Positions... " + counter + "/" + positions.Count + " Approved: " + candidates.Count, counter, positions.Count);
            //    counter++;
            //}

            EditorUtilityDisplayProgressBar.HideProgress();
            
            candidates.AddRange(positions);
            positions.Clear();
            positions.AddRange(candidates);
            candidates.Clear();

            int[] indeces = new int[positions.Count];
            Vector3[] finalPositions = new Vector3[positions.Count];
            Lightmapping.Tetrahedralize(positions.ToArray(), out indeces, out finalPositions);

            finalProbeGroup.probePositions = null;
            finalProbeGroup.probePositions = finalPositions;
            combinedVolumeComponent.combined = true;

            //totalProbesInVolume = finalProbeGroup.probePositions.Length;

            EditorUtilityDisplayProgressBar.ShowMessage("Calculation finished.");

            Debug.Log("<color=yellow>MLP:</color> All volumes merged successfully.");

            Selection.activeGameObject = finalProbeGroup.gameObject;
        }

        public bool CheckIfInside(VolumeParameters volume, Vector3 position)
        {
            float minX = volume.position.x - (volume.demensions.x / 2);
            float maxX = volume.position.x + (volume.demensions.x / 2);
            float minY = volume.position.y - (volume.demensions.y / 2);
            float maxY = volume.position.y + (volume.demensions.y / 2);
            float minZ = volume.position.z - (volume.demensions.z / 2);
            float maxZ = volume.position.z + (volume.demensions.z / 2);

            return
                (position.x >= minX && position.x <= maxX) &&
                (position.y >= minY && position.y <= maxY) &&
                (position.z >= minZ && position.z <= maxZ);
        }

        public bool CheckIfInside(Transform volumeParameners, Vector3 position)
        {
            float minX = volumeParameners.position.x - (volumeParameners.localScale.x / 2);
            float maxX = volumeParameners.position.x + (volumeParameners.localScale.x / 2);
            float minY = volumeParameners.position.y - (volumeParameners.localScale.y / 2);
            float maxY = volumeParameners.position.y + (volumeParameners.localScale.y / 2);
            float minZ = volumeParameners.position.z - (volumeParameners.localScale.z / 2);
            float maxZ = volumeParameners.position.z + (volumeParameners.localScale.z / 2);

            return
                (position.x >= minX && position.x <= maxX) &&
                (position.y >= minY && position.y <= maxY) &&
                (position.z >= minZ && position.z <= maxZ);
        }

        public void CancelCalculation()
        {
            EditorApplication.update -= SubPassIteratorUpdate;
            EditorApplication.update -= MainIteratorUpdate;
            lightProbesVolumeCalculatingSubRoutine = null;
            lightProbesVolumeCalculatingRoutine = null;

            RestoreScene();
            calculatingVolume = false;

            Debug.LogFormat("<color=yellow>MLP:</color> Calculation of \"" + name + "\" is stopped. Scene state has been restored.");
        }
#endif
    }

#if UNITY_EDITOR
    public class EditorUtilityDisplayProgressBar : EditorWindow
    {
        public static void ShowMessage(string text)
        {
            EditorUtility.DisplayDialog("Magic Light Probes", text, "OK");
        }

        public static void ShowProgress(string action, float current, float total)
        {
            EditorUtility.DisplayProgressBar("Magic Light Probes", action, current / total);
        }

        public static void HideProgress()
        {
            EditorUtility.ClearProgressBar();
        }

        public static void ShowCancelableProgress(MagicLightProbes parent, string action, float current, float total)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Magic Light Probes", action, current / total))
            {
                parent.CancelCalculation();
            }
        }

        public static void ShowCancelableProgressForCombination(MagicLightProbes parent, string action, float current, float total)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Magic Light Probes", action, current / total))
            {
                parent.cancelCombination = true;
            }
        }
    }

    public static class EditorProgressBar
    {
        static MethodInfo m_Display = null;
        static MethodInfo m_Clear = null;
        static EditorProgressBar()
        {
            var type = typeof(Editor).Assembly.GetTypes().Where(t => t.Name == "AsyncProgressBar").FirstOrDefault();
            if (type != null)
            {
                m_Display = type.GetMethod("Display");
                m_Clear = type.GetMethod("Clear");
            }
        }

        public static void ShowProgressBar(string aText, float aProgress)
        {
            if (m_Display != null)
                m_Display.Invoke(null, new object[] { aText, aProgress });
        }
        public static void ClearProgressBar()
        {
            if (m_Clear != null)
                m_Clear.Invoke(null, null);
        }
    }
#endif    
}