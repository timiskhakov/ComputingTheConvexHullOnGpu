using System;
using System.Collections.Generic;
using ComputingTheConvexHullOnGpu.Models;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Algorithms.ScanReduceOperations;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using ILGPU.Util;

namespace ComputingTheConvexHullOnGpu.Aos
{
    public static class ConvexHullGpuParallelized
    {
        public static HashSet<Point> QuickHull(Points points)
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

            using var context = new Context();
            context.EnableAlgorithms();
            
            using var accelerator = new CudaAccelerator(context);
            using var xsBuffer = accelerator.Allocate<float>(points.Xs.Length);
            using var ysBuffer = accelerator.Allocate<float>(points.Ys.Length);
            xsBuffer.CopyFrom(points.Xs, 0, 0, points.Xs.Length);
            xsBuffer.CopyFrom(points.Ys, 0, 0, points.Ys.Length);
            
            FindHull(in points, left, right, 1, result, accelerator, xsBuffer.View, ysBuffer.View);
            FindHull(in points, left, right, -1, result, accelerator, xsBuffer.View, ysBuffer.View);

            return result;
        }

        private static void FindHull(
            in Points points,
            Point p1,
            Point p2,
            int side,
            HashSet<Point> result,
            Accelerator accelerator,
            ArrayView<float> xsView,
            ArrayView<float> ysView)
        {
            var (gridDim, groupDim) = accelerator.ComputeGridStrideLoopExtent(xsView.Length, out _);
            using var output = accelerator.Allocate<DataBlock<int, float>>(gridDim);
            
            var kernel = accelerator.LoadStreamKernel<
                ArrayView<float>, ArrayView<float>, int, Point, Point, ArrayView<DataBlock<int, float>>
            >(FindMaxIndexKernel);
            kernel(new KernelConfig(gridDim, groupDim), xsView, ysView, side, p1, p2, output);
            accelerator.Synchronize();
            
            var maxIndex = -1;
            var maxDistance = 0f;

            var candidates = output.GetAsArray();
            foreach (var candidate in candidates)
            {
                if (candidate.Item1 < 0) continue;
                FindMaxIndex(p1, p2, points.Xs[candidate.Item1], points.Ys[candidate.Item1], side, candidate.Item1, ref maxIndex, ref maxDistance);
            }

            if (maxIndex < 0) 
            {
                result.Add(p1); 
                result.Add(p2); 
                return;
            }

            var maxIndexPoint = new Point(points.Xs[maxIndex], points.Ys[maxIndex]);
            var newSide = Side(points.Xs[maxIndex], points.Ys[maxIndex], p1.X, p1.Y, p2.X, p2.Y);
            FindHull(points, maxIndexPoint, p1, -newSide, result, accelerator, xsView, ysView);
            FindHull(points, maxIndexPoint, p2, newSide, result, accelerator, xsView, ysView);
        }

        private static void FindMaxIndexKernel(
            ArrayView<float> xsView,
            ArrayView<float> ysView,
            int side,
            Point a,
            Point b,
            ArrayView<DataBlock<int, float>> output)
        {
            var stride = GridExtensions.GridStrideLoopStride;
            var index = -1;
            var distance = 0f;

            for (var i = Grid.GlobalIndex.X; i < xsView.Length; i += stride)
            {
                FindMaxIndex(a, b, xsView[i], ysView[i], side, i, ref index, ref distance);
            }
            
            var maxGroupDistance = GroupExtensions.Reduce<float, MaxFloat>(distance);
            if (!distance.Equals(maxGroupDistance))
            {
                index = -1;
            }
            
            var maxGroupIndex = GroupExtensions.Reduce<int, MaxInt32>(index);
            
            if (Group.IsFirstThread)
            {
                output[Grid.IdxX] = new DataBlock<int, float>(maxGroupIndex, maxGroupDistance);
            }
        }

        private static void FindMaxIndex(
            Point a,
            Point b,
            float candidateX,
            float candidateY,
            int side,
            int candidateIndex,
            ref int maxIndex,
            ref float maxDistance)
        {
            if (Side(a.X, a.Y, b.X, b.Y, candidateX, candidateY) != side) return;
            
            var candidateDistance = Distance(a.X, a.Y, b.X, b.Y, candidateX, candidateY);
            if (candidateDistance <= maxDistance) return;
            
            maxIndex = candidateIndex;
            maxDistance = candidateDistance;
        }
        
        private static int Side(float aX, float aY, float bX, float bY, float pX, float pY) 
        { 
            var side = (pY - aY) * (bX - aX) - (bY - aY) * (pX - aX);
            if (side > 0) return 1; 
            if (side < 0) return -1; 
            return 0; 
        }
        
        private static float Distance(float aX, float aY, float bX, float bY, float pX, float pY)
        {
            return IntrinsicMath.Abs((pY - aY) * (bX - aX) - (bY - aY) * (pX - aX)); 
        }
    }
}