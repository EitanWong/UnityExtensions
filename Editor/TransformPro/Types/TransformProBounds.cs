namespace TransformPro.Scripts
{
    using System;
    using UnityEngine;

    [Serializable]
    public class TransformProBounds
    {
        [SerializeField]
        private Transform transform;

        [SerializeField]
        private Bounds local;

        [SerializeField]
        private Bounds world;

        public TransformProBounds(Transform transform, Bounds worldBounds, Vector3 localSize)
            : this(transform, worldBounds.center, worldBounds.size, localSize)
        {
        }

        public TransformProBounds(Transform transform, Vector3 worldCenter, Vector3 worldSize, Vector3 localSize)
        {
            this.transform = transform;
            this.world = new Bounds(worldCenter, worldSize);
            this.local = this.WorldToLocal(this.world, localSize);
        }

        private TransformProBounds()
        {
        }

        public Bounds Local { get { return this.local; } }

        public Transform Transform { get { return this.transform; } }

        public Bounds World { get { return this.world; } }

        public void Encapsulate(Bounds worldBounds, Vector3 localSize)
        {
            this.world.Encapsulate(worldBounds);
            this.local.Encapsulate(this.WorldToLocal(worldBounds, localSize));
        }

        public void Encapsulate(TransformProBounds bounds)
        {
            if (this.transform != bounds.Transform)
            {
                throw new ArgumentException("[TransformPro] Cannot encapsulate local space bounds with different transforms.");
            }

            this.world.Encapsulate(bounds.World);
            this.local.Encapsulate(bounds.Local);
        }

        private Bounds WorldToLocal(Bounds worldBounds, Vector3 localSize)
        {
            if (this.transform == null)
            {
                return new Bounds();
            }

            Vector3 localCenter = this.transform.worldToLocalMatrix.MultiplyPoint3x4(worldBounds.center);
            return new Bounds(localCenter, localSize);
        }
    }
}
