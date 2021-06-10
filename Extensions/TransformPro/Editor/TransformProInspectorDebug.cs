namespace TransformPro.Scripts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    public static class TransformProInspectorDebug
    {
        private static IEnumerable<Assembly> assemblies;
        private static Type editorType;
        private static IEnumerable<Type> otherTypes;
        private static IEnumerable<Type> types;

        public static IEnumerable<Assembly> Assemblies { get { return TransformProInspectorDebug.assemblies ?? (TransformProInspectorDebug.assemblies = AppDomain.CurrentDomain.GetAssemblies()); } }

        public static Type EditorType { get { return TransformProInspectorDebug.editorType ?? (TransformProInspectorDebug.editorType = typeof(Editor)); } }

        public static bool OtherInspectorsInstalled()
        {
            return TransformProInspectorDebug.GetOtherTypes(TransformProInspectorDebug.GetInspectorTypes()).Any();
        }

        public static void OutputInspectors()
        {
            TransformProInspectorDebug.types = TransformProInspectorDebug.GetInspectorTypes().ToList();
            TransformProInspectorDebug.otherTypes = TransformProInspectorDebug.GetOtherTypes(TransformProInspectorDebug.types).ToList();
            if (!TransformProInspectorDebug.otherTypes.Any())
            {
                Debug.Log("[<color=red>TransformPro</color>] TransformPro installed correctly.");
                return;
            }

            StringBuilder stringBuilder = new StringBuilder();
            int otherTypesCount = TransformProInspectorDebug.otherTypes.Count();
            stringBuilder.AppendLine(string.Format("[<color=red>TransformPro</color>] {0} conflicting Transform Inspector{1} found. {2}",
                                                   otherTypesCount,
                                                   otherTypesCount == 1 ? "" : "s",
                                                   otherTypesCount > 1 ? "(select this message to see all)" : ""));
            foreach (Type type in TransformProInspectorDebug.otherTypes)
            {
                stringBuilder.AppendLine(string.Format("    {0}", type.FullName));
            }

            Debug.LogWarning(stringBuilder.ToString());
        }

        private static IEnumerable<Type> GetInspectorTypes()
        {
            List<Type> validTypes = new List<Type>();
            foreach (Assembly assembly in TransformProInspectorDebug.Assemblies)
            {
                try
                {
                    Type[] assemblyTypes = assembly.GetTypes();
                    validTypes.AddRange(assemblyTypes.Where(assemblyType => TransformProInspectorDebug.EditorType.IsAssignableFrom(assemblyType) && assemblyType.IsClass));
                }
                catch
                {
                    Debug.LogWarning(string.Format("[<color=red>TransformPro</color>] Assembly {0} could not be loaded.\nThis usually means the assembly has been complied using the wrong .NET version.", assembly.FullName));
                }
            }

            foreach (Type type in validTypes)
            {
                IEnumerable<Attribute> attributes = type.GetCustomAttributes(true).Cast<Attribute>();
                foreach (Attribute attribute in attributes)
                {
                    CustomEditor customEditor = attribute as CustomEditor;
                    if (customEditor == null)
                    {
                        continue;
                    }

                    FieldInfo reflectedTypeField = typeof(CustomEditor).GetField("m_InspectedType", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (reflectedTypeField == null)
                    {
                        Debug.LogError("Failed to locate CustomEditor attribute inspected type field.");
                        continue;
                    }
                    Type inspectedType = (Type) reflectedTypeField.GetValue(customEditor);
#if UNITY_5_0
                    if (typeof(Transform) == inspectedType)
                    {
                        yield return type;
                    }

#else
                    if ((typeof(Transform) == inspectedType) || (typeof(Transform).IsAssignableFrom(inspectedType) && customEditor.isFallback))
                    {
                        yield return type;
                    }
#endif
                }
            }
        }

        private static IEnumerable<Type> GetOtherTypes(IEnumerable<Type> types)
        {
            return types.Where(type => (type != typeof(TransformProEditor)) && (type.FullName != "UnityEditor.TransformInspector"));
        }
    }
}
