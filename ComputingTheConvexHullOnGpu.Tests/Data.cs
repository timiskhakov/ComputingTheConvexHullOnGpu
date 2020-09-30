using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ComputingTheConvexHullOnGpu.Tests
{
    internal static class Data
    {
        internal static readonly Point[] TwoPoints =
        {
            new Point(6639.4673f, 5346.256f),
            new Point(2962.2266f, 8454.807f)
        };
        
        public static IEnumerable<Point> GetPoints(string fileName)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), "Assets", fileName);
            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                yield return new Point(
                    float.Parse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture),
                    float.Parse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture));
            }
        }
    }
}