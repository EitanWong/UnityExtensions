#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

namespace MagicLightProbes
{
    /// <summary>
    /// Cannot work in multithreading mode
    /// </summary>
    public class CalculateLightIntensity
    {
        public IEnumerator ExecutePass(MagicLightProbes parent, MLPVolume currentVolume = null)
        {
            parent.currentPass = "Calculating Lights Intensity...";
            parent.currentPassProgressCounter = 0;
            parent.currentPassProgressFrameSkipper = 0;

            if (parent.debugMode)
            {
                parent.tmpSharedPointsArray.Clear();
                parent.tmpSharedPointsArray.AddRange(parent.debugAcceptedPoints);
                parent.debugAcceptedPoints.Clear();
            }

            for (int i = 0; i < parent.tmpSharedPointsArray.Count; i++)
            {
                float resultIntensity = 0;

                foreach (var light in parent.lights)
                {
                    RaycastHit hitInfo;
                    Ray rayToLight = new Ray();

                    switch (light.lightType)
                    {
                        case MLPLight.MLPLightType.Directional:
                            rayToLight = new Ray(parent.tmpSharedPointsArray[i].position, -light.transform.forward);

                            if (!Physics.Raycast(rayToLight, out hitInfo, Mathf.Infinity, parent.layerMask))
                            {
                                if (light.lightMode == LightmapBakeType.Mixed && QualitySettings.shadowmaskMode == ShadowmaskMode.DistanceShadowmask)
                                {
                                    if (parent.tmpSharedPointsArray[i].savedNearGeometry)
                                    {
                                        resultIntensity += light.intensity;
                                    }
                                }
                                else
                                {
                                    resultIntensity += light.intensity;
                                }
                            }
                            break;
                        default:
                            if (light.accurateTrace)
                            {
                                if (parent.tmpSharedPointsArray[i].inRangeForLights.Contains(light) && !parent.tmpSharedPointsArray[i].inShadowForLights.Contains(light))
                                {
                                    float tracePointWeight = 1f / light.tracePoints.Count;

                                    foreach (var tracePoint in light.tracePointsData)
                                    {
                                        tracePoint.pointGameObject.SetActive(true);
                                        rayToLight = new Ray(parent.tmpSharedPointsArray[i].position, (tracePoint.position - parent.tmpSharedPointsArray[i].position).normalized);

                                        if (Physics.Raycast(rayToLight, out hitInfo, Mathf.Infinity, parent.layerMask))
                                        {
                                            if (hitInfo.collider.name == tracePoint.name)
                                            {
                                                //resultIntensity += tracePointWeight;
                                                resultIntensity += 1.0F / (1.0F + 25.0F * (tracePoint.position - parent.tmpSharedPointsArray[i].position).sqrMagnitude / (light.range * light.range));
                                                parent.tmpSharedPointsArray[i].distancesToLights.Add(hitInfo.distance);
                                            }
                                        }

                                        tracePoint.pointGameObject.SetActive(false);
                                    }
                                }
                            }
                            else
                            {
                                if (parent.tmpSharedPointsArray[i].inRangeForLights.Contains(light) && !parent.tmpSharedPointsArray[i].inShadowForLights.Contains(light))
                                {
                                    rayToLight = new Ray(parent.tmpSharedPointsArray[i].position, (light.transform.position - parent.tmpSharedPointsArray[i].position).normalized);

                                    if (Physics.Raycast(rayToLight, out hitInfo, Mathf.Infinity, parent.layerMask))
                                    {
                                        if (hitInfo.collider.name == light.gameObject.name)
                                        {
                                            if (light.calculationMode == MLPLight.CalculationMode.AccurateShadows)
                                            {
                                                if (parent.tmpSharedPointsArray[i].inRangeForLights.Contains(light) && !parent.tmpSharedPointsArray[i].inShadowForLights.Contains(light))
                                                {
                                                    if (light.lightMode == LightmapBakeType.Mixed && QualitySettings.shadowmaskMode == ShadowmaskMode.DistanceShadowmask)
                                                    {
                                                        if (parent.tmpSharedPointsArray[i].savedNearGeometry)
                                                        {
                                                            //resultIntensity += light.intensity;
                                                            resultIntensity += 1.0F / (1.0F + 25.0F * (light.position - parent.tmpSharedPointsArray[i].position).sqrMagnitude / (light.range * light.range));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        //resultIntensity += light.intensity;
                                                        resultIntensity += 1.0F / (1.0F + 25.0F * (light.position - parent.tmpSharedPointsArray[i].position).sqrMagnitude / (light.range * light.range));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (light.lightMode == LightmapBakeType.Mixed && QualitySettings.shadowmaskMode == ShadowmaskMode.DistanceShadowmask)
                                                {
                                                    if (parent.tmpSharedPointsArray[i].savedNearGeometry)
                                                    {
                                                        //resultIntensity += light.intensity;
                                                        resultIntensity += 1.0F / (1.0F + 25.0F * (light.position - parent.tmpSharedPointsArray[i].position).sqrMagnitude / (light.range * light.range));
                                                    }
                                                }
                                                else
                                                {
                                                    //resultIntensity += light.intensity;
                                                    resultIntensity += 1.0F / (1.0F + 25.0F * (light.position - parent.tmpSharedPointsArray[i].position).sqrMagnitude / (light.range * light.range));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }

                parent.tmpSharedPointsArray[i].SetLightIntensity(resultIntensity);

                if (parent.tmpSharedPointsArray[i].lightIntensity == 0 && 
                    !parent.tmpSharedPointsArray[i].contrastOnOutOfRangeArea && 
                    !parent.tmpSharedPointsArray[i].contrastOnShadingArea)
                {
                    parent.tmpSharedPointsArray[i].isUnlit = true;
                    parent.tmpUnlitPoints.Add(parent.tmpSharedPointsArray[i]);
                    currentVolume.localUnlitPointsPositions.Add(parent.tmpSharedPointsArray[i].position);
                }

                if (!parent.isInBackground)
                {
                    if (parent.UpdateProgress(parent.tmpSharedPointsArray.Count, 1000))
                    {
                        yield return null;
                    }
                }
            }

            if (parent.debugMode)
            {
                switch (parent.debugPass)
                {
                    case MagicLightProbes.DebugPasses.OutOfRangeBorders:
                    case MagicLightProbes.DebugPasses.ShadingBorders:
                    case MagicLightProbes.DebugPasses.ContrastAreas:
                    case MagicLightProbes.DebugPasses.EqualProbes:
                        parent.debugAcceptedPoints.AddRange(parent.tmpSharedPointsArray);
                        parent.totalProbesInSubVolume = parent.debugAcceptedPoints.Count;
                        break;
                    case MagicLightProbes.DebugPasses.LightIntensity:
                        parent.debugAcceptedPoints.AddRange(parent.tmpSharedPointsArray);
                        parent.totalProbesInSubVolume = parent.debugAcceptedPoints.Count;
                        break;
                    case MagicLightProbes.DebugPasses.UnlitProbes:
                        parent.debugCulledPoints.AddRange(parent.tmpUnlitPoints);
                        parent.totalProbesInSubVolume = parent.debugCulledPoints.Count;
                        break;
                }
            }

            parent.calculatingVolumeSubPass = false;
        }
    }
}
#endif
