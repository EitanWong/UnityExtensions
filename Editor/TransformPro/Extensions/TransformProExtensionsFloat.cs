namespace TransformPro.Scripts
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public static class TransformProExtensionsFloat
    {
        public static bool ApproximatelyEquals(this float f1, float f2)
        {
            return Mathf.Approximately(f1, f2);
        }

        public static bool Mixed(this IEnumerable<float> floats)
        {
            floats = floats.ToList();
            if (!floats.Any())
            {
                return false;
            }

            float value = floats.First();
            return floats.Any(x => !value.ApproximatelyEquals(x));
        }
    }
}
