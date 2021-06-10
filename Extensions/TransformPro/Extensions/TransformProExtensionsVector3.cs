namespace TransformPro.Scripts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public static class TransformProExtensionsVector3
    {
        /// <summary>
        ///     Helper function for comparing two <see cref="Vector3" /> values. Used to help reduce axis drift.
        /// </summary>
        /// <param name="v1">The left hand <see cref="Vector3" />.</param>
        /// <param name="v2">The right hand <see cref="Vector3" />.</param>
        /// <returns>A <see cref="bool" /> indicating if the two values are approximately the same.</returns>
        public static bool ApproximatelyEquals(this Vector3 v1, Vector3 v2)
        {
            return Mathf.Approximately(v1.x, v2.x) && Mathf.Approximately(v1.y, v2.y) && Mathf.Approximately(v1.z, v2.z);
        }

        public static Vector3 Average(this IEnumerable<Vector3> source)
        {
            if (source == null)
            {
                throw new NullReferenceException("Could not calculate Average, source collection is null.");
            }
            // Ensure any complex LINQ selectors are resolved.
            source = source.ToList();

            // As we're using single floats to prevent drifting any single values will be returned without any modification.
            if (source.Count() == 1)
            {
                return source.First();
            }

            Vector3 sum = Vector3.zero;
            // LOOK: The .NET Average methods allow for a full long for the counter, we do not need to average any collections that large.
            int count = 0;
            // LOOK: The .NET Average methods check for overflows using 'checked', we should never deal with anything nearly that large.
            foreach (Vector3 v in source)
            {
                sum += v;
                count++;
            }
            if (count > 0)
            {
                return sum / count;
            }

            // LOOK: We treat an average of no elements as a zero vector here, unlike the .NET method which treats it as an error.
            return Vector3.zero;
        }

        public static Vector3 ComponentAdd(this Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
        }

        public static Vector3 ComponentDivide(this Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(lhs.x / rhs.x, lhs.y / rhs.y, lhs.z / rhs.z);
        }

        public static Vector3 ComponentMultiply(this Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z);
        }

        public static Vector3 ComponentSubtract(this Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
        }

        public static bool IsScalar(this Vector3 vector3)
        {
            return Mathf.Approximately(vector3.x, vector3.y) && Mathf.Approximately(vector3.x, vector3.z);
        }

        public static bool Mixed(this IEnumerable<Vector3> vectors)
        {
            vectors = vectors.ToList();
            if (!vectors.Any())
            {
                return false;
            }

            Vector3 value = vectors.First();
            return vectors.Any(x => !value.ApproximatelyEquals(x));
        }

        public static bool MixedAxis(this IEnumerable<Vector3> vectors)
        {
            vectors = vectors.ToList();
            return vectors.Select(v => v.x).Mixed() || vectors.Select(v => v.y).Mixed() || vectors.Select(v => v.z).Mixed();
        }
    }
}
