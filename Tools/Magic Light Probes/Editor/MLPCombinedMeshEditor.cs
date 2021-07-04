using UnityEditor;
using UnityEngine;

namespace MagicLightProbes
{
    [CustomEditor(typeof(MLPCombinedMesh))]
    public class MLPCombinedMeshEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MLPCombinedMesh mLPCombinedMesh = (MLPCombinedMesh)target;

            base.DrawDefaultInspector();

            if (GUILayout.Button("Combine"))
            {
                mLPCombinedMesh.Combine();
            }
        }
    }
}
