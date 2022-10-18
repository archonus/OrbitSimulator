using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrbitSimulator.DataService;
using static OrbitSimulatorTests.TestingUtils;

namespace OrbitSimulatorTests.DataServiceTesting
{
    [TestClass]
    public class DatabaseEntitiesTests
    {
        private static Stage[] stagesData => new Stage[] { AtlasV_stage1, AtlasV_stage2 };

        private static void CheckIdOfRocketStages(Rocket rocketData)
        {
            foreach (var rocketStage in rocketData.RocketStages)
            {
                Assert.AreEqual(rocketData.Id, rocketStage.RocketId);
            }
        }

        [TestMethod]
        public void SetStages_ShouldSetRocketIds()
        {
            var rocketData = new Rocket { Id = 5 };
            rocketData.SetStages(stagesData);
            Assert.AreEqual(stagesData.Length, rocketData.NumStages);
            CheckIdOfRocketStages(rocketData);
        }

        [TestMethod]
        public void SetStagesArgumentNull_ShouldThrowException()
        {
            var rocketData = new Rocket();
            Action invalidSetStages = () => rocketData.SetStages(null);
            Assert.ThrowsException<ArgumentNullException>(invalidSetStages);
        }

        [TestMethod]
        public void ChangingRocketId_ShouldChangeRocketStageRocketIds()
        {
            var rocketData = new Rocket { Id = 5 };
            rocketData.SetStages(stagesData);
            CheckIdOfRocketStages(rocketData);
            rocketData.Id = 6;
            CheckIdOfRocketStages(rocketData);
        }

        [TestMethod]
        public void ChangingStageId_ShouldChangeStageDataId()
        {
            var rocketStage = new RocketStage
            {
                StageData = new Stage() { Id = 4 }
            };
            Assert.AreEqual(4, rocketStage.StageId);
            rocketStage.StageId = 5;
            Assert.AreEqual(5, rocketStage.StageData.Id);
        }

        [TestMethod]
        public void ChangingEngineId_ShouldChangeEngineDataId()
        {
            var stage = new Stage
            {
                EngineData = new Engine { Id = 1 }
            };
            Assert.AreEqual(1, stage.EngineId);
            stage.EngineId = 3;
            Assert.AreEqual(3, stage.EngineData.Id);
        }
    }
}
