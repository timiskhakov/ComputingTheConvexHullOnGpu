using System.IO;
using BenchmarkDotNet.Attributes;

namespace ComputingTheConvexHullOnGpu
{
    [MemoryDiagnoser]
    public class Comparison
    {
        private Point[] _points;
        
        private IConvexHull _baseline;
        private IConvexHull _cpuParallelized;
        private IConvexHull _gpuParallelized;

        [GlobalSetup]
        public void Setup()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "points.txt");
            var lines = File.ReadAllLines(file);
            
            _points = new Point[lines.Length];
            for (var i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                _points[i] = new Point(double.Parse(parts[0]), double.Parse(parts[1]));
            }
            
            _baseline = new ConvexHull();
            _cpuParallelized = new ConvexHullCpuParallelized();
            _gpuParallelized = new ConvexHullGpuParallelized();
        }
        
        [Benchmark]
        public void Baseline()
        {
            _baseline.QuickHull(_points);
        }
        
        [Benchmark]
        public void CpuParallelized()
        {
            _cpuParallelized.QuickHull(_points);
        }
        
        [Benchmark]
        public void GpuParallelized()
        {
            _gpuParallelized.QuickHull(_points);
        }
    }
}