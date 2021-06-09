using System.Collections.Generic;
using UnityEngine;

namespace PrefabBrush.PrefabBrushData
{
    [CreateAssetMenu(fileName = "[NEW]PB_SaveFile", menuName = "PrefabBrush/Prefab Brush Save", order = 0), System.Serializable]
    public class PB_SaveObject : ScriptableObject
    {
        public List<GameObject> prefabList = new List<GameObject>();
        public List<PB_PrefabData> prefabData = new List<PB_PrefabData>();

        public PB_PaintType paintType = PB_PaintType.Surface;

        public float brushSize = 1;
        public float minBrushSize = .1f, maxBrushSize = 20;
        public float paintDeltaDistance = .4f;
        public float maxPaintDeltaDistance = 3, minPaintDeltaDistance = .1f;
        public int prefabsPerStroke = 1;
        public int maxprefabsPerStroke = 20, minprefabsPerStroke = 1;

        public float spawnHeight = 10;
        public bool addRigidbodyToPaintedPrefab = true;
        public float physicsIterations = 100;

        public bool checkLayer = false;
        public bool checkTag = false;
        public bool checkSlope = false;

        public bool ignorePaintedPrefabs = false;
        public bool ignoreTriggers = true;

        public PB_Direction chainPivotAxis;
        public PB_Direction chainDirection;

        public int requiredTagMask, requiredLayerMask;
        public float minRequiredSlope, maxRequiredSlope;

        public Vector3 prefabOriginOffset, prefabRotationOffset;

        public PB_DragModType draggingAction;
        public PB_Direction rotationAxis;
        public float rotationSensitivity = 10;

        public PB_ParentingStyle parentingStyle;
        public GameObject parent;

        public bool rotateToMatchSurface = false;
        public PB_Direction rotateSurfaceDirection;

        public bool randomizeRotation;
        public float minXRotation, maxXRotation;
        public float minYRotation, maxYRotation;
        public float minZRotation, maxZRotation;

        public PB_ScaleType scaleType;
        public PB_SaveApplicationType scaleApplicationType;
        public float minScale = 1, maxScale = 1;
        public float minXScale = 1, maxXScale = 1;
        public float minYScale = 1, maxYScale = 1;
        public float minZScale = 1, maxZScale = 1;

        public List<GameObject> parentList = new List<GameObject>();

        public float eraseBrushSize = 1;
        public float minEraseBrushSize = .1f, maxEraseBrushSize = 20;
        public PB_EraseTypes eraseType;

        public bool checkLayerForErase = false;
        public bool checkTagForErase = false;
        public bool checkSlopeForErase = false;
        public bool mustBeSelectedInBrush = false;

        public int requiredTagMaskForErase, requiredLayerMaskForErase;
        public float minRequiredSlopeForErase, maxRequiredSlopeForErase;

        public PB_EraseDetectionType eraseDetection;

        public KeyCode paintBrushHotKey = KeyCode.P;
        public bool paintBrushHoldKey = true;

        public KeyCode removeBrushHotKey = KeyCode.LeftControl;
        public bool removeBrushHoldKey = true;

        public KeyCode disableBrushHotKey = KeyCode.I;
        public bool disableBrushHoldKey = true;

        public void AddPrefab(GameObject prefab)
        {
            PB_PrefabData newData = new PB_PrefabData();
            newData.prefab = prefab;
            newData.selected = true;
            prefabData.Add(newData);
        }

        public void UpgradeSave()
        {
            if (prefabList.Count == 0)
                return;

            for(int i = 0; i < prefabList.Count; i++)
            {
                AddPrefab(prefabList[i]);
            }

            prefabList.Clear();
        }

        public List<PB_PrefabData> GetActivePrefabs()
        {
            List<PB_PrefabData> allData = new List<PB_PrefabData>();

            for (int i = 0; i < prefabData.Count; i++)
            {
                if (prefabData[i].selected)
                    allData.Add(prefabData[i]);
            }

            return allData;
        }
    }
}