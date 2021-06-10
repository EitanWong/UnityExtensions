namespace TransformPro.Scripts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    ///     Extends the AngleAxis class with helper methods. Added as extension methods for consistancy.
    /// </summary>
    public static class TransformProExtensionsAngleAxis
    {
        public static AngleAxis Average(this IEnumerable<AngleAxis> source)
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

            float angle = 0;
            Vector3 axis = Vector3.zero;
            // LOOK: The .NET Average methods allow for a full long for the counter, we do not need to average any collections that large.
            int count = 0;
            // LOOK: The .NET Average methods check for overflows using 'checked', we should never deal with anything nearly that large.
            foreach (AngleAxis v in source)
            {
                angle += v.Angle;
                axis += v.Axis;
                count++;
            }
            if (count > 0)
            {
                angle = angle / count;
                axis = axis / count;
                return new AngleAxis(angle, axis);
            }

            // LOOK: We treat an average of no elements as a zero vector here, unlike the .NET method which treats it as an error.
            return AngleAxis.Identity;
        }

        public static Quaternion ToQuaternion(this AngleAxis angleAxis)
        {
            return Quaternion.AngleAxis(angleAxis.Angle, angleAxis.Axis);
        }
    }
}
