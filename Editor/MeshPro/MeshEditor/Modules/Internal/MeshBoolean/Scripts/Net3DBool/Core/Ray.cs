// Copyright 2006 Herre Kuijpers - <herre@xs4all.nl>
//
// This source file(s) may be redistributed, altered and customized
// by any means PROVIDING the authors name and all copyright
// notices remain intact.
// THIS SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED. USE IT AT YOUR OWN RISK. THE AUTHOR ACCEPTS NO
// LIABILITY FOR ANY DATA DAMAGE/LOSS THAT THIS PRODUCT MAY CAUSE.
//-----------------------------------------------------------------------
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
    [Flags]
    public enum IntersectionType { None = 0, FrontFace = 1, BackFace = 2, Both = FrontFace | BackFace };

    /// <summary>
    /// 用起点和方向定义的射线
    /// 完全没有在网格布尔计算中用到过
    /// </summary>
    public class Ray
    {
        public Vector3Double origin;
        public Vector3Double directionNormal;
        public float minDistanceToConsider;
        public float maxDistanceToConsider;
        public Vector3Double oneOverDirection;
        public bool isShadowRay;
        public IntersectionType intersectionType;

        public enum Sign { negative = 1, positive = 0 };

        public Sign[] sign = new Sign[3];

        public Ray(Vector3Double origin, Vector3Double directionNormal, float minDistanceToConsider = 0, float maxDistanceToConsider = float.PositiveInfinity, IntersectionType intersectionType = IntersectionType.FrontFace)
        {
            this.origin = origin;
            this.directionNormal = directionNormal;
            this.minDistanceToConsider = minDistanceToConsider;
            this.maxDistanceToConsider = maxDistanceToConsider;
            this.intersectionType = intersectionType;
            oneOverDirection = 1 / directionNormal;

            sign[0] = (oneOverDirection.x < 0) ? Sign.negative : Sign.positive;
            sign[1] = (oneOverDirection.y < 0) ? Sign.negative : Sign.positive;
            sign[2] = (oneOverDirection.z < 0) ? Sign.negative : Sign.positive;
        }

        public Ray(Ray rayToCopy)
        {
            origin = rayToCopy.origin;
            directionNormal = rayToCopy.directionNormal;
            minDistanceToConsider = rayToCopy.minDistanceToConsider;
            maxDistanceToConsider = rayToCopy.maxDistanceToConsider;
            oneOverDirection = rayToCopy.oneOverDirection;
            isShadowRay = rayToCopy.isShadowRay;
            intersectionType = rayToCopy.intersectionType;
            sign[0] = rayToCopy.sign[0];
            sign[1] = rayToCopy.sign[1];
            sign[2] = rayToCopy.sign[2];
        }
    }
}