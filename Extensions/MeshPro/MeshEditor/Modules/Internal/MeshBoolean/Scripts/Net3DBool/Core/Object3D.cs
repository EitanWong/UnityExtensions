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
using Net3dBool.CommonTool;

namespace Net3dBool
{
    /// <summary>
    /// Data structure about a 3d solid to apply boolean operations in it.
    /// 用于两个 3D 物体间布尔运算的数据结构。
    /// Tipically, two 'Object3d' objects are created to apply boolean operation. 
    /// 典型地，构造两个 <see cref="Object3D"/> 实例用于布尔运算。
    /// The methods splitFaces() and classifyFaces() are called in this sequence for both objects,always using the other one as parameter.
    /// always using the other one as parameter.Then the faces from both objects are collected
    /// according their status.
    /// 对两个实例，<see cref="SplitFace(int, Segment, Segment)"/> 和 <see cref="ClassifyFaces(Object3D)"/> 都按顺序调用，
    /// 以另一个物体为参照。最后收集相互干涉（交叉、分割）后得到的面片。
    /// </summary>
    public class Object3D : IDisposable
    {
        /// <summary>
        /// tolerance value to test equalities
        /// </summary>
        private const double EqualityTolerance = 1e-5f;
        /// <summary>
        /// object representing the solid extremes
        /// </summary>
        private Bound m_bound;
        /// <summary>
        /// solid faces
        /// </summary>
        private CollectionPool<Face>.ListCell m_faces;
        /// <summary>
        /// solid vertices
        /// </summary>
        private CollectionPool<Vertex>.ListCell m_vertices;

