#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MagicLightProbes
{
    /// <summary>
    /// Cannot work in multithreading mode
    /// </summary>
    public class CheckNearbyGeometry
    {
        public IEnumerator ExecutePass(MagicLightProbes parent, float volumeSpacing, float cornersDetectionThreshold, MLPVolume currentVolume = null)
        {
            parent.currentPass = "Check For Near Geometry: Array Processing...";
            parent.currentPassProgressCounter = 0;
            parent.currentPassProgressFrameSkipper = 0;

            List<MLPPointData> addedPoints = new List<MLPPointData>();
            List<MLPPointData> removedPoints = new List<MLPPointData>();

            GameObject cameraObject = new GameObject("Temp Camera");
            Camera renderCamera = cameraObject.AddComponent<Camera>();
            renderCamera.cullingMask = parent.layerMask;

            Texture2D tex = new Texture2D(128, 128, TextureFormat.RGB24, false);
            RenderTexture renderTexture = new RenderTexture(128, 128, 24);

            if (parent.debugMode)
            {
                parent.tmpSharedPointsArray.Clear();
                parent.tmpSharedPointsArray.AddRange(parent.debugAcceptedPoints);
                parent.debugAcceptedPoints.Clear();
            }

            for (int i = 0; i < parent.tmpSharedPointsArray.Count; i++)
            {
                SortedList<float, Vector3> results = new SortedList<float, Vector3>();
                RaycastHit hitInfo;

                Ray[] checkRays =
                {
                    new Ray(parent.tmpSharedPointsArray[i].position, Vector3.forward),
                    new Ray(parent.tmpSharedPointsArray[i].position, -Vector3.forward),
                    new Ray(parent.tmpSharedPointsArray[i].position, Vector3.right),
                    new Ray(parent.tmpSharedPointsArray[i].position, -Vector3.right),
                    new Ray(parent.tmpSharedPointsArray[i].position, Vector3.up),
                    new Ray(parent.tmpSharedPointsArray[i].position, -Vector3.up),
                };

                foreach (var ray in checkRays)
                {
                    if (Physics.Raycast(ray, out hitInfo, volumeSpacing + (volumeSpacing * 0.5f), parent.layerMask)) // parent.volumeSpacing + 0.1f?
                    {
                        if (!results.Keys.Contains(hitInfo.distance))
                        {
                            if (parent.CheckIfInside(parent.probesVolume.transform, hitInfo.point))
                            {
                                bool inSubVolume = false;

                                foreach (var volume in parent.innerVolumes)
                                {
                                    if (parent.CheckIfInside(volume, hitInfo.point))
                                    {
                                        inSubVolume = true;
                                        break;
                                    }
                                }

                                if (!inSubVolume)
                                {
                                    results.Add(hitInfo.distance, hitInfo.point);
                                }
                            }
                        }
                    }
                }

                if (results.Count > 0)
                {
                    foreach (var result in results)
                    {
                        MLPPointData newPoint = new MLPPointData();
                        Vector3 closestPoint = result.Value;
                        Ray rayToPoint = new Ray(parent.tmpSharedPointsArray[i].position, (closestPoint - parent.tmpSharedPointsArray[i].position).normalized);

                        if (Physics.Raycast(rayToPoint, out hitInfo, volumeSpacing + 0.1f, parent.layerMask))
                        {
                            if (parent.CheckIfStatic(hitInfo.collider.gameObject))
                            {
                                if (hitInfo.collider.gameObject.GetComponent<MLPForceNoProbes>() != null)
                                {
                                    removedPoints.Add(parent.tmpSharedPointsArray[i]);
                                    continue;
                                }

                                removedPoints.Add(parent.tmpSharedPointsArray[i]);

                                newPoint.position = hitInfo.point;

                                Vector3 direction = (parent.tmpSharedPointsArray[i].position - newPoint.position).normalized;

                                newPoint.position += direction * parent.distanceFromNearbyGeometry;

                                bool tooClose = false;

                                foreach (var point in currentVolume.localCornerPoints)
                                {
                                    if (Vector3.Distance(point.position, newPoint.position) < cornersDetectionThreshold)
                                    {
                                        tooClose = true;
                                        break;
                                    }
                                }

                                if (!tooClose)
                                {
                                    renderCamera.targetTexture = renderTexture;
                                    renderCamera.nearClipPlane = 0.01f;
                                    renderCamera.farClipPlane = volumeSpacing * 2;
                                    renderCamera.fieldOfView = 1;
                                    renderCamera.transform.position = newPoint.position;
                                    renderCamera.transform.LookAt(hitInfo.point);
                                    renderCamera.Render();

                                    RenderTexture.active = renderTexture;
                                    tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

                                    Color pixel;
                                    float red = 0;
                                    float green = 0;
                                    float blue = 0;
                                    int count = 0;

                                    for (int x = 0; x < renderTexture.width; x++)
                                    {
                                        for (int y = 0; y < renderTexture.height; y++)
                                        {
                                            pixel = tex.GetPixel(x, y);

                                            red += pixel.r;
                                            green += pixel.g;
                                            blue += pixel.b;

                                            count++;
                                        }
                                    }

                                    red /= count;
                                    green /= count;
                                    blue /= count;

                                    Color average = new Color(red, green, blue, 1);

                                    newPoint.col = parent.tmpSharedPointsArray[i].col;
                                    newPoint.row = parent.tmpSharedPointsArray[i].row;
                                    newPoint.depth = parent.tmpSharedPointsArray[i].depth;
                                    newPoint.collisionNormal = hitInfo.normal;
                                    newPoint.savedNearGeometry = true;
                                    newPoint.contactPoint = closestPoint;
                                    newPoint.collisionObject = hitInfo.collider.gameObject;
                                    newPoint.averagedColor = average;
                                    newPoint.xStartPoint = parent.tmpSharedPointsArray[i].xStartPoint;
                                    newPoint.yStartPoint = parent.tmpSharedPointsArray[i].yStartPoint;
                                    newPoint.zStartPoint = parent.tmpSharedPointsArray[i].zStartPoint;
                                    newPoint.xEndPoint = parent.tmpSharedPointsArray[i].xEndPoint;
                                    newPoint.yEndPoint = parent.tmpSharedPointsArray[i].yEndPoint;
                                    newPoint.zEndPoint = parent.tmpSharedPointsArray[i].zEndPoint;

                                    parent.tmpNearbyGeometryPoints.Add(newPoint);

                                    //if (currentVolume != null)
                                    //{
                                    //    currentVolume.localDirections.Add(direction);
                                    //    currentVolume.localNearbyGeometryPointsPositions.Add(newPoint.position);
                                    //}

                                    RenderTexture.active = null;
                                }
                            }
                        }
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

            Object.DestroyImmediate(cameraObject);

            if (parent.debugMode)
            {
                switch (parent.debugPass)
                {
                    case MagicLightProbes.DebugPasses.NearGeometry:
                    case MagicLightProbes.DebugPasses.GeometryEdges:
                    case MagicLightProbes.DebugPasses.EqualColor:
                        parent.debugAcceptedPoints.AddRange(parent.tmpNearbyGeometryPoints);
                        parent.totalProbesInSubVolume = parent.debugAcceptedPoints.Count;
                        break;
                    default:
                        parent.debugAcceptedPoints.AddRange(parent.tmpSharedPointsArray);
                        parent.debugAcceptedPoints.AddRange(parent.tmpNearbyGeometryPoints);
                        parent.totalProbesInSubVolume = parent.debugAcceptedPoints.Count;
                        break;
                }
            }
            else
            {
                parent.currentPass = "Check For Near Geometry: Removing Unused Points...";
                parent.currentPassProgressCounter = 0;
                parent.currentPassProgressFrameSkipper = 0;

                Parallel.For(0, removedPoints.Count, (i, state) =>
                {
                    lock (parent.tmpSharedPointsArray)
                    {
                        parent.tmpSharedPointsArray.Remove(removedPoints[i]);
                    }
                });

                parent.tmpFreePoints.AddRange(parent.tmpSharedPointsArray);

                for (int i = 0; i < parent.tmpSharedPointsArray.Count; i++)
                {
                    currentVolume.localFreePointsPositions.Add(parent.tmpSharedPointsArray[i].position);
                }

                parent.currentPass = "Check For Near Geometry: Removing Of Closest Points...";
                parent.currentPassProgressCounter = 0;
                parent.currentPassProgressFrameSkipper = 0;

                List<MLPPointData> removeList = new List<MLPPointData>();

                foreach (var point in parent.tmpNearbyGeometryPoints)
                {
                    foreach (var checkPoint in parent.tmpPointsNearGeometryIntersections)
                    {
                        if (Vector3.Distance(point.position, checkPoint.position) <= cornersDetectionThreshold)
                        {
                            removeList.Add(point);
                            break;
                        }
                    }

                    if (!parent.isInBackground)
                    {
                        if (parent.UpdateProgress(parent.tmpNearbyGeometryPoints.Count, 1000))
                        {
                            yield return null;
                        }
                    }
                }

                Parallel.For(0, removeList.Count, (i, state) =>
                {
                    lock (parent.tmpNearbyGeometryPoints)
                    {
                        parent.tmpNearbyGeometryPoints.Remove(removeList[i]);
                    }
                });

                //currentVolume.localNearbyGeometryPoints.AddRange(parent.tmpNearbyGeometryPoints);
                parent.tmpSharedPointsArray.AddRange(parent.tmpNearbyGeometryPoints);
                parent.totalProbesInSubVolume += parent.tmpNearbyGeometryPoints.Count;
            }

            parent.calculatingVolumeSubPass = false;
        }
    }
}
#endif
