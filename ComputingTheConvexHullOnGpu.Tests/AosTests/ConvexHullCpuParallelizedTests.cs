using System;
using System.Linq;
using ComputingTheConvexHullOnGpu.Aos;
using ComputingTheConvexHullOnGpu.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ComputingTheConvexHullOnGpu.Tests.AosTests
{
    [TestClass]
    public class ConvexHullCpuParallelizedTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsArgumentException_ZeroPoints()
        {
            ConvexHullCpuParallelized.QuickHull(new Point[0]);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsArgumentException_NotEnoughPoints()
        {
            ConvexHullCpuParallelized.QuickHull(Data.AosTwoPoints);
        }

        [DataTestMethod]
        [DataRow("20-input.txt", "20-expected.txt")]
        [DataRow("200-input.txt", "200-expected.txt")]
        [DataRow("2000-input.txt", "2000-expected.txt")]
        public void ReturnsPoints_EnoughPoints(string inputFile, string expectedFile)
        {
            var input = Data.GetAosPoints(inputFile).ToArray();
            var expected = Data.GetAosPoints(expectedFile).ToArray();
            
            CollectionAssert.AreEqual(expected, ConvexHullCpuParallelized.QuickHull(input).ToArray());
        }
    }
}