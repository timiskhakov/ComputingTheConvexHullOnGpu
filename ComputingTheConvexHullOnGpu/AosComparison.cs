using System.Globalization;
using System.IO;
using BenchmarkDotNet.Attributes;
using ComputingTheConvexHullOnGpu.Aos;
using ComputingTheConvexHullOnGpu.Models;

namespace ComputingTheConvexHullOnGpu
{
    [MemoryDiagnoser]
    public class AosComparison
    {
        private Point[] _points;
        
        [GlobalSetup]
        public void Setup()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "benchmark-points.txt");
            var lines = File.ReadAllLines(file);
            
            _points = new Point[lines.Length];
            for (var i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                _points[i] = new Point(
                    float.Parse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture),
                    float.Parse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture));
            }
        }
        
        [Benchmark]
        public void Baseline()
        {
            ConvexHull.QuickHull(_points);
        }
        
        [Benchmark]
        public void CpuParallelized()
        {
            ConvexHullCpuParallelized.QuickHull(_points);
        }

        [Benchmark]
        public void GpuParallelized()
        {
            ConvexHullGpuParallelized.QuickHull(_points);
        }
    }
}