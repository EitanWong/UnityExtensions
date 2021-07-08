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
#if UNITY_EDITOR

using System;
using Net3dBool;

namespace Extensions.MeshPro.MeshEditor.Modules.Internal.MeshBoolean.Scripts.Net3DBool.Core
{
    /// <summary>
    /// 由起点位置和方向表示的 3D 直线或射线
    /// </summary>
    internal struct Line
    {
        /// <summary>
        /// 判断相等的容差
        /// </summary>
        const double EqualityTolerance = 1e-10;
        private static Random random = new Random();

        public Vector3Double StartPoint { get { return startPoint; } }
        private Vector3Double startPoint;

        /// <summary>
        /// 射线方向
        /// </summary>
        public Vector3Double Direction { get; private set; }

        /// <summary>
        /// 构造直线，为两平面的交线
        /// </summary>
        public Line(Face face1, Face face2)
        {
            Vector3Double normalFace1 = face1.PlaneNormal;
            Vector3Double normalFace2 = face2.PlaneNormal;
            Direction = Vector3Double.Cross(normalFace1, normalFace2);

            if (!(Direction.magnitude < EqualityTolerance))  // 两平面不平行
            {
                //getting a line point, zero is set to a coordinate whose direction
                // 获取线上一点，方向坐标设为零点
                //component isn't zero (line intersecting its origin plan)
                // 成员不为零（线与第一个平面相交）
                var d1 = -Vector3Double.Dot(normalFace1, face1.v1.Position);
                var d2 = -Vector3Double.Dot(normalFace2, face2.v1.Position);
                if (Math.Abs(Direction.x) > EqualityTolerance)
                {
                    startPoint = new Vector3Double
                    {
                        x = 0,
                        y = (d2 * normalFace1.z - d1 * normalFace2.z) / Direction.x,
                        z = (d1 * normalFace2.y - d2 * normalFace1.y) / Direction.x
                    };
                }
                else if (Math.Abs(Direction.y) > EqualityTolerance)
                {
                    startPoint = new Vector3Double
                    {
                        x = (d1 * normalFace2.z - d2 * normalFace1.z) / Direction.y,
                        y = 0,
                        z = (d2 * normalFace1.x - d1 * normalFace2.x) / Direction.y
                    };
                }
                else
                {
                    startPoint = new Vector3Double
                    {
                        x = (d2 * normalFace1.y - d1 * normalFace2.y) / Direction.z,
                        y = (d1 * normalFace2.x - d2 * normalFace1.x) / Direction.z,
                        z = 0
                    };
                }
            }
            else
            {
                startPoint = default;
            }
            Direction.Normalize();
        }

        /// <summary>
        /// 用方向和起点构造射线
        /// </summary>
        /// <param name="direction">射线方向</param>
        /// <param name="point">射线起点坐标</param>
        public Line(Vector3Double direction, Vector3Double point)
        {
            Direction = direction;
            startPoint = point;
            direction.Normalize();
        }

        /// <summary>
        /// Computes the point resulting from the intersection with another line
        /// </summary>
        /// <param name="otherLine">the other line to apply the intersection. The lines are supposed to intersect</param>
        /// <returns>point resulting from the intersection. If the point coundn't be obtained, return null</returns>
        public Vector3Double ComputeLineIntersection(Line otherLine)
        {
            //x = x1 + a1*t = x2 + b1*s
            //y = y1 + a2*t = y2 + b2*s
            //z = z1 + a3*t = z2 + b3*s

            Vector3Double lineP = otherLine.StartPoint;
            Vector3Double lineDir = otherLine.Direction;

            double t = 0;
            if (Math.Abs(Direction.y * lineDir.x - Direction.x * lineDir.y) > EqualityTolerance)
            {
                t = (-startPoint.y * lineDir.x + lineP.y * lineDir.x + lineDir.y * startPoint.x - lineDir.y * lineP.x) / 
                    (Direction.y * lineDir.x - Direction.x * lineDir.y);
            }
            else if (Math.Abs(-Direction.x * lineDir.z + Direction.z * lineDir.x) > EqualityTolerance)
            {
                t = -(-lineDir.z * startPoint.x + lineDir.z * lineP.x + lineDir.x * startPoint.z - lineDir.x * lineP.z) / 
                    (-Direction.x * lineDir.z + Direction.z * lineDir.x);
            }
            else if (Math.Abs(-Direction.z * lineDir.y + Direction.y * lineDir.z) > EqualityTolerance)
            {
                t = (startPoint.z * lineDir.y - lineP.z * lineDir.y - lineDir.z * startPoint.y + lineDir.z * lineP.y) / 
                    (-Direction.z * lineDir.y + Direction.y * lineDir.z);
            }
            else
            {
#if DEBUG
                throw new InvalidOperationException();
#else
				return Vector3Double.Zero;
#endif
            }
            return StartPoint + (Direction * t);
        }

        /// <summary>
        /// Compute the point resulting from the intersection with a plane
        /// </summary>
        /// <param name="normal">the plane normal</param>
        /// <param name="planePoint">a plane point.</param>
        /// <returns>intersection point.If they don't intersect, return null</returns>
        public Vector3Double ComputePlaneIntersection(Plane plane)
        {
            var distanceToStartFromOrigin = Vector3Double.Dot(plane.planeNormal, startPoint);

            var distanceFromPlane = distanceToStartFromOrigin - plane.distanceToPlaneFromOrigin;
            var denominator = Vector3Double.Dot(plane.planeNormal, Direction);

            if (Math.Abs(denominator) < EqualityTolerance)  // 射线与平面垂直
            {
                return Math.Abs(distanceFromPlane) < EqualityTolerance ? startPoint : Vector3Double.PositiveInfinity;
            }
            else // 射线被平面拦截
            {
                return StartPoint + (Direction * -distanceFromPlane / denominator);
            }
        }

        /// <summary>
        /// 计算从线到另一点的距离
        /// </summary>
        /// <param name="otherPoint">the point to compute the distance from the line point. The point is supposed to be on the same line.</param>
        /// <returns>如果射线起点到另一点的向量与射线方向相反，会得到负数的值</returns>
        public double ComputePointToPointDistance(Vector3Double otherPoint)
        {
            var distance = (otherPoint - startPoint).magnitude;
            return(Vector3Double.Dot((otherPoint - startPoint).normalized, Direction) < 0) ? -distance : distance;
        }

        /// <summary>
        /// 略微抖动射线方向
        /// </summary>
        public void PerturbDirection()
        {
            Vector3Double perturbedDirection = Direction;
            perturbedDirection.x += 1e-5 * random.NextDouble();
            perturbedDirection.y += 1e-5 * random.NextDouble();
            perturbedDirection.z += 1e-5 * random.NextDouble();

            Direction = perturbedDirection;
        }

        public override string ToString()
        {
            return "Direction: " + Direction.ToString() + "\nPoint: " + startPoint.ToString();
        }
    }
}
#endif