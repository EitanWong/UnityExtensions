namespace TransformPro.Scripts
{
    using UnityEngine;

    public class TransformProClipboard
    {
        private Vector3 position = Vector3.zero;
        private Quaternion rotation = Quaternion.identity;
        private Vector3 scale = Vector3.one;

        public Vector3 Position { get { return this.position; } set { this.position = value; } }

        public Quaternion Rotation { get { return this.rotation; } set { this.rotation = value; } }

        public Vector3 Scale { get { return this.scale; } set { this.scale = value; } }

        /// <summary>
        ///     Copy the current display values.
        /// </summary>
        public void Copy(TransformPro transform)
        {
            this.position = transform.Position;
            this.rotation = transform.Rotation;
            this.scale = transform.Scale;
        }

        /// <summary>
        ///     Copy the current display values.
        /// </summary>
        public void Copy(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        /// <summary>
        ///     Copy the current position display values.
        /// </summary>
        public void CopyPosition(TransformPro transform)
        {
            this.position = transform.Position;
        }

        public void CopyPosition(Vector3 position)
        {
            this.position = position;
        }

        /// <summary>
        ///     Copy the current rotation display values.
        /// </summary>
        public void CopyRotation(TransformPro transform)
        {
            this.rotation = transform.Rotation;
        }

        public void CopyRotation(Quaternion quaternion)
        {
            this.rotation = quaternion;
        }

        /// <summary>
        ///     Copy the current scale display values.
        /// </summary>
        public void CopyScale(TransformPro transform)
        {
            this.scale = transform.Scale;
        }

        public void CopyScale(Vector3 scale)
        {
            this.scale = scale;
        }

        /// <summary>
        ///     Pastes the copied values to the curent <see cref="Transform" />.
        /// </summary>
        public void Paste(TransformPro transform)
        {
            transform.Position = this.Position;
            transform.Rotation = this.Rotation;
            transform.Scale = this.Scale;
        }

        /// <summary>
        ///     Pastes the copied position to the curent <see cref="Transform" />.
        /// </summary>
        public void PastePosition(TransformPro transform)
        {
            transform.Position = this.Position;
        }

        /// <summary>
        ///     Pastes the copied rotation to the curent <see cref="Transform" />.
        /// </summary>
        public void PasteRotation(TransformPro transform)
        {
            transform.Rotation = this.Rotation;
        }

        /// <summary>
        ///     Pastes the copied scale to the curent <see cref="Transform" />.
        /// </summary>
        public void PasteScale(TransformPro transform)
        {
            transform.Scale = this.Scale;
        }

        public void PointAt(Transform transform)
        {
            transform.LookAt(this.Position);
        }

        public void Reset()
        {
            this.Position = Vector3.zero;
            this.Rotation = Quaternion.identity;
            this.Scale = Vector3.one;
        }
    }
}
