using UnityEngine;

namespace MagicLightProbes
{
    [System.Serializable]
    public class MLPTracePoint
    {
        public Vector3 position;
        public Vector3 forward;
        public string name;
        public GameObject pointGameObject;
    }
}
