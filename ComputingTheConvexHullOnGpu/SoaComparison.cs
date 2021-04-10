using System.Globalization;
using System.IO;
using BenchmarkDotNet.Attributes;
using ComputingTheConvexHullOnGpu.Soa;

namespace ComputingTheConvexHullOnGpu
{
    [MemoryDiagnoser]
    public class SoaComparison
    {
        private Points _points;

        [GlobalSetup]
        public void Setup()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "benchmark-points.txt");
            var lines = File.ReadAllLines(file);

            _points = new Points(lines.Length);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var parts = line.Split(',');
                _points.Xs[i] = float.Parse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture);
                _points.Ys[i] = float.Parse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture);
            }
        }
        
        [Benchmark]
        public void Intrinsics()
        {
            ConvexHullIntrinsics.QuickHull(_points);
        }
        
        [Benchmark]
        public void Gpu()
        {
            ConvexHullGpuParallelized.QuickHull(_points);
        }
    }
}