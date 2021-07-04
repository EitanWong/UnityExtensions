using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLightProbes
{
    public class MLPPointData
    {
        public float lightIntensity;
        public float maxIntencity;
        public Vector3 position;
        public int col;
        public int row;
        public int depth;
        public List<float> distancesToLights = new List<float>();
        public List<Vector3> collisionPoints = new List<Vector3>();
        public List<MLPPointData> nearbyPoints = new List<MLPPointData>();
        public List<MLPPointData> nearbyPointsOptimaziable = new List<MLPPointData>();
        public bool removed;
        public bool outOfRange;
        public bool inSaveRange;
        public bool savedNearGeometry;
        public bool contrastOnOutOfRangeArea;
        public bool inRangeOfLight;
        public bool contrastOnShadingArea;
        public bool equalIntensity;
        public bool equalColor;
        public bool borderPoint;
        public bool outOfMaxHeight;
        public float angleToLight;
        public Vector3 contactPoint;
        public bool inCorner;
        public bool lockForCull;
        public bool onGeometryEdge;
        public bool lightLeakLocked;
        public Vector3 collisionNormal;
        public List<MLPLight> inRangeForLights = new List<MLPLight>();
        public List<MLPLight> inShadowForLights = new List<MLPLight>();
        public GameObject temporaryObject;
        public GameObject collisionObject;
        public Color averagedColor;
        public bool xEndPoint;
        public bool yEndPoint;
        public bool zEndPoint;
        public bool xStartPoint;
        public bool yStartPoint;
        public bool zStartPoint;
        public int equivalent;
        public float avaragedColorValue;
        public bool isUnlit;

        public void SetPosition(Vector3 position)
        {
            this.position = position;
        }

        public void SetOutOfRange(bool visibility)
        {
            this.outOfRange = visibility;
        }

        public void SetLightIntensity(float intensity)
        {
            this.lightIntensity = intensity;
        }

        public void SetInSaveRange(bool value)
        {
            this.inSaveRange = value;
        }

        public void SetInContrastArea(bool value)
        {
            this.contrastOnShadingArea = value;
        }
    }
}
