namespace TransformPro.Scripts
{
    using UnityEngine;

    public partial class TransformPro
    {
        //private static bool autoSnap = true;
        private static Vector3 snapPositionGrid = Vector3.one;

        private static Vector3 snapPositionOrigin = Vector3.zero;
        private static Vector3 snapRotationGrid = new Vector3(90, 90, 90);
        private static Vector3 snapRotationOrigin = Vector3.zero;

        /// <summary>
        ///     Gets or sets the size of the positioning grid.
        ///     Use 0 on any axis to disable snapping.
        ///     Please note, while in the editor this value will be managed.
        /// </summary>
        public static Vector3 SnapPositionGrid { get { return TransformPro.snapPositionGrid; } set { TransformPro.snapPositionGrid = value; } }

        /// <summary>
        ///     Gets or sets the origin of the position snapping grid.
        ///     Please note, while in the editor this value will be managed.
        /// </summary>
        public static Vector3 SnapPositionOrigin { get { return TransformPro.snapPositionOrigin; } set { TransformPro.snapPositionOrigin = value; } }

        /// <summary>
        ///     Gets or sets the size of the rotation grid.
        ///     Use 0 on any axis to disable snapping.
        ///     Please note, while in the editor this value will be managed.
        /// </summary>
        public static Vector3 SnapRotationGrid { get { return TransformPro.snapRotationGrid; } set { TransformPro.snapRotationGrid = value; } }

        /// <summary>
        ///     Gets or sets the origin of the rotation snapping grid.
        ///     If the rotations are snapped to 90 degrees snapped values will be 0, 90, 180 and 270. If the origin is set to 30,
        ///     snapped values will be 30, 120, 210 and 300.
        ///     Please note, while in the editor this value will be managed.
        /// </summary>
        public static Vector3 SnapRotationOrigin { get { return TransformPro.snapRotationOrigin; } set { TransformPro.snapRotationOrigin = value; } }

        /// <summary>
        ///     Get the nearest grid position for a given <see cref="Vector3" /> position.
        ///     This method will use the current grid settings but does not affect the current <see cref="Transform" />.
        ///     <example>
        ///         Vector3 snapped = TransformPro.SnapPosition(originalPosition);
        ///     </example>
        /// </summary>
        /// <param name="position">The position to snap to the nearest position, using the current grid settings.</param>
        /// <returns>The snapped grid position, using the current grid settings.</returns>
        public static Vector3 SnapPosition(Vector3 position)
        {
            position -= TransformPro.snapPositionOrigin;
            position.x = TransformPro.SnapPositionGrid.x > 0 ? Mathf.Round(position.x / TransformPro.SnapPositionGrid.x) * TransformPro.SnapPositionGrid.x : position.x;
            position.y = TransformPro.SnapPositionGrid.y > 0 ? Mathf.Round(position.y / TransformPro.SnapPositionGrid.y) * TransformPro.SnapPositionGrid.y : position.y;
            position.z = TransformPro.SnapPositionGrid.z > 0 ? Mathf.Round(position.z / TransformPro.SnapPositionGrid.z) * TransformPro.SnapPositionGrid.z : position.z;
            position += TransformPro.snapPositionOrigin;
            return position;
        }

        /// <summary>
        ///     Get a <see cref="Quaternion" /> representing the snapped rotation for a <see cref="Quaternion" /> angle input.
        ///     This method will use the current grid settings but does not affect the current <see cref="Transform" />.
        ///     <example>
        ///         Quaternion snapped = TransformPro.SnapRotation(originalRotation);
        ///     </example>
        /// </summary>
        /// <param name="rotation">The <see cref="Quaternion" /> to get the snapped rotation for, using the current grid settings.</param>
        /// <returns>The snapped grid <see cref="Quaternion" /> using the current grid settings.</returns>
        public static Quaternion SnapRotation(Quaternion rotation)
        {
            return Quaternion.Euler(TransformPro.SnapRotationEuler(rotation.eulerAngles));
        }

        /// <summary>
        ///     Get a <see cref="Quaternion" /> representing the snapped rotation for a <see cref="Vector3" /> euler angle input.
        ///     This method will use the current grid settings but does not affect the current <see cref="Transform" />.
        ///     <example>
        ///         Quaternion snapped = TransformPro.SnapRotation(originalRotationEuler);
        ///     </example>
        /// </summary>
        /// <param name="rotation">
        ///     The <see cref="Vector3" /> euler angles to get the snapped rotation for, using the current grid
        ///     settings.
        /// </param>
        /// <returns>The snapped grid <see cref="Quaternion" /> using the current grid settings.</returns>
        public static Quaternion SnapRotation(Vector3 rotation)
        {
            return Quaternion.Euler(TransformPro.SnapRotationEuler(rotation));
        }

        /// <summary>
        ///     Get a <see cref="Vector3" /> euler angle for the snapped rotation for a <see cref="Quaternion" /> angle input.
        ///     This method will use the current grid settings but does not affect the current <see cref="Transform" />.
        ///     <example>
        ///         Vector3 snappedEuler = TransformPro.SnapRotation(originalRotation);
        ///     </example>
        /// </summary>
        /// <param name="rotation">The <see cref="Quaternion" /> to get the snapped rotation for, using the current grid settings.</param>
        /// <returns>The snapped grid <see cref="Vector3" /> euler angle, using the current grid settings.</returns>
        public static Vector3 SnapRotationEuler(Quaternion rotation)
        {
            return TransformPro.SnapRotationEuler(rotation.eulerAngles);
        }

        /// <summary>
        ///     Get a <see cref="Vector3" /> euler angle for the snapped rotation for a <see cref="Vector3" /> euler angle input.
        ///     This method will use the current grid settings but does not affect the current <see cref="Transform" />.
        ///     <example>
        ///         Vector3 snappedEuler = TransformPro.SnapRotation(originalRotationEuler);
        ///     </example>
        /// </summary>
        /// <param name="rotation">
        ///     The <see cref="Vector3" /> euler angles to get the snapped rotation for, using the current grid
        ///     settings.
        /// </param>
        /// <returns>The snapped grid <see cref="Vector3" /> euler angle, using the current grid settings.</returns>
        public static Vector3 SnapRotationEuler(Vector3 rotation)
        {
            rotation -= TransformPro.snapRotationOrigin;
            rotation.x = TransformPro.SnapRotationGrid.x > 0 ? Mathf.Round(rotation.x / TransformPro.SnapRotationGrid.x) * TransformPro.SnapRotationGrid.x : rotation.x;
            rotation.y = TransformPro.SnapRotationGrid.y > 0 ? Mathf.Round(rotation.y / TransformPro.SnapRotationGrid.y) * TransformPro.SnapRotationGrid.y : rotation.y;
            rotation.z = TransformPro.SnapRotationGrid.z > 0 ? Mathf.Round(rotation.z / TransformPro.SnapRotationGrid.z) * TransformPro.SnapRotationGrid.z : rotation.z;
            rotation += TransformPro.snapRotationOrigin;
            return rotation;
        }

        /// <summary>
        ///     Snaps the current <see cref="Transform" /> in both position and rotation.
        /// </summary>
        public void Snap()
        {
            this.SnapPosition();
            this.SnapRotation();
        }

        public void SnapPosition()
        {
            this.Position = TransformPro.SnapPosition(this.Position);
        }

        public void SnapRotation()
        {
            this.RotationEuler = TransformPro.SnapRotationEuler(this.RotationEuler);
        }
    }
}
