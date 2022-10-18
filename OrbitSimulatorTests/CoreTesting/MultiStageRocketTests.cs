using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrbitSimulator.Core;
using static OrbitSimulatorTests.TestingUtils;

namespace OrbitSimulatorTests.CoreTesting
{
    [TestClass]
    public class MultiStageRocketTests : IRocketTests
    {
        double stage2Mass = AtlasV_stage2.PropellantMass + AtlasV_stage2.StructuralMass + AtlasV_stage2.EngineData.Mass;
        double stage1Mass = AtlasV_stage1.EngineData.Mass + AtlasV_stage1.PropellantMass + AtlasV_stage1.StructuralMass;
        protected override IRocket GetRocket()
        {
            return GetMultiStageRocket(AtlasVStages);
        }

        [TestMethod]
        public void NumStages_Correct()
        {
            var rocket = GetRocket();
            Assert.AreEqual(2, rocket.NumStages);
        }

        [TestMethod]
        public void CurrentStageIndex_ShouldBe0()
        {
            var rocket = GetRocket();
            Assert.AreEqual(0, rocket.CurrentStageIndex);
        }

        [TestMethod]
        [DataRow(0)] //No change
        [DataRow(0.5)]
        [DataRow(1)]
        [DataRow(2)]
        public void FuelBurningTestStage1(double time)
        {
            var rocket = GetRocket();
            var initialFuel = AtlasV_stage1.PropellantMass;
            Assert.AreEqual(initialFuel, rocket.FuelLevel, DecimalTolerance, "Initial fuel is not correct");
            rocket.UpdateTime(time);
            CheckFuelUse(initialFuel, RD180_engine.MassFlowRate, time, rocket.FuelLevel);
        }

        [TestMethod]
        public void Eject_ShouldKeepVelocitySame()
        {
            var rocket = GetRocket();
            rocket.UpdateTime(4);
            var initialVelocity = rocket.Velocity.Magnitude;
            rocket.EjectStage();
            Assert.AreEqual(initialVelocity, rocket.Velocity.Magnitude, DecimalTolerance, "Velocity is incorrect");
        }

        [TestMethod]
        public void Eject_ShouldChangeCurrentStageIndex()
        {
            var rocket = GetRocket();
            rocket.EjectStage();
            Assert.AreEqual(1, rocket.CurrentStageIndex);
            Assert.IsFalse(rocket.CanEject);
        }

        [TestMethod]
        public void Eject_ShouldKeepPositionSame()
        {
            var rocket = GetRocket();
            rocket.UpdateTime(4);
            var initialHeight = rocket.Position.Y;
            rocket.EjectStage();
            Assert.AreEqual(initialHeight, rocket.Position.Y, "Height is incorrect");
        }

        [TestMethod]
        public void Eject_ShouldChangeMass()
        {
            var rocket = GetRocket();
            rocket.UpdateTime(4);
            rocket.EjectStage();
            CheckMass(rocket, stage2Mass + payload, AtlasV_stage2.PropellantMass);
        }

        [TestMethod]
        public void MassCalculationTest()
        {
            var rocket = GetRocket();
            double expectedMass = stage1Mass + stage2Mass + payload;
            CheckMass(rocket, expectedMass, AtlasV_stage1.PropellantMass);
        }
    }
}
