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

using System.Collections.Generic;

namespace Net3dBool
{
    /// <summary>
    /// 表示 3D 物件在坐标系中的包围盒
    /// </summary>
    public struct Bound
    {
        /** maximum from the x coordinate */
        private double xMax;
        /** minimum from the x coordinate */
        private double xMin;
        /** maximum from the y coordinate */
        private double yMax;
        /** minimum from the y coordinate */
        private double yMin;
        /** maximum from the z coordinate */
        private double zMax;
        /** minimum from the z coordinate */
        private double zMin;

        /** tolerance value to test equalities */
        private readonly static double EqualityTolerance = 1e-10;

        //---------------------------------CONSTRUCTORS---------------------------------//

        /// <summary>
        /// 计算面的包围盒
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        public Bound(Vector3Double p1, Vector3Double p2, Vector3Double p3)
        {
            xMax = xMin = p1.x;
            yMax = yMin = p1.y;
            zMax = zMin = p1.z;

            CheckVertex(p2);
            CheckVertex(p3);
        }

        /// <summary>
        /// 计算 3D 物体的包围盒
        /// </summary>
        /// <param name="vertices"></param>
        public Bound(IList<Vector3Double> vertices)
        {
            xMax = xMin = vertices[0].x;
            yMax = yMin = vertices[0].y;
            zMax = zMin = vertices[0].z;

            for (int i = 1; i < vertices.Count; i++) { CheckVertex(vertices[i]); }
        }

        //----------------------------------OVERRIDES-----------------------------------//

        /**
        * Makes a string definition for the bound object
        * 
        * @return the string definition
        */
        public override string ToString()
        {
            return "x: " + xMin + " .. " + xMax + "\ny: " + yMin + " .. " + yMax + "\nz: " + zMin + " .. " + zMax;
        }

        //--------------------------------------OTHERS----------------------------------//

        /// <summary>
        /// 判断包围盒是否重叠
        /// </summary>
        /// <param name="bound"></param>
        /// <returns></returns>
        public bool Overlap(Bound bound)
        {
            return !((xMin > bound.xMax + EqualityTolerance) || (xMax < bound.xMin - EqualityTolerance) ||
                    (yMin > bound.yMax + EqualityTolerance) || (yMax < bound.yMin - EqualityTolerance) ||
                    (zMin > bound.zMax + EqualityTolerance) || (zMax < bound.zMin - EqualityTolerance));
        }

        //-------------------------------------PRIVATES---------------------------------//

        /**
        * Checks if one of the coordinates of a vertex exceed the ones found before 
        * 
        * @param vertex vertex to be tested
        */
        private void CheckVertex(Vector3Double vertex)
        {
            if (vertex.x > xMax) { xMax = vertex.x; }
            else if (vertex.x < xMin) { xMin = vertex.x; }

            if (vertex.y > yMax) { yMax = vertex.y; }
            else if (vertex.y < yMin) { yMin = vertex.y; }

            if (vertex.z > zMax) { zMax = vertex.z; }
            else if (vertex.z < zMin) { zMin = vertex.z; }
        }
    }
}

