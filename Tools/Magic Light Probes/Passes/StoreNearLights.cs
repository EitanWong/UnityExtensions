#if UNITY_EDITOR
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MagicLightProbes
{
    /// <summary>
    /// Work in multithreading mode
    /// </summary>
    public class StoreNearLights
    {
        public IEnumerator ExecutePass(MagicLightProbes parent, float volumeSpacing)
        {
            parent.currentPass = "Saving Probes Nearby Lights...";
            parent.currentPassProgressCounter = 0;
            parent.currentPassProgressFrameSkipper = 0;

            if (parent.debugMode)
            {
                parent.tmpSharedPointsArray.Clear();
                parent.tmpSharedPointsArray.AddRange(parent.debugAcceptedPoints);
                parent.debugAcceptedPoints.Clear();
            }

            Thread calculationThread = new Thread(new ThreadStart(() =>
            {
                Parallel.For(0, parent.tmpSharedPointsArray.Count, (i, state) =>
                {
                    if (!parent.tmpSharedPointsArray[i].contrastOnOutOfRangeArea)
                    {
                        foreach (var light in parent.lights)
                        {
                            if (parent.tmpSharedPointsArray[i].inRangeForLights.Contains(light))
                            {
                                if (light.lightMode != LightmapBakeType.Mixed)
                                {
                                    if (light.saveNearbyProbes)
                                    {
                                        if (light.accurateTrace)
                                        {
                                            foreach (var tracePoint in light.tracePointsData)
                                            {
                                                if (Vector3.Distance(parent.tmpSharedPointsArray[i].position, tracePoint.position).EqualsApproximately(volumeSpacing, volumeSpacing / 2))
                                                {
                                                    parent.tmpSharedPointsArray[i].contrastOnShadingArea = true;
                                                    parent.tmpSharedPointsArray[i].SetInSaveRange(true);
                                                    parent.tmpNearbyLightsPoints.Add(parent.tmpSharedPointsArray[i]);

                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (Vector3.Distance(parent.tmpSharedPointsArray[i].position, light.position) <= volumeSpacing * 2)
                                            {
                                                parent.tmpSharedPointsArray[i].SetInSaveRange(true);

                                                lock (parent.tmpNearbyLightsPoints)
                                                {
                                                    parent.tmpNearbyLightsPoints.Add(parent.tmpSharedPointsArray[i]);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    parent.UpdateProgress(parent.tmpSharedPointsArray.Count);
                });
            }));

            calculationThread.Start();

            while (calculationThread.ThreadState == ThreadState.Running)
            {
                yield return null;
            }

            if (parent.debugMode)
            {
                parent.debugAcceptedPoints.AddRange(parent.tmpNearbyLightsPoints);
                parent.totalProbesInSubVolume = parent.debugAcceptedPoints.Count;
            }

            parent.calculatingVolumeSubPass = false;
        }
    }
}
#endif
