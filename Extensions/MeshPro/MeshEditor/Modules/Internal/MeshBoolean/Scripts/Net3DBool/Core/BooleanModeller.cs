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
using System.Collections.Generic;
using System.Linq;
using Net3dBool;
using Net3dBool.CommonTool;

namespace Extensions.MeshPro.MeshEditor.Modules.Internal.MeshBoolean.Scripts.Net3DBool.Core
{
    /// <summary>
    /// 为<see cref="Solid"/>实例应用集合运算的类；
    /// 在构造函数中提交两个<see cref="Solid"/>实例；
    /// 每个布尔运算的方法都会返回一个新的<see cref="Solid"/>实例存放运算结果；
    /// </summary>
    public class BooleanModeller : IDisposable
    {
        /** solid where boolean operations will be applied */
        private Object3D object1;
        private Object3D object2;

        //--------------------------------CONSTRUCTORS----------------------------------//

        /// <summary>
        /// 传入两个物体并初步划分表面，不会影响原物体
        /// </summary>
        /// <param name="solid1">布尔运算的第一个参数</param>
        /// <param name="solid2">布尔运算的第二个参数</param>
        public BooleanModeller(Solid solid1, Solid solid2)
        {
            //representation to apply boolean operations
            object1 = new Object3D(solid1);
            object2 = new Object3D(solid2);

            //split the faces so that none of them intercepts each other
            object1.SplitFaces(object2);
            object2.SplitFaces(object1);

            //classify faces as being inside or outside the other solid
            object1.ClassifyFaces(object2);
            object2.ClassifyFaces(object1);
        }

        private BooleanModeller() { }

        //-------------------------------BOOLEAN_OPERATIONS-----------------------------//

        /// <summary>
        /// 取补集
        /// </summary>
        /// <returns></returns>
        public Solid GetDifference()
        {
            object2.InvertInsideFaces();
            Solid result = ComposeSolid(Status.OUTSIDE, Status.OPPOSITE, Status.INSIDE);
            object2.InvertInsideFaces();  // 重置以取消副作用
            return result;
        }

        /// <summary>
        /// 取交集
        /// </summary>
        /// <returns></returns>
        public Solid GetIntersection()
        {
            return ComposeSolid(Status.INSIDE, Status.SAME, Status.INSIDE);
        }

        /// <summary>
        /// 取并集
        /// </summary>
        /// <returns></returns>
        public Solid GetUnion()
        {
            return ComposeSolid(Status.OUTSIDE, Status.SAME, Status.OUTSIDE);
        }

        //--------------------------PRIVATES--------------------------------------------//

        /// <summary>
        /// 基于面的状态和作为参数的物体生成新物体。
        /// 状态：<see cref="Status.INSIDE"/>, <see cref="Status.OUTSIDE"/>, 
        /// <see cref="Status.SAME"/>, <see cref="Status.OPPOSITE"/>
        /// </summary>
        /// <param name="faceStatus1">筛选第一个物体上的面</param>
        /// <param name="faceStatus2">筛选第一个物体上的面
        /// (与第二个物体共面时选取此状态的面)</param>
        /// <param name="faceStatus3">筛选第二个物体上的面</param>
        /// <returns></returns>
        private Solid ComposeSolid(Status faceStatus1, Status faceStatus2, Status faceStatus3)
        {
            using (var vertices = CollectionPool<Vertex>.ListCell.Create())
            {
                using (var indices = CollectionPool<int>.ListCell.Create())
                {
                    // group the elements of the two solids whose faces fit with the desired status
                    GroupObjectComponents(object1, vertices, indices, faceStatus1, faceStatus2);
                    GroupObjectComponents(object2, vertices, indices, faceStatus3, faceStatus3);

                    using (var verticesPos =
                        CollectionPool<Vector3Double>.ListCell.Create(from c in vertices select c.Position))
                    {
                        //returns the solid containing the grouped elements
                        return new Solid(verticesPos, indices);
                    }
                }
            }
        }

        /// <summary>
        /// 按特定条件选取物件中适合的三角面填充入容器
        /// </summary>
        /// <param name="obj">选取面的物体</param>
        /// <param name="vertices">存放选取的顶点</param>
        /// <param name="indices">存放选取的三角形序号</param>
        /// <param name="faceStatus1">第一个筛选条件</param>
        /// <param name="faceStatus2">第二个筛选条件</param>
        private void GroupObjectComponents(Object3D obj, List<Vertex> vertices, List<int> indices, Status faceStatus1, Status faceStatus2)
        {
            using (var faceVs = CollectionPool<Vertex>.ListCell.Create(3))
            {
                for (int faceCount = 0; faceCount < obj.GetNumFaces(); faceCount++)
                {
                    var face = obj.GetFace(faceCount);
                    if (face.Status == faceStatus1 || face.Status == faceStatus2)  // if the face status fits with the desired status...
                    {
                        faceVs[0] = face.v1;
                        faceVs[1] = face.v2;
                        faceVs[2] = face.v3;
                        for (int triIndex = 0; triIndex < 3; triIndex++)
                        {
                            if (vertices.Contains(faceVs[triIndex]))
                            {
                                indices.Add(vertices.IndexOf(faceVs[triIndex]));
                            }
                            else
                            {
                                indices.Add(vertices.Count);
                                vertices.Add(faceVs[triIndex]);
                            }
                        }
                    }
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
                    object1.Dispose();
                    object2.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~BooleanModeller() {
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
#endif