        /// <summary>
        /// Constructs a Object3d object based on a solid file.
        /// </summary>
        /// <param name="solid">solid used to construct the Object3d object</param>
        public Object3D(Solid solid)
        {
            var verticesPoints = CollectionPool<Vector3Double>.ListCell.Create();
            solid.GetVertices(verticesPoints);

            var indices = CollectionPool<int>.ListCell.Create();
            solid.GetTriangles(indices);

            using (var vsCache = CollectionPool<Vertex>.ListCell.Create())
            {
                //create vertices
                m_vertices = CollectionPool<Vertex>.ListCell.Create();
                for (int i = 0; i < verticesPoints.Count; i++)
                {
                    var vertex = AddVertex(verticesPoints[i], Status.UNKNOWN);
                    vsCache.Add(vertex);
                }

                //create faces
                m_faces = CollectionPool<Face>.ListCell.Create();
                for (int i = 0; i < indices.Count; i += 3)
                {
                    var v1 = vsCache[indices[i]];
                    var v2 = vsCache[indices[i + 1]];
                    var v3 = vsCache[indices[i + 2]];
                    AddFace(v1, v2, v3);
                }
            }

            //create bound
            m_bound = new Bound(verticesPoints);
            indices.Dispose();
            verticesPoints.Dispose();
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        private Object3D() { }

        /// <summary>
        /// 划分自身平面为在其他对象的内部、外部或边界上
        /// </summary>
        /// <param name="otherObject">用来比较的 3D 物件</param>
        public void ClassifyFaces(Object3D otherObject)
        {
            Face face;
            for (int i = 0; i < GetNumFaces(); i++)  // 指定邻接点
            {
                face = GetFace(i);
                face.v1.AddAdjacentVertex(face.v2);
                face.v1.AddAdjacentVertex(face.v3);
                face.v2.AddAdjacentVertex(face.v1);
                face.v2.AddAdjacentVertex(face.v3);
                face.v3.AddAdjacentVertex(face.v1);
                face.v3.AddAdjacentVertex(face.v2);
            }
            for (int i = 0; i < GetNumFaces(); i++)  // 采样每个面
            {
                face = GetFace(i);
                if (!face.SimpleClassify())  // 简单分类没有分类的顶点
                {
                    face.RayTraceClassify(otherObject);  // 用射线采样分类
                    // 标记顶点
                    if (face.v1.Status == Status.UNKNOWN) { face.v1.Mark(face.Status); }
                    if (face.v2.Status == Status.UNKNOWN) { face.v2.Mark(face.Status); }
                    if (face.v3.Status == Status.UNKNOWN) { face.v3.Mark(face.Status); }
                }
            }
        }

        /// <summary>
        /// 返回指定序号的面
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal Face GetFace(int index)
        {
            if (index < 0 || index >= m_faces.Count) { return default; }
            return m_faces[index];
        }

        /// <summary>
        /// 返回物件的面数
        /// </summary>
        /// <returns></returns>
        public int GetNumFaces() { return m_faces.Count; }

        /// <summary>
        /// 反转被标记为<see cref="Status.INSIDE"/>的面，使其法线反向。通常用于网格求补集时“减去”的部分
        /// </summary>
        public void InvertInsideFaces()
        {
            for (int i = 0; i < GetNumFaces(); i++)
            {
                var face = GetFace(i);
                if (face.Status == Status.INSIDE) { face.Invert(); }
            }
        }

        /// <summary>
        /// 分割平面，按照 3D 物件上平面之间的交线切开
        /// </summary>
        /// <param name="obj"></param>
        public void SplitFaces(Object3D obj)
        {
            if (!m_bound.Overlap(obj.m_bound)) { return; }
            for (int i = 0; i < GetNumFaces(); i++)
            {
                var face1 = GetFace(i);
                if (!face1.Bound.Overlap(obj.m_bound)) { continue; }
                for (int j = 0; j < obj.GetNumFaces(); j++)
                {
                    var face2 = obj.GetFace(j);
                    if (!face1.Bound.Overlap(face2.Bound)) { continue; }

                    // 第一步：处理两个多边形相交的情况
                    // 可能的结果：相交，不相交，共面

                    // face1 上的点到 face2 的距离，为正时表示点在正面一侧
                    var distFace1Vert1 = ComputeDistance(face1.v1, face2);
                    var distFace1Vert2 = ComputeDistance(face1.v2, face2);
                    var distFace1Vert3 = ComputeDistance(face1.v3, face2);

                    // 以下表示点到面距离的符号
                    var signFace1Vert1 = distFace1Vert1 > EqualityTolerance ? 1 : (distFace1Vert1 < -EqualityTolerance ? -1 : 0);
                    var signFace1Vert2 = distFace1Vert2 > EqualityTolerance ? 1 : (distFace1Vert2 < -EqualityTolerance ? -1 : 0);
                    var signFace1Vert3 = distFace1Vert3 > EqualityTolerance ? 1 : (distFace1Vert3 < -EqualityTolerance ? -1 : 0);

                    // 全为零：共面；全为正或者全为负：不相交
                    if (signFace1Vert1 == signFace1Vert2 && signFace1Vert2 == signFace1Vert3) { continue; }

                    // face2 上的点到 face1 的距离
                    var distFace2Vert1 = ComputeDistance(face2.v1, face1);
                    var distFace2Vert2 = ComputeDistance(face2.v2, face1);
                    var distFace2Vert3 = ComputeDistance(face2.v3, face1);

                    // 以下表示点到面距离的符号
                    var signFace2Vert1 = distFace2Vert1 > EqualityTolerance ? 1 : (distFace2Vert1 < -EqualityTolerance ? -1 : 0);
                    var signFace2Vert2 = distFace2Vert2 > EqualityTolerance ? 1 : (distFace2Vert2 < -EqualityTolerance ? -1 : 0);
                    var signFace2Vert3 = distFace2Vert3 > EqualityTolerance ? 1 : (distFace2Vert3 < -EqualityTolerance ? -1 : 0);

                    // 全为零：共面；全为正或者全为负：不相交
                    if (signFace2Vert1 == signFace2Vert2 && signFace2Vert2 == signFace2Vert3) { continue; }

                    var line = new Line(face1, face2);  // 两平面的交线
                    // face1 在 face2 所在平面上的相交线段
                    var segment1 = new Segment(line, face1, signFace1Vert1, signFace1Vert2, signFace1Vert3);
                    // face2 在 face1 所在平面上的相交线段
                    var segment2 = new Segment(line, face2, signFace2Vert1, signFace2Vert2, signFace2Vert3);

                    if (!segment1.Intersect(segment2)) { continue; }
                    //if the two segments intersect...

                    // 第二步：分离共面的多边形
                    SplitFace(i, segment1, segment2);

                    //if the face in the position isn't the same, there was a break
                    if (face1 == GetFace(i)) { continue; }

                    //if the generated solid is equal the origin...
                    int lastFaceIndex = GetNumFaces() - 1;
                    if (face1.Equals(GetFace(lastFaceIndex)) && i != lastFaceIndex)
                    {
                        //return it to its position and jump it
                        m_faces.RemoveAt(lastFaceIndex);
                        m_faces.Insert(i, face1);
                    }
                    //else: test next face
                    else
                    {
                        i--;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Method used to add a face properly for internal methods
        /// </summary>
        /// <param name="v1">a face vertex</param>
        /// <param name="v2">a face vertex</param>
        /// <param name="v3">a face vertex</param>
        /// <returns></returns>
        private Face AddFace(Vertex v1, Vertex v2, Vertex v3)
        {
            if (v1.Equals(v2) || v1.Equals(v3) || v2.Equals(v3)) { return default; }
            if (area(v1, v2, v3) > EqualityTolerance)
            {
                Face face = Face.GetInstance(v1, v2, v3);
                m_faces.Add(face);
                return face;
            }
            return default;

            double area(Vector3Double a, Vector3Double b, Vector3Double c) =>
                Vector3Double.Cross(b - a, c - a).magnitude * 0.5;
        }

        /// <summary>
        /// Method used to add a vertex properly for internal methods
        /// </summary>
        /// <param name="pos">vertex position</param>
        /// <param name="status">vertex status</param>
        /// <returns>The vertex inserted (if a similar vertex already exists, this is returned).</returns>
        private Vertex AddVertex(Vector3Double pos, Status status)
        {
            //if already there is an equal vertex, it is not inserted
            for (int i = 0; i < m_vertices.Count; i++)
            {
                if (pos.EqualsTol(m_vertices[i].Position, Vertex.EqualityTolerance))
                {
                    m_vertices[i].Status = status;
                    return m_vertices[i];
                }
            }
            var vertex = Vertex.GetInstane(pos, status);
            m_vertices.Add(vertex);
            return vertex;
        }

        /// <summary>
        /// Face breaker for FACE-FACE-FACE
        /// </summary>
        /// <param name="facePos">face position on the faces array</param>
        /// <param name="newPos1">new vertex position</param>
        /// <param name="newPos2">new vertex position</param>
        /// <param name="linedVertex">linedVertex what vertex is more lined with the interersection found</param>
        private void BreakFaceInFive(int facePos, Vector3Double newPos1, Vector3Double newPos2, int linedVertex)
        {
            Face face = m_faces[facePos];
            m_faces.RemoveAt(facePos);

            Vertex vertex1 = AddVertex(newPos1, Status.BOUNDARY);
            Vertex vertex2 = AddVertex(newPos2, Status.BOUNDARY);

            if (linedVertex == 1)
            {
                AddFace(face.v2, face.v3, vertex1);
                AddFace(face.v2, vertex1, vertex2);
                AddFace(face.v3, vertex2, vertex1);
                AddFace(face.v2, vertex2, face.v1);
                AddFace(face.v3, face.v1, vertex2);
            }
            else if (linedVertex == 2)
            {
                AddFace(face.v3, face.v1, vertex1);
                AddFace(face.v3, vertex1, vertex2);
                AddFace(face.v1, vertex2, vertex1);
                AddFace(face.v3, vertex2, face.v2);
                AddFace(face.v1, face.v2, vertex2);
            }
            else
            {
                AddFace(face.v1, face.v2, vertex1);
                AddFace(face.v1, vertex1, vertex2);
                AddFace(face.v2, vertex2, vertex1);
                AddFace(face.v1, vertex2, face.v3);
                AddFace(face.v2, face.v3, vertex2);
            }
            face.Dispose();
        }

        /// <summary>
        /// Face breaker for EDGE-FACE-FACE / FACE-FACE-EDGE
        /// </summary>
        /// <param name="facePos">face position on the faces array</param>
        /// <param name="newPos1">new vertex position</param>
        /// <param name="newPos2">new vertex position</param>
        /// <param name="endVertex">vertex used for the split</param>
        private void BreakFaceInFour(int facePos, Vector3Double newPos1, Vector3Double newPos2, Vertex endVertex)
        {
            Face face = m_faces[facePos];
            m_faces.RemoveAt(facePos);

            Vertex vertex1 = AddVertex(newPos1, Status.BOUNDARY);
            Vertex vertex2 = AddVertex(newPos2, Status.BOUNDARY);

            if (endVertex.Equals(face.v1))
            {
                AddFace(face.v1, vertex1, vertex2);
                AddFace(vertex1, face.v2, vertex2);
                AddFace(face.v2, face.v3, vertex2);
                AddFace(face.v3, face.v1, vertex2);
            }
            else if (endVertex.Equals(face.v2))
            {
                AddFace(face.v2, vertex1, vertex2);
                AddFace(vertex1, face.v3, vertex2);
                AddFace(face.v3, face.v1, vertex2);
                AddFace(face.v1, face.v2, vertex2);
            }
            else
            {
                AddFace(face.v3, vertex1, vertex2);
                AddFace(vertex1, face.v1, vertex2);
                AddFace(face.v1, face.v2, vertex2);
                AddFace(face.v2, face.v3, vertex2);
            }
            face.Dispose();
        }

        /// <summary>
        /// Face breaker for EDGE-EDGE-EDGE
        /// </summary>
        /// <param name="facePos">face position on the faces array</param>
        /// <param name="newPos1">new vertex position</param>
        /// <param name="newPos2">new vertex position</param>
        /// <param name="splitEdge">edge that will be split</param>
        private void BreakFaceInThree(int facePos, Vector3Double newPos1, Vector3Double newPos2, int splitEdge)
        {
            Face face = m_faces[facePos];
            m_faces.RemoveAt(facePos);

            Vertex vertex1 = AddVertex(newPos1, Status.BOUNDARY);
            Vertex vertex2 = AddVertex(newPos2, Status.BOUNDARY);

            if (splitEdge == 1)
            {
                AddFace(face.v1, vertex1, face.v3);
                AddFace(vertex1, vertex2, face.v3);
                AddFace(vertex2, face.v2, face.v3);
            }
            else if (splitEdge == 2)
            {
                AddFace(face.v2, vertex1, face.v1);
                AddFace(vertex1, vertex2, face.v1);
                AddFace(vertex2, face.v3, face.v1);
            }
            else
            {
                AddFace(face.v3, vertex1, face.v2);
                AddFace(vertex1, vertex2, face.v2);
                AddFace(vertex2, face.v1, face.v2);
            }
            face.Dispose();
        }

        /// <summary>
        /// Face breaker for VERTEX-FACE-FACE / FACE-FACE-VERTEX
        /// </summary>
        /// <param name="facePos">face position on the faces array</param>
        /// <param name="newPos">new vertex position</param>
        /// <param name="endVertex">vertex used for the split</param>
        private void BreakFaceInThree(int facePos, Vector3Double newPos, Vertex endVertex)
        {
            Face face = m_faces[facePos];
            m_faces.RemoveAt(facePos);

            Vertex vertex = AddVertex(newPos, Status.BOUNDARY);

            if (endVertex.Equals(face.v1))
            {
                AddFace(face.v1, face.v2, vertex);
                AddFace(face.v2, face.v3, vertex);
                AddFace(face.v3, face.v1, vertex);
            }
            else if (endVertex.Equals(face.v2))
            {
                AddFace(face.v2, face.v3, vertex);
                AddFace(face.v3, face.v1, vertex);
                AddFace(face.v1, face.v2, vertex);
            }
            else
            {
                AddFace(face.v3, face.v1, vertex);
                AddFace(face.v1, face.v2, vertex);
                AddFace(face.v2, face.v3, vertex);
            }
            face.Dispose();
        }

        /// <summary>
        /// Face breaker for EDGE-FACE-EDGE
        /// </summary>
        /// <param name="facePos">face position on the faces array</param>
        /// <param name="newPos1">new vertex position</param>
        /// <param name="newPos2">new vertex position</param>
        /// <param name="startVertex">vertex used for the new faces creation</param>
        /// <param name="endVertex">vertex used for the new faces creation</param>
        private void BreakFaceInThree(int facePos,
            Vector3Double newPos1, Vector3Double newPos2,
            Vertex startVertex, Vertex endVertex)
        {
            Face face = m_faces[facePos];
            m_faces.RemoveAt(facePos);

            Vertex vertex1 = AddVertex(newPos1, Status.BOUNDARY);
            Vertex vertex2 = AddVertex(newPos2, Status.BOUNDARY);

            if (startVertex.Equals(face.v1) && endVertex.Equals(face.v2))
            {
                AddFace(face.v1, vertex1, vertex2);
                AddFace(face.v1, vertex2, face.v3);
                AddFace(vertex1, face.v2, vertex2);
            }
            else if (startVertex.Equals(face.v2) && endVertex.Equals(face.v1))
            {
                AddFace(face.v1, vertex2, vertex1);
                AddFace(face.v1, vertex1, face.v3);
                AddFace(vertex2, face.v2, vertex1);
            }
            else if (startVertex.Equals(face.v2) && endVertex.Equals(face.v3))
            {
                AddFace(face.v2, vertex1, vertex2);
                AddFace(face.v2, vertex2, face.v1);
                AddFace(vertex1, face.v3, vertex2);
            }
            else if (startVertex.Equals(face.v3) && endVertex.Equals(face.v2))
            {
                AddFace(face.v2, vertex2, vertex1);
                AddFace(face.v2, vertex1, face.v1);
                AddFace(vertex2, face.v3, vertex1);
            }
            else if (startVertex.Equals(face.v3) && endVertex.Equals(face.v1))
            {
                AddFace(face.v3, vertex1, vertex2);
                AddFace(face.v3, vertex2, face.v2);
                AddFace(vertex1, face.v1, vertex2);
            }
            else
            {
                AddFace(face.v3, vertex2, vertex1);
                AddFace(face.v3, vertex1, face.v2);
                AddFace(vertex2, face.v1, vertex1);
            }
            face.Dispose();
        }

        /// <summary>
        /// Face breaker for FACE-FACE-FACE (a point only)
        /// </summary>
        /// <param name="facePos">face position on the faces array</param>
        /// <param name="newPos">new vertex position</param>
        private void BreakFaceInThree(int facePos, Vector3Double newPos)
        {
            Face face = m_faces[facePos];
            m_faces.RemoveAt(facePos);

            Vertex vertex = AddVertex(newPos, Status.BOUNDARY);

            AddFace(face.v1, face.v2, vertex);
            AddFace(face.v2, face.v3, vertex);
            AddFace(face.v3, face.v1, vertex);
            face.Dispose();
        }

        /// <summary>
        /// Face breaker for VERTEX-EDGE-EDGE / EDGE-EDGE-VERTEX
        /// </summary>
        /// <param name="facePos">face position on the faces array</param>
        /// <param name="newPos">new vertex position</param>
        /// <param name="splitEdge">edge that will be split</param>
        private void BreakFaceInTwo(int facePos, Vector3Double newPos, int splitEdge)
        {
            Face face = m_faces[facePos];
            m_faces.RemoveAt(facePos);

            Vertex vertex = AddVertex(newPos, Status.BOUNDARY);

            if (splitEdge == 1)
            {
                AddFace(face.v1, vertex, face.v3);
                AddFace(vertex, face.v2, face.v3);
            }
            else if (splitEdge == 2)
            {
                AddFace(face.v2, vertex, face.v1);
                AddFace(vertex, face.v3, face.v1);
            }
            else
            {
                AddFace(face.v3, vertex, face.v2);
                AddFace(vertex, face.v1, face.v2);
            }
            face.Dispose();
        }

        /// <summary>
        /// Face breaker for VERTEX-FACE-EDGE / EDGE-FACE-VERTEX
        /// </summary>
        /// <param name="facePos">face position on the faces array</param>
        /// <param name="newPos">new vertex position</param>
        /// <param name="endVertex">vertex used for splitting</param>
        private void BreakFaceInTwo(int facePos, Vector3Double newPos, Vertex endVertex)
        {
            Face face = m_faces[facePos];
            m_faces.RemoveAt(facePos);

            Vertex vertex = AddVertex(newPos, Status.BOUNDARY);

            if (endVertex.Equals(face.v1))
            {
                AddFace(face.v1, vertex, face.v3);
                AddFace(vertex, face.v2, face.v3);
            }
            else if (endVertex.Equals(face.v2))
            {
                AddFace(face.v2, vertex, face.v1);
                AddFace(vertex, face.v3, face.v1);
            }
            else
            {
                AddFace(face.v3, vertex, face.v2);
                AddFace(vertex, face.v1, face.v2);
            }
            face.Dispose();
        }

        /// <summary>
        /// 计算从顶点到平面的最近距离
        /// </summary>
        private double ComputeDistance(Vertex vertex, Face face)
        {
            Vector3Double normal = face.PlaneNormal;
            return Vector3Double.Dot(normal, vertex.Position) - Vector3Double.Dot(normal, face.v1.Position);
        }

        /// <summary>
        /// Split an individual face
        /// </summary>
        /// <param name="facePos">face position on the array of faces</param>
        /// <param name="segment1">segment representing the intersection of the face with the plane</param>
        /// <param name="segment2">segment representing the intersection of other face with the plane of the current face plane</param>
        private void SplitFace(int facePos, Segment segment1, Segment segment2)
        {
            Vector3Double startPos, endPos;

            int startType;
            double startDist;
            //starting point: deeper starting point
            if (segment2.StartDistance > segment1.StartDistance + EqualityTolerance)
            {
                startDist = segment2.StartDistance;
                startType = segment1.IntermediateType;
                startPos = segment2.StartPosition;
            }
            else
            {
                startDist = segment1.StartDistance;
                startType = segment1.StartType;
                startPos = segment1.StartPosition;
            }

            int endType;
            double endDist;
            //ending point: deepest ending point
            if (segment2.EndDistance < segment1.EndDistance - EqualityTolerance)
            {
                endDist = segment2.EndDistance;
                endType = segment1.IntermediateType;
                endPos = segment2.EndPosition;
            }
            else
            {
                endDist = segment1.EndDistance;
                endType = segment1.EndType;
                endPos = segment1.EndPosition;
            }
            int middleType = segment1.IntermediateType;

            Vertex startVertex = segment1.StartVertex;
            Vertex endVertex = segment1.EndVertex;
            //set vertex to BOUNDARY if it is start type
            if (startType == Segment.VERTEX) { startVertex.Status = Status.BOUNDARY; }

            //set vertex to BOUNDARY if it is end type
            if (endType == Segment.VERTEX) { endVertex.Status = Status.BOUNDARY; }

            Face face = GetFace(facePos);
            //VERTEX-_______-VERTEX
            if (startType == Segment.VERTEX && endType == Segment.VERTEX) { return; }
            //______-EDGE-______
            else if (middleType == Segment.EDGE)
            {
                //gets the edge
                int splitEdge = (startVertex == face.v1 && endVertex == face.v2) || (startVertex == face.v2 && endVertex == face.v1) ? 1 :
                                (startVertex == face.v2 && endVertex == face.v3) || (startVertex == face.v3 && endVertex == face.v2) ? 2 : 3;

                //VERTEX-EDGE-EDGE
                if (startType == Segment.VERTEX)
                {
                    BreakFaceInTwo(facePos, endPos, splitEdge);
                    return;
                }

                //EDGE-EDGE-VERTEX
                else if (endType == Segment.VERTEX)
                {
                    BreakFaceInTwo(facePos, startPos, splitEdge);
                    return;
                }

                // EDGE-EDGE-EDGE
                else if (startDist == endDist) { BreakFaceInTwo(facePos, endPos, splitEdge); }
                else
                {
                    if ((startVertex == face.v1 && endVertex == face.v2) ||
                        (startVertex == face.v2 && endVertex == face.v3) ||
                        (startVertex == face.v3 && endVertex == face.v1))
                    {
                        BreakFaceInThree(facePos, startPos, endPos, splitEdge);
                    }
                    else
                    {
                        BreakFaceInThree(facePos, endPos, startPos, splitEdge);
                    }
                }
                return;
            }

            //______-FACE-______

            //VERTEX-FACE-EDGE
            else if (startType == Segment.VERTEX && endType == Segment.EDGE)
            {
                BreakFaceInTwo(facePos, endPos, endVertex);
            }
            //EDGE-FACE-VERTEX
            else if (startType == Segment.EDGE && endType == Segment.VERTEX)
            {
                BreakFaceInTwo(facePos, startPos, startVertex);
            }
            //VERTEX-FACE-FACE
            else if (startType == Segment.VERTEX && endType == Segment.FACE)
            {
                BreakFaceInThree(facePos, endPos, startVertex);
            }
            //FACE-FACE-VERTEX
            else if (startType == Segment.FACE && endType == Segment.VERTEX)
            {
                BreakFaceInThree(facePos, startPos, endVertex);
            }
            //EDGE-FACE-EDGE
            else if (startType == Segment.EDGE && endType == Segment.EDGE)
            {
                BreakFaceInThree(facePos, startPos, endPos, startVertex, endVertex);
            }
            //EDGE-FACE-FACE
            else if (startType == Segment.EDGE && endType == Segment.FACE)
            {
                BreakFaceInFour(facePos, startPos, endPos, startVertex);
            }
            //FACE-FACE-EDGE
            else if (startType == Segment.FACE && endType == Segment.EDGE)
            {
                BreakFaceInFour(facePos, endPos, startPos, endVertex);
            }
            //FACE-FACE-FACE
            else if (startType == Segment.FACE && endType == Segment.FACE)
            {
                Vector3Double segmentVector = startPos - endPos;

                //if the intersection segment is a point only...
                if (Math.Abs(segmentVector.x) < EqualityTolerance &&
                    Math.Abs(segmentVector.y) < EqualityTolerance &&
                    Math.Abs(segmentVector.z) < EqualityTolerance)
                {
                    BreakFaceInThree(facePos, startPos);
                    return;
                }

                //gets the vertex more lined with the intersection segment
                var dot1 = Math.Abs(Vector3Double.Dot(segmentVector, (endPos - face.v1).normalized));
                var dot2 = Math.Abs(Vector3Double.Dot(segmentVector, (endPos - face.v2).normalized));
                var dot3 = Math.Abs(Vector3Double.Dot(segmentVector, (endPos - face.v3).normalized));

                int linedVertex = 3;
                Vector3Double linedVertexPos = face.v3;
                if (dot1 > dot2 && dot1 > dot3)
                {
                    linedVertex = 1;
                    linedVertexPos = face.v1;
                }
                else if (dot2 > dot3 && dot2 > dot1)
                {
                    linedVertex = 2;
                    linedVertexPos = face.v2;
                }

                // Now find which of the intersection endpoints is nearest to that vertex.
                if ((linedVertexPos - startPos).magnitude > (linedVertexPos - endPos).magnitude)
                {
                    BreakFaceInFive(facePos, startPos, endPos, linedVertex);
                }
                else
                {
                    BreakFaceInFive(facePos, endPos, startPos, linedVertex);
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
                    for (int i = 0; i < m_faces.Count; i++) { m_faces[i].Dispose(); }
                    m_faces.Dispose();
                    for (int i = 0; i < m_vertices.Count; i++) { m_vertices[i].Dispose(); }
                    m_vertices.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~Object3D() {
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