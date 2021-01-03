using System.Collections.Generic;
using ComputingTheConvexHullOnGpu.Models;

namespace ComputingTheConvexHullOnGpu.Soa
{
    public interface IConvexHull
    {
        HashSet<Point> QuickHull(Point[] points);
    }
}