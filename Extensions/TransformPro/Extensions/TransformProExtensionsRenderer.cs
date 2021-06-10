namespace TransformPro.Scripts
{
    using UnityEngine;

    public static class TransformProExtensionsRenderer
    {
        public static Vector3 GetLocalSize(this Renderer renderer)
        {
            MeshRenderer meshRenderer = renderer as MeshRenderer;
            if (meshRenderer != null)
            {
                return meshRenderer.GetLocalSize();
            }

            SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
            if (skinnedMeshRenderer != null)
            {
                return skinnedMeshRenderer.GetLocalSize();
            }

            return Vector3.zero;
        }

        public static Vector3 GetLocalSize(this MeshRenderer meshRenderer)
        {
            if (meshRenderer == null)
            {
                return Vector3.zero;
            }
            MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
            if ((meshFilter == null) || (meshFilter.sharedMesh == null))
            {
                return Vector3.zero;
            }
            return meshFilter.sharedMesh.bounds.size;
        }

        public static Vector3 GetLocalSize(this SkinnedMeshRenderer skinnedMeshRenderer)
        {
            if ((skinnedMeshRenderer == null) || (skinnedMeshRenderer.sharedMesh == null))
            {
                return Vector3.zero;
            }
            return skinnedMeshRenderer.sharedMesh.bounds.size;
        }
    }
}
