using System;
using System.Collections.Generic;
using ComputingTheConvexHullOnGpu.Models;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Algorithms.ScanReduceOperations;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Util;

namespace ComputingTheConvexHullOnGpu.Aos
{
    public static class ConvexHullGpuParallelized
    {
        public static HashSet<Point> QuickHull(Point[] points)
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

            using var context = Context.Create().ToContext();                               
            using var accelerator = context.CreateCudaAccelerator(0);
            //using var accelerator = context.CreateCPUAccelerator(0);            
            using var buffer = accelerator.Allocate1D<Point>(points.Length);
            buffer.CopyFromCPU(points);
            
            FindHull(points, left, right, 1, result, accelerator, buffer.View);
            FindHull(points, left, right, -1, result, accelerator, buffer.View);

            return result;
        }

        private static void FindHull(
            Point[] points,
            Point p1,
            Point p2,
            int side,
            HashSet<Point> result,
            Accelerator accelerator,
            ArrayView<Point> view)
        {
            var (gridDim, groupDim) = accelerator.ComputeGridStrideLoopExtent(points.Length, out _);
            using var output = accelerator.Allocate1D<DataBlock<int, float>>(gridDim);
            
            var kernel = accelerator.LoadStreamKernel<
                ArrayView<Point>, int, Point, Point, ArrayView<DataBlock<int, float>>>(FindMaxIndexKernel);
            kernel(new KernelConfig(gridDim, groupDim), view, side, p1, p2, output.AsContiguous());
            accelerator.Synchronize();
            
            var maxIndex = -1;
            var maxDistance = 0f;

            var candidates = output.GetAsArray1D();
            foreach (var candidate in candidates)
            {
                if (candidate.Item1 < 0) continue;
                FindMaxIndex(p1, p2, points[candidate.Item1], side, candidate.Item1, ref maxIndex, ref maxDistance);
            }

            if (maxIndex < 0) 
            {
                result.Add(p1); 
                result.Add(p2); 
                return;
            }

            var newSide = Side(points[maxIndex], p1, p2);
            FindHull(points, points[maxIndex], p1, -newSide, result, accelerator, view);
            FindHull(points, points[maxIndex], p2, newSide, result, accelerator, view);
        }

        private static void FindMaxIndexKernel(
            ArrayView<Point> input,
            int side,
            Point a,
            Point b,
            ArrayView<DataBlock<int, float>> output)
        {
            var stride = GridExtensions.GridStrideLoopStride;
            var index = -1;
            var distance = 0f;

            for (var i = Grid.GlobalIndex.X; i < input.Length; i += stride)
            {
                FindMaxIndex(a, b, input[i], side, i, ref index, ref distance);
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
            Point candidatePoint,
            int side,
            int candidateIndex,
            ref int maxIndex,
            ref float maxDistance)
        {
            if (Side(a, b, candidatePoint) != side) return;
            
            var candidateDistance = Distance(a, b, candidatePoint);
            if (candidateDistance <= maxDistance) return;
            
            maxIndex = candidateIndex;
            maxDistance = candidateDistance;
        }
        
        private static int Side(Point p1, Point p2, Point p) 
        { 
            var side = (p.Y - p1.Y) * (p2.X - p1.X) - (p2.Y - p1.Y) * (p.X - p1.X);
            if (side > 0) return 1; 
            if (side < 0) return -1; 
            return 0; 
        }
        
        private static float Distance(Point a, Point b, Point p)
        {
            return IntrinsicMath.Abs((p.Y - a.Y) * (b.X - a.X) - (b.Y - a.Y) * (p.X - a.X)); 
        }
    }
}