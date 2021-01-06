using System;
using System.Linq;
using ComputingTheConvexHullOnGpu.Aos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ComputingTheConvexHullOnGpu.Tests.AosTests
{
    [TestClass]
    public class ConvexHullIntrinsicsTests
    {
        private ConvexHullIntrinsics _convexHull;

        [TestInitialize]
        public void Setup()
        {
            _convexHull = new ConvexHullIntrinsics();
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsArgumentException_ZeroPoints()
        {
            _convexHull.QuickHull(new Points(0));
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsArgumentException_NotEnoughPoints()
        {
            _convexHull.QuickHull(Data.GetTwoAosPoints());
        }

        [DataTestMethod]
        [DataRow("20-input.txt", "20-expected.txt")]
        [DataRow("200-input.txt", "200-expected.txt")]
        [DataRow("2000-input.txt", "2000-expected.txt")]
        public void ReturnsPoints_EnoughPoints(string inputFile, string expectedFile)
        {
            var input = Data.GetAosPoints(inputFile);
            var expected = Data.GetSoaPoints(expectedFile).ToArray();
            
            CollectionAssert.AreEqual(expected, _convexHull.QuickHull(input).ToArray());
        }
    }
}