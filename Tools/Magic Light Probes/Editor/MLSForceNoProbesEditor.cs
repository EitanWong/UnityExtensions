using UnityEditor;

namespace MagicLightProbes
{
    [CustomEditor(typeof(MLPForceNoProbes))]
    public class MLSForceNoProbesEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MLPForceNoProbes forceNoProbes = (MLPForceNoProbes)target;
            
            if (forceNoProbes.transform.root == forceNoProbes.transform)
            {
                EditorGUILayout.HelpBox("The component is automatically added to all children containing the MeshRenderer component.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("The component is added automatically because the root object contains it.", MessageType.Info);
            }
        }
    }
}
