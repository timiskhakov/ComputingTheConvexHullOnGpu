using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using ComputingTheConvexHullOnGpu.Models;

namespace ComputingTheConvexHullOnGpu.Soa
{
    public static class ConvexHullIntrinsics
    {
        private static readonly int BlockSize = Vector256<float>.Count;
        private static readonly Vector256<float> Zeros = Vector256.Create(0f);
        
        public static HashSet<Point> QuickHull(Points points)
        {
            if (points.Xs.Length != points.Ys.Length) throw new ArgumentException($"Invalid {nameof(Points)} structure");
            if (points.Xs.Length <= 2) throw new ArgumentException($"Too little points: {points.Xs.Length}, expected 3 or more");

            var result = new HashSet<Point>();

            var left = (points.Xs[0], points.Ys[0]);
            var right = (points.Xs[0], points.Ys[0]);
            for (var i = 0; i < points.Xs.Length; i++)
            {
                if (points.Xs[i] < left.Item1)
                {
                    left.Item1 = points.Xs[i];
                    left.Item2 = points.Ys[i];
                }
                if (points.Xs[i] > right.Item1)
                {
                    right.Item1 = points.Xs[i];
                    right.Item2 = points.Ys[i];
                }
            }

            FindHull(in points, left.Item1, left.Item2, right.Item1, right.Item2, 1, result);
            FindHull(in points, left.Item1, left.Item2, right.Item1, right.Item2, -1, result);

            return result;
        }

        private static unsafe void FindHull(
            in Points points,
            float aX,
            float aY,
            float bX,
            float bY,
            float side,
            HashSet<Point> result)
        {
            var maxIndex = -1;
            var maxDistance = 0f;

            var p1X = Vector256.Create(aX);
            var p1Y = Vector256.Create(aY);
            var p2X = Vector256.Create(bX);
            var p2Y = Vector256.Create(bY);

            var i = 0;
            fixed (float* pXs = points.Xs)
            fixed (float* pYs = points.Ys)
            {
                for (; i < points.Xs.Length - BlockSize; i += BlockSize)
                {
                    var pX = Avx.LoadVector256(pXs + i);
                    var pY = Avx.LoadVector256(pYs + i);

                    // Compute side
                    var left = Avx.Multiply(Avx.Subtract(pY, p1Y), Avx.Subtract(p2X, p1X));
                    var right = Avx.Multiply(Avx.Subtract(p2Y, p1Y), Avx.Subtract(pX, p1X));
                    var distances = Avx.Subtract(left, right);

                    var compared = Avx.Compare(distances, Zeros, side > 0
                        ? FloatComparisonMode.UnorderedNotLessThanOrEqualSignaling
                        : FloatComparisonMode.UnorderedNotGreaterThanOrEqualSignaling);

                    var mask = Avx.MoveMask(compared);
                    while (mask > 0)
                    {
                        var position = (int) Bmi1.TrailingZeroCount((uint) mask);
                        var distance = distances.GetElement(position);
                        if (side > 0 && distance > maxDistance || side < 0 && distance < maxDistance)
                        {
                            maxIndex = i + position;
                            maxDistance = distance;
                        }

                        mask = (int) Bmi1.ResetLowestSetBit((uint) mask);
                    }
                }
            }

            for (; i < points.Xs.Length; i++)
            {
                var distance = Distance(aX, aY, bX, bY, points.Xs[i], points.Ys[i]);
                if (side > 0 && distance > maxDistance || side < 0 && distance < maxDistance)
                {
                    maxIndex = i;
                    maxDistance = distance;
                }
            }

            if (maxIndex == -1) 
            { 
                result.Add(new Point(aX, aY)); 
                result.Add(new Point(bX, bY)); 
                return;
            }

            FindHull(in points, points.Xs[maxIndex], points.Ys[maxIndex], aX, aY, -maxDistance, result); 
            FindHull(in points, points.Xs[maxIndex], points.Ys[maxIndex], bX, bY, maxDistance, result);
        }

        private static float Distance(float aX, float aY, float bX, float bY, float pX, float pY)
        {
            return (pY - aY) * (bX - aX) - (bY - aY) * (pX - aX); 
        }
    }
}