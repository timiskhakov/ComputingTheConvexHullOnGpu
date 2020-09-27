using System;
using System.Collections.Generic;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Algorithms.ScanReduceOperations;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;

namespace ComputingTheConvexHullOnGpu
{
    public class ConvexHullGpuParallelized : IConvexHull
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
            
            using var distances = accelerator.Allocate<float>(points.Length);
            using var maxIndex = accelerator.Allocate<int>(1);

            FindHull(points, left, right, 1, result, accelerator, pointsView, distances, maxIndex);
            FindHull(points, left, right, -1, result, accelerator, pointsView, distances, maxIndex);

            return result;
        }

        private static void FindHull(
            Point[] points,
            Point p1,
            Point p2,
            int side,
            HashSet<Point> result,
            Accelerator accelerator,
            ArrayView<Point> pointsView,
            MemoryBuffer<float> distances,
            MemoryBuffer<int> maxIndex)
        {
            var distanceKernel = accelerator.LoadAutoGroupedStreamKernel
                <Index1, ArrayView<Point>, Point, Point, int, ArrayView<float>>(DistanceKernel);
            distanceKernel(pointsView.Length, pointsView, p1, p2, side, distances);
            accelerator.Synchronize();
            
            var maxValue = accelerator.Reduce<float, MaxFloat>(accelerator.DefaultStream, distances);
            maxIndex.CopyFrom(-1, 0);

            var findMaxIndexKernel = accelerator.LoadAutoGroupedStreamKernel
                <Index1, ArrayView<float>, float, ArrayView<int>>(FindMaxIndexKernel);
            findMaxIndexKernel(distances.Length, distances, maxValue, maxIndex);
            accelerator.Synchronize();

            var index = maxIndex.GetAsArray()[0];
            if (index == -1) 
            { 
                result.Add(p1); 
                result.Add(p2); 
                return;
            }

            var newSide = Side(points[index], p1, p2);
            FindHull(points, points[index], p1, -newSide, result, accelerator, pointsView, distances, maxIndex); 
            FindHull(points, points[index], p2, newSide, result, accelerator, pointsView, distances, maxIndex);
        }

        private static void DistanceKernel(Index1 i, ArrayView<Point> points, Point p1, Point p2, int side, ArrayView<float> distances)
        {
            distances[i] = Side(p1, p2, points[i]) == side ? Distance(p1, p2, points[i]) : 0;
        }

        private static void FindMaxIndexKernel(Index1 index, ArrayView<float> distances, float targetValue, ArrayView<int> output)
        {
            var value = distances[index];
            if (value > 0 && value == targetValue)
            {
                Atomic.Max(ref output[0], index);
            }
        }

        private static float Distance(Point p1, Point p2, Point p)
        {
            return IntrinsicMath.Abs((p.Y - p1.Y) * (p2.X - p1.X) - (p2.Y - p1.Y) * (p.X - p1.X)); 
        }
        
        private static int Side(Point p1, Point p2, Point p) 
        { 
            var side = (p.Y - p1.Y) * (p2.X - p1.X) - (p2.Y - p1.Y) * (p.X - p1.X);
            if (side > 0) return 1; 
            if (side < 0) return -1; 
            return 0; 
        }
    }
}