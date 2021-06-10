namespace TransformPro.Scripts
{
    using System.Diagnostics.CodeAnalysis;
    using UnityEngine;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class TransformProExtensionsMatrix4x4
    {
        /// <summary>
        ///     A helper function to get a <see cref="Quaternion" /> value from a <see cref="Matrix4x4" />.
        /// </summary>
        /// <param name="matrix">The <see cref="Matrix4x4" /> to get the <see cref="Quaternion" /> from.</param>
        /// <returns>The <see cref="Quaternion" /> contained with the <see cref="Matrix4x4" />.</returns>
        public static Quaternion ToQuaternion(this Matrix4x4 matrix)
        {
            return Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
        }
    }
}
