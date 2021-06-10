namespace TransformPro.Scripts
{
    using UnityEditor;
    using UnityEngine;

    public static class TransformProDrawing
    {
        public static void Cube(Vector3 center, float size, Matrix4x4 matrix)
        {
            Handles.matrix = matrix;
            TransformProDrawing.Cube(center, size);
            Handles.matrix = Matrix4x4.identity;
        }

        public static void Cube(Vector3 center, float size)
        {
#if UNITY_5_5_OR_NEWER
            Handles.CubeHandleCap(0, center, Quaternion.identity, size, EventType.Repaint);
#elif UNITY_5_3_OR_NEWER
            Handles.CubeCap(0, center, Quaternion.identity, size);
#endif
        }

        public static void WireCube(Vector3 center, Vector3 size)
        {
#if UNITY_5_4_OR_NEWER
            Handles.DrawWireCube(center, size);
#else
            Vector3 a = center - (size / 2);
            Vector3 b = center + (size / 2);

            Vector3 aaa = new Vector3(a.x, a.y, a.z);
            Vector3 aab = new Vector3(a.x, a.y, b.z);
            Vector3 aba = new Vector3(a.x, b.y, a.z);
            Vector3 abb = new Vector3(a.x, b.y, b.z);
            Vector3 baa = new Vector3(b.x, a.y, a.z);
            Vector3 bab = new Vector3(b.x, a.y, b.z);
            Vector3 bba = new Vector3(b.x, b.y, a.z);
            Vector3 bbb = new Vector3(b.x, b.y, b.z);

            // X
            Handles.DrawPolyLine(aaa, aba, abb, aab, aaa);
            Handles.DrawPolyLine(baa, bba, bbb, bab, baa);

            // Y
            Handles.DrawPolyLine(aaa, baa, bab, aab, aaa);
            Handles.DrawPolyLine(bbb, abb, aba, bba, bbb);

            // Z
            Handles.DrawPolyLine(aaa, baa, bba, aba, aaa);
            Handles.DrawPolyLine(bbb, abb, aab, bab, bbb);
#endif
        }

        public static void WireCube(Vector3 center, Vector3 size, Matrix4x4 matrix)
        {
            Handles.matrix = matrix;
            TransformProDrawing.WireCube(center, size);
            Handles.matrix = Matrix4x4.identity;
        }

        public static void WireCube(Matrix4x4 matrix)
        {
            Handles.matrix = matrix;
            TransformProDrawing.WireCube(Vector3.zero, Vector3.one);
            Handles.matrix = Matrix4x4.identity;
        }
    }
}
