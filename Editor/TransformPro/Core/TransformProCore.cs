namespace TransformPro.Scripts
{
    using System;
    using UnityEngine;

    public partial class TransformPro
    {
        /// <summary>
        ///     Gets or sets the <see cref="Vector3" /> Position of the current object, in the current
        ///     <see cref="TransformProSpace" />.
        ///     If you attempt to update a position when <see cref="CanChangePosition" /> is false, the request will be ignored.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                switch (TransformPro.Space)
                {
                    default:
                        throw new Exception(string.Format("[<color=red>TransformPro</color>] Space mode {0} not handled!", TransformPro.space));
                    case TransformProSpace.Local:
                        return this.PositionLocal;
                    case TransformProSpace.World:
                        return this.PositionWorld;
                }
            }
            set
            {
                switch (TransformPro.Space)
                {
                    default:
                        throw new Exception(string.Format("[<color=red>TransformPro</color>] Space mode {0} not handled!", TransformPro.space));
                    case TransformProSpace.Local:
                        this.PositionLocal = value;
                        break;
                    case TransformProSpace.World:
                        this.PositionWorld = value;
                        break;
                }
            }
        }

        public Vector3 PositionLocal
        {
            get { return this.Transform.localPosition; }
            set
            {
                if (!this.ShouldChangePositionLocal(value))
                {
                    return;
                }
                this.Transform.localPosition = value;
            }
        }

        public Vector3 PositionWorld
        {
            get { return this.Transform.position; }
            set
            {
                if (!this.ShouldChangePositionWorld(value))
                {
                    return;
                }
                this.Transform.position = value;
            }
        }

        public float PositionX
        {
            get { return this.Position.x; }
            set
            {
                Vector3 position = this.Position;
                position.x = value;
                this.Position = position;
            }
        }

        public float PositionY
        {
            get { return this.Position.y; }
            set
            {
                Vector3 position = this.Position;
                position.y = value;
                this.Position = position;
            }
        }

        public float PositionZ
        {
            get { return this.Position.z; }
            set
            {
                Vector3 position = this.Position;
                position.z = value;
                this.Position = position;
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="Quaternion" /> Rotation of the current object, in the current
        ///     <see cref="TransformProSpace" />.
        ///     If you attempt to update a rotation when <see cref="CanChangeRotation" /> is false, the request will be ignored.
        ///     Setting this property will update the bounds as required.
        /// </summary>
        public Quaternion Rotation
        {
            get
            {
                switch (TransformPro.space)
                {
                    default:
                        throw new Exception(string.Format("[<color=red>TransformPro</color>] Space mode {0} not handled!", TransformPro.space));
                    case TransformProSpace.Local:
                        return this.RotationLocal;
                    case TransformProSpace.World:
                        return this.RotationWorld;
                }
            }
            set
            {
                switch (TransformPro.space)
                {
                    default:
                        throw new Exception(string.Format("[<color=red>TransformPro</color>] Space mode {0} not handled!", TransformPro.space));
                    case TransformProSpace.Local:
                        this.RotationLocal = value;
                        break;
                    case TransformProSpace.World:
                        this.RotationWorld = value;
                        break;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="Vector3" /> Euler Rotation of the current object, in the current
        ///     <see cref="TransformProSpace" />.
        ///     If you attempt to update a rotation when <see cref="CanChangeRotation" /> is false, the request will be ignored.
        ///     Setting this property will update the bounds as required.
        /// </summary>
        public Vector3 RotationEuler
        {
            get
            {
                switch (TransformPro.space)
                {
                    default:
                        throw new Exception(string.Format("[<color=red>TransformPro</color>] Space mode {0} not handled!", TransformPro.space));
                    case TransformProSpace.Local:
                        return this.RotationEulerLocal;
                    case TransformProSpace.World:
                        return this.RotationEulerWorld;
                }
            }
            set
            {
                switch (TransformPro.space)
                {
                    default:
                        throw new Exception(string.Format("[<color=red>TransformPro</color>] Space mode {0} not handled!", TransformPro.space));
                    case TransformProSpace.Local:
                        this.RotationEulerLocal = value;
                        break;
                    case TransformProSpace.World:
                        this.RotationEulerWorld = value;
                        break;
                }
            }
        }

        public Vector3 RotationEulerLocal
        {
            //get { return this.Transform.localEulerAngles; }
            get { return this.HintEulerLocal; }
            set
            {
                if (!this.ShouldChangeRotationEulerLocal(value))
                {
                    return;
                }
                //this.RotationEulerLocal = value;
                this.HintEulerLocal = value;
            }
        }

        public Vector3 RotationEulerWorld
        {
            //get { return this.Transform.eulerAngles; }
            get { return this.HintEulerWorld; }
            set
            {
                if (!this.ShouldChangeRotationEulerWorld(value))
                {
                    return;
                }
                //this.RotationEulerWorld = value;
                this.HintEulerWorld = value;
            }
        }

        public float RotationEulerX
        {
            get { return this.RotationEuler.x; }
            set
            {
                Vector3 rotation = this.RotationEuler;
                rotation.x = value;
                this.RotationEuler = rotation;
            }
        }

        public float RotationEulerY
        {
            get { return this.RotationEuler.y; }
            set
            {
                Vector3 rotation = this.RotationEuler;
                rotation.y = value;
                this.RotationEuler = rotation;
            }
        }

        public float RotationEulerZ
        {
            get { return this.RotationEuler.z; }
            set
            {
                Vector3 rotation = this.RotationEuler;
                rotation.z = value;
                this.RotationEuler = rotation;
            }
        }

        public Quaternion RotationLocal
        {
            get { return this.Transform.localRotation; }
            set
            {
                if (!this.ShouldChangeRotationLocal(value))
                {
                    return;
                }
                this.Transform.localRotation = value;
            }
        }

        public Quaternion RotationWorld
        {
            get { return this.Transform.rotation; }
            set
            {
                if (!this.ShouldChangeRotationWorld(value))
                {
                    return;
                }
                this.Transform.rotation = value;
            }
        }

        public Vector3 Scale
        {
            get
            {
                switch (TransformPro.Space)
                {
                    default:
                        throw new Exception(string.Format("[<color=red>TransformPro</color>] Space mode {0} not handled!", TransformPro.space));
                    case TransformProSpace.Local:
                        return this.ScaleLocal;
                    case TransformProSpace.World:
                        return this.ScaleWorld;
                }
            }
            set
            {
                switch (TransformPro.Space)
                {
                    default:
                        throw new Exception(string.Format("[<color=red>TransformPro</color>] Space mode {0} not handled!", TransformPro.space));
                    case TransformProSpace.Local:
                        this.ScaleLocal = value;
                        break;
                    case TransformProSpace.World:
                        this.ScaleWorld = value;
                        break;
                }
            }
        }

        public Vector3 ScaleLocal
        {
            get { return this.Transform.localScale; }
            set
            {
                if (!this.ShouldChangeScaleLocal(value))
                {
                    return;
                }
                this.Transform.localScale = value;
            }
        }

        public Vector3 ScaleWorld
        {
            get { return this.Transform.lossyScale; }
            set
            {
                if (!this.ShouldChangeScaleWorld(value))
                {
                    return;
                }
                throw new Exception("Cannot yet invert lossy scale.");
            }
        }

        public float ScaleX
        {
            get { return this.Scale.x; }
            set
            {
                Vector3 scale = this.Scale;
                scale.x = value;
                this.Scale = scale;
            }
        }

        public float ScaleY
        {
            get { return this.Scale.y; }
            set
            {
                Vector3 scale = this.Scale;
                scale.y = value;
                this.Scale = scale;
            }
        }

        public float ScaleZ
        {
            get { return this.Scale.z; }
            set
            {
                Vector3 scale = this.Scale;
                scale.z = value;
                this.Scale = scale;
            }
        }

        private bool ShouldChangePosition(Vector3 position)
        {
            return this.CanChangePosition && !this.Position.ApproximatelyEquals(position);
        }

        private bool ShouldChangePositionLocal(Vector3 position)
        {
            return this.CanChangePosition && !this.PositionLocal.ApproximatelyEquals(position);
        }

        private bool ShouldChangePositionWorld(Vector3 position)
        {
            return this.CanChangePosition && !this.PositionWorld.ApproximatelyEquals(position);
        }

        private bool ShouldChangeRotation(Quaternion rotation)
        {
            return this.CanChangeRotation && !this.Rotation.ApproximatelyEquals(rotation);
        }

        private bool ShouldChangeRotationEuler(Vector3 rotation)
        {
            return this.CanChangeRotation && !this.RotationEuler.ApproximatelyEquals(rotation);
        }

        private bool ShouldChangeRotationEulerLocal(Vector3 rotation)
        {
            return this.CanChangeRotation && !this.RotationEulerLocal.ApproximatelyEquals(rotation);
        }

        private bool ShouldChangeRotationEulerWorld(Vector3 rotation)
        {
            return this.CanChangeRotation && !this.RotationEulerWorld.ApproximatelyEquals(rotation);
        }

        private bool ShouldChangeRotationLocal(Quaternion rotation)
        {
            return this.CanChangeRotation && !this.RotationLocal.ApproximatelyEquals(rotation);
        }

        private bool ShouldChangeRotationWorld(Quaternion rotation)
        {
            return this.CanChangeRotation && !this.RotationWorld.ApproximatelyEquals(rotation);
        }

        private bool ShouldChangeScale(Vector3 position)
        {
            return this.CanChangeScale && !this.Scale.ApproximatelyEquals(position);
        }

        private bool ShouldChangeScaleLocal(Vector3 position)
        {
            return this.CanChangeScale && !this.ScaleLocal.ApproximatelyEquals(position);
        }

        private bool ShouldChangeScaleWorld(Vector3 position)
        {
            return this.CanChangeScale && !this.ScaleWorld.ApproximatelyEquals(position);
        }
    }
}
