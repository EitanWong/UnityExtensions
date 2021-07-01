using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dialogue.Scripts.Utilities
{
    public static class DialogueInternalUtility
    {
        public static Assembly assemblyEditor => Assembly.Load("Dialogue");

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
                    T ins = Activator.CreateInstance(type) as T;
                    reference.Add(ins);
                }
            }
            return reference;
        }
    }
}
