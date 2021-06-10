using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Net3dBool;

namespace N3dBoolExample {
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