namespace TransformPro.Scripts
{
    using UnityEngine;

    public partial class TransformPro
    {
        private TransformProBounds colliderBounds;
        private TransformProBounds rendererBounds;

        public TransformProBounds ColliderBounds { get { return this.colliderBounds; } }
        public TransformProBounds RendererBounds { get { return this.rendererBounds; } }

        public void UpdateColliderBounds()
        {
            this.colliderBounds = null;

            if ((this.Transform == null) || (this.Colliders == null))
            {
                return;
            }

            foreach (Collider collider in this.Colliders)
            {
                if ((collider == null) || !collider.enabled || !collider.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Vector3 childLocalSize = collider.GetLocalSize();
                if (childLocalSize == Vector3.zero)
                {
                    continue;
                }

                // TODO: Some colliders don't scale in a linear fashion, namely spheres and wheels. Handle these here.
                Vector3 childWorldSize = collider.transform.localToWorldMatrix.MultiplyVector(childLocalSize);
                Vector3 thisLocalSize = this.Transform.worldToLocalMatrix.MultiplyVector(childWorldSize);

                // Wheel Colliders do not implement the bounds property correctly.
                Bounds worldBounds = collider.bounds;
                if (collider is WheelCollider)
                {
                    worldBounds.size = childWorldSize;
                }

                if (this.colliderBounds == null)
                {
                    this.colliderBounds = new TransformProBounds(this.Transform, worldBounds, thisLocalSize);
                }
                else
                {
                    this.colliderBounds.Encapsulate(worldBounds, thisLocalSize);
                }
            }
        }

        public void UpdateRendererBounds()
        {
            this.rendererBounds = null;

            if ((this.Transform == null) || (this.Renderers == null))
            {
                return;
            }

            foreach (Renderer renderer in this.Renderers)
            {
                if ((renderer == null) || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Vector3 childLocalSize = renderer.GetLocalSize();
                if (childLocalSize == Vector3.zero)
                {
                    continue;
                }

                Vector3 childWorldSize = renderer.transform.localToWorldMatrix.MultiplyVector(childLocalSize);
                Vector3 thisLocalSize = this.Transform.worldToLocalMatrix.MultiplyVector(childWorldSize);

                if (this.rendererBounds == null)
                {
                    this.rendererBounds = new TransformProBounds(this.Transform, renderer.bounds, thisLocalSize);
                }
                else
                {
                    this.rendererBounds.Encapsulate(renderer.bounds, thisLocalSize);
                }
            }
        }
    }
}
