using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrbitSimulator.Core;
using static OrbitSimulatorTests.TestingUtils;

namespace OrbitSimulatorTests.CoreTesting
{
    [TestClass]
    public class SingleStageRocketTests : IRocketTests
    {

        protected override IRocket GetRocket()
        {
            return GetSingleStageRocket(AtlasV_stage1, payload);
        }

        protected IRocket GetRocket(double p, double f, double s, RocketEngine? engine = null)
        {
            return new IdealSingleStageRocket(engine ?? GetRocketEngine(RD180_engine),
                                              p,
                                              f,
                                              s);
        }

        [TestMethod]
        public void MassCalculation()
        {
            var rocket = GetRocket();
            var expectedMass = AtlasV_stage1.PropellantMass + AtlasV_stage1.StructuralMass + AtlasV_stage1.EngineData.Mass + payload;
            Assert.AreEqual(expectedMass, rocket.CurrentMass, DecimalTolerance, "Mass is not correctly calculated");
            Assert.AreEqual(AtlasV_stage1.PropellantMass, rocket.FuelLevel, "Fuel is not correct");
        }

        [TestMethod]
        public void NegativeInitialisation_ShouldIgnore()
        {
            var rocket = GetRocket(-1, -1, -1); //Invalid values
            var expectedMass = RD180_engine.Mass;
            Assert.AreEqual(expectedMass, rocket.CurrentMass, "Mass should only be of the engine");
            Assert.AreEqual(0, rocket.FuelLevel, "Fuel should be zero");
        }

        [TestMethod]
        [DataRow(0)] //No change
        [DataRow(0.5)]
        [DataRow(1)]
        [DataRow(2)]
        public void FuelCorrectlyUsed(double time)
        {
            var rocket = GetRocket();
            var initialFuel = AtlasV_stage1.PropellantMass;
            Assert.AreEqual(initialFuel, rocket.FuelLevel, DecimalTolerance, "Initial fuel is not correct");
            rocket.UpdateTime(time);
            CheckFuelUse(initialFuel, RD180_engine.MassFlowRate, time, rocket.FuelLevel);
        }

        [TestMethod]
        [DataRow(1, 3.78311, 1.88691, 1)] //Full throttle, time = 1
        [DataRow(3, 11.51766, 17.1500850, 1)] //Full throttle, time = 3
        [DataRow(1, 0.3796077, 0.18719445, 0.75)] //0.75 throttle, time = 1
        [DataRow(3, 1.233247, 1.77898005, 0.75)] //0.75 throttle, time = 3
        public override void UpdateTimeTests(double dt, double expected_velocity, double expected_height, double r)
        {
            base.UpdateTimeTests(dt, expected_velocity, expected_height, r);
        }

        //TODO Add falling tests

    }
}
