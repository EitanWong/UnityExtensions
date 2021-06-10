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
using System.Collections;
using System.Collections.Generic;

namespace Net3dBool
{
    /// <summary>
    /// 定义平面之间交线的线段
    /// </summary>
    internal struct Segment
    {
        /// <summary>
        /// 缓存两平面的交线
        /// </summary>
        private Line line;

        /** define as vertex one of the segment ends */
        public const int VERTEX = 1;  // 段结束于顶点
        /** define as face one of the segment ends */
        public const int FACE = 2;
        /** define as edge one of the segment ends */
        public const int EDGE = 3;

        /// <summary>
        /// 公差
        /// </summary>
        private const double TOL = 1e-5f;

        //-------------------------------------PROPERTIES-------------------------------------//

        /// <summary>
        /// 离线段起始点最近的顶点
        /// </summary>
        public Vertex StartVertex { get; private set; }

        /// <summary>
        /// 离线段结束点最近的顶点
        /// </summary>
        public Vertex EndVertex { get; private set; }

        /// <summary>
        /// 线段起始点位置
        /// </summary>
        public Vector3Double StartPosition { get; private set; }

        /// <summary>
        /// 线段结束点位置
        /// </summary>
        public Vector3Double EndPosition { get; private set; }

        /// <summary>
        /// 线段起始点到平面顶点的距离
        /// </summary>
        public double StartDistance { get; private set; }

        /// <summary>
        /// 线段结束点到平面顶点的距离
        /// </summary>
        public double EndDistance { get; private set; }

        /// <summary>
        /// 相对于平面的起始点状态
        /// </summary>
        public int StartType { get; private set; }
        
        /// <summary>
        /// 相对于平面的中间状态
        /// </summary>
        public int IntermediateType { get; private set; }

        /// <summary>
        /// 相对于平面的结束点状态
        /// </summary>
        public int EndType { get; private set; }

        /// <summary>
        /// 指定结束点的次数
        /// </summary>
        public int NumEndsSet { get; private set; }

        //---------------------------------CONSTRUCTORS---------------------------------//

        /// <summary>
        /// Constructs a Segment based on elements obtained from the two planes relations
        /// </summary>
        /// <param name="line">resulting from the two planes intersection</param>
        /// <param name="face">face that intersects with the plane</param>
        /// <param name="sign1">position of the face vertex1 relative to the plane (-1 behind, 1 front, 0 on)</param>
        /// <param name="sign2">position of the face vertex1 relative to the plane (-1 behind, 1 front, 0 on)</param>
        /// <param name="sign3">position of the face vertex1 relative to the plane (-1 behind, 1 front, 0 on)</param>
        public Segment(Line line, Face face, int sign1, int sign2, int sign3)
        {
            this.line = line;

            NumEndsSet = default;
            StartVertex = default;
            EndVertex = default;
            StartPosition = default;
            EndPosition = default;
            StartDistance = default;
            EndDistance = default;
            StartType = default;
            EndType = default;
            IntermediateType = default;

            if (sign1 == 0)  // 点1 在平面上，是交线的端点
            {
                SetVertex(face.v1);
                // 其他两个点在平面同一侧
                if (sign2 == sign3) { SetVertex(face.v1); }
            }
            if (sign2 == 0)  // 点2 在平面上，是交线的端点
            {
                SetVertex(face.v2);
                //other vertices on the same side - VERTEX-VERTEX VERTEX
                if (sign1 == sign3) { SetVertex(face.v2); }
            }
            if (sign3 == 0)  // 点3 在平面上，是交线的端点
            {
                SetVertex(face.v3);
                //other vertices on the same side - VERTEX-VERTEX VERTEX
                if (sign1 == sign2) { SetVertex(face.v3); }
            }

            //There are undefined ends - one or more edges cut the planes intersection line
            // 有未定义的端点：至少一条边切割交线
            if (NumEndsSet != 2)
            {
                //EDGE is an end
                if ((sign1 == 1 && sign2 == -1) || (sign1 == -1 && sign2 == 1)) { SetEdge(face.v1, face.v2); }
                //EDGE is an end
                if ((sign2 == 1 && sign3 == -1) || (sign2 == -1 && sign3 == 1)) { SetEdge(face.v2, face.v3); }
                //EDGE is an end
                if ((sign3 == 1 && sign1 == -1) || (sign3 == -1 && sign1 == 1)) { SetEdge(face.v3, face.v1); }
            }
        }

