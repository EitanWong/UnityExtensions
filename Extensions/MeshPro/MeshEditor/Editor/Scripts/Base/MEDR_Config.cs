#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Extensions.MeshPro.MeshEditor.Editor.Scripts.Base
{
    public abstract class MEDR_Config
    {
        private Dictionary<string, object> _configCache = new Dictionary<string, object>();

        protected void SaveConfig<T>(string key, [NotNull] T value)
        {
            var saveConfig = typeof(T) == typeof(Color)
                ? ColorUtility.ToHtmlStringRGB((Color) (object) value)
                : value.ToString();

            if (!_configCache.ContainsKey(key))
            {
                _configCache.Add(key, saveConfig);
                EditorPrefs.SetString(key, saveConfig);
            }
            else
            {
                var cache = _configCache[key].ToString();
                if (cache != saveConfig)
                {
                    _configCache[key] = saveConfig;
                    EditorPrefs.SetString(key, saveConfig);
                }
            }
        }

        protected bool GetConfig<T>(string key, ref T value)
        {
            var hasCache = _configCache.ContainsKey(key);
            var hasConfig = EditorPrefs.HasKey(key);

            if (hasConfig && !hasCache)
            {
                var config = EditorPrefs.GetString(key);
                _configCache.Add(key, config);
                hasCache = _configCache.ContainsKey(key);
            }

            if (hasCache)
            {
                var cache = _configCache[key].ToString();
                if (typeof(T) == typeof(Color))
                {
                    Color nowColor;
                    ColorUtility.TryParseHtmlString(String.Format("#{0}", cache), out nowColor);
                    value = (T) (object) nowColor;
                }
                else
                {
                    value = (T) Convert.ChangeType(_configCache[key], typeof(T));
                }
            }
            
            return hasCache || hasConfig;
        }
    }
}
#endif