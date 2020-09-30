﻿using System;
using System.Collections.Generic;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Algorithms.ScanReduceOperations;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using ILGPU.Util;

namespace ComputingTheConvexHullOnGpu
{
    public class ConvexHullGpuParallelized2 : IConvexHull
    {
        public HashSet<Point> QuickHull(Point[] points)
        {
            if (points.Length <= 2) throw new ArgumentException($"Too little points: {points.Length}, expected 3 or more");

            var result = new HashSet<Point>();

            var left = points[0];
            var right = points[0];
            for (var i = 1; i < points.Length; i++)
            {
                if (points[i].X < left.X) left = points[i];
                if (points[i].X > right.X) right = points[i];
            }

            using var context = new Context();
            context.EnableAlgorithms();
            
            using var accelerator = new CudaAccelerator(context);
            using var pointsBuffer = accelerator.Allocate<Point>(points.Length);
            pointsBuffer.CopyFrom(points, 0, 0, points.Length);
            var pointsView = pointsBuffer.View;
            
            FindHull(points, left, right, 1, result, accelerator, pointsView);
            FindHull(points, left, right, -1, result, accelerator, pointsView);

            return result;
        }

        private static void FindHull(
            Point[] points,
            Point p1,
            Point p2,
            int side,
            HashSet<Point> result,
            Accelerator accelerator,
            ArrayView<Point> pointsView)
        {
            var (gridDim, groupDim) = accelerator.ComputeGridStrideLoopExtent(points.Length, out _);
            using var output = accelerator.Allocate<DataBlock<float, int>>(gridDim);
            
            var kernel = accelerator.LoadStreamKernel<ArrayView<Point>, int, Point, Point, ArrayView<DataBlock<float, int>>>(FindIndexKernel);
            kernel((gridDim, groupDim), pointsView, side, p1, p2, output);
            accelerator.Synchronize();
            
            var maxDistance = 0f;
            var maxIndex = -1;
            
            var candidates = output.GetAsArray();
            foreach (var candidate in candidates)
            {
                if (candidate.Item2 < 0) continue;
                FindMaxDistance(side, p1, p2, points[candidate.Item2], candidate.Item2, ref maxDistance, ref maxIndex);
            }

            if (maxIndex < 0) 
            { 
                result.Add(p1); 
                result.Add(p2); 
                return;
            }

            var newSide = Side(points[maxIndex], p1, p2);
            FindHull(points, points[maxIndex], p1, -newSide, result, accelerator, pointsView); 
            FindHull(points, points[maxIndex], p2, newSide, result, accelerator, pointsView);
        }

        private static float Distance(Point a, Point b, Point p)
        {
            return IntrinsicMath.Abs((p.Y - a.Y) * (b.X - a.X) - (b.Y - a.Y) * (p.X - a.X)); 
        }
        
        private static int Side(Point p1, Point p2, Point p) 
        { 
            var side = (p.Y - p1.Y) * (p2.X - p1.X) - (p2.Y - p1.Y) * (p.X - p1.X);
            if (side > 0) return 1; 
            if (side < 0) return -1; 
            return 0; 
        }

        private static void FindIndexKernel(ArrayView<Point> input, int side, Point a, Point b, ArrayView<DataBlock<float, int>> output)
        {
            var stride = GridExtensions.GridStrideLoopStride;
            var maxDistance = 0f;
            var maxIndex = -1;
            
            for (var i = Grid.GlobalIndex.X; i < input.Length; i += stride)
            {
                FindMaxDistance(side, a, b, input[i], i, ref maxDistance, ref maxIndex);
            }
            
            var maxGroupDistance = GroupExtensions.Reduce<float, MaxFloat>(maxDistance);

            if (!maxGroupDistance.Equals(maxDistance))
            {
                maxIndex = -1;
            }
            
            var bestGroupIndex = GroupExtensions.Reduce<int, MaxInt32>(maxIndex);

            if (Group.IsFirstThread)
            {
                output[Grid.IdxX] = new DataBlock<float, int>(maxGroupDistance, bestGroupIndex);
            }
        }

        private static void FindMaxDistance(int side, Point a, Point b, Point candidatePoint, int candidateIndex, ref float maxDistance, ref int maxIndex)
        {
            if (Side(a, b, candidatePoint) != side) return;;
            
            var candidateDistance = Distance(a, b, candidatePoint);
            if (candidateDistance <= maxDistance) return;
            
            maxDistance = candidateDistance;
            maxIndex = candidateIndex;
        }
    }
}