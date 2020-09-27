using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ComputingTheConvexHullOnGpu.Tests
{
    [TestClass]
    public class ConvexHullGpuParallelizedTests
    {
        private IConvexHull _convexHull;

        [TestInitialize]
        public void Setup()
        {
            _convexHull = new ConvexHullGpuParallelized();
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsArgumentException_ZeroPoints()
        {
            _convexHull.QuickHull(new Point[0]);
        }

        [TestMethod]
        public void ReturnsPoints_Input8()
        {
            CollectionAssert.AreEquivalent(Data.ExpectedSmall, _convexHull.QuickHull(Data.InputSmall).ToArray());
        }

        [TestMethod]
        public void ReturnsPoints_Input200()
        {
            var input = Data.GetLarge().ToArray();
            CollectionAssert.AreEquivalent(Data.ExpectedLarge, _convexHull.QuickHull(input).ToArray());
        }
    }
}