using System.Collections.Generic;

namespace ComputingTheConvexHullOnGpu
{
    public interface IConvexHull
    {
        HashSet<Point> QuickHull(Point[] points);
    }
}