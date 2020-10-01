using BenchmarkDotNet.Running;

namespace ComputingTheConvexHullOnGpu
{
    internal static class Program
    {
        private static void Main()
        {
            BenchmarkRunner.Run<Comparison>();
        }
    }
}