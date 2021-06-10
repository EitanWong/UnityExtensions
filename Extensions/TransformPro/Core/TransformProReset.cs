namespace TransformPro.Scripts
{
    using UnityEngine;

    public partial class TransformPro
    {
        /// <summary>
        ///     Resets the position, rotation and scale to default values.
        ///     If <see cref="Space" /> is set to <see cref="TransformProSpace.World" />, this will return the object
        ///     to the origin.
        ///     If <see cref="Space" /> is set to <see cref="TransformProSpace.Local" />, this will return the object
        ///     to the parent. If the object has no parent it will return to the origin.
        /// </summary>
        public void Reset()
        {
            this.ResetPosition();
            this.ResetRotation();
            this.ResetScale();
        }

        /// <summary>
        ///     Resets the position to default values.
        ///     If <see cref="Space" /> is set to <see cref="TransformProSpace.World" />, this will return the object
        ///     to the origin position.
        ///     If <see cref="Space" /> is set to <see cref="TransformProSpace.Local" />, this will return the object
        ///     to the parent position. If the object has no parent it will return to the origin position.
        /// </summary>
        public void ResetPosition()
        {
            this.Position = Vector3.zero;
        }

        /// <summary>
        ///     Resets the rotation to default values.
        ///     If <see cref="Space" /> is set to <see cref="TransformProSpace.World" />, this will return the object
        ///     to world rotation, i.e. world Z is forward.
        ///     If <see cref="Space" /> is set to <see cref="TransformProSpace.Local" />, this will return the object
        ///     to the parents rotation. If the object has no parent it will return to world rotation.
        /// </summary>
        public void ResetRotation()
        {
            this.RotationEuler = Vector3.zero;
        }

        /// <summary>
        ///     Resets the scale to default values. The current scale system is always relative to the parent.
        /// </summary>
        public void ResetScale()
        {
            this.Scale = Vector3.one;
        }
    }
}
