#if UNITY_EDITOR
using System.Collections;
using System.Threading.Tasks;

namespace MagicLightProbes
{
    /// <summary>
    /// Cannot work in multithreading mode
    /// </summary>
    public class FindOutOfRangePoins
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

            foreach (var light in parent.lights)
            {
                parent.currentPass = "Finding Out Of Range Points For Light " + light.name;
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
                        switch (light.lightType)
                        {
                            case MLPLight.MLPLightType.Directional:
                                parent.tmpSharedPointsArray[i].inRangeForLights.Add(light);
                                break;
                            default:
                                if (light.accurateTrace)
                                {
                                    foreach (var tracePoint in light.tracePointsData)
                                    {
                                        parent.CheckIfOutOfRange(light, tracePoint, parent.tmpSharedPointsArray[i]);
                                        tracePoint.pointGameObject.SetActive(false);
                                    }
                                }
                                else
                                {
                                    parent.CheckIfOutOfRange(light, light.mainTracePoint, parent.tmpSharedPointsArray[i]);
                                }
                                break;
                        }
                    }
                }
            }            

            if (parent.debugMode)
            {
                parent.debugAcceptedPoints.Clear();

                for (int i = 0; i < parent.tmpSharedPointsArray.Count; i++)
                {
                    switch (parent.debugPass)
                    {
                        case MagicLightProbes.DebugPasses.OutOfRange:
                            if (parent.tmpSharedPointsArray[i].inRangeForLights.Count > 0)
                            {
                                parent.debugAcceptedPoints.Add(parent.tmpSharedPointsArray[i]);
                            }
                            else
                            {
                                parent.debugCulledPoints.Add(parent.tmpSharedPointsArray[i]);
                            }
                            break;
                        case MagicLightProbes.DebugPasses.OutOfRangeBorders:
                        case MagicLightProbes.DebugPasses.LightIntensity:
                        case MagicLightProbes.DebugPasses.UnlitProbes:
                        case MagicLightProbes.DebugPasses.ShadingBorders:
                        case MagicLightProbes.DebugPasses.ContrastAreas:
                        case MagicLightProbes.DebugPasses.EqualProbes:
                            parent.debugAcceptedPoints.Add(parent.tmpSharedPointsArray[i]);
                            break;
                        case MagicLightProbes.DebugPasses.NearLights:
                            if (parent.tmpSharedPointsArray[i].inRangeForLights.Count > 0)
                            {
                                parent.debugAcceptedPoints.Add(parent.tmpSharedPointsArray[i]);
                            }
                            break;
                    }
                }
            }

            parent.calculatingVolumeSubPass = false;
        }
    }
}
#endif
