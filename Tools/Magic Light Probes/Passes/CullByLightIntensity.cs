#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MagicLightProbes
{
    /// <summary>
    /// Can work in multithreading mode
    /// </summary>
    public class CullByLightIntensity
    {
        public IEnumerator ExecutePass(MagicLightProbes parent, MLPVolume currentVolume = null, bool realtimeEditing = false)
        {
            parent.currentPass = "Culling By Light Intensity...";
            parent.currentPassProgressCounter = 0;
            parent.currentPassProgressFrameSkipper = 0;

            if (parent.debugMode)
            {
                parent.tmpSharedPointsArray.Clear();
                parent.tmpSharedPointsArray.AddRange(parent.debugAcceptedPoints);
                parent.debugAcceptedPoints.Clear();
            }

            List<MLPPointData> tempList = new List<MLPPointData>();

            if (realtimeEditing)
            {
                parent.tmpSharedPointsArray.Clear();

                for (int i = 0; i < tempList.Count; i++)
                {
                    tempList[i].equalIntensity = false;
                }
            }
            else
            {
                tempList.AddRange(parent.tmpSharedPointsArray);
            }

            int equivalent;            

            //Parallel.For(0, parent.acceptedPoints.Count, (i, state) =>
            for (int i = 0; i < tempList.Count; i++)
            {
                lock (tempList)
                {
                    if (tempList[i].nearbyPoints.Count > 0)
                    {
                        if (!tempList[i].inSaveRange &&
                            !tempList[i].contrastOnShadingArea &&
                            !tempList[i].contrastOnOutOfRangeArea &&
                            !tempList[i].savedNearGeometry)
                        {
                            if (!realtimeEditing)
                            {
                                //currentVolume.savedPointsForLightIntensityThresholdEditing.Add(tempList[i]);
                            }

                            if (tempList[i].lightIntensity > 0)
                            {
                                equivalent = 0;

                                foreach (MLPPointData neigboringPoint in tempList[i].nearbyPoints)
                                {
                                    if (neigboringPoint != null)
                                    {
                                        if (neigboringPoint.lightIntensity == 0)
                                        {
                                            equivalent = 0;
                                            break;
                                        }
                                        else
                                        {
                                            if (neigboringPoint.lightIntensity.EqualsApproximately(tempList[i].lightIntensity, tempList[i].lightIntensity * (1 - parent.lightIntensityTreshold)))
                                            {
                                                equivalent++;
                                            }
                                        }
                                    }
                                }

                                if (equivalent == tempList[i].nearbyPoints.Count)
                                {
                                    tempList[i].equalIntensity = true;
                                }
                                else
                                {
                                    tempList[i].contrastOnShadingArea = true;
                                }
                            }
                            else
                            {
                                equivalent = 0;

                                foreach (MLPPointData neigboringPoint in tempList[i].nearbyPoints)
                                {
                                    if (neigboringPoint != null)
                                    {
                                        if (neigboringPoint.lightIntensity > 0)
                                        {
                                            equivalent = 0;
                                            break;
                                        }
                                        else
                                        {
                                            if (!tempList[i].inSaveRange && !tempList[i].savedNearGeometry)
                                            {
                                                equivalent++;
                                            }
                                        }
                                    }
                                }

                                if (equivalent == tempList[i].nearbyPoints.Count)
                                {
                                    tempList[i].equalIntensity = true;
                                }
                                else
                                {
                                    tempList[i].contrastOnShadingArea = true;
                                }
                            }
                        }
                    }
                }
            }//);

            if (realtimeEditing)
            {
                parent.tmpEqualPoints.Clear();
                parent.tmpSharedPointsArray.Clear();
                parent.tmpSharedPointsArray.AddRange(tempList);
            }

            parent.currentPass = "Storing Equal Points...";
            parent.currentPassProgressCounter = 0;
            parent.currentPassProgressFrameSkipper = 0;

            foreach (var point in tempList)
            {
                if (point.equalIntensity &&
                    !point.inSaveRange &&
                    !point.contrastOnOutOfRangeArea &&
                    !point.contrastOnShadingArea &&
                    !point.inCorner &&
                    !point.savedNearGeometry)
                {
                    if (point.inShadowForLights.Count == parent.lights.Count || point.isUnlit)
                    {
                        parent.tmpSharedPointsArray.Remove(point);                        
                    }
                    else
                    {
                        parent.tmpSharedPointsArray.Remove(point);
                        parent.tmpEqualPoints.Add(point);
                        currentVolume.localEquivalentPointsPositions.Add(point.position);
                    }
                }
                else if (point.contrastOnOutOfRangeArea || point.contrastOnShadingArea)
                {
                    currentVolume.localContrastPoints.Add(point);
                }

                if (!parent.isInBackground)
                {
                    if (parent.UpdateProgress(tempList.Count, 1000))
                    {
                        yield return null;
                    }
                }
            }
            
            tempList.Clear();

            if (!parent.debugMode)
            {
                if (!realtimeEditing)
                {
                    //currentVolume.contrastPointsForQuickEditing.AddRange(parent.acceptedPoints);
                    parent.totalProbesInSubVolume = parent.tmpSharedPointsArray.Count;
                }
            }
            else
            {
                parent.debugAcceptedPoints.AddRange(parent.tmpEqualPoints);
                parent.totalProbesInSubVolume = parent.debugAcceptedPoints.Count;
            }

            parent.calculatingVolumeSubPass = false;
        }
    }
}
#endif
