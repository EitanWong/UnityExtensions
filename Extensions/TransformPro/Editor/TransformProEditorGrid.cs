namespace TransformPro.Scripts
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    public static class TransformProEditorGrid
    {
        private static PropertyInfo annotationUtilityShowGridProperty;
        private static Type annotationUtilityType;

        //private static TransformProGrid current;
        //private static TransformProGrid world;

        public static bool UnityGridVisible
        {
            get
            {
                if (TransformProEditorGrid.ReflectInternal())
                {
                    return (bool) TransformProEditorGrid.annotationUtilityShowGridProperty.GetValue(null, null);
                }

                Debug.LogWarning("[<color=red>TransformPro</color>] Could not find required assembly or type to get the unity grid status.");
                return false;
            }
            set
            {
                if (!TransformProEditorGrid.ReflectInternal())
                {
                    Debug.LogWarning("[<color=red>TransformPro</color>] Could not find required assembly or type to set the unity grid status.");
                    return;
                }

                TransformProEditorGrid.annotationUtilityShowGridProperty.SetValue(null, value, BindingFlags.NonPublic | BindingFlags.Static, null, null, CultureInfo.InvariantCulture);
            }
        }

        public static void DrawHandles()
        {
            /*
            Camera camera = TransformProEditor.Camera;
            Ray cameraRay = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            // https://docs.unity3d.com/ScriptReference/GeometryUtility.CalculateFrustumPlanes.html
            // Ordering: [0] = Left, [1] = Right, [2] = Down, [3] = Up, [4] = Near, [5] = Far
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);

            // Ordering: [0] = X, [1] = Y, [2] = Z
            Plane[] gridPlanes =
            {
                new Plane(Vector3.left, TransformPro.PositionWorld),
                new Plane(Vector3.up, TransformPro.PositionWorld),
                new Plane(Vector3.forward, TransformPro.PositionWorld)
            };
            Vector3[] ups = {Vector3.up, Vector3.forward, Vector3.up};

            for (int axis = 0; axis < 3; axis++)
            {
                float distance;
                if (gridPlanes[axis].Raycast(cameraRay, out distance))
                {
                    Vector3 hitPoint = cameraRay.origin + (cameraRay.direction * distance);
                    Vector3 normal = gridPlanes[axis].normal;

                    Vector3 v0 = Vector3.Cross(normal, ups[axis]).normalized;
                    Vector3 v1 = Quaternion.AngleAxis(90, normal) * v0;
                    Vector3 v2 = Quaternion.AngleAxis(180, normal) * v0;
                    Vector3 v3 = Quaternion.AngleAxis(270, normal) * v0;

                    v0 = TransformProEditorGrid.FindFrustumEdge(frustumPlanes, hitPoint, v0);
                    v1 = TransformProEditorGrid.FindFrustumEdge(frustumPlanes, hitPoint, v1);
                    v2 = TransformProEditorGrid.FindFrustumEdge(frustumPlanes, hitPoint, v2);
                    v3 = TransformProEditorGrid.FindFrustumEdge(frustumPlanes, hitPoint, v3);

                    /*
                    Handles.DrawLine(hitPoint + v0, hitPoint + v1);
                    Handles.DrawLine(hitPoint + v1, hitPoint + v2);
                    Handles.DrawLine(hitPoint + v2, hitPoint + v3);
                    Handles.DrawLine(hitPoint + v3, hitPoint + v0);
                    Handles.color = Color.red;
                    Handles.DrawSolidDisc(hitPoint + v0, gridPlanes[axis].normal, 0.1f);
                    Handles.color = Color.green;
                    Handles.DrawSolidDisc(hitPoint + v1, gridPlanes[axis].normal, 0.1f);
                    Handles.color = Color.blue;
                    Handles.DrawSolidDisc(hitPoint + v2, gridPlanes[axis].normal, 0.1f);
                    Handles.color = Color.white;
                    Handles.DrawSolidDisc(hitPoint + v3, gridPlanes[axis].normal, 0.1f);
                    //
                }
            }
            */

            /*
            Mesh mesh = PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Plane);
            Graphics.DrawMeshNow(mesh, Vector3.zero, Quaternion.LookRotation(Vector3.forward, Vector3.up));
            Graphics.DrawMeshNow(mesh, Vector3.zero, Quaternion.LookRotation(Vector3.right, Vector3.up));
            Graphics.DrawMeshNow(mesh, Vector3.zero, Quaternion.LookRotation(Vector3.up, Vector3.forward));
            */
        }

        public static void DrawSceneGUI()
        {
        }

        private static void DrawPlane(Vector3 position, Vector3 normal)
        {
            Vector3 v3 = Vector3.zero;

            if (normal.normalized != Vector3.forward)
            {
                v3 = Vector3.Cross(normal, Vector3.forward).normalized * normal.magnitude;
            }
            else
            {
                v3 = Vector3.Cross(normal, Vector3.up).normalized * normal.magnitude;
            }

            Vector3 corner0 = position + v3;
            Vector3 corner2 = position - v3;
            Quaternion q = Quaternion.AngleAxis(90.0f, normal);
            v3 = q * v3;
            Vector3 corner1 = position + v3;
            Vector3 corner3 = position - v3;

            Debug.DrawLine(corner0, corner2, Color.green);
            Debug.DrawLine(corner1, corner3, Color.green);
            Debug.DrawLine(corner0, corner1, Color.green);
            Debug.DrawLine(corner1, corner2, Color.green);
            Debug.DrawLine(corner2, corner3, Color.green);
            Debug.DrawLine(corner3, corner0, Color.green);
            Debug.DrawRay(position, normal, Color.red);
        }

        private static Vector3 FindFrustumEdge(Plane[] frustumPlanes, Vector3 hitPoint, Vector3 vertex)
        {
            float maxDistance = 0;
            for (int frustum = 0; frustum < 4; frustum++)
            {
                float frustumDistance = 0;
                if (frustumPlanes[frustum].Raycast(new Ray(hitPoint, hitPoint + vertex), out frustumDistance))
                {
                    if (frustumDistance > maxDistance)
                    {
                        maxDistance = frustumDistance;
                    }
                }
            }
            vertex *= maxDistance;
            return vertex;
        }

        private static bool ReflectInternal()
        {
            try
            {
                if (TransformProEditorGrid.annotationUtilityType == null)
                {
                    Assembly unityEditorAssembly = typeof(Editor).Assembly;
                    TransformProEditorGrid.annotationUtilityType = unityEditorAssembly.GetType("AnnotationUtility");
                }
                if ((TransformProEditorGrid.annotationUtilityType != null) && (TransformProEditorGrid.annotationUtilityShowGridProperty == null))
                {
                    TransformProEditorGrid.annotationUtilityShowGridProperty = TransformProEditorGrid.annotationUtilityType.GetProperty("showGrid", BindingFlags.NonPublic | BindingFlags.Static);
                }
                return TransformProEditorGrid.annotationUtilityShowGridProperty != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
