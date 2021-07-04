#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MagicLightProbes
{
    [ExecuteInEditMode]
    public class MLPPoint : MonoBehaviour
    {
        [ShowOnly]
        public float lightIntencity;
        [ShowOnly]
        public bool equivalentIntensity;
        [HideInInspector]
        public Vector3 position;
        [ShowOnly]
        public int col;
        [ShowOnly]
        public int row;
        [ShowOnly]
        public int depth;
        [ShowOnly]
        public bool outOfRange;
        [ShowOnly]
        public bool contrastOnShadingArea;
        [ShowOnly]
        public bool contrastOnOutOfRangeArea;
        [ShowOnly]
        public bool savedNearGeometry;        
        [ShowOnly]
        public bool savedOnGeometryIntersection;
        [ShowOnly]
        public bool saveOnGeometryEdge;
        [ShowOnly]
        public bool xStartPoint;
        [ShowOnly]
        public bool yStartPoint;
        [ShowOnly]
        public bool zStartPoint;
        [ShowOnly]
        public bool xEndPoint;
        [ShowOnly]
        public bool yEndPoint;
        [ShowOnly]
        public bool zEndPoint;
        [ShowOnly]
        public List<MLPLight> inRangeForLights = new List<MLPLight>();
        [ShowOnly]
        public List<MLPLight> inShadowForLight = new List<MLPLight>();
        [ShowOnly]
        public List<MLPPointData> nearbyPoints = new List<MLPPointData>();        
    }
}
#endif
