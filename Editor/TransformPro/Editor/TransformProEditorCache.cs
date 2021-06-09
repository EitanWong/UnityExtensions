namespace TransformPro.Scripts
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    ///     System for caching TransformPro instances within the editor.
    ///     At runtime you would manage storing references to the transform instances yourself, but this isn't possible in the
    ///     editor.
    /// </summary>
    public class TransformProEditorCache
    {
        /// <summary>
        ///     Holds the TransformPro editor cache.
        /// </summary>
        private Dictionary<Transform, TransformPro> cachedTransforms;

        /// <summary>
        ///     Holds a list of hashes for whether the transform is active.
        ///     Currently this is just a single bool value, as it only applies to the root game object.
        /// </summary>
        private Dictionary<Transform, bool> hashedActive;

        /// <summary>
        ///     Holds a list of hashes of the transform data.
        /// </summary>
        private Dictionary<Transform, int> hashedTransforms;

        /// <summary>
        ///     Holds a list of hashes of certain component data, such as renderers and colliders.
        /// </summary>
        private Dictionary<Transform, int> hashedComponents;

        /// <summary>
        ///     The currently selected objects.
        /// </summary>
        private IEnumerable<Object> selected;

        /// <summary>
        ///     The currently selected objects.
        /// </summary>
        private IEnumerable<Transform> selectedTransforms;

        public TransformProEditorCache()
        {
            this.cachedTransforms = new Dictionary<Transform, TransformPro>();
            this.hashedActive = new Dictionary<Transform, bool>();
            this.hashedTransforms = new Dictionary<Transform, int>();
            this.hashedComponents = new Dictionary<Transform, int>();
        }

        public TransformPro this[Transform transform]
        {
            get
            {
                TransformPro transformPro = null;

                if (this.cachedTransforms.ContainsKey(transform))
                {
                    transformPro = this.cachedTransforms[transform];
                }
                if (transformPro == null)
                {
                    transformPro = new TransformPro(transform);
                    this.cachedTransforms[transform] = transformPro;
                }
                return transformPro;
            }
        }

        public IEnumerable<TransformPro> Selected
        {
            get
            {
                if ((this.selectedTransforms == null) || (this.cachedTransforms == null))
                {
                    yield break;
                }
                foreach (Transform transform in this.selectedTransforms)
                {
                    yield return this[transform];
                }
            }
        }

        public int SelectedCount { get { return this.selectedTransforms == null ? 0 : this.selectedTransforms.Count(); } }

        public void Clear()
        {
            this.cachedTransforms.Clear();
            this.hashedActive.Clear();
            this.hashedComponents.Clear();
            this.hashedTransforms.Clear();
        }

        public void Select(IEnumerable<Object> objects)
        {
            if (objects == null)
            {
                this.selected = null;
                return;
            }

            objects = objects.ToList();
            if ((this.selected != null) && objects.SequenceEqual(this.selected))
            {
                this.Test();
                return;
            }

            this.selected = objects.ToList();
            if (this.selected == null)
            {
                this.selectedTransforms = null;
            }
            else
            {
                List<Transform> selectedTransforms = new List<Transform>();
                foreach (Object obj in this.selected)
                {
                    Transform transform = obj as Transform;
                    if (transform != null)
                    {
                        selectedTransforms.Add(transform);
                        continue;
                    }

                    GameObject gameObject = obj as GameObject;
                    if (gameObject != null)
                    {
                        selectedTransforms.Add(gameObject.transform);
                    }
                }
                this.selectedTransforms = selectedTransforms;
            }
            Selection.objects = objects.ToArray();
            this.Test();

            SceneView.RepaintAll();
        }

        public bool Test(TransformPro transform)
        {
            if ((transform == null) || (transform.Transform == null))
            {
                return false;
            }
            if (!this.cachedTransforms.ContainsKey(transform.Transform))
            {
                this.cachedTransforms[transform.Transform] = transform;
            }

            bool changed = this.CheckTransformChanged(transform);
            if (!changed)
            {
                return false;
            }

            transform.UpdateRendererBounds();
            transform.UpdateColliderBounds();
            transform.Transform.hasChanged = false;
            return true;
        }

        private bool CheckTransformChanged(TransformPro transform)
        {
            // If the transform hasChanged flag is set, we can skip everything else.
            // This isn't true very often, but is worth taking advantage of as it's so cheap.
            bool changed = transform.Transform.hasChanged;
            if (!changed)
            {
                // Has the active status of the object changed?
                // This is a fairly cheap initial test, and handles enabling and disabling objects without rehashing the entire component tree.
                bool hashActive = this.HashActive(transform.Transform);
                if (!this.hashedActive.ContainsKey(transform.Transform) || (this.hashedActive[transform.Transform] != hashActive))
                {
                    changed = true;
                    this.hashedActive[transform.Transform] = hashActive;
                }
                else
                {
                    // Have child transforms changed?
                    // Medium cost, hashes whether child objects are enabled, and their position rotation and scale data.
                    int hashTransform = this.HashTransform(transform.Transform);
                    if (!this.hashedTransforms.ContainsKey(transform.Transform) || (this.hashedTransforms[transform.Transform] != hashTransform))
                    {
                        changed = true;
                        this.hashedTransforms[transform.Transform] = hashTransform;
                    }
                    else
                    {
                        // Have any components changed?
                        // Highest cost final test - have any colliders or renderers changed in such a way that bounds are no longer valid?
                        int hashComponent = this.HashComponent(transform.Transform);
                        if (!this.hashedComponents.ContainsKey(transform.Transform) || (this.hashedComponents[transform.Transform] != hashComponent))
                        {
                            changed = true;
                            this.hashedComponents[transform.Transform] = hashComponent;
                        }
                    }
                }
            }
            return changed;
        }

        private bool HashActive(Transform transform)
        {
            return transform.gameObject.activeInHierarchy;
        }

        private int HashComponent(Transform transform)
        {
            int hash = 0;
            Component[] components = transform.GetComponentsInChildren<Component>();
            foreach (Component child in components)
            {
                if (child == null)
                {
                    continue;
                }

                hash = (hash * 31) + child.GetInstanceID();

                Collider collider = child as Collider;
                if (collider != null)
                {
                    hash = (hash * 31) + collider.bounds.center.GetHashCode();
                    hash = (hash * 31) + collider.bounds.size.GetHashCode();
                }

                Renderer renderer = child as Renderer;
                if (renderer != null)
                {
                    hash = (hash * 31) + (renderer.enabled ? 1 : 0);
                    hash = (hash * 31) + renderer.bounds.center.GetHashCode();
                    hash = (hash * 31) + renderer.bounds.size.GetHashCode();
                }
            }
            return hash;
        }

        private int HashTransform(Transform transform)
        {
            int hash = 0;
            foreach (Transform child in transform)
            {
                hash = (hash * 31) + (child.gameObject.activeInHierarchy ? 1 : 0);
                hash = (hash * 31) + child.transform.position.GetHashCode();
                hash = (hash * 31) + child.transform.rotation.GetHashCode();
                hash = (hash * 31) + child.transform.lossyScale.GetHashCode();
            }
            return hash;
        }

        private void Test()
        {
            foreach (TransformPro transformPro in this.Selected)
            {
                this.Test(transformPro);
            }
        }
    }
}
