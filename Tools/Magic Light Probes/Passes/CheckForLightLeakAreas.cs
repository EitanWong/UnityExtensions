using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLightProbes
{
    /// <summary>
    /// Cannot work in multithreading mode
    /// </summary>
    public class CheckForLightLeakAreas
    {
        public IEnumerator ExecutePass(MagicLightProbes parent)
        {
            parent.totalProbesInSubVolume = parent.tmpSharedPointsArray.Count;

            parent.currentPass = "Check For Light Leak Areas...";
            parent.currentPassProgressCounter = 0;
            parent.currentPassProgressFrameSkipper = 0;

            for (int i = 0; i < parent.tmpNearbyGeometryPoints.Count; i++)
            {
                GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                point.name = "Temporary Point " + i;
                point.transform.position = parent.tmpNearbyGeometryPoints[i].position;
                point.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                point.AddComponent<SphereCollider>().radius = 0.1f;

                parent.tmpNearbyGeometryPoints[i].temporaryObject = point;

                if (!parent.isInBackground)
                {
                    if (parent.UpdateProgress(parent.tmpNearbyGeometryPoints.Count))
                    {
                        yield return null;
                    }
                }
            }

            for (int i = 0; i < parent.tmpNearbyGeometryPoints.Count; i++)
            {
                if (parent.tmpNearbyGeometryPoints[i].collisionObject != null)
                {
                    parent.tmpNearbyGeometryPoints[i].collisionObject.SetActive(false);

                    Ray ray = new Ray(parent.tmpNearbyGeometryPoints[i].position, (parent.tmpNearbyGeometryPoints[i].contactPoint - parent.tmpNearbyGeometryPoints[i].position).normalized);
                    RaycastHit raycastHit;

                    if (Physics.Raycast(ray, out raycastHit, Mathf.Infinity, parent.layerMask))
                    {
                        if (raycastHit.collider.name.Contains("Temporary Point"))
                        {
                            parent.tmpNearbyGeometryPoints[i].lightLeakLocked = true;
                        }
                    }

                    parent.tmpNearbyGeometryPoints[i].collisionObject.SetActive(true);
                }
            }

            for (int i = 0; i < parent.tmpNearbyGeometryPoints.Count; i++)
            {
                Object.DestroyImmediate(parent.tmpNearbyGeometryPoints[i].temporaryObject);
            }

            parent.totalProbesInSubVolume = parent.tmpSharedPointsArray.Count;
            parent.calculatingVolumeSubPass = false;
        }
    }
}
