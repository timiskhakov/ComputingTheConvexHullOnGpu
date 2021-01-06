using System;
using System.Collections.Generic;
using ComputingTheConvexHullOnGpu.Models;

namespace ComputingTheConvexHullOnGpu.Soa
{
    public class ConvexHull
    {
        public HashSet<Point> QuickHull(Point[] points)
        {
            if (points.Length <= 2) throw new ArgumentException($"Too little points: {points.Length}, expected 3 or more");

            var result = new HashSet<Point>();

            var left = points[0];
            var right = points[0];
            for (var i = 1; i < points.Length; i++)
            {
                if (points[i].X < left.X) left = points[i];
                if (points[i].X > right.X) right = points[i];
            }

            FindHull(points, left, right, 1, result);
            FindHull(points, left, right, -1, result);

            return result;
        }

        private static void FindHull(Point[] points, Point p1, Point p2, int side, HashSet<Point> result)
        {
            var maxIndex = -1;
            var maxDistance = 0f;
            for (var i = 0; i < points.Length; i++)
            {
                if (Side(p1, p2, points[i]) != side) continue;
                
                var distance = Distance(p1, p2, points[i]);
                if (distance <= maxDistance) continue;
                
                maxIndex = i; 
                maxDistance = distance;
            } 
            
            if (maxIndex == -1) 
            { 
                result.Add(p1); 
                result.Add(p2); 
                return;
            }

            var newSide = Side(points[maxIndex], p1, p2);
            FindHull(points, points[maxIndex], p1, -newSide, result); 
            FindHull(points, points[maxIndex], p2, newSide, result);
        }
        
        private static float Distance(Point p1, Point p2, Point p) 
        { 
            return Math.Abs((p.Y - p1.Y) * (p2.X - p1.X) - (p2.Y - p1.Y) * (p.X - p1.X)); 
        }
        
        private static int Side(Point p1, Point p2, Point p) 
        { 
            var side = (p.Y - p1.Y) * (p2.X - p1.X) - (p2.Y - p1.Y) * (p.X - p1.X);
            if (side > 0) return 1; 
            if (side < 0) return -1; 
            return 0; 
        }
    }
}