#region --- License ---

/*
Copyright (c) 2006 - 2008 The Open Toolkit library.

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

#endregion --- License ---

using System;
using System.Runtime.InteropServices;

namespace Net3dBool
{
    /// <summary>
    /// 双精度浮点数表示的 3D 向量
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3Double : IEquatable<Vector3Double>
    {
        #region Fields

        public double x;
        public double y;
        public double z;

        #endregion Fields

        #region Constructors
        /// <summary>
        /// 构造新的向量
        /// </summary>
        /// <param name="x">向量的 x 坐标</param>
        /// <param name="y">向量的 y 坐标</param>
        /// <param name="z">向量的 z 坐标</param>
        public Vector3Double(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// 从已有坐标构造新实例
        /// </summary>
        /// <param name="v">复制源</param>
        public Vector3Double(Vector3Double v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        /// <summary>
        /// 构造新的向量，数组长度不要短于 3
        /// </summary>
        /// <param name="doubleArray"></param>
        public Vector3Double(double[] doubleArray)
        {
            x = doubleArray[0];
            y = doubleArray[1];
            z = doubleArray[2];
        }
        #endregion Constructors

        #region Properties
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return x;
                    case 1: return y;
                    case 2: return z;
                    default: throw new IndexOutOfRangeException($"Get index of {GetType()} is out of range.");
                }
            }
            set
            {
                switch (index)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    case 2: z = value; break;
                    default: throw new IndexOutOfRangeException($"Set index of {GetType()} is out of range.");
                }
            }
        }

        public double MaxComponent { get { return Math.Max(x, Math.Max(y, z)); } }

        public double MinComponent { get { return Math.Min(x, Math.Min(y, z)); } }
        #endregion Properties

        #region Instance
        /// <summary>
        /// 返回向量长度（向量的模）
        /// </summary>
        public double magnitude { get { return Math.Sqrt(x * x + y * y + z * z); } }

        /// <summary>
        /// 返回向量长度的平方值
        /// </summary>
        public double sqrMagnitude { get { return x * x + y * y + z * z; } }

        /// <summary>
        /// 返回归一化后的向量作为副本
        /// </summary>
        /// <returns></returns>
        public Vector3Double normalized
        {
            get
            {
                Vector3Double temp = this;
                temp.Normalize();
                return temp;
            }
        }

        /// <summary>
        /// 归一化向量
        /// </summary>
        public void Normalize()
        {
            double scale = 1 / magnitude;
            x *= scale;
            y *= scale;
            z *= scale;
        }

        public double[] ToArray() { return new double[] { x, y, z }; }
        #endregion Instance

        #region Static
        #region 常量

        /// <summary>
        /// 表示向量 (1, 0, 0)
        /// </summary>
        public static readonly Vector3Double right = new Vector3Double(1, 0, 0);

        /// <summary>
        /// 表示向量 (0, 1, 0)
        /// </summary>
        public static readonly Vector3Double up = new Vector3Double(0, 1, 0);

        /// <summary>
        /// 表示向量 (0, 0, 1)
        /// </summary>
        public static readonly Vector3Double forward = new Vector3Double(0, 0, 1);

        /// <summary>
        /// 表示向量 (0, 0, 0)
        /// </summary>
        public static readonly Vector3Double zero = new Vector3Double(0, 0, 0);

        /// <summary>
        /// 表示向量 (1, 1, 1)
        /// </summary>
        public static readonly Vector3Double One = new Vector3Double(1, 1, 1);

        /// <summary>
        /// 表示成员均为正数极大值的向量
        /// </summary>
        public static readonly Vector3Double PositiveInfinity = new Vector3Double(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);

        /// <summary>
        /// 表示成员均为负数极大值的向量
        /// </summary>
        public static readonly Vector3Double NegativeInfinity = new Vector3Double(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);

        /// <summary>
        /// 表示结构体实例的字节大小
        /// </summary>
        public static readonly int SizeInBytes = Marshal.SizeOf(new Vector3Double());

        #endregion 常量
        #region 四则运算
        /// <summary>
        /// 向量相加
        /// </summary>
        /// <param name="a">Left operand.</param>
        /// <param name="b">Right operand.</param>
        /// <returns>Result of operation.</returns>
        public static Vector3Double Add(Vector3Double a, Vector3Double b)
        {
            return new Vector3Double(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        /// <summary>
        /// 向量相减
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <returns>Result of subtraction</returns>
        public static Vector3Double Subtract(Vector3Double a, Vector3Double b)
        {
            return new Vector3Double(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        /// <summary>
        /// 按比例放大向量
        /// </summary>
        /// <param name="vector">被放大的向量</param>
        /// <param name="scale">放大比例</param>
        public static Vector3Double Multiply(Vector3Double vector, double scale)
        {
            return new Vector3Double(vector.x * scale, vector.y * scale, vector.z * scale);
        }

        /// <summary>
        /// 按比例放大向量
        /// </summary>
        /// <param name="vector">被放大的向量</param>
        /// <param name="scale">各轴的放大比例</param>
        public static Vector3Double Multiply(Vector3Double vector, Vector3Double scale)
        {
            return new Vector3Double(vector.x * scale.x, vector.y * scale.y, vector.z * scale.z);
        }

        /// <summary>
        /// 向量除以实数
        /// </summary>
        public static Vector3Double Divide(Vector3Double vector, double scale)
        {
            return new Vector3Double(vector.x / scale, vector.y / scale, vector.z / scale);
        }

        /// <summary>
        /// 按比例缩小向量
        /// </summary>
        /// <param name="vector">被缩小的向量</param>
        /// <param name="scale">各轴的缩小比例</param>
        public static Vector3Double Divide(Vector3Double vector, Vector3Double scale)
        {
            return new Vector3Double(vector.x / scale.x, vector.y / scale.y, vector.z / scale.z);
        }
        #endregion 四则运算
        #region 最小、最大、夹紧
        /// <summary>
        /// 取两个向量的分量最小值
        /// </summary>
        public static Vector3Double ComponentMin(Vector3Double a, Vector3Double b)
        {
            a.x = a.x < b.x ? a.x : b.x;
            a.y = a.y < b.y ? a.y : b.y;
            a.z = a.z < b.z ? a.z : b.z;
            return a;
        }

        /// <summary>
        /// 取两个向量的分量最大值
        /// </summary>
        public static Vector3Double ComponentMax(Vector3Double a, Vector3Double b)
        {
            a.x = a.x > b.x ? a.x : b.x;
            a.y = a.y > b.y ? a.y : b.y;
            a.z = a.z > b.z ? a.z : b.z;
            return a;
        }

        /// <summary>
        /// 取两向量之间长度较小者
        /// </summary>
        public static Vector3Double Min(Vector3Double left, Vector3Double right)
        {
            return left.sqrMagnitude < right.sqrMagnitude ? left : right;
        }

        /// <summary>
        /// 取两向量之间长度较大者
        /// </summary>
        public static Vector3Double Max(Vector3Double left, Vector3Double right)
        {
            return left.sqrMagnitude >= right.sqrMagnitude ? left : right;
        }

        /// <summary>
        /// 将向量分量夹紧至最小值和最大值之间
        /// </summary>
        /// <param name="vec">输入向量</param>
        /// <param name="min">最小值分量</param>
        /// <param name="max">最大值分量</param>
        /// <returns>The clamped vector</returns>
        public static Vector3Double Clamp(Vector3Double vec, Vector3Double min, Vector3Double max)
        {
            vec.x = vec.x < min.x ? min.x : vec.x > max.x ? max.x : vec.x;
            vec.y = vec.y < min.y ? min.y : vec.y > max.y ? max.y : vec.y;
            vec.z = vec.z < min.z ? min.z : vec.z > max.z ? max.z : vec.z;
            return vec;
        }
        #endregion 最小、最大、夹紧
        #region 归一化，内积，共线
        /// <summary>
        /// 求归一化后的向量
        /// </summary>
        public static Vector3Double Normalize(Vector3Double vec)
        {
            double scale = 1 / vec.magnitude;
            vec.x *= scale;
            vec.y *= scale;
            vec.z *= scale;
            return vec;
        }

        /// <summary>
        /// 求向量内积
        /// </summary>
        public static double Dot(Vector3Double left, Vector3Double right)
        {
            return left.x * right.x + left.y * right.y + left.z * right.z;
        }

        /// <summary>
        /// 求向量外积
        /// </summary>
        /// <param name="left">左向量</param>
        /// <param name="right">右向量</param>
        /// <returns></returns>
        public static Vector3Double Cross(Vector3Double left, Vector3Double right)
        {
            return new Vector3Double(left.y * right.z - left.z * right.y,
                left.z * right.x - left.x * right.z,
                left.x * right.y - left.y * right.x);
        }

        /// <summary>
        /// 检查三点共线
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public static bool Collinear(Vector3Double a, Vector3Double b, Vector3Double c, double epsilon = 1e-5)
        {
            return Math.Abs(Cross(b - a, c - a).magnitude) < epsilon;
        }
        #endregion 归一化，内积，共线
        #region 插值

        /// <summary>
        /// Returns a new Vector that is the linear blend of the 2 given Vectors
        /// </summary>
        /// <param name="a">First input vector</param>
        /// <param name="b">Second input vector</param>
        /// <param name="blend">The blend factor. a when blend=0, b when blend=1.</param>
        /// <returns>a when blend=0, b when blend=1, and a linear combination otherwise</returns>
        public static Vector3Double Lerp(Vector3Double a, Vector3Double b, double blend)
        {
            a.x = blend * (b.x - a.x) + a.x;
            a.y = blend * (b.y - a.y) + a.y;
            a.z = blend * (b.z - a.z) + a.z;
            return a;
        }

        /// <summary>
        /// Returns a new Vector that is the linear blend of the 2 given Vectors
        /// </summary>
        /// <param name="a">First input vector</param>
        /// <param name="b">Second input vector</param>
        /// <param name="blend">The blend factor. a when blend=0, b when blend=1.</param>
        /// <param name="result">a when blend=0, b when blend=1, and a linear combination otherwise</param>
        public static void Lerp(ref Vector3Double a, ref Vector3Double b, double blend, out Vector3Double result)
        {
            result.x = blend * (b.x - a.x) + a.x;
            result.y = blend * (b.y - a.y) + a.y;
            result.z = blend * (b.z - a.z) + a.z;
        }

        /// <summary>
        /// 重心坐标下三个向量的插值
        /// </summary>
        /// <param name="a">u=v=0 时返回 a</param>
        /// <param name="b">u=1, v=0 时返回 b</param>
        /// <param name="c">u=0, v=1 时返回 c</param>
        /// <param name="u">第一个重心坐标</param>
        /// <param name="v">第二个重心坐标</param>
        /// <returns>u=v=0 时得到 a; u=1, v=0 时得到 b; u=0, v=1 时得到 c; 否则得到 a,b,c 的线性插值</returns>
        public static Vector3Double BaryCentric(Vector3Double a, Vector3Double b, Vector3Double c, double u, double v)
        {
            return a + u * (b - a) + v * (c - a);
        }
        #endregion 插值
        #region 夹角
        /// <summary>
        /// 计算两向量间的角度，表示为大小不超过 pi 的弧度
        /// </summary>
        public static double AngleInRadian(Vector3Double first, Vector3Double second)
        {
            return Math.Acos(Dot(first, second) / (first.magnitude * second.magnitude));
        }

        /// <summary>
        /// 计算两向量间的角度，结果不超过 180 度
        /// </summary>
        public static double Angle(Vector3Double first, Vector3Double second)
        {
            return Math.Acos(Dot(first, second) / (first.magnitude * second.magnitude)) * 180 / Math.PI;
        }
        #endregion 夹角
        #endregion Static

        #region Operators
        public static Vector3Double operator +(Vector3Double left, Vector3Double right)
        {
            left.x += right.x;
            left.y += right.y;
            left.z += right.z;
            return left;
        }

        public static Vector3Double operator -(Vector3Double left, Vector3Double right)
        {
            left.x -= right.x;
            left.y -= right.y;
            left.z -= right.z;
            return left;
        }

        public static Vector3Double operator -(Vector3Double vec)
        {
            vec.x = -vec.x;
            vec.y = -vec.y;
            vec.z = -vec.z;
            return vec;
        }

        public static Vector3Double operator *(Vector3Double vecA, Vector3Double vecB)
        {
            vecA.x *= vecB.x;
            vecA.y *= vecB.y;
            vecA.z *= vecB.z;
            return vecA;
        }

        public static Vector3Double operator *(Vector3Double vec, double scale)
        {
            vec.x *= scale;
            vec.y *= scale;
            vec.z *= scale;
            return vec;
        }

        public static Vector3Double operator *(double scale, Vector3Double vec)
        {
            vec.x *= scale;
            vec.y *= scale;
            vec.z *= scale;
            return vec;
        }

        public static Vector3Double operator /(double numerator, Vector3Double vec)
        {
            return new Vector3Double(numerator / vec.x, numerator / vec.y, numerator / vec.z);
        }

        public static Vector3Double operator /(Vector3Double vec, double scale)
        {
            double mult = 1 / scale;
            vec.x *= mult;
            vec.y *= mult;
            vec.z *= mult;
            return vec;
        }

        public static bool operator ==(Vector3Double left, Vector3Double right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3Double left, Vector3Double right)
        {
            return !left.Equals(right);
        }
        #endregion Operators

        #region Overrides
        public override string ToString() { return string.Format("({0}, {1}, {2})", x, y, z); }

        /// <summary>
        /// Returns the hashcode for this instance.
        /// </summary>
        /// <returns>A System.Int32 containing the unique hashcode for this instance.</returns>
        public override int GetHashCode() { return new { x, y, z }.GetHashCode(); }

        public override bool Equals(object obj) { return obj is Vector3Double && Equals((Vector3Double)obj); }

        public bool Equals(Vector3Double other) { return x == other.x && y == other.y && z == other.z; }

        /// <summary>
        /// 带公差的相等判断
        /// </summary>
        /// <param name="OtherVector">另一个值</param>
        /// <param name="tolerance">指定公差</param>
        /// <returns></returns>
        public bool Equals(Vector3Double OtherVector, double tolerance)
        {
            return x < OtherVector.x + tolerance && x > OtherVector.x - tolerance &&
                y < OtherVector.y + tolerance && y > OtherVector.y - tolerance &&
                z < OtherVector.z + tolerance && z > OtherVector.z - tolerance;
        }
        #endregion Overrides
    }
}