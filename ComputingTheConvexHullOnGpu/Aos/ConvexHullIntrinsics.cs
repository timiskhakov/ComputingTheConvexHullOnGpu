using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using ComputingTheConvexHullOnGpu.Models;

namespace ComputingTheConvexHullOnGpu.Aos
{
    public class ConvexHullIntrinsics
    {
        private static readonly int BlockSize = Vector256<float>.Count;
        private static readonly Vector256<float> Zeros = Vector256.Create(0f);
        
        public HashSet<Point> QuickHull(Points points)
        {
            if (points.Xs.Length != points.Ys.Length) throw new ArgumentException($"Invalid {nameof(Points)} structure");
            if (points.Xs.Length <= 2) throw new ArgumentException($"Too little points: {points.Xs.Length}, expected 3 or more");

            var result = new HashSet<Point>();

            var left = new Point(points.Xs[0], points.Ys[0]);
            var right = new Point(points.Xs[0], points.Ys[0]);
            for (var i = 0; i < points.Xs.Length; i++)
            {
                if (points.Xs[i] < left.X) left = new Point(points.Xs[i], points.Ys[i]);
                if (points.Xs[i] > right.X) right = new Point(points.Xs[i], points.Ys[i]);
            }

            FindHull(in points, left, right, 1, result);
            FindHull(in points, left, right, -1, result);

            return result;
        }

        private static unsafe void FindHull(
            in Points points,
            Point p1,
            Point p2,
            float side,
            HashSet<Point> result)
        {
            var maxIndex = -1;
            var maxDistance = 0f;

            var p1X = Vector256.Create(p1.X);
            var p1Y = Vector256.Create(p1.Y);
            var p2X = Vector256.Create(p2.X);
            var p2Y = Vector256.Create(p2.Y);

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
                var distance = Distance(p1, p2, new Point(points.Xs[i], points.Ys[i]));
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

            var maxIndexPoint = new Point(points.Xs[maxIndex], points.Ys[maxIndex]);
            
            FindHull(in points, maxIndexPoint, p1, -maxDistance, result); 
            FindHull(in points, maxIndexPoint, p2, maxDistance, result);
        }

        private static float Distance(Point p1, Point p2, Point p) 
        { 
            return (p.Y - p1.Y) * (p2.X - p1.X) - (p2.Y - p1.Y) * (p.X - p1.X);
        }
    }
}