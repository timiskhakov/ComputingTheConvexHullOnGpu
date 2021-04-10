using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ComputingTheConvexHullOnGpu.Models;
using ComputingTheConvexHullOnGpu.Soa;

namespace ComputingTheConvexHullOnGpu.Tests
{
    internal static class Data
    {
        internal static readonly Point[] AosTwoPoints =
        {
            new Point(6639.4673f, 5346.256f),
            new Point(2962.2266f, 8454.807f)
        };

        internal static Points GetSoaTwoPoints()
        {
            var points = new Points(2);
            points.Xs[0] = 6639.4673f;
            points.Xs[1] = 2962.2266f;
            points.Ys[0] = 5346.256f;
            points.Ys[1] = 8454.807f;

            return points;
        }

        internal static IEnumerable<Point> GetAosPoints(string fileName)
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

        internal static Points GetSoaPoints(string fileName)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), "Assets", fileName);
            var lines = File.ReadAllLines(file);

            var points = new Points(lines.Length);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var parts = line.Split(',');
                points.Xs[i] = float.Parse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture);
                points.Ys[i] = float.Parse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture);
            }

            return points;
        }
    }
}