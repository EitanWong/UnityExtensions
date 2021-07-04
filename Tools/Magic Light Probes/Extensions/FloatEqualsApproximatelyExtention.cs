#if UNITY_EDITOR
using UnityEngine;

namespace MagicLightProbes
{
    public static class FloatEqualsApproximatelyExtention
    {
        public static bool EqualsApproximately(this float a, float compared, float accuracy)
        {
            float difference = Mathf.Abs(a - compared);

            if (difference < accuracy)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
#endif
