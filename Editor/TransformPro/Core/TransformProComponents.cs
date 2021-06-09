namespace TransformPro.Scripts
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    ///     Holds the components that TransformPro references
    /// </summary>
    public partial class TransformPro
    {
        private Collider[] colliders;
        private bool collidersDirty;
        private Renderer[] renderers;
        private bool renderersDirty;

        public IEnumerable<Collider> Colliders
        {
            get
            {
                if (!this.collidersDirty && (this.colliders != null))
                {
                    return this.colliders;
                }

                this.collidersDirty = false;
                return this.colliders = this.Transform.GetComponentsInChildren<Collider>();
            }
        }

        public int CollidersCount { get { return this.Colliders.Count(); } }

        public bool HasColliders { get { return this.CollidersCount > 0; } }

        public bool HasRenderers { get { return this.RenderersCount > 0; } }

        public IEnumerable<Renderer> Renderers
        {
            get
            {
                if (!this.renderersDirty && (this.renderers != null))
                {
                    return this.renderers;
                }

                this.renderersDirty = false;
                return this.renderers = this.Transform.GetComponentsInChildren<Renderer>();
            }
        }

        public int RenderersCount { get { return this.Renderers.Count(); } }

        public void SetComponentsDirty()
        {
            this.collidersDirty = true;
            this.renderersDirty = true;
        }
    }
}
