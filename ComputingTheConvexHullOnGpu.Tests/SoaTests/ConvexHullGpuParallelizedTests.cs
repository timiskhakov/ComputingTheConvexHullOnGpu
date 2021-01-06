using System;
using System.Linq;
using ComputingTheConvexHullOnGpu.Models;
using ComputingTheConvexHullOnGpu.Soa;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ComputingTheConvexHullOnGpu.Tests.SoaTests
{
    [TestClass]
    public class ConvexHullGpuParallelizedTests
    {
        private ConvexHullGpuParallelized _convexHullGpuParallelized;

        [TestInitialize]
        public void Setup()
        {
            _convexHullGpuParallelized = new ConvexHullGpuParallelized();
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsArgumentException_ZeroPoints()
        {
            _convexHullGpuParallelized.QuickHull(new Point[0]);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsArgumentException_NotEnoughPoints()
        {
            _convexHullGpuParallelized.QuickHull(Data.TwoSoaPoints);
        }

        [DataTestMethod]
        [DataRow("20-input.txt", "20-expected.txt")]
        [DataRow("200-input.txt", "200-expected.txt")]
        [DataRow("2000-input.txt", "2000-expected.txt")]
        public void ReturnsPoints_EnoughPoints(string inputFile, string expectedFile)
        {
            var input = Data.GetSoaPoints(inputFile).ToArray();
            var expected = Data.GetSoaPoints(expectedFile).ToArray();
            
            CollectionAssert.AreEqual(expected, _convexHullGpuParallelized.QuickHull(input).ToArray());
        }
    }
}