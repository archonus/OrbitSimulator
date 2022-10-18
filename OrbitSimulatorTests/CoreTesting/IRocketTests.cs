using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrbitSimulator.Core;
using static OrbitSimulator.Core.PhysicsUtils;
using static OrbitSimulatorTests.TestingUtils;

namespace OrbitSimulatorTests.CoreTesting
{
    public abstract class IRocketTests
    {
        protected double payload = 400;
        protected abstract IRocket GetRocket();

        protected static void CheckFuelUse(double initial, double mdot, double dt, double final)
        {
            var expected_final = initial - mdot * dt;
            Assert.AreEqual(expected_final, final, DecimalTolerance);
        }

        protected static void CheckState(IRocket rocket, double velocity, double height, double fuel)
        {
            Assert.AreEqual(fuel, rocket.FuelLevel, DecimalTolerance, "Fuel level does not match");
            CheckState(rocket, velocity, height);
        }

        protected static void CheckState(IRocket rocket, double velocity, double height)
        {
            Assert.AreEqual(height, rocket.Position.Y, DecimalTolerance, "Height does not match");
            Assert.AreEqual(velocity, rocket.Velocity.Magnitude, DecimalTolerance, "Velocity does not match");
        }


        protected static void CheckMass(IRocket rocket, double expectedMass, double expectedFuel)
        {
            Assert.AreEqual(expectedFuel, rocket.FuelLevel, DecimalTolerance, "Fuel does not match");
            Assert.AreEqual(expectedMass, rocket.CurrentMass, DecimalTolerance, "Total mass does not match");
        }

        [TestMethod]
        public void InvalidThrottle_ThrowException()
        {
            var rocket = GetRocket();
            Action setBelowRange = () => rocket.Throttle = -1;
            Action setAboveRange = () => rocket.Throttle = 1.1;
            Assert.ThrowsException<ArgumentOutOfRangeException>(setBelowRange);
            Assert.ThrowsException<ArgumentOutOfRangeException>(setAboveRange);
        }

        [TestMethod]
        public void InvalidThrustDirection_ThrowsException()
        {
            var rocket = GetRocket();
            Action setBelowRange = () => rocket.ChangeThrustDirection(-3 * Pi / 4);
            Action setAboveRange = () => rocket.ChangeThrustDirection(3 * Pi / 4);
            Assert.ThrowsException<ArgumentOutOfRangeException>(setBelowRange);
            Assert.ThrowsException<ArgumentOutOfRangeException>(setAboveRange);
        }

        [TestMethod]
        [DataRow(Pi / 6)]
        [DataRow(-Pi / 6)]
        public void ThrustDirectionTests(double direction)
        {
            var rocket = GetRocket();
            rocket.ChangeThrustDirection(direction);
            var expectedDirection = direction + rocket.Velocity.Direction;
            Assert.AreEqual(expectedDirection, rocket.Thrust.Direction, TestingUtils.DecimalTolerance);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(0.5)]
        [DataRow(0)]
        public void ChangingThrottle_ShouldChangeThrust(double throttle)
        {
            var rocket = GetRocket();
            var expected_thrust = rocket.Thrust.Magnitude * throttle;
            rocket.Throttle = throttle;
            Assert.AreEqual(expected_thrust, rocket.Thrust.Magnitude, TestingUtils.DecimalTolerance);
        }

        [TestMethod]
        public void NegativeTimeUpdateTime_ShouldDoNothing()
        {
            var rocket = GetRocket();
            var initialMass = rocket.CurrentMass;
            var initialFuel = rocket.FuelLevel;
            var initialVelocity = rocket.Velocity.Magnitude;
            var initialHeight = rocket.Position.Y;

            rocket.UpdateTime(-1);
            CheckState(rocket, initialVelocity, initialHeight, initialFuel);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(5)]
        [DataRow(0.5)]
        public void CumulativeUpdateTime_ShouldBeSame(double dt)
        {
            var r1 = GetRocket();
            var r2 = GetRocket();
            r1.UpdateTime(dt);
            r1.UpdateTime(dt);
            r2.UpdateTime(2 * dt);
            Assert.AreEqual(r1.CurrentTime, r2.CurrentTime);
            CheckState(r1, r2.Velocity.Magnitude, r2.Position.Y, r2.FuelLevel);
        }

        [TestMethod]
        public void ShouldDetectCrashed()
        {
            var rocket = GetRocket();
            Assert.IsFalse(rocket.IsCrashed);
            rocket.UpdateTime(1);
            rocket.Throttle = 0;
            rocket.UpdateTime(10); //Should crash
            Assert.IsTrue(rocket.IsCrashed);
        }

        public virtual void UpdateTimeTests(double dt, double expected_velocity, double expected_height, double r)
        {
            var rocket = GetRocket();
            rocket.Throttle = r;
            rocket.UpdateTime(dt);
            CheckState(rocket, expected_velocity, expected_height);
            Assert.AreEqual(dt, rocket.CurrentTime);
        }
    }
}
