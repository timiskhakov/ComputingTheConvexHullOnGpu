using System.Globalization;
using System.IO;
using BenchmarkDotNet.Attributes;
using ComputingTheConvexHullOnGpu.Aos;
using ComputingTheConvexHullOnGpu.Models;
using ComputingTheConvexHullOnGpu.Soa;

namespace ComputingTheConvexHullOnGpu
{
    [MemoryDiagnoser]
    public class SoaComparison
    {
        private Point[] _points;
        
        private ConvexHull _convexHull;
        private ConvexHullCpuParallelized _convexHullCpuParallelized;
        private ConvexHullGpuParallelized _convexHullGpuParallelized;

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
            
            _convexHull = new ConvexHull();
            _convexHullCpuParallelized = new ConvexHullCpuParallelized();
            _convexHullGpuParallelized = new ConvexHullGpuParallelized();
        }
        
        [Benchmark]
        public void Baseline()
        {
            _convexHull.QuickHull(_points);
        }
        
        [Benchmark]
        public void CpuParallelized()
        {
            _convexHullCpuParallelized.QuickHull(_points);
        }

        [Benchmark]
        public void GpuParallelized()
        {
            _convexHullGpuParallelized.QuickHull(_points);
        }
    }
}