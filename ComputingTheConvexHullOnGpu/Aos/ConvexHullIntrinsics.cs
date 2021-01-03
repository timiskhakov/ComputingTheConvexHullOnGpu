using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using ComputingTheConvexHullOnGpu.Models;
using ComputingTheConvexHullOnGpu.Soa;

namespace ComputingTheConvexHullOnGpu.Aos
{
    public class ConvexHullIntrinsics : IConvexHull
    {
        private static readonly int BlockSize = Vector256<float>.Count;
        private static readonly Vector256<float> Zeros = Vector256.Create(0f);
        
        public HashSet<Point> QuickHull(Point[] points)
        {
            if (points.Length <= 2) throw new ArgumentException($"Too little points: {points.Length}, expected 3 or more");

            var soa = new Points(points.Length);
            var result = new HashSet<Point>();

            var left = points[0];
            var right = points[0];
            for (var i = 0; i < points.Length; i++)
            {
                soa.Xs[i] = points[i].X;
                soa.Ys[i] = points[i].Y;
                
                if (points[i].X < left.X) left = points[i];
                if (points[i].X > right.X) right = points[i];
            }

            FindHull(points, left, right, 1, result, ref soa);
            FindHull(points, left, right, -1, result, ref soa);

            return result;
        }

        // I'm not sure this is the best implementation
        private static unsafe void FindHull(
            Point[] points,
            Point p1,
            Point p2,
            float side,
            HashSet<Point> result,
            ref Points soa)
        {
            var maxIndex = -1;
            var maxDistance = 0f;

            var p1X = Vector256.Create(p1.X);
            var p1Y = Vector256.Create(p1.Y);
            var p2X = Vector256.Create(p2.X);
            var p2Y = Vector256.Create(p2.Y);

            var i = 0;
            fixed (float* pXs = soa.Xs)
            fixed (float* pYs = soa.Ys)
            {
                for (; i < soa.Xs.Length - BlockSize; i += BlockSize)
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

            for (; i < soa.Xs.Length; i++)
            {
                var distance = Distance(p1, p2, points[i]);
                if (side > 0 && distance > maxDistance || side < 0 && distance < maxDistance)
                {
                    maxIndex = i;
                    maxDistance = distance;
                }
            }

            if (maxIndex == -1) 
            { 
                result.Add(p1); 
                result.Add(p2); 
                return;
            }

            FindHull(points, points[maxIndex], p1, -maxDistance, result, ref soa); 
            FindHull(points, points[maxIndex], p2, maxDistance, result, ref soa);
        }

        private static float Distance(Point p1, Point p2, Point p) 
        { 
            return (p.Y - p1.Y) * (p2.X - p1.X) - (p2.Y - p1.Y) * (p.X - p1.X);
        }
    }
}