        //------------------------------------OTHERS------------------------------------//

        /// <summary>
        /// 两条线段相交
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public bool Intersect(Segment segment)
        {
            return !(EndDistance < segment.StartDistance + TOL || segment.EndDistance < StartDistance + TOL);
        }

        //---------------------------------PRIVATES-------------------------------------//

        /**
        * Sets an end as vertex (starting point if none end were defined, ending point otherwise)
        * 
        * @param vertex the vertex that is an segment end 
        * @return false if all the ends were already defined, true otherwise
        */
        private bool SetVertex(Vertex vertex)
        {
            //none end were defined - define starting point as VERTEX
            if (NumEndsSet == 0)
            {
                StartVertex = vertex;
                StartType = VERTEX;
                StartDistance = line.ComputePointToPointDistance(vertex.Position);
                StartPosition = StartVertex.Position;
                NumEndsSet++;
                return true;
            }
            //starting point were defined - define ending point as VERTEX
            if (NumEndsSet == 1)
            {
                EndVertex = vertex;
                EndType = VERTEX;
                EndDistance = line.ComputePointToPointDistance(vertex.Position);
                EndPosition = EndVertex.Position;
                NumEndsSet++;

                //defining middle based on the starting point
                //VERTEX-VERTEX-VERTEX
                if (StartVertex.Equals(EndVertex)) { IntermediateType = VERTEX; }
                //VERTEX-EDGE-VERTEX
                else if (StartType == VERTEX) { IntermediateType = EDGE; }

                //the ending point distance should be smaller than  starting point distance 
                if (StartDistance > EndDistance) { SwapEnds(); }

                return true;
            }
            else { return false; }
        }

        /// <summary>
        /// 设交线的结束点在平面的边上；
        /// 如果没有指定过结束点，就指定起点在边上，否则指定结束点在边上
        /// </summary>
        /// <param name="vertex1">交线线段的其中一点</param>
        /// <param name="vertex2">交线线段的其中一点</param>
        /// <returns></returns>
        private bool SetEdge(Vertex vertex1, Vertex vertex2)
        {
            Vector3Double point1 = vertex1.Position;
            Vector3Double point2 = vertex2.Position;
            Vector3Double edgeDirection = new Vector3Double(point2.x - point1.x, point2.y - point1.y, point2.z - point1.z);
            Line edgeLine = new Line(edgeDirection, point1);

            if (NumEndsSet == 0)
            {
                StartVertex = vertex1;
                StartType = EDGE;
                StartPosition = line.ComputeLineIntersection(edgeLine);
                StartDistance = line.ComputePointToPointDistance(StartPosition);
                IntermediateType = FACE;
                NumEndsSet++;
                return true;
            }
            else if (NumEndsSet == 1)
            {
                EndVertex = vertex1;
                EndType = EDGE;
                EndPosition = line.ComputeLineIntersection(edgeLine);
                EndDistance = line.ComputePointToPointDistance(EndPosition);
                IntermediateType = FACE;
                NumEndsSet++;

                //the ending point distance should be smaller than  starting point distance 
                if (StartDistance > EndDistance) { SwapEnds(); }

                return true;
            }
            else { return false; }
        }

        /** Swaps the starting point and the ending point */
        private void SwapEnds()
        {
            double distTemp = StartDistance;
            StartDistance = EndDistance;
            EndDistance = distTemp;

            int typeTemp = StartType;
            StartType = EndType;
            EndType = typeTemp;

            Vertex vertexTemp = StartVertex;
            StartVertex = EndVertex;
            EndVertex = vertexTemp;

            Vector3Double posTemp = StartPosition;
            StartPosition = EndPosition;
            EndPosition = posTemp;
        }
    }
}

