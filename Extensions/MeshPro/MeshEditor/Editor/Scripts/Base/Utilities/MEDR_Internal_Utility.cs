#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Extensions.MeshPro.MeshEditor.Editor.Scripts.Base.Utilities
{
    public static class MEDR_Internal_Utility
    {
        //public static Assembly assembly => Assembly.Load("Assembly-CSharp");
        //public static Assembly assemblyEditor => Assembly.Load("Assembly-CSharp-Editor");
        public static Assembly assemblyEditor => Assembly.Load("MeshEditor");

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


        /// <summary>
        /// 显示通知在Scene窗口
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="time"></param>
        public static void ShowNotificationOnSceneView(string msg, float time)
        {
            var currentView = SceneView.currentDrawingSceneView;
            if (currentView)
            {
                currentView.ShowNotification(new GUIContent(msg), time);
            }
            else
            {
                var views = SceneView.sceneViews;
                if (views != null && views.Count > 0)
                {
                    var firstView = (EditorWindow) views[0];
                    firstView.ShowNotification(new GUIContent(msg), time);
                }
            }
        }
    }
}
#endif