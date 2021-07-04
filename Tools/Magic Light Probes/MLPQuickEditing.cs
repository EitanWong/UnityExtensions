using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace MagicLightProbes
{
    [HelpURL("https://motion-games-studio.gitbook.io/magic-light-probes/system-components/mlp-quick-editing")]
    public class MLPQuickEditing : MonoBehaviour
    {
        public MagicLightProbes parent;
        public float gizmoScale;
        public float drawDistance = 10;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            for (int i = 0; i < parent.localFinishedPositions.Count; i++)
            {
                if (Vector3.Distance(SceneView.lastActiveSceneView.camera.transform.position, parent.localFinishedPositions[i]) <= drawDistance)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(parent.localFinishedPositions[i], gizmoScale);
                }
            }
        }
#endif
    }    
}
