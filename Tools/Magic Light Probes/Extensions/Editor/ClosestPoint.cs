using UnityEngine;

namespace MagicLightProbes
{
    public class ClosestPoint : MonoBehaviour
    {
        public GameObject origin;
        public LayerMask layerMask;

        private void OnDrawGizmos()
        {
            Ray[] checkRays =
                            {
                                    new Ray(origin.transform.position, Vector3.down),
                                    new Ray(origin.transform.position, -Vector3.down),
                                    new Ray(origin.transform.position, Vector3.right),
                                    new Ray(origin.transform.position, -Vector3.right),
                                    new Ray(origin.transform.position, Vector3.forward),
                                    new Ray(origin.transform.position, -Vector3.forward)
                                };

            foreach (var ray in checkRays)
            {
                RaycastHit hitInfoForward;

                if (Physics.Raycast(ray, out hitInfoForward, Mathf.Infinity, layerMask))
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(hitInfoForward.point, 0.1f);
                }
            }
        }
    }
}
