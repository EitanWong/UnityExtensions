using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Net3dBool.CommonTool
{
    /// <summary>
    /// 容器池
    /// </summary>
    /// <typeparam name="T">池中元素的类型</typeparam>
    public class CollectionPool<T>
    {
        readonly ConcurrentStack<ListCell> lRecycled;

        static readonly CollectionPool<T> m_instance = new CollectionPool<T>();
        CollectionPool()
        {
            // 初始化...
            lRecycled = new ConcurrentStack<ListCell>();
        }

        public class ListCell : IList<T>, IDisposable
        {
            readonly List<T> list;
            public List<T> Body => list;
            public static implicit operator List<T>(ListCell lc) => lc.Body;

            bool active;

            ListCell()
            {
                list = new List<T>();
            }

            public static ListCell Create(int initLen = 0, T initValue = default)
            {
                if (m_instance.lRecycled.IsEmpty || !m_instance.lRecycled.TryPop(out ListCell cell))
                {
                    cell = new ListCell() { active = true };
                }
                else
                {
                    cell.active = true;
                }
                for (int i = 0; i < initLen; i++) { cell.Add(initValue); }
                return cell;
            }

            public static ListCell Create(IEnumerable<T> source)
            {
                var cell = Create();
                foreach (var c in source) { cell.Add(c); }
                return cell;
            }

            public static ListCell Create(params T[] source) => Create(source as IEnumerable<T>);

            public void Dispose()
            {
                if (active)
                {
                    active = false;
                    list.Clear();
                    m_instance.lRecycled.Push(this);
                }
            }

            public bool Contains(T item, IEqualityComparer<T> comparer)
            {
                for (int i = 0; i < Body.Count; i++)
                {
                    if (comparer.Equals(Body[i], item)) { return true; }
                }
                return false;
            }

            #region IList
            public T this[int index] { get => ((IList<T>)list)[index]; set => ((IList<T>)list)[index] = value; }

            public int Count => ((IList<T>)list).Count;

            public bool IsReadOnly => ((IList<T>)list).IsReadOnly;

            public void Add(T item)
            {
                ((IList<T>)list).Add(item);
            }

            public void Clear()
            {
                ((IList<T>)list).Clear();
            }

            public bool Contains(T item)
            {
                return ((IList<T>)list).Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                ((IList<T>)list).CopyTo(array, arrayIndex);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return ((IList<T>)list).GetEnumerator();
            }

            public int IndexOf(T item)
            {
                return ((IList<T>)list).IndexOf(item);
            }

            public void Insert(int index, T item)
            {
                ((IList<T>)list).Insert(index, item);
            }

            public bool Remove(T item)
            {
                return ((IList<T>)list).Remove(item);
            }

            public void RemoveAt(int index)
            {
                ((IList<T>)list).RemoveAt(index);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IList<T>)list).GetEnumerator();
            }
            #endregion
        }
    }
}