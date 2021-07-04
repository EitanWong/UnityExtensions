using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MagicLightProbes
{
    [ExecuteInEditMode]
    public class MLPVolume : MonoBehaviour
    {
        [HideInInspector]
        public MagicLightProbes parentRootComponent;
        [HideInInspector]
        public MeshRenderer selfRenderer;
        [HideInInspector]
        public bool showGizmo;
        public bool showGizmoSelected;
        public bool isPartVolume;
        public bool isSubdividedPart;
        public bool isCalculated;
        public bool isInProcess;
        public bool skipped;
        public int id;
        public Color colorOnSelection;        

        public List<MLPPointData> localAcceptedPoints = new List<MLPPointData>();
        public List<MLPPointData> localNearbyGeometryPoints = new List<MLPPointData>();        
        public List<MLPPointData> localContrastPoints = new List<MLPPointData>();
        public List<MLPPointData> localCornerPoints = new List<MLPPointData>();
        public List<Vector3> localNearbyGeometryPointsPositions = new List<Vector3>();
        public List<Vector3> resultNearbyGeometryPointsPositions = new List<Vector3>();
        public List<Vector3> localCornerPointsPositions = new List<Vector3>();
        public List<Vector3> resultLocalCornerPointsPositions = new List<Vector3>();
        public List<Vector3> localEquivalentPointsPositions = new List<Vector3>();
        public List<Vector3> resultLocalEquivalentPointsPositions = new List<Vector3>();
        public List<Vector3> resultLocalFreePointsPositions = new List<Vector3>();
        public List<Vector3> localUnlitPointsPositions = new List<Vector3>();
        public List<Vector3> localFreePointsPositions = new List<Vector3>();
        public List<Vector3> resultLocalUnlitPointsPositions = new List<Vector3>();
        public List<Vector3> localDirections = new List<Vector3>();
        public List<Vector3> localAvaragedDirections = new List<Vector3>();
        public List<MLPPointData> localColorThresholdEditingPoints = new List<MLPPointData>();
        public int objectsInside;

#if UNITY_EDITOR

        private void Start()
        {
            parentRootComponent = GetComponentInParent<MagicLightProbes>();
            selfRenderer = GetComponent<MeshRenderer>();
        }

        private static void DrawVolumeWithBounds(MLPVolume volume, Color mainColor, Color boundsColor)
        {
            //Gizmos.matrix = volume.transform.localToWorldMatrix;

            Gizmos.color = mainColor;
            Gizmos.DrawCube(volume.transform.position, volume.transform.localScale);
            Gizmos.color = boundsColor;
            Gizmos.DrawWireCube(volume.transform.position, volume.transform.localScale);
        }

        private static void DrawVolumeWithOnlyBounds(MLPVolume volume, Color boundsColor)
        {
            //Gizmos.matrix = volume.transform.localToWorldMatrix;

            Gizmos.color = boundsColor;
            Gizmos.DrawWireCube(volume.transform.parent.position, volume.transform.parent.localScale);
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.InSelectionHierarchy | GizmoType.Active)]
        private static void DrawGizmoOnSelection(MLPVolume volume, GizmoType gizmoType)
        {
            if (volume.showGizmoSelected)
            {
                DrawVolumeWithBounds(volume, new Color(1, 1, 0, 0.5f), Color.yellow); 
            }
        }

        [DrawGizmo(GizmoType.NonSelected | GizmoType.NotInSelectionHierarchy)]
        private static void DrawGizmoAlways(MLPVolume volume, GizmoType gizmoType)
        {
            if (volume.parentRootComponent.calculatingVolume)
            {
                if (volume.parentRootComponent.subVolumesDivided.Count > 0)
                {
                    if (volume.isPartVolume)
                    {
                        if (volume.skipped)
                        {
                            DrawVolumeWithBounds(volume, Color.red, Color.red);
                            return;
                        }

                        if (volume.isCalculated)
                        {
                            DrawVolumeWithOnlyBounds(volume, Color.yellow);
                        }
                        else
                        {
                            if (volume.isInProcess)
                            {
                                DrawVolumeWithBounds(volume, Color.yellow, Color.red);
                            }
                            else
                            {
                                DrawVolumeWithOnlyBounds(volume, Color.yellow);
                            }
                        }
                    }
                }
                else
                {
                    if (volume.isInProcess)
                    {
                        DrawVolumeWithBounds(volume, Color.yellow, Color.yellow);
                    }
                    else
                    {
                        DrawVolumeWithBounds(volume, Color.red, Color.yellow);
                    }
                }

            }
            else
            {
                if (volume.showGizmo)
                {
                    DrawVolumeWithBounds(volume, new Color(0, 1, 0, 0.5f), Color.yellow);
                }
            }
        }
#endif
    }
}
