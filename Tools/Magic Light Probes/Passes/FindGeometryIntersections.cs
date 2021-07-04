#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MagicLightProbes
{
    /// <summary>
    /// Cannot work in multithreading mode
    /// </summary>
    public class FindGeometryIntersections
    {
        [System.Serializable]
        private struct TempPointData
        {
            public float xPos;
            public float yPos;
            public float zPos;
            public float collisionNormalX;
            public float collisionNormalY;
            public float collisionNormalZ;

            public TempPointData(Vector3 _position, Vector3 _collisionNormal)
            {
                xPos = _position.x;
                yPos = _position.y;
                zPos = _position.z;
                collisionNormalX = _collisionNormal.x;
                collisionNormalY = _collisionNormal.y;
                collisionNormalZ = _collisionNormal.z;
            }
        }

        bool probeSpacingCalculating;
        
        public IEnumerator ExecutePass(MagicLightProbes parent, int volumePartIndex, float cornersDetectionThreshold, MLPVolume currentVolume = null, bool realtimeEditing = false)
        {
            parent.currentPass = "Check For Geometry Intersections: Array Processing...";
            parent.currentPassProgressCounter = 0;
            parent.currentPassProgressFrameSkipper = 0;

            List<TempPointData> tempPointDatas = new List<TempPointData>();

            string dirPath = parent.assetEditorPath + "/Scene Data/" + SceneManager.GetActiveScene().name + "/" + parent.name + "/GeometryIntersectionsData";
            string fullFilePath = dirPath + "/" + parent.name + "_" + "vol_" + volumePartIndex + "_GeometryIntersectionsData.mlpdat";

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            if (!realtimeEditing)
            {
                List<MLPPointData> removedPoints = new List<MLPPointData>();

                if (parent.debugMode)
                {
                    parent.tmpSharedPointsArray.Clear();
                    parent.tmpSharedPointsArray.AddRange(parent.debugAcceptedPoints);
                    parent.debugAcceptedPoints.Clear();
                }

                for (int i = 0; i < parent.tmpSharedPointsArray.Count; i++)
                {
                    Ray[] checkRays =
                    {
                        new Ray(parent.tmpSharedPointsArray[i].position, Vector3.forward),
                        new Ray(parent.tmpSharedPointsArray[i].position, -Vector3.forward),
                        new Ray(parent.tmpSharedPointsArray[i].position, Vector3.right),
                        new Ray(parent.tmpSharedPointsArray[i].position, -Vector3.right),
                        new Ray(parent.tmpSharedPointsArray[i].position, Vector3.up),
                        new Ray(parent.tmpSharedPointsArray[i].position, -Vector3.up),
                    };

                    SortedList<float, Vector3> results = new SortedList<float, Vector3>();
                    RaycastHit hitInfo;

                    foreach (var ray in checkRays)
                    {
                        if (Physics.Raycast(ray, out hitInfo, cornersDetectionThreshold + 0.1f, parent.layerMask))
                        {
                            if (parent.CheckIfStatic(hitInfo.collider.gameObject))
                            {
                                if (hitInfo.collider.gameObject.GetComponent<MLPForceNoProbes>() == null)
                                {
                                    if (!results.Keys.Contains(hitInfo.distance))
                                    {
                                        results.Add(hitInfo.distance, hitInfo.point);
                                    }
                                }
                            }
                        }
                    }

                    if (results.Count > 1)
                    {
                        Vector3 targetPoint = new Vector3();
                        Vector3 avaragedDirection = new Vector3();

                        foreach (var result in results)
                        {
                            targetPoint.x += result.Value.x;
                            targetPoint.y += result.Value.y;
                            targetPoint.z += result.Value.z;

                            avaragedDirection.x += result.Value.x - parent.tmpSharedPointsArray[i].position.x;
                            avaragedDirection.y += result.Value.y - parent.tmpSharedPointsArray[i].position.y;
                            avaragedDirection.z += result.Value.z - parent.tmpSharedPointsArray[i].position.z;
                        }

                        targetPoint.x /= results.Count;
                        targetPoint.y /= results.Count;
                        targetPoint.z /= results.Count;

                        avaragedDirection.x /= results.Count;
                        avaragedDirection.y /= results.Count;
                        avaragedDirection.z /= results.Count;

                        avaragedDirection = Vector3.Normalize(avaragedDirection);

                        Ray rayToPoint = new Ray(parent.tmpSharedPointsArray[i].position, (targetPoint - parent.tmpSharedPointsArray[i].position).normalized);
                        MLPPointData newPoint = new MLPPointData();

                        if (Physics.Raycast(rayToPoint, out hitInfo, cornersDetectionThreshold * 2, parent.layerMask))
                        {
                            newPoint.position = hitInfo.point;
                            newPoint.position -= avaragedDirection * parent.distanceFromNearbyGeometry;
                            newPoint.collisionNormal = hitInfo.normal;
                            newPoint.inCorner = true;
                            newPoint.contactPoint = targetPoint;
                            
                            currentVolume.localCornerPoints.Add(newPoint);
                            currentVolume.localCornerPointsPositions.Add(newPoint.position);
                            currentVolume.localAvaragedDirections.Add(avaragedDirection);
                            tempPointDatas.Add(new TempPointData(newPoint.position, newPoint.collisionNormal));                            

                            //if (Vector3.Distance(parent.tmpSharedPointsArray[i].position, hitInfo.point) < parent.cornersDetectionThreshold / 2)
                            //{
                            //    removedPoints.Add(parent.tmpSharedPointsArray[i]);

                            //    //collisionNormals.Add(newPoint.collisionNormal);
                            //}
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

                if (!parent.debugMode)
                {
                    MLPDataSaver.SaveData(tempPointDatas, fullFilePath);
                }

                parent.currentPass = "Check For Geometry Intersections: Array Optimization...";
                parent.currentPassProgressCounter = 0;
                parent.currentPassProgressFrameSkipper = 0;

                CalculateProbeSpacing(parent, currentVolume, realtimeEditing);

                while (probeSpacingCalculating)
                {
                    yield return null;
                }

                if (parent.debugMode)
                {
                    removedPoints.Clear();
                    parent.tmpSharedPointsArray.Clear();

                    switch (parent.debugPass)
                    {
                        case MagicLightProbes.DebugPasses.GeometryIntersections:
                            parent.debugAcceptedPoints.AddRange(parent.tmpPointsNearGeometryIntersections);
                            break;
                        case MagicLightProbes.DebugPasses.NearGeometry:
                            parent.debugAcceptedPoints.AddRange(parent.tmpPointsNearGeometryIntersections);
                            break;
                    }

                    parent.totalProbesInSubVolume = parent.debugAcceptedPoints.Count;
                }
            }
            else
            {
                tempPointDatas = MLPDataSaver.LoadData(tempPointDatas, fullFilePath);
                currentVolume.localCornerPoints.Clear();

                if (tempPointDatas.Count > 0)
                {
                    for (int i = 0; i < tempPointDatas.Count; i++)
                    {
                        MLPPointData tempPoint = new MLPPointData();
                        tempPoint.position = new Vector3(tempPointDatas[i].xPos, tempPointDatas[i].yPos, tempPointDatas[i].zPos);
                        tempPoint.collisionNormal = new Vector3(tempPointDatas[i].collisionNormalX, tempPointDatas[i].collisionNormalY, tempPointDatas[i].collisionNormalZ);
                        currentVolume.localCornerPoints.Add(tempPoint);
                    }
                }

                CalculateProbeSpacing(parent, currentVolume, realtimeEditing);

                if (!parent.isInBackground)
                {
                    while (probeSpacingCalculating)
                    {
                        yield return null;
                    }
                }
            }

            parent.calculatingVolumeSubPass = false;
        }

        private void CalculateProbeSpacing(MagicLightProbes parent, MLPVolume currentVolume, bool realtimeEditing = false)
        {
            probeSpacingCalculating = true;

            #region Test Implementations

            //if (currentVolume.localCornerPointsPositions.Count > 0)
            //{
            //    if (realtimeEditing)
            //    {
            //        parent.currentPass = "Recalculate Corner Probe Spacing...";
            //        parent.currentPassProgressCounter = 0;
            //        parent.currentPassProgressFrameSkipper = 0;
            //    }

            //    currentVolume.resultLocalCornerPointsPositions.Clear();

            //    ComputeBuffer inputArray;
            //    ComputeBuffer exitArray;
            //    //ComputeBuffer collisionNormals;

            //    inputArray = new ComputeBuffer(currentVolume.localCornerPointsPositions.Count, 3 * sizeof(float), ComputeBufferType.Default);
            //    exitArray = new ComputeBuffer(currentVolume.localCornerPointsPositions.Count, 3 * sizeof(float), ComputeBufferType.Default);
            //    //collisionNormals = new ComputeBuffer(currentVolume.localCornerPointsPositions.Count, 3 * sizeof(float), ComputeBufferType.Default);

            //    inputArray.SetData(currentVolume.localCornerPointsPositions.ToArray());
            //    exitArray.SetData(currentVolume.localCornerPointsPositions.ToArray());
            //    //collisionNormals.SetData(tmpCollisionNormals.ToArray());

            //    parent.calculateProbeSpacing.SetBuffer(parent.calculateProbeSpacing.FindKernel("CSMain"), "inputArray", inputArray);
            //    parent.calculateProbeSpacing.SetBuffer(parent.calculateProbeSpacing.FindKernel("CSMain"), "exitArray", exitArray);
            //    //parent.calculateProbeSpacing.SetBuffer(parent.calculateProbeSpacing.FindKernel("CSMain"), "collisionNormals", collisionNormals);
            //    parent.calculateProbeSpacing.SetFloat("probeSpacing", parent.cornerProbesSpacing);
            //    parent.calculateProbeSpacing.SetInt("arrayCount", currentVolume.localCornerPointsPositions.Count);

            //    parent.calculateProbeSpacing.Dispatch(parent.calculateProbeSpacing.FindKernel("CSMain"), 256, 1, 1);

            //    Vector3[] exit = new Vector3[inputArray.count];
            //    exitArray.GetData(exit);

            //    inputArray.Dispose();
            //    exitArray.Dispose();
            //    //collisionNormals.Dispose();

            //    List<MLPPointData> tempList = new List<MLPPointData>();
            //    tempList.AddRange(parent.tmpPointsNearGeometryIntersections);
            //    parent.tmpPointsNearGeometryIntersections.Clear();

            //    for (int i = 0; i < exit.Length; i++)
            //    {
            //        if (exit[i] == Vector3.zero)
            //        {
            //            continue;
            //        }

            //        if (!realtimeEditing)
            //        {
            //            tempList[i].position = exit[i];
            //            parent.tmpPointsNearGeometryIntersections.Add(tempList[i]);
            //        }

            //        currentVolume.resultLocalCornerPointsPositions.Add(exit[i]);

            //        parent.UpdateProgress(exit.Length, 1000);
            //    }

            //    tempList.Clear();
            //}

            //for (int i = 0; i < parent.tmpPointsNearGeometryIntersections.Count; i++)
            //{
            //    for (int j = 0; j < parent.tmpPointsNearGeometryIntersections.Count; j++)
            //    {
            //        if (parent.tmpPointsNearGeometryIntersections[j] != parent.tmpPointsNearGeometryIntersections[i])
            //        {
            //            float distance = Vector3.Distance(parent.tmpPointsNearGeometryIntersections[i].position, parent.tmpPointsNearGeometryIntersections[j].position);
            //            float angle = Vector3.Dot(parent.tmpPointsNearGeometryIntersections[i].collisionNormal, parent.tmpPointsNearGeometryIntersections[j].collisionNormal);

            //            if (distance <= parent.cornerProbesSpacing && angle > 0.9f)
            //            {
            //                parent.tmpPointsNearGeometryIntersections.RemoveAt(i);
            //            }
            //        }
            //    }
            //}

            #endregion

            List<MLPPointData> cullList = new List<MLPPointData>();

            for (int i = 0; i < currentVolume.localCornerPoints.Count; i++)
            {
                currentVolume.localCornerPoints[i].lockForCull = false;
                currentVolume.localCornerPoints[i].removed = false;
            }

            for (int i = 0; i < currentVolume.localCornerPoints.Count; i++)
            {
                if (!currentVolume.localCornerPoints[i].removed)
                {
                    for (int j = 0; j < currentVolume.localCornerPoints.Count; j++)
                    {
                        if (currentVolume.localCornerPoints[j] != currentVolume.localCornerPoints[i])
                        {
                            float distance = Vector3.Distance(currentVolume.localCornerPoints[i].position, currentVolume.localCornerPoints[j].position);
                            float angle = Vector3.Dot(currentVolume.localCornerPoints[i].collisionNormal, currentVolume.localCornerPoints[j].collisionNormal);

                            if (distance <= parent.cornerProbesSpacing && angle > 0.9f && !currentVolume.localCornerPoints[j].lockForCull)
                            {
                                currentVolume.localCornerPoints[i].lockForCull = true;
                                currentVolume.localCornerPoints[j].removed = true;
                                cullList.Add(currentVolume.localCornerPoints[j]);
                            }
                        }
                    }
                }
            }

            currentVolume.resultLocalCornerPointsPositions.Clear();

            for (int i = 0; i < currentVolume.localCornerPoints.Count; i++)
            {
                if (!cullList.Contains(currentVolume.localCornerPoints[i]))
                {
                    currentVolume.resultLocalCornerPointsPositions.Add(currentVolume.localCornerPoints[i].position);

                    if (!realtimeEditing)
                    {
                        parent.tmpPointsNearGeometryIntersections.Add(currentVolume.localCornerPoints[i]); 
                    }
                }
            }

            probeSpacingCalculating = false;
        }
    }
}
#endif
