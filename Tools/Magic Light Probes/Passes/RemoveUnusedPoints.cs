using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLightProbes
{
    /// <summary>
    /// Can work in multithreading mode
    /// </summary>
    public class RemoveUnusedPoints
    {
        public IEnumerator ExecutePass(MagicLightProbes parent)
        {
            parent.currentPass = "Romoving Unused Points...";
            parent.currentPassProgressCounter = 0;
            parent.currentPassProgressFrameSkipper = 0;

            List<MLPPointData> pointsToRemove = new List<MLPPointData>();

            if (parent.debugMode)
            {
                if (parent.subVolumesDivided.Count > 0)
                {
                    for (int i = 0; i < parent.debugAcceptedPoints.Count; i++)
                    {
                        if (parent.debugAcceptedPoints[i].col == parent.xPointsCount - 1 || parent.debugAcceptedPoints[i].depth == parent.zPointsCount)
                        {
                            pointsToRemove.Add(parent.debugAcceptedPoints[i]);
                        }

                        if (!parent.isInBackground)
                        {
                            if (parent.UpdateProgress(parent.debugAcceptedPoints.Count))
                            {
                                yield return null;
                            }
                        }
                    }

                    for (int i = 0; i < pointsToRemove.Count; i++)
                    {
                        parent.debugAcceptedPoints.Remove(pointsToRemove[i]);
                    }
                }
            }
            else
            {
                if (parent.subVolumesDivided.Count > 0)
                {
                    for (int i = 0; i < parent.tmpNearbyGeometryPoints.Count; i++)
                    {
                        if (
                            parent.tmpNearbyGeometryPoints[i].col == parent.xPointsCount - 1 ||
                            parent.tmpNearbyGeometryPoints[i].depth == parent.zPointsCount)
                        {
                            pointsToRemove.Add(parent.tmpNearbyGeometryPoints[i]);
                        }

                        if (!parent.isInBackground)
                        {
                            if (parent.UpdateProgress(parent.tmpNearbyGeometryPoints.Count))
                            {
                                yield return null;
                            }
                        }
                    }

                    for (int i = 0; i < pointsToRemove.Count; i++)
                    {
                        parent.tmpNearbyGeometryPoints.Remove(pointsToRemove[i]);
                    }
                }

                //parent.tmpNearbyGeometryPoints.AddRange(parent.tmpPointsNearGeometryIntersections);
            }

            pointsToRemove.Clear();
            parent.calculatingVolumeSubPass = false;
        }
    }
}
