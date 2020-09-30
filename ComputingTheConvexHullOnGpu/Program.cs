using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Running;

namespace ComputingTheConvexHullOnGpu
{
    internal static class Program
    {
        private static readonly Random Random = new Random();
        
        private static void Main()
        {
            //BenchmarkRunner.Run<Comparison>();

            var points = Enumerable.Range(0, 20)
                .Select(_ => new Point(RandomFloat(0, 10_000), RandomFloat(0, 10_000)))
                .ToArray();
            
            var sb = new StringBuilder();
            foreach (var point in points)
            {
                sb.AppendLine($"{point.X.ToString("G", CultureInfo.InvariantCulture)},{point.Y.ToString("G", CultureInfo.InvariantCulture)}");
            }
            
            File.WriteAllText("20-test-points.txt", sb.ToString());
        }

        private static float RandomFloat(float min, float max)
        {
            var f = (float) Random.NextDouble();
            return f * (max - min) + min;
        }
    }
}