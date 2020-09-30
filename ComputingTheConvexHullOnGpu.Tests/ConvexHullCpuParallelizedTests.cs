using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ComputingTheConvexHullOnGpu.Tests
{
    [TestClass]
    public class ConvexHullCpuParallelizedTests
    {
        private IConvexHull _convexHull;

        [TestInitialize]
        public void Setup()
        {
            _convexHull = new ConvexHullCpuParallelized();
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsArgumentException_ZeroPoints()
        {
            _convexHull.QuickHull(new Point[0]);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsArgumentException_NotEnoughPoints()
        {
            _convexHull.QuickHull(Data.TwoPoints);
        }

        [DataTestMethod]
        [DataRow("20-input.txt", "20-expected.txt")]
        [DataRow("200-input.txt", "200-expected.txt")]
        [DataRow("2000-input.txt", "2000-expected.txt")]
        public void ReturnsPoints_EnoughPoints(string inputFile, string expectedFile)
        {
            var input = Data.GetPoints(inputFile).ToArray();
            var expected = Data.GetPoints(expectedFile).ToArray();
            
            CollectionAssert.AreEqual(expected, _convexHull.QuickHull(input).ToArray());
        }
    }
}