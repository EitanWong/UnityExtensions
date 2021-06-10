namespace TransformPro.Scripts
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityObject = UnityEngine.Object;

    public class TransformProAnnotationUtility
    {
        private static readonly ICollection<int> colliderClasses = new List<int> {56, 64, 65, 135, 136, 146, 154};
        private static readonly ICollection<int> rendererClasses = new List<int> {23, 25, 26, 96, 120, 137, 161, 199, 212, 222, 227};
        private static IEnumerable annotationList;
        private static object annotationObjects;
        private static List<ReflectedAnnotation> annotations;
        private static Assembly editorAssembly;
        private static MethodInfo getAnnotations;
        private static MethodInfo setGizmoEnabled;
        private static MethodInfo setIconEnabled;
        private static Type utilityType;

        static TransformProAnnotationUtility()
        {
            TransformProAnnotationUtility.editorAssembly = Assembly.GetAssembly(typeof(Editor));
            TransformProAnnotationUtility.utilityType = TransformProAnnotationUtility.editorAssembly.GetType("UnityEditor.AnnotationUtility");
            if (TransformProAnnotationUtility.utilityType == null)
            {
                return;
            }

            TransformProAnnotationUtility.getAnnotations = TransformProAnnotationUtility.utilityType.GetMethod("GetAnnotations", BindingFlags.Static | BindingFlags.NonPublic);
            TransformProAnnotationUtility.annotationObjects = TransformProAnnotationUtility.getAnnotations.Invoke(null, null);
            TransformProAnnotationUtility.annotationList = (IEnumerable) TransformProAnnotationUtility.annotationObjects;

            TransformProAnnotationUtility.setGizmoEnabled = TransformProAnnotationUtility.utilityType.GetMethod("SetGizmoEnabled", BindingFlags.Static | BindingFlags.NonPublic);
            TransformProAnnotationUtility.setIconEnabled = TransformProAnnotationUtility.utilityType.GetMethod("SetIconEnabled", BindingFlags.Static | BindingFlags.NonPublic);

            TransformProAnnotationUtility.annotations = new List<ReflectedAnnotation>();
            foreach (object annotation in TransformProAnnotationUtility.annotationList)
            {
                Type annotationType = annotation.GetType();
                FieldInfo fieldClassID = annotationType.GetField("classID", BindingFlags.Public | BindingFlags.Instance);
                FieldInfo fieldScriptClass = annotationType.GetField("scriptClass", BindingFlags.Public | BindingFlags.Instance);
                if ((fieldClassID == null) || (fieldScriptClass == null))
                {
                    continue;
                }

                TransformProAnnotationUtility.annotations.Add(new ReflectedAnnotation(annotation, fieldClassID, fieldScriptClass));
            }

            TransformProAnnotationUtility.UpdateRendererGizmoVisibility();
            TransformProAnnotationUtility.UpdateColliderGizmoVisibility();
        }

        public static void SetColliderGizmoVisibility(bool visible)
        {
            TransformProAnnotationUtility.SetGizmoVisibility(visible, TransformProAnnotationUtility.colliderClasses);
        }

        public static void SetRendererGizmoVisibility(bool visible)
        {
            TransformProAnnotationUtility.SetGizmoVisibility(visible, TransformProAnnotationUtility.rendererClasses);
        }

        public static void UpdateColliderGizmoVisibility()
        {
            TransformProAnnotationUtility.SetColliderGizmoVisibility(TransformProPreferences.ShowColliderBounds);
        }

        public static void UpdateRendererGizmoVisibility()
        {
            TransformProAnnotationUtility.SetRendererGizmoVisibility(TransformProPreferences.ShowRendererBounds);
        }

        private static void SetGizmoVisibility(bool visible, ICollection<int> classList)
        {
            foreach (ReflectedAnnotation annotation in TransformProAnnotationUtility.annotations.Where(annotation => classList.Contains(annotation.ClassID)))
            {
                TransformProAnnotationUtility.setGizmoEnabled.Invoke(null, new object[] {annotation.ClassID, annotation.ScriptClass, visible ? 1 : 0});
                TransformProAnnotationUtility.setIconEnabled.Invoke(null, new object[] {annotation.ClassID, annotation.ScriptClass, visible ? 1 : 0});
            }
        }

        private struct ReflectedAnnotation
        {
            private readonly int classID;
            private readonly string scriptClass;

            public ReflectedAnnotation(object annotation, FieldInfo fieldClassID, FieldInfo fieldScriptClass)
            {
                this.classID = (int) fieldClassID.GetValue(annotation);
                this.scriptClass = (string) fieldScriptClass.GetValue(annotation);
            }

            public int ClassID { get { return this.classID; } }

            public string ScriptClass { get { return this.scriptClass; } }
        }
    }
}
