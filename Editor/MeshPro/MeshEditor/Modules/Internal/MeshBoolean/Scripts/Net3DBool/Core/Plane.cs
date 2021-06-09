/*
Copyright (c) 2014, Lars Brubaker
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies,
either expressed or implied, of the FreeBSD Project.
*/
using System;

namespace Net3dBool
{
    /// <summary>
    /// 用 法线方向 和 坐标系原点到平面距离 描述的平面
    /// </summary>
    public struct Plane : IEquatable<Plane>
    {
        /// <summary>
        /// 从原点到平面的距离？
        /// </summary>
        public double distanceToPlaneFromOrigin;
        public Vector3Double planeNormal;
        private const double TreatAsZero = 1e-5;

        public Plane(Vector3Double planeNormal, double distanceFromOrigin)
        {
            this.planeNormal = planeNormal.normalized;
            distanceToPlaneFromOrigin = distanceFromOrigin;
        }

        public Plane(Vector3Double point0, Vector3Double point1, Vector3Double point2)
        {
            planeNormal = Vector3Double.Cross(point1 - point0, point2 - point0).normalized;
            distanceToPlaneFromOrigin = Vector3Double.Dot(planeNormal, point0);
        }

        public Plane(Vector3Double planeNormal, Vector3Double pointOnPlane)
        {
            this.planeNormal = planeNormal.normalized;
            distanceToPlaneFromOrigin = Vector3Double.Dot(planeNormal, pointOnPlane);
        }

        public double GetDistanceFromPlane(Vector3Double positionToCheck)
        {
            var distanceToPointFromOrigin = Vector3Double.Dot(positionToCheck, planeNormal);
            return distanceToPointFromOrigin - distanceToPlaneFromOrigin;
        }

        public double GetDistanceToIntersection(Ray ray, out bool inFront)
        {
            inFront = false;
            var normalDotRayDirection = Vector3Double.Dot(planeNormal, ray.directionNormal);

            // the ray is parallel to the plane
            if (Math.Abs(normalDotRayDirection) < TreatAsZero) { return double.PositiveInfinity; }

            if (normalDotRayDirection < 0) { inFront = true; }
            return (distanceToPlaneFromOrigin - Vector3Double.Dot(planeNormal, ray.origin)) / normalDotRayDirection;
        }

        public double GetDistanceToIntersection(Vector3Double pointOnLine, Vector3Double lineDirection)
        {
            var normalDotRayDirection = Vector3Double.Dot(planeNormal, lineDirection);

            // the ray is parallel to the plane
            return Math.Abs(normalDotRayDirection) < TreatAsZero ? double.PositiveInfinity :
                (distanceToPlaneFromOrigin - Vector3Double.Dot(planeNormal, pointOnLine)) / normalDotRayDirection;
        }

        public bool RayHitPlane(Ray ray, out double distanceToHit, out bool hitFrontOfPlane)
        {
            distanceToHit = double.PositiveInfinity;
            hitFrontOfPlane = false;

            var normalDotRayDirection = Vector3Double.Dot(planeNormal, ray.directionNormal);
            //  the ray is parallel to the plane
            if (Math.Abs(normalDotRayDirection) < TreatAsZero) { return false; }

            if (normalDotRayDirection < 0) { hitFrontOfPlane = true; }

            var distanceToPlaneFromRayOrigin = distanceToPlaneFromOrigin - Vector3Double.Dot(planeNormal, ray.origin);

            bool originInFrontOfPlane = distanceToPlaneFromRayOrigin < 0;

            bool originAndHitAreOnSameSide = originInFrontOfPlane == hitFrontOfPlane;
            if (!originAndHitAreOnSameSide) { return false; }

            distanceToHit = distanceToPlaneFromRayOrigin / normalDotRayDirection;
            return true;
        }

        public bool LineHitPlane(Vector3Double start, Vector3Double end, out Vector3Double intersectionPosition)
        {
            var distanceToStartFromOrigin = Vector3Double.Dot(planeNormal, start);
            if (distanceToStartFromOrigin == 0)
            {
                intersectionPosition = start;
                return true;
            }

            var distanceToEndFromOrigin = Vector3Double.Dot(planeNormal, end);
            if (distanceToEndFromOrigin == 0)
            {
                intersectionPosition = end;
                return true;
            }

            if ((distanceToStartFromOrigin < 0 && distanceToEndFromOrigin > 0)
                || (distanceToStartFromOrigin > 0 && distanceToEndFromOrigin < 0))
            {
                Vector3Double direction = (end - start).normalized;

                var startDistanceFromPlane = distanceToStartFromOrigin - distanceToPlaneFromOrigin;
                var endDistanceFromPlane = distanceToEndFromOrigin - distanceToPlaneFromOrigin;
                var lengthAlongPlanNormal = endDistanceFromPlane - startDistanceFromPlane;

                var ratioToPlanFromStart = startDistanceFromPlane / lengthAlongPlanNormal;
                intersectionPosition = start + direction * ratioToPlanFromStart;

                return true;
            }

            intersectionPosition = Vector3Double.PositiveInfinity;
            return false;
        }

        public bool Equals(Plane plane)
        {
            return plane.distanceToPlaneFromOrigin == distanceToPlaneFromOrigin &&
                plane.planeNormal.Equals(planeNormal, TreatAsZero);
        }
        public static bool operator ==(Plane a, Plane b) => a.Equals(b);
        public static bool operator !=(Plane a, Plane b) => !a.Equals(b);
        public override bool Equals(object obj)
        {
            return (Plane)obj == this;
        }

        public override int GetHashCode()
        {
            return new { distanceToPlaneFromOrigin, planeNormal }.GetHashCode();
        }
    }
}