using System.Globalization;
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
        private IConvexHull _gpuParallelized2;

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
            
            _baseline = new ConvexHull();
            _cpuParallelized = new ConvexHullCpuParallelized();
            _gpuParallelized = new ConvexHullGpuParallelized();
            _gpuParallelized2 = new ConvexHullGpuParallelized2();
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
        
        [Benchmark]
        public void GpuParallelized2()
        {
            _gpuParallelized2.QuickHull(_points);
        }
    }
}