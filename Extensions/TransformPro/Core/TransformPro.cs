namespace TransformPro.Scripts
{
#if UNITY_EDITOR
    using System;
    using UnityEngine;
    using Object = UnityEngine.Object;
    using Random = System.Random;

#else
    using System;
    using UnityEngine;
    using Object = UnityEngine.Object;
    using Random = System.Random;
#endif

    /// <summary>
    ///     Provides the runtime features for the system. All main functionality can be found here.
    ///     More information can be found at http://transformpro.untitledgam.es
    /// </summary>
    public partial class TransformPro
    {
        /// <summary>
        ///     The current version number, used to display in the preferences.
        /// </summary>
        private static readonly string version = "v1.3.2";

        private static bool calculateBounds = true;

        /// <summary>
        ///     The random number provider for the rotation randomisation.
        /// </summary>
        private static Random random;

        private Transform transform;

        public TransformPro(Transform transform)
        {
            this.transform = transform;
            this.ReadHints();
        }

        /// <summary>
        ///     Private constructor for TransformPro, prevents an instance without a Transform being created.
        /// </summary>
        private TransformPro()
        {
        }

        public static bool CalculateBounds { get { return TransformPro.calculateBounds; } set { TransformPro.calculateBounds = value; } }

        /// <summary>
        ///     A <see cref="System.Random" /> instance used to set random data in the editor.
        ///     <see cref="UnityEngine.Random" /> is not used to avoid polluting and procedural generation that may be being used
        ///     elsewhere.
        /// </summary>
        public static Random Random { get { return TransformPro.random ?? (TransformPro.random = new Random(DateTime.Now.GetHashCode())); } }

        /// <summary>
        ///     Gets a string containing the current version of <see cref="TransformPro" />.
        /// </summary>
        public static string Version { get { return TransformPro.version; } }

        /// <summary>
        ///     The current <see cref="Transform" /> is currently able to have its position changed.
        /// </summary>
        public bool CanChangePosition { get { return true; } }

        /// <summary>
        ///     The current <see cref="Transform" /> is currently able to have its rotation changed.
        /// </summary>
        public bool CanChangeRotation { get { return true; } }

        /// <summary>
        ///     The current <see cref="Transform" /> is currently able to have its scale changed.
        /// </summary>
        public bool CanChangeScale { get { return TransformPro.Space != TransformProSpace.World; } }

        /// <summary>
        ///     Returns true if the currently selected <see cref="Transform" /> has any child transforms. If no
        ///     <see cref="Transform" /> is selected false will be returned.
        /// </summary>
        public bool HasChildren { get { return this.Transform.childCount > 0; } }

        public string Name { get { return this.transform.name; } }

        public Transform Transform { get { return this.transform; } }

        /// <summary>
        ///     Clones the GameObject for the currently selected Transform, retaining the same name and parent data.
        ///     The new transform will be automatically selected allowing for fast simple scene creation.
        /// </summary>
        /// <returns>The newly created transform.</returns>
        public Transform Clone()
        {
            GameObject gameObjectOld = this.Transform.gameObject;
            Transform transformOld = gameObjectOld.transform;

            GameObject gameObjectNew = Object.Instantiate(gameObjectOld);
            gameObjectNew.name = gameObjectOld.name; // Get rid of the (Clone)(Clone)(Clone)(Clone) madness

            Transform transformNew = gameObjectNew.transform;
            transformNew.SetParent(transformOld.parent);
            transformNew.localPosition = transformOld.localPosition;
            transformNew.localRotation = transformOld.localRotation;
            transformNew.localScale = transformOld.localScale;

            return transformNew;
        }

        public void LookAt(Vector3 position)
        {
            this.Transform.LookAt(position);
        }

        public void LookAt(TransformProClipboard clipboard)
        {
            this.Transform.LookAt(clipboard.Position);
        }
    }
}
