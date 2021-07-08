#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Extensions.MeshPro.MeshEditor.Editor.Scripts.Base;

namespace Extensions.MeshPro.MeshEditor.Editor.Scripts.Manager
{
    // ReSharper disable once InconsistentNaming
    public static class MEDR_ConfigManager
    {
        private static readonly Dictionary<Type, MEDR_Config> CacheConfigs = new Dictionary<Type, MEDR_Config>();

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetConfig<T>() where T : MEDR_Config
        {
            var type = typeof(T);
            var hasInCache = CacheConfigs.ContainsKey(type);
            if (hasInCache)
                return GetConfigInCache<T>();
            
            var ins = System.Activator.CreateInstance<T>();
            CacheConfigs.Add(type, ins);
            return ins;
        }

        private static T GetConfigInCache<T>()
        {
            var type = typeof(T);
            if (!CacheConfigs.ContainsKey(type))
                return default;
            var config = CacheConfigs[type];
            return (T) Convert.ChangeType(config, type);
        }
    }
}
#endif