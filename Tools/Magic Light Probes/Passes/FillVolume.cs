#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MagicLightProbes
{
    /// <summary>
    /// Can work in multithreading mode
    /// </summary>
    public class FillVolume
    {
        public IEnumerator ExecutePass(MagicLightProbes parent, float spacing, MagicLightProbes.VolumeParameters volumePart, MagicLightProbes.CalculationTarget calculationTarget)
        {
            parent.currentPass = "Fill Volume...";
            parent.currentPassProgressCounter = 0;
            parent.currentPassProgressFrameSkipper = 0;

            MLPPointData pointData;

            parent.recalculationRequired = false;

            var offset = volumePart.position;

            var stepX = Mathf.FloorToInt(volumePart.demensions.x / spacing);
            var stepY = Mathf.FloorToInt(volumePart.demensions.y / spacing);
            var stepZ = Mathf.FloorToInt(volumePart.demensions.z / spacing) + 1;

            parent.xPointsCount = stepX;
            parent.yPointsCount = stepY;
            parent.zPointsCount = stepZ;

            offset -= volumePart.demensions * 0.5f;
            offset.x += (volumePart.demensions.x - stepX * spacing) * 0.5f;
            offset.y += (volumePart.demensions.y - stepY * spacing) * 0.5f;
            offset.z += (volumePart.demensions.z - stepZ * spacing) * 0.5f;

            for (int x = 0; x < stepX; x++)
            {
                for (int y = 0; y < stepY; y++)
                {
                    for (int z = 0; z < stepZ; z++)
                    {
                        var pointPosition = offset + new Vector3(x * spacing, y * spacing, z * spacing);

                        pointData = new MLPPointData();
                        pointData.position = pointPosition;
                        pointData.col = x;
                        pointData.row = y;
                        pointData.depth = z;

                        if (parent.innerVolumes.Count > 0)
                        {
                            int containsIn = 0;

                            foreach (var volume in parent.innerVolumes)
                            {
                                if (parent.CheckIfInside(volume, pointPosition))
                                {
                                    containsIn++;
                                }
                            }

                            if (containsIn == 0)
                            {
                                parent.tmpSharedPointsArray.Add(pointData);
                                parent.totalProbes++;
                            }
                        }
                        else
                        {
                            parent.tmpSharedPointsArray.Add(pointData);
                            parent.totalProbes++;
                        }

                        parent.UpdateProgress(stepX * stepY * stepZ);
                    }
                }
            }

            if (!parent.isInBackground)
            {
                yield return null;
            }

            parent.calculatingVolumeSubPass = false;
        }
    }
}
#endif
