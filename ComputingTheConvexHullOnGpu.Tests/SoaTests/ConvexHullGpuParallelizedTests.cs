// using System;
// using System.Linq;
// using ComputingTheConvexHullOnGpu.Soa;
// using Microsoft.VisualStudio.TestTools.UnitTesting;
//
// namespace ComputingTheConvexHullOnGpu.Tests.SoaTests
// {
//     [TestClass]
//     public class ConvexHullGpuParallelizedTests
//     {
//         [TestMethod]
//         [ExpectedException(typeof(ArgumentException))]
//         public void ThrowsArgumentException_ZeroPoints()
//         {
//             ConvexHullGpuParallelized.QuickHull(new Points(0));
//         }
//         
//         [TestMethod]
//         [ExpectedException(typeof(ArgumentException))]
//         public void ThrowsArgumentException_NotEnoughPoints()
//         {
//             ConvexHullGpuParallelized.QuickHull(Data.GetSoaTwoPoints());
//         }
//
//         [DataTestMethod]
//         [DataRow("20-input.txt", "20-expected.txt")]
//         [DataRow("200-input.txt", "200-expected.txt")]
//         [DataRow("2000-input.txt", "2000-expected.txt")]
//         public void ReturnsPoints_EnoughPoints(string inputFile, string expectedFile)
//         {
//             var input = Data.GetSoaPoints(inputFile);
//             var expected = Data.GetAosPoints(expectedFile).ToArray();
//
//             var actual = ConvexHullGpuParallelized.QuickHull(input).ToArray();
//             
//             CollectionAssert.AreEqual(expected, actual);
//         }
//     }
// }