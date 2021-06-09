using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Net3dBool.CommonTool
{
    /// <summary>
    /// 对象池模板，生成对象池元素的方法来自外部
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class ObjectPool<T> where T : IDisposable
    {
        static readonly ConcurrentStack<T> m_recycled;
        static readonly ConcurrentBag<T> m_created;
        static ObjectPool()
        {
            m_recycled = new ConcurrentStack<T>();
            m_created = new ConcurrentBag<T>();
        }

        /// <summary>
        /// 简单取出一个实例
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public static bool GetInstance(out T cell) => m_recycled.TryPop(out cell);
        /// <summary>
        /// 取出或者构造实例
        /// </summary>
        /// <param name="construct">无法取出时调用的构造函数</param>
        /// <returns></returns>
        public static T Create(Func<T> construct) => GetInstance(out T cell) ? cell : construct();
        /// <summary>
        /// 取出或者构造实例，取出后应用初始化动作
        /// </summary>
        /// <param name="initFromPool">取出后的初始化动作</param>
        /// <param name="construct">无法取出时调用的构造函数</param>
        /// <returns></returns>
        public static T Create(Action<T> initFromPool, Func<T> construct)
        {
            if (GetInstance(out T cell))
            {
                initFromPool(cell);
                return cell;
            }
            T p = construct();
            m_created.Add(p);
            return p;
        }
        /// <summary>
        /// 回收实例到池，在 <see cref="T.Dispose()"/> 中调用
        /// </summary>
        /// <param name="cell"></param>
        public static void Recycle(T cell)
        {
            m_recycled.Push(cell);
        }
        /// <summary>
        /// 回收池创建的所有实例，注意可能导致数据失效或引用丢失
        /// </summary>
        public static void RecycleAll()
        {
            m_recycled.Clear();
            foreach (var c in m_created)
            {
                c.Dispose();
                m_recycled.Push(c);
            }
        }
        /// <summary>
        /// 清空池的内容并试图回收内存
        /// </summary>
        public static void Clear()
        {
            m_recycled.Clear();
            while (m_created.TryTake(out T c))
            {
                c.Dispose();
            }
            GC.Collect();
        }
    }
}