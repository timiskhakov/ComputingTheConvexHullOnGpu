using System.Collections.Generic;
using System.IO;

namespace ComputingTheConvexHullOnGpu.Tests
{
    public static class Data
    {
        public static readonly Point[] InputSmall =
        {
            new Point(10.41d, 11.5674d),
            new Point(24.7907412d, 30.12631d),
            new Point(40.12457d, 40.98119d),
            new Point(30.741548d, 9.7161d),
            new Point(20.731422536373d, 19.54745741d),
            new Point(30.74521d, 29.191d),
            new Point(19.92629d, 21.99811d),
            new Point(16.6644d, 18.0014d)
        };

        public static readonly Point[] ExpectedSmall =
        {
            new Point(24.7907412d, 30.12631d),
            new Point(10.41d, 11.5674d),
            new Point(40.12457d, 40.98119d),
            new Point(30.741548d, 9.7161d)
        };

        public static IEnumerable<Point> GetLarge()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "200-test-points.txt");
            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                yield return new Point(double.Parse(parts[0]), double.Parse(parts[1]));
            }
        }

        public static readonly Point[] ExpectedLarge =
        {
            new Point(2795.889704858833, 9911.88013922045),
            new Point(7450.333841820404, 9690.21396231382),
            new Point(1124.1666931305856, 9895.332637194233),
            new Point(8760.59674600167, 9570.668823817125),
            new Point(9317.817887904968, 9369.339714464424),
            new Point(236.88656754646757, 9588.713110233059),
            new Point(77.10349283977574, 8997.483313548139),
            new Point(11.664745403297593, 7404.62354263506),
            new Point(9950.835346221382, 4109.778545847991),
            new Point(9980.19356279643, 2958.742777332078),
            new Point(114.18900457871567, 794.7832861890938),
            new Point(269.8608721931748, 389.1321925395784),
            new Point(48.606921941324565, 2633.4887243031935),
            new Point(6196.64643248387, 62.2076541475056),
            new Point(1723.769992461321, 140.86596674326154),
            new Point(8668.785914158814, 108.81860279888781),
            new Point(9318.852238039883, 761.9753949167092)
        };
    }
}