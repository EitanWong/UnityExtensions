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

using System;
using System.Collections.Generic;

namespace Net3dBool
{
    /// <summary>
    /// 表示 3D 物体
    /// </summary>
    public class Solid
    {
        /** 表示顶点的三角形序列 */
        protected int[] triangles;
        /** 表示所有顶点的坐标 */
        protected Vector3Double[] vertices;

        //--------------------------------CONSTRUCTORS----------------------------------//

        /// <summary>
        /// 生成一个空的实例
        /// </summary>
        public Solid()
        {
            vertices = new Vector3Double[0];
            triangles = new int[0];
        }

        /// <summary>
        /// 按照顶点信息和三角形信息生成实例，复制参数的内容
        /// </summary>
        /// <param name="vertices">顶点坐标集合</param>
        /// <param name="triangles">长度需要为 3 的倍数</param>
        public Solid(IList<Vector3Double> vertices, IList<int> triangles)
        {
            this.vertices = new Vector3Double[vertices.Count];
            this.triangles = new int[triangles.Count];
            for (int i = 0; i < vertices.Count; i++) { this.vertices[i] = vertices[i]; }
            triangles.CopyTo(this.triangles, 0);
        }

        //---------------------------------------GETS-----------------------------------//

        /// <summary>
        /// 返回顶点坐标集合的副本
        /// </summary>
        public Vector3Double[] Vertices
        {
            get
            {
                Vector3Double[] newVertices = new Vector3Double[vertices.Length];
                for (int i = 0; i < newVertices.Length; i++) { newVertices[i] = vertices[i]; }
                return newVertices;
            }
        }

        public void GetVertices(List<Vector3Double> destination) => destination.AddRange(vertices);

        /// <summary>
        /// 返回顶点三角形序列的副本
        /// </summary>
        public int[] Triangles
        {
            get
            {
                int[] newIndices = new int[triangles.Length];
                Array.Copy(triangles, 0, newIndices, 0, triangles.Length);
                return newIndices;
            }
        }

        public void GetTriangles(List<int> destination) => destination.AddRange(triangles);

        /// <summary>
        /// 判断物体信息为空
        /// </summary>
        /// <returns></returns>
        public bool isEmpty()
        {
            return triangles.Length == 0;
        }

        //-------------------------GEOMETRICAL_TRANSFORMATIONS-------------------------//

        /// <summary>
        /// 平移所有顶点
        /// </summary>
        public void translate(double dx, double dy, double dz)
        {
            if (dx == 0 && dy == 0 && dz == 0) { return; }
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].x += dx;
                vertices[i].y += dy;
                vertices[i].z += dz;
            }
        }

        /// <summary>
        /// 平移所有顶点
        /// </summary>
        public void translate(Vector3Double dv)
        {
            if (dv.x == 0 && dv.y == 0 && dv.z == 0) { return; }
            for (int i = 0; i < vertices.Length; i++) { vertices[i] += dv; }
        }

        /**
        * Applies a rotation into a solid
        * 
        * @param dx rotation on the x axis
        * @param dy rotation on the y axis
        */
        public void rotate(double dx, double dy)
        {
            double cosX = Math.Cos(dx);
            double cosY = Math.Cos(dy);
            double sinX = Math.Sin(dx);
            double sinY = Math.Sin(dy);

            if (dx != 0 || dy != 0)
            {
                //get mean
                Vector3Double mean = getMean();

                double newX, newY, newZ;
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i].x -= mean.x;
                    vertices[i].y -= mean.y;
                    vertices[i].z -= mean.z;

                    //x rotation
                    if (dx != 0)
                    {
                        newY = vertices[i].y * cosX - vertices[i].z * sinX;
                        newZ = vertices[i].y * sinX + vertices[i].z * cosX;
                        vertices[i].y = newY;
                        vertices[i].z = newZ;
                    }

                    //y rotation
                    if (dy != 0)
                    {
                        newX = vertices[i].x * cosY + vertices[i].z * sinY;
                        newZ = -vertices[i].x * sinY + vertices[i].z * cosY;
                        vertices[i].x = newX;
                        vertices[i].z = newZ;
                    }

                    vertices[i].x += mean.x;
                    vertices[i].y += mean.y;
                    vertices[i].z += mean.z;
                }
            }
        }

        /**
        * Applies a zoom into a solid
        * 
        * @param dz translation on the z axis
        */
        public void zoom(double dz)
        {
            if (dz != 0) { for (int i = 0; i < vertices.Length; i++) { vertices[i].z += dz; } }
        }

        /**
        * Applies a scale changing into the solid
        * 
        * @param dx scale changing for the x axis 
        * @param dy scale changing for the y axis
        * @param dz scale changing for the z axis
        */
        public void scale(double dx, double dy, double dz)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].x *= dx;
                vertices[i].y *= dy;
                vertices[i].z *= dz;
            }
        }

        //-----------------------------------PRIVATES--------------------------------//

        /**
        * Gets the solid mean
        * 
        * @return point representing the mean
        */
        protected Vector3Double getMean()
        {
            Vector3Double mean = new Vector3Double();
            for (int i = 0; i < vertices.Length; i++)
            {
                mean.x += vertices[i].x;
                mean.y += vertices[i].y;
                mean.z += vertices[i].z;
            }
            mean.x /= vertices.Length;
            mean.y /= vertices.Length;
            mean.z /= vertices.Length;

            return mean;
        }
    }
}

