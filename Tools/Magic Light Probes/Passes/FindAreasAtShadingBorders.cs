#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLightProbes
{
    /// <summary>
    /// Cannot work in multithreading mode
    /// </summary>
    public class FindAreasAtShadingBorders
    {
        public IEnumerator ExecutePass(MagicLightProbes parent, MLPVolume currentVolume = null)
        {
            if (parent.debugMode)
            {
                parent.tmpSharedPointsArray.Clear();
                parent.tmpSharedPointsArray.AddRange(parent.debugAcceptedPoints);
                parent.debugAcceptedPoints.Clear();
                parent.debugCulledPoints.Clear();
            }

            List<MLPPointData> candidatesList = new List<MLPPointData>();

            foreach (var light in parent.lights)
            {
                parent.currentPass = "Finding Areas At Shading Borders For Light: \"" + light.name + "\"";
                parent.currentPassProgressCounter = 0;
                parent.currentPassProgressFrameSkipper = 0;

                if (!parent.isInBackground)
                {
                    if (parent.UpdateProgress(parent.tmpSharedPointsArray.Count, 1))
                    {
                        yield return null;
                    }
                }

                if (
                    light.lightType == MLPLight.MLPLightType.Directional ||
                    light.parentVolume == parent ||
                    (parent.parentVolume != null && light.parentVolume == parent.gameObject))
                { 
                    for (int i = 0; i < parent.tmpSharedPointsArray.Count; i++)
                    {
                        switch (light.calculationMode)
                        {
                            case MLPLight.CalculationMode.AccurateShadows:
                                if (light.accurateTrace)
                                {
                                    if (parent.tmpSharedPointsArray[i].inRangeForLights.Contains(light))
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
                                            foreach (var tracePoint in light.tracePointsData)
                                            {
                                                for (int np = 0; np < parent.tmpSharedPointsArray[i].nearbyPoints.Count; np++)
                                                {
                                                    if (parent.tmpSharedPointsArray[i].nearbyPoints[np] != null)
                                                    {
                                                        if (!parent.CheckIfInShadow(light, tracePoint, parent.tmpSharedPointsArray[i].nearbyPoints[np]))
                                                        {
                                                            if (light.lightMode == LightmapBakeType.Mixed && QualitySettings.shadowmaskMode == ShadowmaskMode.DistanceShadowmask)
                                                            {
                                                                if (parent.tmpSharedPointsArray[i].savedNearGeometry)
                                                                {
                                                                    parent.tmpSharedPointsArray[i].nearbyPoints[np].contrastOnShadingArea = true;

                                                                    if (!parent.tmpContrastShadingBordersPoints.Contains(parent.tmpSharedPointsArray[i].nearbyPoints[np]))
                                                                    {
                                                                        parent.tmpContrastShadingBordersPoints.Add(parent.tmpSharedPointsArray[i].nearbyPoints[np]);
                                                                    }

                                                                    for (int n = 0; n < parent.tmpSharedPointsArray[i].nearbyPoints[np].nearbyPoints.Count; n++)
                                                                    {
                                                                        if (!parent.tmpSharedPointsArray[i].nearbyPoints[np].nearbyPoints[n].savedNearGeometry)
                                                                        {
                                                                            parent.tmpSharedPointsArray[i].nearbyPoints[np].nearbyPoints[n].contrastOnShadingArea = true;

                                                                            if (!parent.tmpContrastShadingBordersPoints.Contains(parent.tmpSharedPointsArray[i].nearbyPoints[np].nearbyPoints[n]))
                                                                            {
                                                                                parent.tmpContrastShadingBordersPoints.Add(parent.tmpSharedPointsArray[i].nearbyPoints[np].nearbyPoints[n]);
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                parent.tmpSharedPointsArray[i].contrastOnShadingArea = true;
                                                                parent.tmpSharedPointsArray[i].nearbyPoints[np].contrastOnShadingArea = true;

                                                                if (!parent.tmpContrastShadingBordersPoints.Contains(parent.tmpSharedPointsArray[i].nearbyPoints[np]))
                                                                {
                                                                    parent.tmpContrastShadingBordersPoints.Add(parent.tmpSharedPointsArray[i].nearbyPoints[np]);
                                                                }

                                                                if (!parent.tmpContrastShadingBordersPoints.Contains(parent.tmpSharedPointsArray[i]))
                                                                {
                                                                    parent.tmpContrastShadingBordersPoints.Add(parent.tmpSharedPointsArray[i]);
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (!candidatesList.Contains(parent.tmpSharedPointsArray[i]))
                                                            {
                                                                candidatesList.Add(parent.tmpSharedPointsArray[i]);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!candidatesList.Contains(parent.tmpSharedPointsArray[i]))
                                        {
                                            candidatesList.Add(parent.tmpSharedPointsArray[i]);
                                        }
                                    }
                                }
                                else
                                {
                                    if (parent.tmpSharedPointsArray[i].inRangeForLights.Contains(light))
                                    {
                                        if (parent.tmpSharedPointsArray[i].inShadowForLights.Contains(light) && !parent.tmpSharedPointsArray[i].inCorner)
                                        {
                                            Vector3 shadedPointStartPosition = parent.tmpSharedPointsArray[i].position;

                                            for (int np = 0; np < parent.tmpSharedPointsArray[i].nearbyPoints.Count; np++)
                                            {
                                                if (parent.tmpSharedPointsArray[i].nearbyPoints[np] != null && parent.tmpSharedPointsArray[i].nearbyPoints[np].inRangeForLights.Contains(light))
                                                {
                                                    if (!parent.tmpSharedPointsArray[i].nearbyPoints[np].inShadowForLights.Contains(light) && !parent.tmpSharedPointsArray[i].nearbyPoints[np].inCorner)
                                                    {
                                                        if (light.lightMode == LightmapBakeType.Mixed && QualitySettings.shadowmaskMode == ShadowmaskMode.DistanceShadowmask)
                                                        {
                                                            if (parent.tmpSharedPointsArray[i].savedNearGeometry)
                                                            {
                                                                if (parent.tmpSharedPointsArray[i].nearbyPoints[np].savedNearGeometry)
                                                                {
                                                                    if (
                                                                        (parent.tmpSharedPointsArray[i].position - parent.tmpSharedPointsArray[i].nearbyPoints[np].position).normalized !=
                                                                        (parent.tmpSharedPointsArray[i].contactPoint - parent.tmpSharedPointsArray[i].nearbyPoints[np].position).normalized)
                                                                    {

                                                                        parent.tmpSharedPointsArray[i].contrastOnShadingArea = true;
                                                                        parent.tmpSharedPointsArray[i].nearbyPoints[np].contrastOnShadingArea = true;                                                                        
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if ((parent.tmpSharedPointsArray[i].position - parent.tmpSharedPointsArray[i].nearbyPoints[np].position).normalized !=
                                                                (parent.tmpSharedPointsArray[i].contactPoint - parent.tmpSharedPointsArray[i].nearbyPoints[np].position).normalized)
                                                            {   
                                                                parent.tmpSharedPointsArray[i].contrastOnShadingArea = true;
                                                                parent.tmpSharedPointsArray[i].nearbyPoints[np].contrastOnShadingArea = true;                                                                
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                break;                            
                        }
                    }
                }
            }

            if (parent.debugMode)
            {
                parent.currentPass = "Finding Areas At Shading Borders: Array Rebuilding...";
                parent.currentPassProgressCounter = 0;
                parent.currentPassProgressFrameSkipper = 0;

                foreach (var point in parent.tmpSharedPointsArray)
                {
                    if (point.contrastOnShadingArea)
                    {
                        if (!parent.tmpContrastShadingBordersPoints.Contains(point))
                        {
                            parent.tmpContrastShadingBordersPoints.Add(point);
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

                switch (parent.debugPass)
                {
                    case MagicLightProbes.DebugPasses.ShadingBorders:
                        parent.debugAcceptedPoints.AddRange(parent.tmpContrastShadingBordersPoints);
                        parent.totalProbesInSubVolume = parent.debugAcceptedPoints.Count;
                        break;
                    case MagicLightProbes.DebugPasses.ContrastAreas:
                        parent.debugAcceptedPoints.AddRange(parent.tmpContrastOnOutOfRangePoints);
                        parent.debugAcceptedPoints.AddRange(parent.tmpContrastShadingBordersPoints);
                        parent.totalProbesInSubVolume = parent.debugAcceptedPoints.Count;
                        break;
                    case MagicLightProbes.DebugPasses.LightIntensity:
                    case MagicLightProbes.DebugPasses.EqualProbes:
                        parent.debugAcceptedPoints.AddRange(parent.tmpContrastOnOutOfRangePoints);
                        parent.debugAcceptedPoints.AddRange(parent.tmpContrastShadingBordersPoints);
                        parent.debugAcceptedPoints.AddRange(parent.tmpSharedPointsArray);
                        parent.totalProbesInSubVolume = parent.debugAcceptedPoints.Count;
                        break;
                    case MagicLightProbes.DebugPasses.UnlitProbes:
                        parent.debugAcceptedPoints.AddRange(parent.tmpSharedPointsArray);
                        parent.totalProbesInSubVolume = parent.debugAcceptedPoints.Count;
                        break;
                }
            }
            else
            {
                parent.currentPass = "Finding Areas At Shading Borders: Array Rebuilding...";
                parent.currentPassProgressCounter = 0;
                parent.currentPassProgressFrameSkipper = 0;

                List<MLPPointData> tempList = new List<MLPPointData>();

                tempList.AddRange(parent.tmpSharedPointsArray);

                foreach (var point in tempList)
                {
                    if (point.contrastOnShadingArea)
                    {
                        parent.tmpContrastShadingBordersPoints.Add(point);
                        parent.tmpSharedPointsArray.Remove(point);
                        //currentVolume.localContrastPoints.Add(point);
                    }

                    if (point.savedNearGeometry)
                    {
                        parent.tmpSharedPointsArray.Remove(point);
                    }

                    if (!parent.isInBackground)
                    {
                        if (parent.UpdateProgress(tempList.Count, 1000))
                        {
                            yield return null;
                        }
                    }
                }

                parent.totalProbesInSubVolume = parent.tmpSharedPointsArray.Count;
            }

            parent.calculatingVolumeSubPass = false;
        }
    }
}
#endif
