namespace TransformPro.Scripts
{
    using System;
    using UnityEngine;

    [Serializable]
    public struct AngleAxis
    {
        private static readonly AngleAxis identity = new AngleAxis(0, Vector3.up);

        [SerializeField]
        private float angle;

        [SerializeField]
        private Vector3 axis;

        public AngleAxis(float angle, Vector3 axis)
        {
            this.angle = angle;
            this.axis = axis;
        }

        public Vector3 Axis { get { return this.axis; } }

        public float Angle { get { return this.angle; } }

        public static AngleAxis Identity { get { return AngleAxis.identity; } }
    }
}
