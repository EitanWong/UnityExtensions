using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace MagicLightProbes
{
    /// <summary>
    /// Cannot work in multithreading mode
    /// </summary>
    /// 
    public static class RandomGen
    {
        private static System.Random _global = new System.Random();
        [ThreadStatic]
        private static System.Random _local;

        public static int Next(int min, int max)
        {
            System.Random inst = _local;

            if (inst == null)
            {
                int seed;
                lock (_global) seed = _global.Next(min, max);
                _local = inst = new System.Random(seed);
            }

            return inst.Next(min, max);
        }
    }

    public class EquivalentProbesOptimization
    {
        public IEnumerator ExecutePass(MagicLightProbes parent, MLPVolume currentVolume = null, bool realtimeEditing = false)
        {
            parent.currentPass = "Equivalent Probes Optimization...";
            parent.currentPassProgressCounter = 0;
            parent.currentPassProgressFrameSkipper = 0;

            if (parent.debugMode)
            {
                parent.tmpSharedPointsArray.Clear();
                parent.debugAcceptedPoints.Clear();
            }

            if (currentVolume.localEquivalentPointsPositions.Count > 0)
            {
                currentVolume.resultLocalEquivalentPointsPositions.Clear();

                ComputeBuffer inputArray;
                ComputeBuffer exitArray;

                inputArray = new ComputeBuffer(currentVolume.localEquivalentPointsPositions.Count, 3 * sizeof(float), ComputeBufferType.Default);
                exitArray = new ComputeBuffer(currentVolume.localEquivalentPointsPositions.Count, 3 * sizeof(float), ComputeBufferType.Default);

                inputArray.SetData(currentVolume.localEquivalentPointsPositions.ToArray());
                exitArray.SetData(currentVolume.localEquivalentPointsPositions.ToArray());

                parent.calculateVolumeFilling.SetBuffer(parent.calculateVolumeFilling.FindKernel("CSMain"), "inputArray", inputArray);
                parent.calculateVolumeFilling.SetBuffer(parent.calculateVolumeFilling.FindKernel("CSMain"), "exitArray", exitArray);
                parent.calculateVolumeFilling.SetFloat("threshold", parent.equivalentVolumeFillingRate);

                parent.calculateVolumeFilling.Dispatch(parent.calculateVolumeFilling.FindKernel("CSMain"), 256, 1, 1);

                Vector3[] exit = new Vector3[inputArray.count];
                exitArray.GetData(exit);

                inputArray.Dispose();
                exitArray.Dispose();

                List<MLPPointData> tempList = new List<MLPPointData>();
                tempList.AddRange(parent.tmpEqualPoints);

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

                    currentVolume.resultLocalEquivalentPointsPositions.Add(exit[i]);

                    if (!parent.isInBackground)
                    {
                        if (parent.UpdateProgress(exit.Length, 1000))
                        {
                            yield return null;
                        }
                    }
                }

                if (parent.debugMode)
                {
                    parent.debugAcceptedPoints.AddRange(parent.tmpSharedPointsArray);
                }
            }

            parent.totalProbesInSubVolume = parent.tmpSharedPointsArray.Count;
            parent.calculatingVolumeSubPass = false;
        }
    }
}
