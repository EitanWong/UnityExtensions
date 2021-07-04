using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if HDRPPACKAGE_EXIST
#if UNITY_2019_3_OR_NEWER
        using UnityEngine.Rendering.HighDefinition;
#else
using UnityEngine.Experimental.Rendering.HDPipeline;
#endif
#endif

namespace MagicLightProbes
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    [HelpURL("https://motion-games-studio.gitbook.io/magic-light-probes/system-components/mlp-light")]
    public class MLPLight : MonoBehaviour
    {
        public enum CalculationMode
        {
            AccurateShadows = 0,
            LightIntensity = 1
        };

        public enum MLPLightType
        {
            Spot = 0,
            Directional = 1,
            Point = 2,
            Area = 3,
            Mesh = 4
        }

        public enum MLPLightTypeMA
        {
            Area = 3,
            Mesh = 4
        }

        public enum TracePointSettingMode
        {
            Auto,
            Custom
        }

        public enum ShadowmaskMode
        {
            Shadowmask,
            DistanceShadowmask
        }

        public MLPLightType lightType;
        public MLPLightType lastLightType;
        public MLPLightTypeMA lightTypeMA;        
        public CalculationMode calculationMode;
        public TracePointSettingMode tracePointSettingMode;
        public LightmapBakeType lightMode;
        public ShadowmaskMode shadowmaskMode;
        public Light targetLight;
        public GameObject parentGameObject;
        public Vector3 position;
        public Vector3 forward;
        public bool saveNearbyProbes;
        public float saveRadius;
        public float range;
        public bool useSourceParameters;
        public bool reverseDirection;
        public float angle;
        public bool customTracePoints;
        public bool accurateTrace;
        public int accuracy;
        public int lastAccuracy;
        public bool isDirectional;
        public float tracePointSize = 0.3f;
        public float lastTracePointSize;
        public MeshFilter lastMesh;
        public List<GameObject> tracePoints = new List<GameObject>();
        public List<MLPTracePoint> tracePointsData = new List<MLPTracePoint>();
        public MLPTracePoint mainTracePoint;
        public MagicLightProbes parentVolume;
        public bool showOptionsInManagerWindow;
        public float intensity;
        public bool resetEditor;
        public bool showLightOnScene;
        public bool saveOnOutOfRange;
        public bool isHDRP;
        public Vector2 hdrpAreaSize;

#if UNITY_EDITOR
        private void Start()
        {
            targetLight = GetComponent<Light>();
        }

        private void Update()
        {
            position = transform.position;
            SetLightType();
            SetLightMode();
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 endPoint;
            Vector3 arrowEndPoint;

            if (lightType != MLPLightType.Directional)
            {
                if (tracePointsData.Count > 0)
                {
                    foreach (var tracePoint in tracePoints)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(tracePoint.transform.position, 0.05f);

                        Gizmos.color = Color.yellow;

                        switch (lightType)
                        {
                            case MLPLightType.Mesh:
                            case MLPLightType.Point:
                                //endPoint = tracePoint.transform.position + (tracePoint.transform.position - gameObject.transform.position) * 0.5f;
                                //arrowEndPoint = tracePoint.transform.position + (tracePoint.transform.position - gameObject.transform.position) * 0.2f;

                                //Gizmos.DrawLine(tracePoint.transform.position, endPoint);
                                //Gizmos.DrawLine(endPoint, arrowEndPoint + (tracePoint.transform.position + tracePoint.transform.right - arrowEndPoint).normalized * 0.1f);
                                //Gizmos.DrawLine(endPoint, arrowEndPoint + (tracePoint.transform.position - tracePoint.transform.right - arrowEndPoint).normalized * 0.1f);
                                break;
                            default:
                                if (reverseDirection)
                                {
                                    endPoint = tracePoint.transform.position + tracePoint.transform.forward * -0.3f;
                                    arrowEndPoint = tracePoint.transform.position + tracePoint.transform.forward * -0.2f;
                                }
                                else
                                {
                                    endPoint = tracePoint.transform.position + tracePoint.transform.forward * 0.3f;
                                    arrowEndPoint = tracePoint.transform.position + tracePoint.transform.forward * 0.2f;
                                }

                                Gizmos.DrawLine(tracePoint.transform.position, endPoint);
                                Gizmos.DrawLine(endPoint, arrowEndPoint + (tracePoint.transform.position + tracePoint.transform.right - arrowEndPoint).normalized * 0.1f);
                                Gizmos.DrawLine(endPoint, arrowEndPoint + (tracePoint.transform.position - tracePoint.transform.right - arrowEndPoint).normalized * 0.1f);
                                break;
                        }
                    }
                }
                else
                {
                    Gizmos.color = Color.yellow;

                    switch (lightType)
                    {
                        case MLPLightType.Area:
                            //Gizmos.DrawWireCube(transform.position, targetLight.areaSize);

                            if (reverseDirection)
                            {
                                endPoint = transform.position + transform.forward * -0.3f;
                                arrowEndPoint = transform.position + transform.forward * -0.2f;
                            }
                            else
                            {
                                endPoint = transform.position + transform.forward * 0.3f;
                                arrowEndPoint = transform.position + transform.forward * 0.2f;
                            }

                            Gizmos.DrawLine(transform.position, endPoint);
                            Gizmos.DrawLine(endPoint, arrowEndPoint + (transform.position + transform.right - arrowEndPoint).normalized * 0.1f);
                            Gizmos.DrawLine(endPoint, arrowEndPoint + (transform.position - transform.right - arrowEndPoint).normalized * 0.1f);
                            break;
                        default:
                            Gizmos.DrawSphere(transform.position, 0.3f);
                            break;
                    }
                }

                if (saveNearbyProbes)
                {
                    switch (lightType)
                    {
                        case MLPLightType.Area:
                            Gizmos.color = Color.white;
                            Gizmos.DrawWireSphere(position, saveRadius);
                            break;
                        default:
                            Gizmos.color = Color.white;
                            Gizmos.DrawWireSphere(position, saveRadius);
                            break;
                    }
                }

                if (lightType == MLPLightType.Mesh)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(position, range);
                }
            }
        }

        public void SetLightType()
        {
#if HDRPPACKAGE_EXIST
            if (GraphicsSettings.renderPipelineAsset is HDRenderPipelineAsset)
            {
                isHDRP = true;

                if (targetLight == null)
                {
                    lightType = MLPLightType.Mesh;
                }
                else
                {
                    HDAdditionalLightData hDAdditionalLightData = GetComponent<HDAdditionalLightData>();
#if UNITY_2019_3_OR_NEWER
                    if (hDAdditionalLightData.type == HDLightType.Directional)
                    {
                        lightType = MLPLightType.Directional;
                        return;
                    }

                    if (hDAdditionalLightData.type == HDLightType.Area)
                    {
                        lightType = MLPLightType.Area;
                        hdrpAreaSize = new Vector2(hDAdditionalLightData.shapeWidth, hDAdditionalLightData.shapeHeight);
                    }
                    else
                    {
                        if (targetLight.type == LightType.Spot)
                        {
                            lightType = MLPLightType.Spot;
                        }

                        if (targetLight.type == LightType.Point)
                        {
                            lightType = MLPLightType.Point;
                        }

                        if (targetLight.type == LightType.Directional)
                        {
                            lightType = MLPLightType.Directional;
                        }
                    }
#else
                    if (targetLight.type == LightType.Directional)
                    {
                        lightType = MLPLightType.Directional;
                        return;
                    }

                    switch (hDAdditionalLightData.lightTypeExtent)
                    {
                        case LightTypeExtent.Punctual:
                            if (targetLight.type == LightType.Spot)
                            {
                                lightType = MLPLightType.Spot;
                            }

                            if (targetLight.type == LightType.Point)
                            {
                                lightType = MLPLightType.Point;
                            }

                            if (targetLight.type == LightType.Directional)
                            {
                                lightType = MLPLightType.Directional;
                            }
                            break;
                        case LightTypeExtent.Rectangle:
                        case LightTypeExtent.Tube:
                            lightType = MLPLightType.Area;
                            hdrpAreaSize = new Vector2(hDAdditionalLightData.shapeWidth, hDAdditionalLightData.shapeHeight);
                            break;
                    }
#endif
                }
            }
#else
            isHDRP = false;

            if (targetLight == null)
            {
                lightType = MLPLightType.Mesh;
            }
            else
            {
                switch (targetLight.type)
                {
                    case LightType.Spot:
                        lightType = MLPLightType.Spot;
                        break;
                    case LightType.Directional:
                        lightType = MLPLightType.Directional;
                        break;
                    case LightType.Point:
                        lightType = MLPLightType.Point;
                        break;
                    case LightType.Area:
                        lightType = MLPLightType.Area;
                        break;
                }
            }

            lastLightType = lightType;
#endif
        }

        public void SetLightMode()
        {
            if (targetLight != null)
            {
                lightMode = targetLight.lightmapBakeType;
            }
        }


        private void OnDestroy()
        {
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }
#endif
    }
}