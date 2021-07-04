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
    public class FindGeometryEdges
    {
        public IEnumerator ExecutePass(MagicLightProbes parent, float volumeSpacing)
        {
            parent.currentPass = "Finding Geometry Edges...";
            parent.currentPassProgressCounter = 0;
            parent.currentPassProgressFrameSkipper = 0;            

            for (int i = 0; i < parent.tmpNearbyGeometryPoints.Count; i++)
            {
                //GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //temp.transform.localScale = Vector3.one * 0.1f;

                parent.layerMasks.Clear();

                for (int j = 0; j < parent.staticObjects.Count; j++)
                {
                    parent.layerMasks.Add(parent.staticObjects[j].layer);

                    if (parent.staticObjects[j] != parent.tmpNearbyGeometryPoints[i].collisionObject)
                    {
                        //parent.staticObjects[j].gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                    }
                }
                
                RaycastHit hitInfo;
                Vector3[] directions =
                {
                    Vector3.forward,
                    -Vector3.forward,
                    Vector3.right,
                    -Vector3.right,
                    Vector3.up,
                    -Vector3.up
                };

                bool edgeFound = false;

                Vector3 startPosition = new Vector3(
                        parent.tmpNearbyGeometryPoints[i].position.x,
                        parent.tmpNearbyGeometryPoints[i].position.y,
                        parent.tmpNearbyGeometryPoints[i].position.z);

                for (int d = 0; d < directions.Length && !edgeFound; d++)
                {
                    float dot = Vector3.Dot(directions[d], -parent.tmpNearbyGeometryPoints[i].collisionNormal);                  

                    if (dot.EqualsApproximately(0f, 0.1f))
                    {                        
                        parent.tmpNearbyGeometryPoints[i].position = startPosition;                        

                        float prevDistance = Vector3.Distance(startPosition, parent.tmpNearbyGeometryPoints[i].contactPoint);
                        float currentDistance = 0;

                        while (Vector3.Distance(startPosition, parent.tmpNearbyGeometryPoints[i].position) <= volumeSpacing && !edgeFound)
                        {
                            //Debug.DrawRay(parent.tmpNearbyGeometryPoints[i].position, directions[d], Color.red, 1f);
                            //temp.transform.position = parent.tmpNearbyGeometryPoints[i].position;

                            if (Physics.Raycast(new Ray(parent.tmpNearbyGeometryPoints[i].position, -parent.tmpNearbyGeometryPoints[i].collisionNormal), out hitInfo, parent.layerMask))
                            {
                                currentDistance = hitInfo.distance;

                                float difference = Mathf.Abs(prevDistance - currentDistance);

                                if (difference > 0.5f)
                                {
                                    parent.tmpNearbyGeometryPoints[i].position -= directions[d] * 0.1f;
                                    parent.tmpNearbyGeometryPoints[i].onGeometryEdge = true;
                                    edgeFound = true;
                                }
                                else
                                {
                                    parent.tmpNearbyGeometryPoints[i].position += directions[d] * 0.05f;
                                }

                                prevDistance = currentDistance;
                            }
                            else
                            {
                                parent.tmpNearbyGeometryPoints[i].position -= directions[d] * 0.1f;
                                parent.tmpNearbyGeometryPoints[i].onGeometryEdge = true;
                                edgeFound = true;
                            }

                            //while (!parent.nextStep)
                            //{
                            //    yield return null;
                            //}

                            //parent.nextStep = false;
                        }
                    }

                    //while (!parent.nextStep)
                    //{
                    //    yield return null;
                    //}

                    //parent.nextStep = false;
                }

                if (!edgeFound)
                {
                    parent.tmpNearbyGeometryPoints[i].position = startPosition;
                    //Object.DestroyImmediate(temp.gameObject);
                }
                else
                {
                    //temp.transform.position = parent.tmpNearbyGeometryPoints[i].position;
                }

                for (int j = 0; j < parent.staticObjects.Count; j++)
                {
                    parent.staticObjects[j].layer = parent.layerMasks[j];
                }

                //while (!parent.nextStep)
                //{
                //    yield return null;
                //}

                //parent.nextStep = false;

                if (!parent.isInBackground)
                {
                    if (parent.UpdateProgress(parent.tmpNearbyGeometryPoints.Count, 1000))
                    {
                        yield return null;
                    }
                }
            } 

            if (parent.debugMode)
            {
                List<MLPPointData> exitArray = new List<MLPPointData>();
                
                Parallel.For(0, parent.tmpNearbyGeometryPoints.Count, i =>
                {
                    if (parent.tmpNearbyGeometryPoints[i].onGeometryEdge)
                    {
                        lock (exitArray)
                        {
                            exitArray.Add(parent.tmpNearbyGeometryPoints[i]);
                        }
                    }
                });

                parent.debugAcceptedPoints.Clear();
                parent.debugAcceptedPoints.AddRange(exitArray);
                exitArray.Clear();
            }

            parent.calculatingVolumeSubPass = false;
        }
    }
}
#endif
