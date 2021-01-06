using System;
using System.Linq;
using ComputingTheConvexHullOnGpu.Models;
using ComputingTheConvexHullOnGpu.Soa;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ComputingTheConvexHullOnGpu.Tests.SoaTests
{
    [TestClass]
    public class ConvexHullCpuParallelizedTests
    {
        private ConvexHullCpuParallelized _convexHullCpuParallelized;

        [TestInitialize]
        public void Setup()
        {
            _convexHullCpuParallelized = new ConvexHullCpuParallelized();
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsArgumentException_ZeroPoints()
        {
            _convexHullCpuParallelized.QuickHull(new Point[0]);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsArgumentException_NotEnoughPoints()
        {
            _convexHullCpuParallelized.QuickHull(Data.TwoSoaPoints);
        }

        [DataTestMethod]
        [DataRow("20-input.txt", "20-expected.txt")]
        [DataRow("200-input.txt", "200-expected.txt")]
        [DataRow("2000-input.txt", "2000-expected.txt")]
        public void ReturnsPoints_EnoughPoints(string inputFile, string expectedFile)
        {
            var input = Data.GetSoaPoints(inputFile).ToArray();
            var expected = Data.GetSoaPoints(expectedFile).ToArray();
            
            CollectionAssert.AreEqual(expected, _convexHullCpuParallelized.QuickHull(input).ToArray());
        }
    }
}