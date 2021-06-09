namespace TransformPro.Scripts
{
    using UnityEngine;

    public static class TransformProExtensionsCollider
    {
        public static Vector3 GetLocalSize(this Collider collider)
        {
            SphereCollider sphereCollider = collider as SphereCollider;
            if (sphereCollider != null)
            {
                return sphereCollider.GetLocalSize();
            }

            BoxCollider boxCollider = collider as BoxCollider;
            if (boxCollider != null)
            {
                return boxCollider.GetLocalSize();
            }

            CapsuleCollider capsuleCollider = collider as CapsuleCollider;
            if (capsuleCollider != null)
            {
                return capsuleCollider.GetLocalSize();
            }

            MeshCollider meshCollider = collider as MeshCollider;
            if (meshCollider != null)
            {
                return meshCollider.GetLocalSize();
            }

            WheelCollider wheelCollider = collider as WheelCollider;
            if (wheelCollider != null)
            {
                return wheelCollider.GetLocalSize();
            }

            return Vector3.zero;
        }

        public static Vector3 GetLocalSize(this SphereCollider sphereCollider)
        {
            if (sphereCollider == null)
            {
                return Vector3.zero;
            }
            float diameter = sphereCollider.radius * 2;
            return new Vector3(diameter, diameter, diameter);
        }

        public static Vector3 GetLocalSize(this BoxCollider boxCollider)
        {
            if (boxCollider == null)
            {
                return Vector3.zero;
            }
            return boxCollider.size;
        }

        public static Vector3 GetLocalSize(this CapsuleCollider capsuleCollider)
        {
            if (capsuleCollider == null)
            {
                return Vector3.zero;
            }
            float width = capsuleCollider.radius * 2;
            float height = capsuleCollider.height;
            Vector3 size = Vector3.zero;
            switch (capsuleCollider.direction)
            {
                case 0:
                    size = new Vector3(height, width, width);
                    break;
                case 1:
                    size = new Vector3(width, height, width);
                    break;
                case 2:
                    size = new Vector3(width, width, height);
                    break;
            }
            return size;
        }

        public static Vector3 GetLocalSize(this MeshCollider meshCollider)
        {
            if ((meshCollider == null) || (meshCollider.sharedMesh == null))
            {
                return Vector3.zero;
            }
            return meshCollider.sharedMesh.bounds.size;
        }

        public static Vector3 GetLocalSize(this WheelCollider wheelCollider)
        {
            if (wheelCollider == null)
            {
                return Vector3.zero;
            }
            float diameter = wheelCollider.radius * 2;
            float thickness = 0.1f;
            Vector3 size = new Vector3(thickness, diameter, diameter);
            return size;
        }
    }
}
