using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLightProbes
{
    public class SetDistanceFromGeometry
    {
        public IEnumerator ExecutePass(MagicLightProbes parent, MLPVolume currentVolume)
        {
            currentVolume.resultLocalCornerPointsPositions.Clear();

            ComputeBuffer inputArray;
            ComputeBuffer exitArray;
            ComputeBuffer directionsArray;

            inputArray = new ComputeBuffer(currentVolume.localCornerPointsPositions.Count, 3 * sizeof(float), ComputeBufferType.Default);
            exitArray = new ComputeBuffer(currentVolume.localCornerPointsPositions.Count, 3 * sizeof(float), ComputeBufferType.Default);
            directionsArray = new ComputeBuffer(currentVolume.localCornerPointsPositions.Count, 3 * sizeof(float), ComputeBufferType.Default);

            inputArray.SetData(currentVolume.localCornerPointsPositions.ToArray());
            exitArray.SetData(currentVolume.localCornerPointsPositions.ToArray());
            directionsArray.SetData(currentVolume.localAvaragedDirections.ToArray());

            parent.calculateDistanceFromGeometry.SetBuffer(parent.calculateDistanceFromGeometry.FindKernel("CSMain"), "inputArray", inputArray);
            parent.calculateDistanceFromGeometry.SetBuffer(parent.calculateDistanceFromGeometry.FindKernel("CSMain"), "exitArray", exitArray);
            parent.calculateDistanceFromGeometry.SetBuffer(parent.calculateDistanceFromGeometry.FindKernel("CSMain"), "directionsArray", directionsArray);
            parent.calculateDistanceFromGeometry.SetFloat("distance", parent.unlitVolumeFillingRate);

            parent.calculateDistanceFromGeometry.Dispatch(parent.calculateDistanceFromGeometry.FindKernel("CSMain"), 256, 1, 1);

            Vector3[] exit = new Vector3[inputArray.count];
            exitArray.GetData(exit);

            inputArray.Dispose();
            exitArray.Dispose();

            currentVolume.localCornerPointsPositions.Clear();

            for (int i = 0; i < exit.Length; i++)
            {
                if (exit[i] == Vector3.zero)
                {
                    continue;
                }

                currentVolume.localCornerPointsPositions.Add(exit[i]);

                if (parent.UpdateProgress(exit.Length, 1000))
                {
                    yield return null;
                }
            }
        }
    }
}
