using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace UnityExtensions.EditorBase.Editor
{
    public static class EditorPreferenceUtility
    {
        #region 设置加载
        /// <summary>
        /// 颜色配置加载
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="defaultColor">默认颜色</param>
        /// <returns></returns>
        public static  Color LoadColorSetting(string key,Color defaultColor)
        {
            Color r=defaultColor;
            if (!EditorPrefs.HasKey(key)) EditorPrefs.SetString(key, ColorUtility.ToHtmlStringRGB(defaultColor));
            else
            {
                ColorUtility.TryParseHtmlString(String.Format("#{0}", EditorPrefs.GetString(key)), out r);
            }
            return r;
        }
        /// <summary>
        /// 加载整数类型配置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int LoadIntSetting(string key,int defaultValue)
        {
            int resultValue = defaultValue;
            if (!EditorPrefs.HasKey(key)) EditorPrefs.SetInt(key, defaultValue);
            else resultValue =  EditorPrefs.GetInt(key);
            return resultValue;
        }
        #endregion
        #region 设置改变检测

        public static void CheckColorSetting(Color value, string key,Hashtable cache)
        {
            if (!cache.ContainsKey(key)) cache.Add(key, value);
            else
            {
                var lastValue = (Color) cache[key];
                if (lastValue != value)
                {
                    cache[key] = value;
                    EditorPrefs.SetString(key, ColorUtility.ToHtmlStringRGB(value));
                }
            }
        }

        public static void CheckIntSetting(int value, string key,Hashtable cache)
        {
            if (!cache.ContainsKey(key)) cache.Add(key, value);
            else
            {
                var lastValue = (int) cache[key];
                if (lastValue != value)
                {
                    cache[key] = value;
                    EditorPrefs.SetInt(key, value);
                }
            }
        }
        #endregion
    }
}
