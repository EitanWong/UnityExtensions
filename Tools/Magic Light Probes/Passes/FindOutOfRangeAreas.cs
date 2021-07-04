#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MagicLightProbes
{
    /// <summary>
    /// Work in multithreading mode
    /// </summary>
    public class FindOutOfRangeAreas
    {
        public IEnumerator ExecutePass(MagicLightProbes parent)
        {
            if (parent.debugMode)
            {
                parent.tmpSharedPointsArray.Clear();
                parent.tmpSharedPointsArray.AddRange(parent.debugAcceptedPoints);
                parent.debugAcceptedPoints.Clear();
                parent.debugCulledPoints.Clear();
            }
            
            List<MLPPointData> tempList = new List<MLPPointData>();
            List<MLPPointData> addedPoints = new List<MLPPointData>();            

            foreach (var light in parent.lights)
            {
                parent.currentPass = "Finding Out Of Range Areas For Light " + light.name;
                parent.currentPassProgressCounter = 0;
                parent.currentPassProgressFrameSkipper = 0;

                if (!parent.isInBackground)
                {
                    if (parent.UpdateProgress(parent.tmpSharedPointsArray.Count, 1))
                    {
                        yield return null;
                    }
                }

                Parallel.For(0, parent.tmpSharedPointsArray.Count, i =>
                {
                    if (light.saveOnOutOfRange)
                    {
                        switch (light.lightType)
                        {
                            case MLPLight.MLPLightType.Directional:
                                break;
                            default:
                                if (light.calculationMode != MLPLight.CalculationMode.LightIntensity)
                                {
                                    if (light.accurateTrace)
                                    {
                                        foreach (var tracePoint in light.tracePointsData)
                                        {
                                            switch (light.calculationMode)
                                            {
                                                case MLPLight.CalculationMode.AccurateShadows:
                                                    if (parent.tmpSharedPointsArray[i].inRangeForLights.Contains(light) && !parent.tmpSharedPointsArray[i].inCorner)
                                                    {
                                                        for (int np = 0; np < parent.tmpSharedPointsArray[i].nearbyPoints.Count; np++)
                                                        {
                                                            if (parent.tmpSharedPointsArray[i].nearbyPoints[np] != null)
                                                            {
                                                                if (!parent.tmpSharedPointsArray[i].nearbyPoints[np].inRangeForLights.Contains(light) && !parent.tmpSharedPointsArray[i].inCorner)
                                                                {
                                                                    if (light.saveOnOutOfRange)
                                                                    {
                                                                        if (!parent.tmpSharedPointsArray[i].inShadowForLights.Contains(light))
                                                                        {
                                                                            if (light.lightMode == LightmapBakeType.Mixed && QualitySettings.shadowmaskMode == ShadowmaskMode.DistanceShadowmask)
                                                                            {
                                                                                if (parent.tmpSharedPointsArray[i].nearbyPoints[np].savedNearGeometry)
                                                                                {
                                                                                    if ((parent.tmpSharedPointsArray[i].position - parent.tmpSharedPointsArray[i].nearbyPoints[np].position).normalized !=
                                                                                        (parent.tmpSharedPointsArray[i].contactPoint - parent.tmpSharedPointsArray[i].nearbyPoints[np].position).normalized)
                                                                                    {
                                                                                        parent.tmpSharedPointsArray[i].contrastOnOutOfRangeArea = true;
                                                                                        parent.tmpSharedPointsArray[i].nearbyPoints[np].contrastOnOutOfRangeArea = true;
                                                                                    }
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                if ((parent.tmpSharedPointsArray[i].position - parent.tmpSharedPointsArray[i].nearbyPoints[np].position).normalized !=
                                                                                    (parent.tmpSharedPointsArray[i].contactPoint - parent.tmpSharedPointsArray[i].nearbyPoints[np].position).normalized)
                                                                                {
                                                                                    parent.tmpSharedPointsArray[i].contrastOnOutOfRangeArea = true;
                                                                                    parent.tmpSharedPointsArray[i].nearbyPoints[np].contrastOnOutOfRangeArea = true;
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
                                    else
                                    {
                                        switch (light.calculationMode)
                                        {
                                            case MLPLight.CalculationMode.AccurateShadows:
                                                if (parent.tmpSharedPointsArray[i].inRangeForLights.Contains(light) && !parent.tmpSharedPointsArray[i].inCorner)
                                                {
                                                    for (int np = 0; np < parent.tmpSharedPointsArray[i].nearbyPoints.Count; np++)
                                                    {
                                                        if (parent.tmpSharedPointsArray[i].nearbyPoints[np] != null)
                                                        {
                                                            if (!parent.tmpSharedPointsArray[i].nearbyPoints[np].inRangeForLights.Contains(light) && !parent.tmpSharedPointsArray[i].inCorner)
                                                            {
                                                                if (light.saveOnOutOfRange)
                                                                {
                                                                    if (!parent.tmpSharedPointsArray[i].inShadowForLights.Contains(light))
                                                                    {
                                                                        if (light.lightMode == LightmapBakeType.Mixed && QualitySettings.shadowmaskMode == ShadowmaskMode.DistanceShadowmask)
                                                                        {
                                                                            if (parent.tmpSharedPointsArray[i].nearbyPoints[np].savedNearGeometry)
                                                                            {
                                                                                if ((parent.tmpSharedPointsArray[i].position - parent.tmpSharedPointsArray[i].nearbyPoints[np].position).normalized !=
                                                                                    (parent.tmpSharedPointsArray[i].contactPoint - parent.tmpSharedPointsArray[i].nearbyPoints[np].position).normalized)
                                                                                {
                                                                                    parent.tmpSharedPointsArray[i].nearbyPoints[np].contrastOnOutOfRangeArea = true;
                                                                                    parent.tmpSharedPointsArray[i].contrastOnOutOfRangeArea = true;
                                                                                }
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            if ((parent.tmpSharedPointsArray[i].position - parent.tmpSharedPointsArray[i].nearbyPoints[np].position).normalized !=
                                                                                (parent.tmpSharedPointsArray[i].contactPoint - parent.tmpSharedPointsArray[i].nearbyPoints[np].position).normalized)
                                                                            {
                                                                                parent.tmpSharedPointsArray[i].contrastOnOutOfRangeArea = true;
                                                                                parent.tmpSharedPointsArray[i].nearbyPoints[np].contrastOnOutOfRangeArea = true;
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
                                break;
                        }
                    }
                });
            }

            if (parent.debugMode)
            {
                parent.currentPass = "Creating Exit List...";
                parent.currentPassProgressCounter = 0;
                parent.currentPassProgressFrameSkipper = 0;

                parent.tmpSharedPointsArray.AddRange(addedPoints);

                foreach (var point in parent.tmpSharedPointsArray)
                {
                    if (point != null)
                    {
                        if (point.inRangeForLights.Count > 0)
                        {
                            tempList.Add(point);
                        }
                        else
                        {
                            parent.tmpOutOfRangePoints.Add(point);
                        }

                        if (point.contrastOnOutOfRangeArea)
                        {
                            parent.tmpContrastOnOutOfRangePoints.Add(point);
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
                    case MagicLightProbes.DebugPasses.NearGeometry:
                        parent.debugAcceptedPoints.AddRange(tempList);
                        parent.debugAcceptedPoints.AddRange(parent.tmpContrastOnOutOfRangePoints);
                        parent.debugAcceptedPoints.AddRange(parent.tmpOutOfRangePoints);
                        break;
                    case MagicLightProbes.DebugPasses.OutOfRange:
                        parent.debugAcceptedPoints.AddRange(tempList);
                        parent.debugCulledPoints.AddRange(parent.tmpOutOfRangePoints);

                        if (parent.debugPass == MagicLightProbes.DebugPasses.OutOfRange)
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
                        break;
                    case MagicLightProbes.DebugPasses.OutOfRangeBorders:
                        parent.debugAcceptedPoints.AddRange(parent.tmpContrastOnOutOfRangePoints);
                        parent.totalProbesInSubVolume = parent.debugAcceptedPoints.Count;
                        break;
                    case MagicLightProbes.DebugPasses.NearLights:
                    case MagicLightProbes.DebugPasses.ShadingBorders:
                        parent.debugAcceptedPoints.AddRange(parent.tmpSharedPointsArray);
                        parent.totalProbesInSubVolume = parent.debugAcceptedPoints.Count;
                        break;
                    case MagicLightProbes.DebugPasses.LightIntensity:
                    case MagicLightProbes.DebugPasses.ContrastAreas:
                    case MagicLightProbes.DebugPasses.EqualProbes:
                        parent.debugAcceptedPoints.AddRange(parent.tmpContrastOnOutOfRangePoints);
                        parent.debugAcceptedPoints.AddRange(tempList);
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
                parent.totalProbesInSubVolume = parent.tmpSharedPointsArray.Count;
            }

            tempList.Clear();
            tempList = null;

            parent.calculatingVolumeSubPass = false;
        }
    }
}
#endif
