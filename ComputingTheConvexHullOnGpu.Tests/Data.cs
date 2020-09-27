using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ComputingTheConvexHullOnGpu.Tests
{
    public static class Data
    {
        public static readonly Point[] InputSmall =
        {
            new Point(10.41f, 11.5674f),
            new Point(24.7907412f, 30.12631f),
            new Point(40.12457f, 40.98119f),
            new Point(30.741548f, 9.7161f),
            new Point(20.7314225f, 19.54745741f),
            new Point(30.74521f, 29.191f),
            new Point(19.92629f, 21.99811f),
            new Point(16.6644f, 18.0014f)
        };

        public static readonly Point[] ExpectedSmall =
        {
            new Point(24.7907412f, 30.12631f),
            new Point(10.41f, 11.5674f),
            new Point(40.12457f, 40.98119f),
            new Point(30.741548f, 9.7161f)
        };

        public static IEnumerable<Point> GetLarge()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "200-test-points.txt");
            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                yield return new Point(
                    float.Parse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture),
                    float.Parse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture));
            }
        }

        public static readonly Point[] ExpectedLarge =
        {
            new Point(594.70294f, 9620.119f),
            new Point(115.67211f, 8966.397f),
            new Point(686.0761f, 9728.613f),
            new Point(20.231987f, 4984.4277f),
            new Point(4684.2134f, 9989.012f),
            new Point(8791.546f, 9990.231f),
            new Point(9161.8125f, 9822.024f),
            new Point(9783.377f, 7437.026f),
            new Point(9972.946f, 6330.345f),
            new Point(5126.021f, 118.82632f),    
            new Point(970.0732f, 48.044346f),
            new Point(9340.93f, 308.74588f),
            new Point(288.5669f, 717.35236f),
            new Point(9928.408f, 441.15924f)
        };
    }
}