using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrbitSimulator.Core;
using static OrbitSimulator.Core.PhysicsUtils;
using static OrbitSimulatorTests.TestingUtils;

namespace OrbitSimulatorTests.CoreTesting
{
    [TestClass]
    public class VectorPolarTests
    {

        private static IEnumerable<object[]> GetFromCartesianTestData()
        {   //Only used because DataRow requires constant expressions, which Math.Sqrt(2) is not since it is a function call
            var root2 = Math.Sqrt(2);
            yield return new object[] { 0, 0, 0, 0 }; //Zero vector
            yield return new object[] { 0, 1, 1, 0 }; //1st quadrant edge
            yield return new object[] { 1, 1, root2, Pi / 4 }; //1st quadrant
            yield return new object[] { 1, 0, 1, HalfPi }; //1st quadrant edge
            yield return new object[] { 1, -1, root2, 3 * Pi / 4 }; //2nd quadrant
            yield return new object[] { 0, -1, 1, Pi }; //2nd quadrant edge
            yield return new object[] { -2, -2, 2 * root2, -3 * Pi / 4 }; //3rd quadrant
            yield return new object[] { -1, 0, 1, -HalfPi }; //3rd quadrant edge
            yield return new object[] { -1, 1, root2, -Pi / 4 }; //4th quadrant
        }

        private static void Compare_Vector(double expected_r, double expected_phi, VectorPolar v)
        {
            var x = expected_r * Math.Sin(expected_phi);
            var y = expected_r * Math.Cos(expected_phi);
            Assert.AreEqual(v.Magnitude, expected_r, DecimalTolerance, "The magnitude is not correct");
            Assert.AreEqual(v.Direction, expected_phi, DecimalTolerance, "The direction is not correct");
            Assert.AreEqual(v.X, x, DecimalTolerance, "The x component is not correct");
            Assert.AreEqual(v.Y, y, DecimalTolerance, "The y component is not correct");
        }

        [TestMethod]
        //Positive r
        [DataRow(0, 2, 0, 0)] //0 magnitude => 0 direction as well
        [DataRow(1, Pi / 6, 1, Pi / 6)] //1st quadrant
        [DataRow(1, 3 * Pi / 4, 1, 3 * Pi / 4)] //2nd quadrant
        [DataRow(2, Pi, 2, Pi)] //Directly down
        [DataRow(1, 5 * Pi / 4, 1, -3 * Pi / 4)] //3rd quadrant
        [DataRow(2, 3 * HalfPi, 2, -HalfPi)] //To the left
        [DataRow(1, 7 * Pi / 4, 1, -Pi / 4)] //Fourth quadrant
        [DataRow(32, TwoPi, 32, 0)] //Up again
        [DataRow(1, TwoPi + 0.1, 1, 0.1)] //Testing mod TwoPi
        //Negative r
        [DataRow(-1, 0, 1, Pi)] //Straight down
        [DataRow(-2, Pi / 6, 2, -5 * Pi / 6)] //1st quadrant
        [DataRow(-0.5, HalfPi, 0.5, -HalfPi)]
        [DataRow(-1, 2 * Pi / 3, 1, -Pi / 3)] //2nd quadrant
        [DataRow(-1, 5 * Pi / 4, 1, Pi / 4)] //3rd quadrant, positive phi
        [DataRow(-1, -3 * Pi / 4, 1, Pi / 4)] //3rd quadrant, negative phi
        [DataRow(-1, 7 * Pi / 4, 1, 3 * Pi / 4)] //4th quadrant, positive phi
        [DataRow(-1, -Pi / 4, 1, 3 * Pi / 4)] //4th quadrant, negative phi

        public void InitialisationTest(double r, double phi, double expected_r, double expected_phi)
        {
            var v = new VectorPolar(r, phi);
            Compare_Vector(expected_r, expected_phi, v);
        }

        [TestMethod]
        [DataRow(1, Pi, 2, 2)] //Enlargement
        [DataRow(2, HalfPi, 0.5, 1)] //Shrinking
        public void Scaling_ShouldChangeMagnitude(double r, double phi, double scalar, double expected_r)
        {
            var v = new VectorPolar(r, phi);
            var scaled_v = scalar * v;
            Compare_Vector(expected_r, phi, scaled_v);
            scaled_v = scalar * v; //Test left multiplication as well
            Compare_Vector(expected_r, phi, scaled_v);
        }

        [TestMethod]
        [DataRow(1, HalfPi, -1, 1, -HalfPi)] //Negative scaling
        [DataRow(2, -HalfPi, -2, 4, HalfPi)]
        [DataRow(1, Pi / 4, -2, 2, -3 * Pi / 4)]
        public void NegativeScaling_ShouldChangeDirection(double r, double phi, double scalar, double expected_r, double expected_phi)
        {
            var v = new VectorPolar(r, phi);
            var scaled_v = v * scalar;
            Compare_Vector(expected_r, expected_phi, scaled_v);
            scaled_v = scalar * v; //Test left multiplication as well
            Compare_Vector(expected_r, expected_phi, scaled_v);
        }

        [TestMethod]
        public void ScalingByZero_ShouldGiveZeroVector()
        {
            var v = new VectorPolar(1, 2);
            var scaled_v = 0 * v;
            Compare_Vector(0, 0, scaled_v);
            scaled_v = v * 0;
            Compare_Vector(0, 0, scaled_v);
        }

        [TestMethod]
        [DynamicData(nameof(GetFromCartesianTestData), DynamicDataSourceType.Method)]
        public void FromCartesianTest(double x, double y, double expected_r, double expected_phi)
        {
            var v = VectorPolar.FromCartesian(x, y);
            Compare_Vector(expected_r, expected_phi, v);
        }

        //TODO Test addition?
    }
}
