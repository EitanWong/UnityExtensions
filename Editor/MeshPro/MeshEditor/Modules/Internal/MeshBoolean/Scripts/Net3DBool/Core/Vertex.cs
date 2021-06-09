/*
The MIT License (MIT)

Copyright (c) 2014 Sebastian Loncar

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

See:
D. H. Laidlaw, W. B. Trumbore, and J. F. Hughes.
"Constructive Solid Geometry for Polyhedral Objects"
SIGGRAPH Proceedings, 1986, p.161.

original author: Danilo Balby Silva Castanheira (danbalby@yahoo.com)

Ported from Java to C# by Sebastian Loncar, Web: http://www.loncar.de
Project: https://github.com/Arakis/Net3dBool

Optimized and refactored by: Lars Brubaker (larsbrubaker@matterhackers.com)
Project: https://github.com/MatterHackers/agg-sharp (an included library)
*/

using Net3dBool.CommonTool;
using System;
using System.Collections.Generic;

namespace Net3dBool
{
    /// <summary>
    /// 表示 3D 面的顶点
    /// </summary>
    public class Vertex : IDisposable
    {
        public Vector3Double Position { get; private set; }

        /// <summary>
        ///通过边与自身相连的其他顶点
        /// </summary>
        private readonly List<Vertex> adjacentVertices = new List<Vertex>();

        /// <summary>
        /// 顶点状态，相对于其他对象
        /// </summary>
        public Status Status
        {
            get { return status; }
            set { if (value >= Status.UNKNOWN && value <= Status.BOUNDARY) { status = value; } }
        }
        private Status status;

        /// <summary>
        /// 公差，值的差小于此值认为相等
        /// </summary>
        public const double EqualityTolerance = 1e-5;

        public static implicit operator Vector3Double(Vertex self) => self.Position;

        //----------------------------------CONSTRUCTORS--------------------------------//
        /// <summary>
        /// 构造指定状态的顶点
        /// </summary>
        /// <param name="position">顶点坐标</param>
        /// <param name="status">顶点状态：未知，边界，内部 或 外部</param>
        public Vertex(Vector3Double position, Status status = Status.UNKNOWN)
        {
            InitMember(position, status);
        }
        public Vertex InitMember(Vector3Double position, Status status = Status.UNKNOWN)
        {
            Position = position;
            this.status = status;
            disposedValue = false;
            return this;
        }
        public static Vertex GetInstane(Vector3Double position, Status status = Status.UNKNOWN)
        {
            return ObjectPool<Vertex>.Create(initVertex, construct);

            void initVertex(Vertex c) => c.InitMember(position, status);
            Vertex construct() => new Vertex(position, status);
        }

        private Vertex() { }

        public override string ToString() { return Position.ToString(); }

        /// <summary>
        /// 拥有相同位置的顶点被认为相等
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public bool Equals(Vertex vertex) => Position.Equals(vertex.Position, EqualityTolerance);

        //----------------------------------OTHERS--------------------------------------//

        /// <summary>
        /// 指定一个顶点加入自己的邻接点
        /// </summary>
        /// <param name="adjacentVertex"></param>
        public void AddAdjacentVertex(Vertex adjacentVertex)
        {
            if (!adjacentVertices.Contains(adjacentVertex))
            {
                adjacentVertices.Add(adjacentVertex);
            }
        }

        /// <summary>
        /// 为顶点自身及邻接点指定状态
        /// </summary>
        /// <param name="status"></param>
        public void Mark(Status status)
        {
            this.status = status;  // 指定自身状态
            for (int i = 0; i < adjacentVertices.Count; i++)  // 指定邻接点状态
            {
                if (adjacentVertices[i].Status == Status.UNKNOWN)
                {
                    adjacentVertices[i].Mark(status);
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    adjacentVertices.Clear();
                    ObjectPool<Vertex>.Recycle(this);
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~Vertex() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

