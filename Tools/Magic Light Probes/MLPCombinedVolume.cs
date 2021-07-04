using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MagicLightProbes
{
    [ExecuteInEditMode]
    [HelpURL("https://motion-games-studio.gitbook.io/magic-light-probes/system-components/mlp-combined-volume")]
    public class MLPCombinedVolume : MonoBehaviour
    {
#if UNITY_EDITOR
        [HideInInspector] public static MLPCombinedVolume Instance;
        [HideInInspector] public List<Vector3> customPositions = new List<Vector3>();
        [HideInInspector] public List<Vector3> customProbesToRemove = new List<Vector3>();
        [HideInInspector] public float distanceFromGeometry;
        [HideInInspector] public bool pressed = false;
        [HideInInspector] public MagicLightProbes magicLightProbes;
        [HideInInspector] public bool combined;
        [HideInInspector] public LightProbeGroup targetProbeGroup;
        [HideInInspector] public static bool forceRecombine;
        [HideInInspector] public bool warningShow;

        private Event currentEvent;
        private Vector3 mousePos;
        private float pixelsPerPoint;
        private int prevProbesCount;
        private Ray ray;
        private RaycastHit hit;
        private MagicLightProbes[] volumes;
        private const string volumeUpdateRequiredMessage = "Some volumes have been changed. The combined volume needs to be updated.";

        private void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui  -= OnScene;
            SceneView.duringSceneGui  += OnScene;
#else
            SceneView.onSceneGUIDelegate -= OnScene;
            SceneView.onSceneGUIDelegate += OnScene;
#endif
        }

        private void OnDisable()
        {            
            ReactivateCombinedVolumeObject();            
        }

        private void OnDestroy()
        {
            //RecreateCombinedVolumeObject(combined);

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui  -= OnScene;
#else
            SceneView.onSceneGUIDelegate -= OnScene;
#endif
        }

        void OnScene(SceneView scene)
        {            
            if (Selection.activeObject != gameObject)
            {
                pressed = false;
                return;
            }
            
            volumes = FindObjectsOfType<MagicLightProbes>();

            if (volumes.Length > 0)
            {
                if (Selection.activeGameObject == gameObject && !combined)
                {
                    EditorUtility.DisplayDialog("Magic Light Probes", volumeUpdateRequiredMessage, "OK");
                    volumes[0].CombineVolumes(volumes);
                }

                currentEvent = Event.current;

                if (pressed && currentEvent.type == EventType.MouseDown && currentEvent.button == 1)
                {
                    mousePos = currentEvent.mousePosition;

                    pixelsPerPoint = EditorGUIUtility.pixelsPerPoint;
                    mousePos.y = scene.camera.pixelHeight - mousePos.y * pixelsPerPoint;
                    mousePos.x *= pixelsPerPoint;

                    ray = scene.camera.ScreenPointToRay(mousePos);                    

                    if (Physics.Raycast(ray, out hit))
                    {
                        List<Vector3> tempPositionArray = new List<Vector3>();

                        if (targetProbeGroup == null)
                        {
                            targetProbeGroup = GetComponent<LightProbeGroup>();
                        }

                        Vector3 position = hit.point;
                        position = position + (hit.normal * distanceFromGeometry);


                        tempPositionArray.AddRange(targetProbeGroup.probePositions);
                        tempPositionArray.Add(position);

                        customPositions.Add(position);

                        targetProbeGroup.probePositions = null;
                        targetProbeGroup.probePositions = tempPositionArray.ToArray();
                    }

                    currentEvent.Use();
                }
            }

            if (targetProbeGroup.probePositions.Length != prevProbesCount)
            {
                prevProbesCount = targetProbeGroup.probePositions.Length;

                List<Vector3> validPoints = new List<Vector3>();
                
                for (int i = 0; i < targetProbeGroup.probePositions.Length; i++)
                {
                    if (customPositions.Contains(targetProbeGroup.probePositions[i]))
                    {
                        validPoints.Add(targetProbeGroup.probePositions[i]);                        
                    }
                }

                customPositions = new List<Vector3>(validPoints);
            }
        }

        public static MLPCombinedVolume CreateCombinedVolumeObject(bool combined = false)
        {
            GameObject combinedVolumeObject = new GameObject("-- MLP Combined Volume --", typeof(LightProbeGroup), typeof(MLPCombinedVolume));

            MLPCombinedVolume combinedVolumeComponent;

            combinedVolumeComponent = combinedVolumeObject.GetComponent<MLPCombinedVolume>();
            combinedVolumeComponent.targetProbeGroup = combinedVolumeObject.GetComponent<LightProbeGroup>();
            combinedVolumeComponent.combined = combined;
            combinedVolumeObject.transform.parent = GameObject.Find("Magic Light Probes").transform;

            return combinedVolumeComponent;
        }

        private void OnDrawGizmosSelected()
        {
            for (int i = 0; i < customPositions.Count; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(customPositions[i], 0.1f);
            }
        }

        private void RecreateCombinedVolumeObject(bool combined)
        {
            MagicLightProbes[] mlpVolumes = FindObjectsOfType<MagicLightProbes>();

            if (mlpVolumes.Length > 0)
            {
                MLPCombinedVolume combinedVolumeComponent = CreateCombinedVolumeObject(combined);

                for (int i = 0; i < mlpVolumes.Length; i++)
                {
                    combinedVolumeComponent.magicLightProbes = mlpVolumes[i];
                    mlpVolumes[i].combinedVolumeComponent = combinedVolumeComponent;
                }
            }
        }

        private void ReactivateCombinedVolumeObject()
        {
            MagicLightProbes[] mlpVolumes = FindObjectsOfType<MagicLightProbes>();

            if ((!gameObject.activeInHierarchy || !enabled) && mlpVolumes.Length > 0)
            {
                warningShow = true;
                Invoke("ReactivateObject", 0);
                Invoke("DisableWarning", 5);
            }
        }

        private void ReactivateObject()
        {
            gameObject.SetActive(true);
            enabled = true;
        }

        private void DisableWarning()
        {
            warningShow = false;
        }
#endif
    }
}
