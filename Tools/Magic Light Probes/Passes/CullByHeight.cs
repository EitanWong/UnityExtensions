#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MagicLightProbes
{
    /// <summary>
    /// Work in multithreading mode
    /// </summary>
    public class CullByHeight
    {
        public IEnumerator ExecutePass(MagicLightProbes parent, MagicLightProbes.VolumeParameters volumePart, MagicLightProbes.CalculationTarget calculationTarget)
        {
            parent.currentPass = "Culling By Max Height...";
            parent.currentPassProgressCounter = 0;
            parent.currentPassProgressFrameSkipper = 0;

            List<MLPPointData> tempList = new List<MLPPointData>();

            if (parent.useVolumeBottom)
            {
                for (int i = 0; i < parent.tmpSharedPointsArray.Count; i++)
                {
                    if (parent.tmpSharedPointsArray[i].position.y >= volumePart.position.y - (volumePart.demensions.y / 2))
                    {
                        tempList.Add(parent.tmpSharedPointsArray[i]);
                    }
                }
            }
            else
            {
                NativeArray<RaycastHit> raycastResults = new NativeArray<RaycastHit>(parent.tmpSharedPointsArray.Count, Allocator.TempJob);
                NativeArray<RaycastCommand> raycastCommands = new NativeArray<RaycastCommand>(parent.tmpSharedPointsArray.Count, Allocator.TempJob);

                Parallel.For(0, parent.tmpSharedPointsArray.Count, i =>
                {
                    if (parent.tmpSharedPointsArray[i] != null)
                    {
                        raycastCommands[i] = new RaycastCommand(parent.tmpSharedPointsArray[i].position, Vector3.down, Mathf.Infinity, parent.layerMask);
                    }
                });

                JobHandle handle = RaycastCommand.ScheduleBatch(raycastCommands, raycastResults, 1, default);

                handle.Complete();

                for (int i = 0; i < raycastResults.Length; i++)
                {
                    RaycastHit outInfo = raycastResults[i];

                    if (outInfo.collider != null)
                    {
#if TERRAINPACKAGE_EXIST
                        if (outInfo.collider.GetComponent<Terrain>() != null)
                        {
                            if (outInfo.distance <= parent.maxHeightAboveTerrain)
                            {
                                tempList.Add(parent.tmpSharedPointsArray[i]);
                            }
                            else
                            {
                                if (parent.debugMode)
                                {
                                    parent.tmpOutOfMaxHeightPoints.Add(parent.tmpSharedPointsArray[i]);
                                }
                            }
                        }
                        else
                        {
                            //RaycastHit[] topDownRaycastResults = RaycastAllNonConvex(parent.tmpSharedPointsArray[i].position, parent.layerMask);
                            RaycastHit[] topDownRaycastResults = Physics.RaycastAll(parent.tmpSharedPointsArray[i].position, Vector3.down, Mathf.Infinity, parent.layerMask);

                            if (topDownRaycastResults.Length > 0)
                            {
                                for (int j = 0; j < topDownRaycastResults.Length; j++)
                                {
                                    if (CheckIfIsGroundOrFloor(parent, topDownRaycastResults[j].collider, parent.groundAndFloorKeywords))
                                    {
                                        if (outInfo.distance <= parent.maxHeightAboveGeometry)
                                        {
                                            tempList.Add(parent.tmpSharedPointsArray[i]);
                                        }
                                        else
                                        {
                                            if (parent.debugMode)
                                            {
                                                parent.tmpOutOfMaxHeightPoints.Add(parent.tmpSharedPointsArray[i]);
                                            }
                                        }

                                        break;
                                    }
                                }
                            }
                        }
#else
                    //RaycastHit[] topDownRaycastResults = RaycastAllNonConvex(parent.tmpSharedPointsArray[i].position, parent.layerMask);
                    RaycastHit[] topDownRaycastResults = Physics.RaycastAll(parent.tmpSharedPointsArray[i].position, Vector3.down, Mathf.Infinity, parent.layerMask);

                    if (topDownRaycastResults.Length > 0)
                    {
                        for (int j = 0; j < topDownRaycastResults.Length; j++)
                        {
                            if (CheckIfIsGroundOrFloor(parent, topDownRaycastResults[j].collider, parent.groundAndFloorKeywords))
                            {
                                if (outInfo.distance <= parent.maxHeightAboveGeometry)
                                {
                                    tempList.Add(parent.tmpSharedPointsArray[i]);
                                }
                                else
                                {
                                    if (parent.debugMode)
                                    {
                                        parent.tmpOutOfMaxHeightPoints.Add(parent.tmpSharedPointsArray[i]);
                                    }
                                }

                                break;
                            }
                        }
                    }
#endif
                    }

                    if (!parent.isInBackground)
                    {
                        if (parent.UpdateProgress(parent.tmpSharedPointsArray.Count, 1000))
                        {
                            yield return null;
                        }
                    }
                }

                raycastResults.Dispose();
                raycastCommands.Dispose();
            }

            if (parent.debugMode)
            {
                parent.debugAcceptedPoints.AddRange(tempList);
                parent.debugCulledPoints.AddRange(parent.tmpOutOfMaxHeightPoints);

                if (parent.debugPass == MagicLightProbes.DebugPasses.MaximumHeight)
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
                parent.tmpOutOfMaxHeightPoints.Clear();
                parent.tmpSharedPointsArray.Clear();
                parent.tmpSharedPointsArray.AddRange(tempList);
                tempList.Clear();

                parent.totalProbesInSubVolume = parent.tmpSharedPointsArray.Count;
            }

            parent.calculatingVolumeSubPass = false;
        }

        private static RaycastHit[] RaycastAllNonConvex(Vector3 origin, LayerMask layerMask)
        {
            List<RaycastHit> hits = new List<RaycastHit>();
            List<Collider> temporarilyDisabledColliders = new List<Collider>();

            bool clear = false;

            while (!clear)
            {
                RaycastHit hit;

                if (Physics.Raycast(origin, Vector3.down, out hit, Mathf.Infinity, layerMask))
                {
                    hit.collider.enabled = false;
                    temporarilyDisabledColliders.Add(hit.collider);
                    hits.Add(hit);
                }
                else
                {
                    clear = true;
                }
            }

            for (int i = 0; i < temporarilyDisabledColliders.Count; i++)
            {
                temporarilyDisabledColliders[i].enabled = true;
            }

            return hits.ToArray();
        }

        private bool CheckIfIsGroundOrFloor (MagicLightProbes parent, Collider checkObject, List<string> keywords)
        {
            bool result = false;

            for (int i = 0; i < keywords.Count; i++)
            {
                if (checkObject.name.Contains(keywords[i]))
                {
                    result = true;
                    break;
                }
                else if (parent.CheckIfTagExist(keywords[i]))
                {
                    if (checkObject.CompareTag(keywords[i]))
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }
    }
}
#endif
