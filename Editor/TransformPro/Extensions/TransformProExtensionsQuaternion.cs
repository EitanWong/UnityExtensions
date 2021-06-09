namespace TransformPro.Scripts
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public static class TransformProExtensionsQuaternion
    {
        /// <summary>
        ///     Helper function for comparing two <see cref="Quaternion" /> values. Used to help reduce axis drift.
        /// </summary>
        /// <param name="q1">The left hand <see cref="Quaternion" />.</param>
        /// <param name="q2">The right hand <see cref="Quaternion" />.</param>
        /// <param name="matchAlternateForm">
        ///     An optional paramter which enables the matching of the rotations alternate form. This
        ///     is the negative version of the Quaternion, which appears in the same orinetation, but has it's rotational system
        ///     flipped.
        /// </param>
        /// <returns>A <see cref="bool" /> indicating if the two values are approximately the same.</returns>
        public static bool ApproximatelyEquals(this Quaternion q1, Quaternion q2, bool matchAlternateForm = false)
        {
            if (matchAlternateForm)
            {
                return !(Quaternion.Dot(q1, q2) < 0.0f);
            }
            return Mathf.Approximately(q1.x, q2.x) && Mathf.Approximately(q1.y, q2.y) && Mathf.Approximately(q1.z, q2.z) && Mathf.Approximately(q1.w, q2.w);
        }

        public static Quaternion Average(this IEnumerable<Quaternion> source)
        {
            return source.Select(x => x.ToAngleAxis()).Average().ToQuaternion();
        }

        public static Quaternion InverseSign(this Quaternion quaternion)
        {
            return new Quaternion(-quaternion.x, -quaternion.y, -quaternion.z, -quaternion.w);
        }

        public static Quaternion Normalize(this Quaternion quaternion)
        {
            float length = 1.0f / ((quaternion.w * quaternion.w) + (quaternion.x * quaternion.x) + (quaternion.y * quaternion.y) + (quaternion.z * quaternion.z));
            quaternion.w *= length;
            quaternion.x *= length;
            quaternion.y *= length;
            quaternion.z *= length;
            return new Quaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        }

        public static AngleAxis ToAngleAxis(this Quaternion quaternion)
        {
            // For some reason Unities built in AngleAxis method treats an unrotated object as pointing left, rather than up
            if (quaternion.ApproximatelyEquals(Quaternion.identity))
            {
                return AngleAxis.Identity;
            }

            float angle;
            Vector3 axis;
            quaternion.ToAngleAxis(out angle, out axis);
            return new AngleAxis(angle, axis);
        }
    }
}
