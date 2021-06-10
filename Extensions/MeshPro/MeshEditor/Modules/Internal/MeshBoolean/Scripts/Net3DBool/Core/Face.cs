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

namespace Net3dBool
{
    public enum Status
    {
        /// <summary>
        /// 未知（缺省值）
        /// </summary>
        UNKNOWN,
        /// <summary>
        /// 内部
        /// </summary>
        INSIDE,
        /// <summary>
        /// 外部
        /// </summary>
        OUTSIDE,
        /// <summary>
        /// 相同
        /// </summary>
        SAME,
        /// <summary>
        /// 相反
        /// </summary>
        OPPOSITE,
        /// <summary>
        /// 边界
        /// </summary>
        BOUNDARY
    };

    /// <summary>
    /// 表示 3D 平面（三角面）
    /// </summary>
    internal class Face : IEquatable<Face>, IDisposable
    {
        /** first vertex */
        public Vertex v1;
        /** second vertex */
        public Vertex v2;
        /** third vertex */
        public Vertex v3;

        /// <summary>
        /// 顶点的几何中心
        /// </summary>
        public Vector3Double Center => (v1.Position + v2.Position + v3.Position) / 3;

        /** face status relative to a solid  */
        const double epsilon = 1e-5;
        private enum Side { UP, DOWN, ON, NONE };

        private Bound boundCache;
        private bool cachedBounds;
        public Bound Bound
        {
            get
            {
                if (!cachedBounds)
                {
                    boundCache = new Bound(v1.Position, v2.Position, v3.Position);
                    cachedBounds = true;
                }
                return boundCache;
            }
        }
        public Plane Plane { get; private set; }
        public Vector3Double PlaneNormal => Plane.planeNormal;

        public Status Status { get; private set; }
        /** face status if it is still unknown */
        /** face status if it is inside a solid */
        /** face status if it is outside a solid */
        /** face status if it is coincident with a solid face */
        /** face status if it is coincident with a solid face with opposite orientation*/
        /** point status if it is up relative to an edge - see linePositionIn_ methods */
        /** point status if it is down relative to an edge - see linePositionIn_ methods */
        /** point status if it is on an edge - see linePositionIn_ methods */
        /** point status if it isn't up, down or on relative to an edge - see linePositionIn_ methods */
        /** tolerance value to test equalities */
        //---------------------------------CONSTRUCTORS---------------------------------//

        /// <summary>
        /// * Constructs a face with unknown status.
        /// </summary>
        /// <param name="v1">a face vertex</param>
        /// <param name="v2">a face vertex</param>
        /// <param name="v3">a face vertex</param>
        public Face(Vertex v1, Vertex v2, Vertex v3)
        {
            InitMember(v1, v2, v3);
        }
        public Face InitMember(Vertex v1, Vertex v2, Vertex v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;

            boundCache = default;
            cachedBounds = false;
            Plane = new Plane(v1, v2, v3);

            Status = Status.UNKNOWN;
            disposedValue = false;
            return this;
        }
        public static Face GetInstance(Vertex v1, Vertex v2, Vertex v3)
        {
            return ObjectPool<Face>.Create(initFace, construct);

            void initFace(Face c) => c.InitMember(v1, v2, v3);
            Face construct() => new Face(v1, v2, v3);
        }

