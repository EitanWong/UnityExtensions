#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MagicLightProbes
{
    /// <summary>
    /// Work in multithreading mode
    /// </summary>
    public class FindClosestPoints
    {
        public IEnumerator ExecutePass(MagicLightProbes parent, List<MLPPointData> inputList)
        {
            float spacing = 0;
            
            if (parent.useDynamicDensity)
            {
                if (parent.currentVolume.isSubdividedPart)
                {
                    spacing = parent.volumeSpacingMin;
                }
                else
                {
                    spacing = parent.volumeSpacingMax;
                }
            }
            else
            {
                spacing = parent.volumeSpacing;
            }

            List<List<MLPPointData>> separated = new List<List<MLPPointData>>();

            for (int i = 0; i < inputList.Count; i += 1000)
            {
                separated.Add(inputList.GetRange(i, Math.Min(1000, inputList.Count - i)));
            }

            for (int s_list = 0; s_list < separated.Count; s_list++)
            {
                parent.currentPass = "Finding Closest Points: Part " + s_list + "/" + separated.Count;
                parent.currentPassProgressCounter = 0;
                parent.currentPassProgressFrameSkipper = 0;

                if (!parent.isInBackground)
                {
                    if (parent.UpdateProgress(separated[s_list].Count, 1))
                    {
                        yield return null;
                    }
                }

                Parallel.For(0, separated[s_list].Count, i =>
                {
                    foreach (var point in separated[s_list])
                    {
                        if (point != separated[s_list][i])
                        {
                            if (Vector3.Distance(point.position, separated[s_list][i].position).EqualsApproximately(spacing, 0.01f))
                            {
                                separated[s_list][i].nearbyPoints.Add(point);
                            }
                        }
                    }
                });
            }

            if (parent.debugMode)
            {
                parent.debugAcceptedPoints.AddRange(inputList);
            }

            parent.calculatingVolumeSubPass = false;
        }
    }
}
#endif
