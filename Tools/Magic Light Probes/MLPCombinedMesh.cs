#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLightProbes
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class MLPCombinedMesh : MonoBehaviour
    {
        public bool includeInactive;
        public Material combinedMeshMaterial;
        public void Combine()
        {
            MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(includeInactive);
            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(includeInactive);
            List<Material> materials = new List<Material>();
            List<CombineInstance> finalCombiners = new List<CombineInstance>();

            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            foreach (MeshRenderer renderer in renderers)
            {
                if (renderer.transform == transform)
                {
                    continue;
                }

                Material[] localMats = renderer.sharedMaterials;

                if (localMats.Length > 1)
                {
                    foreach (Material localMat in localMats)
                    {
                        if (!materials.Contains(localMat))
                        {
                            materials.Add(localMat);
                        }
                    }
                }
                else
                {
                    materials.Add(localMats[0]);

                    CombineInstance ci = new CombineInstance();
                    ci.mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
                    ci.subMeshIndex = 0;
                    ci.transform = renderer.transform.localToWorldMatrix;

                    finalCombiners.Add(ci);
                }
            }

            List<Mesh> subMeshes = new List<Mesh>();

            foreach (Material material in materials)
            {
                List<CombineInstance> combiners = new List<CombineInstance>();

                foreach (MeshFilter meshFilter in meshFilters)
                {
                    MeshRenderer renderer = meshFilter.GetComponent<MeshRenderer>();

                    if (renderer == null)
                    {
                        continue;
                    }

                    Material[] localMaterials = renderer.sharedMaterials;

                    for (int i = 0; i < localMaterials.Length; i++)
                    {
                        if (localMaterials[i] != material)
                        {
                            continue;
                        }

                        CombineInstance ci = new CombineInstance();

                        ci.mesh = meshFilter.sharedMesh;
                        ci.subMeshIndex = i;
                        ci.transform = Matrix4x4.identity;

                        combiners.Add(ci);
                    }
                }

                Mesh mesh = new Mesh();
                mesh.CombineMeshes(combiners.ToArray(), true);
                subMeshes.Add(mesh);
            }            

            foreach (Mesh mesh in subMeshes)
            {
                CombineInstance ci = new CombineInstance();
                ci.mesh = mesh;
                ci.subMeshIndex = 0;
                ci.transform = Matrix4x4.identity;

                finalCombiners.Add(ci);
            }

            Mesh finalMesh = new Mesh();
            finalMesh.CombineMeshes(finalCombiners.ToArray(), true);

            transform.GetComponent<MeshFilter>().sharedMesh = finalMesh;


            //for (int i = 0; i < meshFilters.Length; i++)
            //{
            //    if (meshFilters[i] == transform.GetComponent<MeshFilter>())
            //    {
            //        continue;
            //    }

            //    combine[i].mesh = meshFilters[i].sharedMesh;
            //    combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            //    meshFilters[i].gameObject.SetActive(false);
            //}

            //transform.GetComponent<MeshFilter>().sharedMesh = new Mesh();
            //transform.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);
            //transform.GetComponent<MeshRenderer>().sharedMaterial = combinedMeshMaterial;
            //transform.gameObject.AddComponent<MeshCollider>();
        }
    }
}
#endif
