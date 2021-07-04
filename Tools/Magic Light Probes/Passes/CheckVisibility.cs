#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

namespace MagicLightProbes
{
    /// <summary>
    /// Cannot work in multithreading mode
    /// </summary>
    public class CheckVisibility
    {
        public IEnumerator ExecutePass(MagicLightProbes parent)
        {
            if (parent.debugMode)
            {
                parent.tmpSharedPointsArray.Clear();
                parent.tmpSharedPointsArray.AddRange(parent.debugAcceptedPoints);
                parent.tmpNearbyGeometryPoints.AddRange(parent.debugAcceptedPoints);
                parent.debugAcceptedPoints.Clear();
            }

            int l_counter = 0;

            foreach (var light in parent.lights)
            {
                l_counter++;
                parent.currentPass = "Check Visibility For Light: \"" + light.name + "\" - " + l_counter + "/" + parent.lights.Count;
                parent.currentPassProgressCounter = 0;
                parent.currentPassProgressFrameSkipper = 0;

                if (light.lightType == MLPLight.MLPLightType.Directional || parent.lights.Contains(light))
                {
                    foreach (var tracePoint in light.tracePointsData)
                    {
                        tracePoint.pointGameObject.SetActive(false);
                    }
                                        
                    for (int i = 0; i < parent.tmpSharedPointsArray.Count; i++)
                    {
                        bool calculate = true;

                        if (light.lightType != MLPLight.MLPLightType.Directional)
                        {
                            if (Vector3.Distance(light.position, parent.tmpSharedPointsArray[i].position) > light.range)
                            {
                                calculate = false;
                            }
                        }

                        if (calculate)
                        {
                            if (light.accurateTrace)
                            {
                                int fullShaded = 0;

                                foreach (var tracePoint in light.tracePointsData)
                                {
                                    if (parent.CheckIfInShadow(light, tracePoint, parent.tmpSharedPointsArray[i]))
                                    {
                                        fullShaded++;
                                    }
                                    else
                                    {
                                        fullShaded--;
                                    }
                                }

                                if (fullShaded == light.tracePointsData.Count)
                                {
                                    if (!parent.tmpSharedPointsArray[i].inShadowForLights.Contains(light))
                                    {
                                        parent.tmpSharedPointsArray[i].inShadowForLights.Add(light);
                                    }
                                }
                                else
                                {
                                    parent.tmpSharedPointsArray[i].inShadowForLights.Remove(light);
                                }
                            }
                            else
                            {
                                parent.CheckIfInShadow(light, light.mainTracePoint, parent.tmpSharedPointsArray[i]);
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

                    light.gameObject.SetActive(true);
                }
            }

            if (parent.debugMode)
            {
                parent.debugAcceptedPoints.AddRange(parent.tmpSharedPointsArray);
            }

            parent.calculatingVolumeSubPass = false;
        }
    }
}
#endif
