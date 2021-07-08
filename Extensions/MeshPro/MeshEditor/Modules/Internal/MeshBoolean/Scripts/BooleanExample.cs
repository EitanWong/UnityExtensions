#if UNITY_EDITOR
using UnityEngine;

namespace Extensions.MeshPro.MeshEditor.Modules.Internal.MeshBoolean.Scripts {
    public class BooleanExample : MonoBehaviour
    {
        public MeshFilter meshFa;
        public MeshFilter meshFb;

        [ContextMenu("difference")]
        public void Difference()
        {
            var faCollider = meshFa.GetComponent<MeshCollider>();

            var difference = MeshBooleanOperator.GetDifference(meshFa, meshFb);
            meshFa.sharedMesh = difference;
            faCollider.sharedMesh = meshFa.sharedMesh;

            faCollider.convex = true;

            if (!meshFa.GetComponent<Rigidbody>())
            {
                meshFa.gameObject.AddComponent<Rigidbody>();
            }
        }
        private void Start()
        {
            Difference();
        }
    }
}
#endif