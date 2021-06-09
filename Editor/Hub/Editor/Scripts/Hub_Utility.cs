using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Hub.Editor.Scripts
{
    public static class Hub_Utility
    {
        //public static Assembly assembly => Assembly.Load("Assembly-CSharp");
        //public static Assembly assemblyEditor => Assembly.Load("Assembly-CSharp-Editor");
        public static Assembly assemblyEditor => Assembly.Load("Hub");
        /// <summary>
        /// 获取所有指定的反射类实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> GetAllReflectionClassIns<T>() where T : class
        {
            List<T> reference = new List<T>();
            var types = assemblyEditor.GetTypes();
            foreach (var type in types)
            {
                if (type.BaseType == typeof(T) && type != typeof(T))
                {
                    T ins = EditorWindow.CreateInstance(type) as T;
                    reference.Add(ins);
                }
            }

            return reference;
        }
    }
}