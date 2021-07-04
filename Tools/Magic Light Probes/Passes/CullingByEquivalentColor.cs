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
    public class CullingByEquivalentColor
    {
        [System.Serializable]
        private struct TempPointData
        {           
            public float xPos;
            public float yPos;
            public float zPos;
            public float colorR;
            public float colorG;
            public float colorB;
            public float[] nearbyAvaragedColors;

            public TempPointData(Vector3 _position, Color _avaragedColor, List<float> _nearbyAvaragedColors)
            {
                xPos = _position.x;
                yPos = _position.y;
                zPos = _position.z;
                colorR = _avaragedColor.r;
                colorG = _avaragedColor.g;
                colorB = _avaragedColor.b;
                nearbyAvaragedColors = _nearbyAvaragedColors.ToArray();
            }
        }

        public IEnumerator ExecutePass(MagicLightProbes parent, int volumePartIndex, MLPVolume currentVolume = null, bool realtimeEditing = false)
        {
            parent.currentPass = "Culling By Equivalent Color...";
            parent.currentPassProgressCounter = 0;
            parent.currentPassProgressFrameSkipper = 0;

            if (parent.debugMode)
            {
                parent.tmpNearbyGeometryPoints.Clear();
                parent.tmpNearbyGeometryPoints.AddRange(parent.debugAcceptedPoints);
                parent.debugAcceptedPoints.Clear();
            }

            List<TempPointData> tempPointDatas = new List<TempPointData>();
            List<MLPPointData> tempList = new List<MLPPointData>();
            List<MLPPointData> lockedForCullList = new List<MLPPointData>();

            string dirPath = parent.assetEditorPath + "/Scene Data/" + SceneManager.GetActiveScene().name + "/" + parent.name + "/ColorThresholdData";
            string fullFilePath = dirPath + "/" + parent.name + "_" + "vol_" + volumePartIndex + "_ColorThresholdData.mlpdat";

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            if (realtimeEditing)
            {
                tempPointDatas = MLPDataSaver.LoadData(tempPointDatas, fullFilePath);

                if (tempPointDatas.Count > 0)
                {
                    for (int i = 0; i < tempPointDatas.Count; i++)
                    {
                        MLPPointData pointData = new MLPPointData();

                        pointData.position = new Vector3(tempPointDatas[i].xPos, tempPointDatas[i].yPos, tempPointDatas[i].zPos);
                        pointData.averagedColor = new Color(tempPointDatas[i].colorR, tempPointDatas[i].colorG, tempPointDatas[i].colorB);

                        for (int j = 0; j < tempPointDatas[i].nearbyAvaragedColors.Length; j++)
                        {
                            MLPPointData nerabyPoint = new MLPPointData();
                            nerabyPoint.avaragedColorValue = tempPointDatas[i].nearbyAvaragedColors[j];

                            pointData.nearbyPoints.Add(nerabyPoint);
                        }
                        
                        tempList.Add(pointData);
                    }

                    parent.tmpSharedPointsArray.Clear();

                    for (int i = 0; i < tempList.Count; i++)
                    {
                        tempList[i].equalColor = false;
                    }
                }
            }
            else
            {
                tempList.AddRange(parent.tmpNearbyGeometryPoints);
            }

            for (int i = 0; i < tempList.Count; i++)
            {
                if (!realtimeEditing)
                {
                    if ((tempList[i].collisionObject != null && tempList[i].collisionObject.gameObject.GetComponent<MLPForceSaveProbes>() != null) ||
                        tempList[i].onGeometryEdge ||
                        tempList[i].lightLeakLocked)
                    {                      
                        TempPointData tempPoint = new TempPointData(tempList[i].position, tempList[i].averagedColor, new List<float>());
                        tempPointDatas.Add(tempPoint);

                        continue; 
                    }
                    else
                    {
                        if (tempList[i].contrastOnShadingArea || tempList[i].contrastOnOutOfRangeArea)
                        {
                            if (parent.forceSaveProbesOnShadingBorders)
                            {
                                TempPointData tempPoint = new TempPointData(tempList[i].position, tempList[i].averagedColor, new List<float>());
                                tempPointDatas.Add(tempPoint);

                                continue;
                            }
                        }
                    }
                }

                int equivalent = 0;

                List<float> tempAvaragedColors = new List<float>();

                for (int j = 0; j < tempList[i].nearbyPoints.Count; j++)
                {
                    if (tempList[i].nearbyPoints[j] != null)
                    {
                        if (tempList[i].nearbyPoints[j].avaragedColorValue == 0)
                        {
                            float averagedColorValue =
                                    (tempList[i].nearbyPoints[j].averagedColor.r +
                                    tempList[i].nearbyPoints[j].averagedColor.g +
                                    tempList[i].nearbyPoints[j].averagedColor.b) / 3;

                            tempList[i].nearbyPoints[j].avaragedColorValue = averagedColorValue;
                        }

                        if (tempList[i].avaragedColorValue == 0)
                        {
                            float averagedColorValueCompared =
                                    (tempList[i].averagedColor.r +
                                    tempList[i].averagedColor.g +
                                    tempList[i].averagedColor.b) / 3;

                            tempList[i].avaragedColorValue = averagedColorValueCompared;
                        }

                        if (tempList[i].nearbyPoints[j].avaragedColorValue.EqualsApproximately(tempList[i].avaragedColorValue, parent.colorTreshold))
                        {
                            equivalent++;
                        }

                        tempAvaragedColors.Add(tempList[i].nearbyPoints[j].avaragedColorValue);
                    }
                }

                if (tempList[i].nearbyPoints.Count > 0 && equivalent == tempList[i].nearbyPoints.Count)
                {
                    tempList[i].equalColor = true;
                }

                if (!realtimeEditing)
                {
                    TempPointData tempPoint = new TempPointData(tempList[i].position, tempList[i].averagedColor, tempAvaragedColors);
                    tempPointDatas.Add(tempPoint);
                    //currentVolume.localColorThresholdEditingPoints.Add(tempList[i]);
                }

                if (!parent.isInBackground)
                {
                    if (parent.UpdateProgress(tempList.Count, 1000))
                    {
                        yield return null;
                    }
                }
            }

            if (!realtimeEditing)
            {
                MLPDataSaver.SaveData(tempPointDatas, fullFilePath);
            }

            if (!parent.debugMode)
            {
                if (realtimeEditing)
                {
                    currentVolume.localNearbyGeometryPoints.Clear();
                    currentVolume.localNearbyGeometryPoints.AddRange(lockedForCullList);
                    currentVolume.localNearbyGeometryPoints.AddRange(tempList);
                }
                
                parent.currentPass = "Removing Equal Points...";
                parent.currentPassProgressCounter = 0;
                parent.currentPassProgressFrameSkipper = 0;

                foreach (var point in tempList)
                {
                    if (point.equalColor)
                    {
                        if (parent.forceSaveProbesOnShadingBorders)
                        {
                            if (!point.contrastOnShadingArea && !point.contrastOnOutOfRangeArea)
                            {
                                if (realtimeEditing)
                                {
                                    currentVolume.localNearbyGeometryPoints.Remove(point);
                                }
                                else
                                {
                                    parent.tmpNearbyGeometryPoints.Remove(point);
                                }
                            }
                        }
                        else
                        {
                            if (realtimeEditing)
                            {
                                currentVolume.localNearbyGeometryPoints.Remove(point);
                            }
                            else
                            {
                                parent.tmpNearbyGeometryPoints.Remove(point);
                            }
                        }
                    }
                    else
                    {
                        if (!realtimeEditing)
                        {
                            currentVolume.resultNearbyGeometryPointsPositions.Add(point.position);
                        }
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

                parent.totalProbesInSubVolume = parent.tmpSharedPointsArray.Count;
            }
            else
            {
                parent.debugAcceptedPoints.AddRange(parent.tmpNearbyGeometryPoints);
                parent.totalProbesInSubVolume = parent.debugAcceptedPoints.Count;
            }

            parent.calculatingVolumeSubPass = false;
        }
    }
}
#endif