        public override bool Equals(object obj)
        {
            return Equals(this, obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(Face face)
        {
            if (disposedValue || face.disposedValue) { return false; }
            bool cond1 = v1.Equals(face.v1) && v2.Equals(face.v2) && v3.Equals(face.v3);
            bool cond2 = v1.Equals(face.v2) && v2.Equals(face.v3) && v3.Equals(face.v1);
            bool cond3 = v1.Equals(face.v3) && v2.Equals(face.v1) && v3.Equals(face.v2);

            return cond1 || cond2 || cond3;
        }

        public static bool operator ==(Face a, Face b) => a.Equals(b);
        public static bool operator !=(Face a, Face b) => !a.Equals(b);

        public double GetArea()
        {
            Vector3Double p1 = v1.Position;
            Vector3Double p2 = v2.Position;
            Vector3Double p3 = v3.Position;
            return Vector3Double.Cross(p2 - p1, p3 - p1).magnitude / 2;
        }

        /// <summary>
        /// 反转三角面
        /// </summary>
        public void Invert()
        {
            Vertex vertexTemp = v2;
            v2 = v1;
            v1 = vertexTemp;
        }

        /// <summary>
        /// 为表面分类，基于光线追踪技术
        /// </summary>
        /// <param name="obj">计算面状态的 3D 物件</param>
        public void RayTraceClassify(Object3D obj)
        {
            Line ray = new Line(PlaneNormal, Center);  // 射线从面的几何中心发出，方向为法线方向
            Face closet = default;
            bool foundCloset = false;
            double closestDistance;
            bool success;
            do
            {
                success = true;
                closestDistance = double.MaxValue;
                for (int faceIndex = 0; faceIndex < obj.GetNumFaces(); faceIndex++)
                {
                    Face face = obj.GetFace(faceIndex);
                    var intersectionPoint = ray.ComputePlaneIntersection(face.Plane);  // 对三角面所在的平面采样
                    if (intersectionPoint.x < float.PositiveInfinity)  // 射线被平面拦截
                    {
                        var distance = ray.ComputePointToPointDistance(intersectionPoint);
                        var dot = Vector3Double.Dot(face.PlaneNormal, ray.Direction);
                        bool parallel = Math.Abs(dot) < epsilon;  // 射线与平面平行
                        //if ray lies in plane...
                        if (Math.Abs(distance) < epsilon)  // 交点是射线起点
                        {
                            if (parallel)
                            {
                                // 略微抖动光线方向以免采样到另一平面
                                ray.PerturbDirection();
                                success = false;
                                break;
                            }
                            else if (face.ContainsPoint(intersectionPoint))
                            {
                                // 面重合
                                closet = face;
                                foundCloset = true;
                                closestDistance = 0;
                                break;
                            }
                        }
                        else if (!parallel && distance > epsilon)  // 射线产生交点
                        {
                            if (distance < closestDistance && face.ContainsPoint(intersectionPoint))
                            {
                                closestDistance = distance;
                                closet = face;  // 当前的面时最近的平面
                                foundCloset = true;
                            }
                        }
                    }
                }
            } while (!success);

            if (!foundCloset) { Status = Status.OUTSIDE; }  // 没有找到面，自己是外部面
            else // 由离自己最近的面，检查方向
            {
                var dot = Vector3Double.Dot(closet.PlaneNormal, ray.Direction);
                if (Math.Abs(closestDistance) < epsilon)  // 距离为零，这个面和自己重合
                {
                    if (dot > epsilon) { Status = Status.SAME; }
                    else if (dot < -epsilon) { Status = Status.OPPOSITE; }
                }
                else if (dot > epsilon) { Status = Status.INSIDE; }  // 不重合，同向，在参数物件内部
                else if (dot < -epsilon) { Status = Status.OUTSIDE; }  // 不重合，反向，在参数物件外部
            }
        }

        /// <summary>
        /// Classifies the face if one of its vertices are classified as INSIDE or OUTSIDE
        /// </summary>
        /// <returns>true if the face could be classified, false otherwise</returns>
        public bool SimpleClassify()
        {
            Status status1 = v1.Status;
            Status status2 = v2.Status;
            Status status3 = v3.Status;

            if (status1 == Status.INSIDE || status1 == Status.OUTSIDE)
            {
                Status = status1;
                return true;
            }
            else if (status2 == Status.INSIDE || status2 == Status.OUTSIDE)
            {
                Status = status2;
                return true;
            }
            else if (status3 == Status.INSIDE || status3 == Status.OUTSIDE)
            {
                Status = status3;
                return true;
            }
            else { return false; }
        }

        public override string ToString()
        {
            return v1.ToString() + "\n" + v2.ToString() + "\n" + v3.ToString();
        }

        //------------------------------------PRIVATES----------------------------------//

        /// <summary>
        /// Gets the position of a point relative to a line in the x plane
        /// </summary>
        /// <param name="point">point to be tested</param>
        /// <param name="pointLine1">one of the line ends</param>
        /// <param name="pointLine2">one of the line ends</param>
        /// <returns>position of the point relative to the line - UP, DOWN, ON, NONE</returns>
        private static Side LinePositionInX(Vector3Double point, Vector3Double pointLine1, Vector3Double pointLine2)
        {
            if ((Math.Abs(pointLine1.y - pointLine2.y) > epsilon) && (((point.y >= pointLine1.y) && (point.y <= pointLine2.y)) || ((point.y <= pointLine1.y) && (point.y >= pointLine2.y))))
            {
                var a = (pointLine2.z - pointLine1.z) / (pointLine2.y - pointLine1.y);
                var b = pointLine1.z - a * pointLine1.y;
                var z = a * point.y + b;
                return z > point.z + epsilon ? Side.UP : z < point.z - epsilon ? Side.DOWN : Side.ON;
            }
            else { return Side.NONE; }
        }

        /// <summary>
        /// Gets the position of a point relative to a line in the y plane
        /// </summary>
        /// <param name="point">point to be tested</param>
        /// <param name="pointLine1">one of the line ends</param>
        /// <param name="pointLine2">one of the line ends</param>
        /// <returns>position of the point relative to the line - UP, DOWN, ON, NONE</returns>
        private static Side LinePositionInY(Vector3Double point, Vector3Double pointLine1, Vector3Double pointLine2)
        {
            if ((Math.Abs(pointLine1.x - pointLine2.x) > epsilon) && 
                (((point.x >= pointLine1.x) && (point.x <= pointLine2.x)) || ((point.x <= pointLine1.x) && (point.x >= pointLine2.x))))
            {
                var a = (pointLine2.z - pointLine1.z) / (pointLine2.x - pointLine1.x);
                var b = pointLine1.z - a * pointLine1.x;
                var z = a * point.x + b;
                return z > point.z + epsilon ? Side.UP : z < point.z - epsilon ? Side.DOWN : Side.ON;
            }
            else { return Side.NONE; }
        }

        /// <summary>
        /// Gets the position of a point relative to a line in the z plane
        /// </summary>
        /// <param name="point">point to be tested</param>
        /// <param name="pointLine1">one of the line ends</param>
        /// <param name="pointLine2">one of the line ends</param>
        /// <returns>position of the point relative to the line - UP, DOWN, ON, NONE</returns>
        private static Side LinePositionInZ(Vector3Double point, Vector3Double pointLine1, Vector3Double pointLine2)
        {
            if ((Math.Abs(pointLine1.x - pointLine2.x) > epsilon) && 
                (((point.x >= pointLine1.x) && (point.x <= pointLine2.x)) || ((point.x <= pointLine1.x) && (point.x >= pointLine2.x))))
            {
                var a = (pointLine2.y - pointLine1.y) / (pointLine2.x - pointLine1.x);
                var b = pointLine1.y - a * pointLine1.x;
                var y = a * point.x + b;
                return y > point.y + epsilon ? Side.UP : y < point.y - epsilon ? Side.DOWN : Side.ON;
            }
            else { return Side.NONE; }
        }

        /// <summary>
        /// Checks if the the face contains a point
        /// </summary>
        /// <param name="point">point to be tested</param>
        /// <returns>true if the face contains the point, false otherwise</returns>
        private bool ContainsPoint(Vector3Double point)
        {
            Side result1;
            Side result2;
            Side result3;
            Vector3Double normal = PlaneNormal;

            //if x is constant...
            if (Math.Abs(normal.x) > epsilon)
            {
                //tests on the x plane
                result1 = LinePositionInX(point, v1.Position, v2.Position);
                result2 = LinePositionInX(point, v2.Position, v3.Position);
                result3 = LinePositionInX(point, v3.Position, v1.Position);
            }

            //if y is constant...
            else if (Math.Abs(normal.y) > epsilon)
            {
                //tests on the y plane
                result1 = LinePositionInY(point, v1.Position, v2.Position);
                result2 = LinePositionInY(point, v2.Position, v3.Position);
                result3 = LinePositionInY(point, v3.Position, v1.Position);
            }
            else
            {
                //tests on the z plane
                result1 = LinePositionInZ(point, v1.Position, v2.Position);
                result2 = LinePositionInZ(point, v2.Position, v3.Position);
                result3 = LinePositionInZ(point, v3.Position, v1.Position);
            }

            // if the point is up and down two lines...
            // if the point is on of the lines...
            return (((result1 == Side.UP) || (result2 == Side.UP) || (result3 == Side.UP)) &&
                    ((result1 == Side.DOWN) || (result2 == Side.DOWN) || (result3 == Side.DOWN))) ||
                (result1 == Side.ON) || (result2 == Side.ON) || (result3 == Side.ON);
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
                    v1 = v2 = v3 = null;
                    ObjectPool<Face>.Recycle(this);
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~Face() {
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