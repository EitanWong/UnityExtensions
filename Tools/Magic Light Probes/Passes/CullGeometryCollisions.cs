#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MagicLightProbes
{
    /// <summary>
    /// Cannot work in multithreading mode
    /// </summary>
    public class CullGeometryCollisions
    {
        public IEnumerator ExecutePass(MagicLightProbes parent, MagicLightProbes.VolumeParameters volumePart, MagicLightProbes.CalculationTarget calculationTarget)
        {
            parent.currentPass = "Culling Geometry Collisions...";
            parent.currentPassProgressCounter = 0;
            parent.currentPassProgressFrameSkipper = 0;

            List<MLPPointData> tempAcceptedList = new List<MLPPointData>();
            List<MLPPointData> tempCulledList = new List<MLPPointData>();

            bool physicsOptionsChanged = false;

            if (Physics.queriesHitBackfaces)
            {
#if UNITY_2020_2_OR_NEWER
                Physics.queriesHitBackfaces = false;
#else
                Physics.queriesHitBackfaces = true;
#endif
                physicsOptionsChanged = true;
            }

            if (parent.debugMode)
            {
                parent.tmpSharedPointsArray.Clear();
                parent.tmpSharedPointsArray.AddRange(parent.debugAcceptedPoints);
                parent.debugAcceptedPoints.Clear();
                parent.debugCulledPoints.Clear();
            }

            for (int i = 0; i < parent.tmpSharedPointsArray.Count; i++)
            {
                Collider[] colliders = Physics.OverlapSphere(parent.tmpSharedPointsArray[i].position, parent.collisionDetectionRadius, parent.layerMask);

                if (colliders.Length == 0)
                {
                    Ray[] checkRays =
                    {
                        new Ray(parent.tmpSharedPointsArray[i].position, Vector3.down),
                        new Ray(parent.tmpSharedPointsArray[i].position, -Vector3.down),
                        new Ray(parent.tmpSharedPointsArray[i].position, Vector3.right),
                        new Ray(parent.tmpSharedPointsArray[i].position, -Vector3.right),
                        new Ray(parent.tmpSharedPointsArray[i].position, Vector3.forward),
                        new Ray(parent.tmpSharedPointsArray[i].position, -Vector3.forward)
                    };

                    int freeRays = 0;

                    foreach (var ray in checkRays)
                    {
                        RaycastHit hitInfoForward;
                        RaycastHit hitInfoBackward;

                        Vector3 endPoint = parent.tmpSharedPointsArray[i].position + (ray.direction * 5000.0f);
                        Ray backRay = new Ray(endPoint, (ray.origin - endPoint).normalized);

                        if (Physics.Raycast(ray, out hitInfoForward, 5000.0f, parent.layerMask))
                        {
                            if (parent.CheckIfStatic(hitInfoForward.collider.gameObject))
                            {
                                if (Physics.Raycast(new Ray(hitInfoForward.point, (parent.tmpSharedPointsArray[i].position - hitInfoForward.point).normalized), out hitInfoBackward, 5000.0f, parent.layerMask))
                                {
                                    if (parent.CheckIfStatic(hitInfoForward.collider.gameObject))
                                    {
                                        if (hitInfoForward.distance <= hitInfoBackward.distance)
                                        {
                                            freeRays++;
                                        }
                                    }
                                    else
                                    {
                                        freeRays++;
                                    }
                                }
                                else
                                {
                                    freeRays++;
                                }
                            }
                            else
                            {
                                freeRays++;
                            }
                        }
                        else
                        {
                            if (Physics.Raycast(backRay, out hitInfoBackward, 5000.0f, parent.layerMask))
                            {
                                float dist = Vector3.Distance(parent.tmpSharedPointsArray[i].position, endPoint);

                                if (hitInfoBackward.distance >= 5000.0f)
                                {
                                    freeRays++;
                                }
                            }
                            else
                            {
                                freeRays++;
                            }
                        }
                    }

                    if (freeRays == checkRays.Length)
                    {
                         tempAcceptedList.Add(parent.tmpSharedPointsArray[i]);
                    }
                    else
                    {
                        tempCulledList.Add(parent.tmpSharedPointsArray[i]);
                    }
                }
                else
                {
                    foreach (var collider in colliders)
                    {
                        if (parent.CheckIfStatic(collider.gameObject))
                        {
                            if (!tempCulledList.Contains(parent.tmpSharedPointsArray[i]))
                            {
                                tempCulledList.Add(parent.tmpSharedPointsArray[i]);
                            }
                        }
                        else
                        {
                            tempAcceptedList.Add(parent.tmpSharedPointsArray[i]);
                        }
                    }
                }

                if (!parent.isInBackground)
                {
                    if (parent.UpdateProgress(parent.tmpSharedPointsArray.Count, 1000))
                    {
                        yield return null;
                    }
                }
            }

            if (physicsOptionsChanged)
            {
                Physics.queriesHitBackfaces = true;
            }

            if (parent.debugMode)
            {
                parent.debugAcceptedPoints.AddRange(tempAcceptedList);
                parent.debugCulledPoints.AddRange(tempCulledList);

                if (parent.debugPass == MagicLightProbes.DebugPasses.GeometryCollision)
                {
                    switch (parent.drawMode)
                    {
                        case MagicLightProbes.DrawModes.Accepted:
                            parent.totalProbesInSubVolume = parent.debugAcceptedPoints.Count;
                            break;
                        case MagicLightProbes.DrawModes.Culled:
                            parent.totalProbesInSubVolume = parent.debugCulledPoints.Count;
                            break;
                        case MagicLightProbes.DrawModes.Both:
                            parent.totalProbesInSubVolume = parent.debugAcceptedPoints.Count + parent.debugCulledPoints.Count;
                            break;
                    }
                }
                else
                {
                    parent.totalProbesInSubVolume = parent.debugAcceptedPoints.Count;
                }
            }
            else
            {
                parent.tmpSharedPointsArray.Clear();
                parent.tmpSharedPointsArray.AddRange(tempAcceptedList);
                tempAcceptedList.Clear();
                tempCulledList.Clear();
            }

            parent.calculatingVolumeSubPass = false;
        }
    }
}
#endif
