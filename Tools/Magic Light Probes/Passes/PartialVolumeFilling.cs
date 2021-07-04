using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLightProbes
{
    /// <summary>
    /// Cannot work in multithreading mode
    /// </summary>
    public class PartialVolumeFilling
    {
        public enum TargetPoint
        {
            Unlit,
            Equivalent,
            Free
        }

        public IEnumerator ExecutePass(MagicLightProbes parent, TargetPoint targetPoint, MLPVolume currentVolume = null, bool realtimeEditing = false)
        {
            List<MLPPointData> tempList = new List<MLPPointData>();
            List<Vector3> realTimeEditingList = new List<Vector3>();
            List<Vector3> targetPoints = new List<Vector3>();
            float fillingRate = 0;

            switch (targetPoint)
            {
                case TargetPoint.Unlit:
                    parent.currentPass = "Unlit Probes Processing Pass 1/2";
                    targetPoints.AddRange(currentVolume.localUnlitPointsPositions);
                    currentVolume.resultLocalFreePointsPositions.Clear();
                    currentVolume.resultLocalUnlitPointsPositions.Clear();
                    fillingRate = parent.unlitVolumeFillingRate;
                    tempList.AddRange(parent.tmpUnlitPoints);
                    break;
                case TargetPoint.Equivalent:
                    parent.currentPass = "Equivalent Probes Processing Pass 1/2";
                    targetPoints.AddRange(currentVolume.localEquivalentPointsPositions);
                    currentVolume.resultLocalFreePointsPositions.Clear();
                    currentVolume.resultLocalEquivalentPointsPositions.Clear();
                    fillingRate = parent.equivalentVolumeFillingRate;
                    tempList.AddRange(parent.tmpEqualPoints);
                    break;
                case TargetPoint.Free:
                    parent.currentPass = "Free Probes Processing";
                    targetPoints.AddRange(currentVolume.localFreePointsPositions);
                    currentVolume.resultLocalFreePointsPositions.Clear();
                    currentVolume.resultLocalEquivalentPointsPositions.Clear();
                    currentVolume.resultLocalUnlitPointsPositions.Clear();
                    fillingRate = parent.freeVolumeFillingRate;
                    tempList.AddRange(parent.tmpFreePoints);
                    break;
            }

            if (!realtimeEditing)
            {
                //tempList.Clear
            }

            realTimeEditingList.AddRange(targetPoints);

            parent.currentPassProgressCounter = 0;
            parent.currentPassProgressFrameSkipper = 0;

            if (targetPoints.Count > 0)
            {
                if (SystemInfo.supportsComputeShaders)
                {
                    ComputeBuffer inputArray;
                    ComputeBuffer exitArray;

                    inputArray = new ComputeBuffer(targetPoints.Count, 3 * sizeof(float), ComputeBufferType.Default);
                    exitArray = new ComputeBuffer(targetPoints.Count, 3 * sizeof(float), ComputeBufferType.Default);

                    inputArray.SetData(targetPoints.ToArray());
                    exitArray.SetData(targetPoints.ToArray());

                    parent.calculateVolumeFilling.SetBuffer(parent.calculateVolumeFilling.FindKernel("CSMain"), "inputArray", inputArray);
                    parent.calculateVolumeFilling.SetBuffer(parent.calculateVolumeFilling.FindKernel("CSMain"), "exitArray", exitArray);
                    parent.calculateVolumeFilling.SetFloat("threshold", fillingRate);

                    parent.calculateVolumeFilling.Dispatch(parent.calculateVolumeFilling.FindKernel("CSMain"), 256, 1, 1);

                    Vector3[] exit = new Vector3[inputArray.count];
                    exitArray.GetData(exit);

                    inputArray.Dispose();
                    exitArray.Dispose();

                    for (int i = 0; i < exit.Length; i++)
                    {
                        if (exit[i] == Vector3.zero)
                        {
                            continue;
                        }

                        if (!realtimeEditing)
                        {
                            tempList[i].position = exit[i];
                            parent.tmpSharedPointsArray.Add(tempList[i]);
                        }

                        switch (targetPoint)
                        {
                            case TargetPoint.Unlit:
                                currentVolume.resultLocalUnlitPointsPositions.Add(exit[i]);
                                break;
                            case TargetPoint.Equivalent:
                                currentVolume.resultLocalEquivalentPointsPositions.Add(exit[i]);
                                break;
                            case TargetPoint.Free:
                                currentVolume.resultLocalFreePointsPositions.Add(exit[i]);
                                break;
                        }

                        if (parent.UpdateProgress(exit.Length, 1000))
                        {
                            yield return null;
                        }
                    }
                }
                else
                {    
                    for (int i = 0; i < Mathf.RoundToInt(targetPoints.Count * (1 - fillingRate)); i++)
                    {
                        realTimeEditingList.Remove(realTimeEditingList[UnityEngine.Random.Range(0, realTimeEditingList.Count)]);                        

                        if (parent.UpdateProgress(Mathf.RoundToInt(targetPoints.Count * (1 - fillingRate))))
                        {
                            yield return null;
                        }
                    }

                    if (!realtimeEditing)
                    {
                        parent.tmpSharedPointsArray.AddRange(tempList);

                        for (int i = 0; i < tempList.Count; i++)
                        {
                            switch (targetPoint)
                            {
                                case TargetPoint.Unlit:
                                    currentVolume.resultLocalUnlitPointsPositions.Add(tempList[i].position);
                                    break;
                                case TargetPoint.Equivalent:
                                    currentVolume.resultLocalEquivalentPointsPositions.Add(tempList[i].position);
                                    break;
                                case TargetPoint.Free:
                                    currentVolume.resultLocalFreePointsPositions.Add(tempList[i].position);
                                    break;
                            }
                        }
                    }
                    else 
                    {
                        for (int i = 0; i < realTimeEditingList.Count; i++)
                        {
                            switch (targetPoint)
                            {
                                case TargetPoint.Unlit:
                                    currentVolume.resultLocalUnlitPointsPositions.Add(realTimeEditingList[i]);
                                    break;
                                case TargetPoint.Equivalent:
                                    currentVolume.resultLocalEquivalentPointsPositions.Add(realTimeEditingList[i]);
                                    break;
                                case TargetPoint.Free:
                                    currentVolume.resultLocalFreePointsPositions.Add(realTimeEditingList[i]);
                                    break;
                            }
                        }
                    }                    
                }

                if (targetPoint == TargetPoint.Unlit)
                {
                    if (!realtimeEditing)
                    {
                        parent.currentPass = "Unlit Probes Optimization Pass 2/2";
                        parent.currentPassProgressCounter = 0;
                        parent.currentPassProgressFrameSkipper = 0;

                        for (int i = 0; i < tempList.Count; i++)
                        {
                            if (!tempList[i].lockForCull)
                            {
                                parent.CheckForNearContrast(tempList[i]);
                            }

                            if (!parent.isInBackground)
                            {
                                if (parent.UpdateProgress(tempList.Count))
                                {
                                    yield return null;
                                }
                            }
                        }
                    }
                }
            }

            parent.totalProbesInSubVolume = parent.tmpSharedPointsArray.Count;
            parent.calculatingVolumeSubPass = false;
        }
    }
}
