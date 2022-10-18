using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrbitSimulator.DataService;
using static OrbitSimulatorTests.TestingUtils;

namespace OrbitSimulatorTests.DataServiceTesting
{
    [TestClass]
    public class DatabaseTests
    {

        private RocketDatabase db = new RocketDatabase();

        private static void CheckRocketStages(Rocket rocketData, Stage[] stages)
        {
            for (int i = 0; i < stages.Length; i++)
            {
                RocketStage rocketStage = rocketData.RocketStages[i];
                Assert.AreEqual(rocketData.Id, rocketStage.RocketId, "Rocket stage id does not match");
                CheckStageData(stages[i], rocketStage.StageData);
            }
        }

        private static void CheckStageData(Stage expectedData, Stage actualData)
        {
            Assert.AreEqual(expectedData.PropellantMass, actualData.PropellantMass, "Fuel does not match");
            Assert.AreEqual(expectedData.StructuralMass, actualData.StructuralMass, "Strucutral mass does not match");
            CheckEngineData(expectedData.EngineData, actualData.EngineData);
        }

        private static void CheckEngineData(Engine expectedData, Engine actualData)
        {
            Assert.AreEqual(expectedData.Mass, actualData.Mass, "Engine mass does not match");
            Assert.AreEqual(expectedData.MassFlowRate, actualData.MassFlowRate, "Mass flow rate does not match");
            Assert.AreEqual(expectedData.SpecificImpulse, actualData.SpecificImpulse, "Specific impulse does not match");
        }

        [ClassInitialize] //Executes once per suite of testing
        public static async Task TestSuiteInitialisation(TestContext context)
        {
            await GenerateDBFile(); //Overwrites any existing file to ensure that all the tests run in the same environment
        }

        [TestInitialize]
        public async Task TestInitialise()
        {
            if (!db.IsConnectionOpen)
            {
                await db.InitialiseConnectionAsync(DBPath);
            }
        }

        [TestMethod]
        public void ConnectingToDB_ShouldOpenConnection()
        {
            Assert.IsTrue(db.IsConnectionOpen);
        }

        [TestMethod]
        public async Task RetrievingRockets_ShouldReturn4RocketsAsync()
        {
            var rocketsData = await db.GetAllRocketsAsync();
            Assert.IsTrue(rocketsData.Count >= 4);
        }

        [TestMethod]
        public async Task RetrievingInvalidId_ShouldReturnNullAsync()
        {
            var rocketData = await db.GetRocketFromIdAsync(-1); //Should return null
            Assert.IsNull(rocketData, "Should have returned null");
        }

        [TestMethod]
        public async Task RetrievedRocket_ShouldMatchDataAsync()
        {
            var rocketData = await db.GetRocketFromIdAsync(1);
            Assert.AreEqual(rocketData.IsCustom, false);
        }

        [TestMethod]
        public async Task RetrievedRocketStages_ShouldMatchDataAsync()
        {
            var rocketData = await db.GetRocketFromIdAsync(1);
            var stage1Data = rocketData.RocketStages[0].StageData;
            var stage2Data = rocketData.RocketStages[1].StageData;
            CheckStageData(AtlasV_stage1, stage1Data);
            CheckStageData(AtlasV_stage2, stage2Data);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        public async Task RetrievedRocket_ShouldHaveStages(int id)
        {
            var rocketData = await db.GetRocketFromIdAsync(id);
            Assert.IsNotNull(rocketData, "Failed to retrieve data from database");
            Assert.IsTrue(rocketData.NumStages == 2);
        }

        [TestMethod]
        public async Task RetrievingStages_ShouldReturn8Async()
        {
            var stagesData = await db.GetAllStagesAsync();
            Assert.IsTrue(stagesData.Count >= 8);
        }

        [TestMethod]
        public async Task InitialisingStage_ShouldMatchDataAsync()
        {
            var stageData = await db.GetStageFromIdAsync(3);
            CheckStageData(AtlasV_stage1, stageData);
        }

        [TestMethod]
        public async Task NoNameRocketInsertion_ShouldFailAsync()
        {
            var rocketData = new Rocket();
            Func<Task> invalidSaveTask = async () => await db.SaveRocketAsync(rocketData);
            var e = await Assert.ThrowsExceptionAsync<ArgumentException>(invalidSaveTask);
            Assert.AreEqual(nameof(rocketData.Name), e.ParamName);
        }

        [TestMethod]
        public async Task StagesMissing_ShouldFailAsync()
        {
            var rocketData = new Rocket { Name = "Atlas V" };
            Func<Task> invalidSaveTask = async () => await db.SaveRocketAsync(rocketData);
            var e = await Assert.ThrowsExceptionAsync<ArgumentException>(invalidSaveTask);
            Assert.AreEqual(nameof(rocketData.RocketStages), e.ParamName);
        }

        [TestMethod]
        public async Task NameNotUnique_ShouldFailAsync()
        {
            var rocketData = new Rocket { Name = "Atlas V" };
            rocketData.SetStages(AtlasVStages);
            Func<Task> invalidSaveTask = async () => await db.SaveRocketAsync(rocketData);
            await Assert.ThrowsExceptionAsync<NameNotUniqueException>(invalidSaveTask);
        }

        [TestMethod]
        public async Task SaveRocket_ShouldUpdateTablesAsync()
        {
            var savedRocket = new Rocket { Name = "TestRocket 1", PayloadMass = 20 };
            savedRocket.SetStages(AtlasVStages);
            int id = await db.SaveRocketAsync(savedRocket);

            var retrievedRocket = await db.GetRocketFromIdAsync(id);
            Assert.AreEqual(savedRocket.Name, retrievedRocket.Name, "Name does not match");
            Assert.AreEqual(savedRocket.NumStages, retrievedRocket.NumStages, "Number of stages does not match");
            Assert.AreEqual(savedRocket.PayloadMass, retrievedRocket.PayloadMass, "Payload mass does not match");
            Assert.IsTrue(retrievedRocket.IsCustom);
            CheckRocketStages(retrievedRocket, AtlasVStages);
        }

        [TestMethod]
        public async Task SaveRocket_WithCustomStagesAsync()
        {
            var savedRocket = new Rocket { Name = "TestRocket 2", PayloadMass = 30 };
            var engineData = new Engine { Name = "Custom Engine", Mass = 10, MassFlowRate = 50, SpecificImpulse = 3 };
            var customStage = new Stage() { Name = "Custom Stage", PropellantMass = 20, StructuralMass = 12, EngineData = engineData };
            var stages = new Stage[3] { AtlasV_stage1, AtlasV_stage2, customStage };
            savedRocket.SetStages(stages);
            int id = await db.SaveRocketAsync(savedRocket);

            var retrievedRocket = await db.GetRocketFromIdAsync(id);
            Assert.AreEqual(savedRocket.Name, retrievedRocket.Name, "Name does not match");
            Assert.AreEqual(savedRocket.NumStages, retrievedRocket.NumStages, "Number of stages does not match");
            Assert.AreEqual(savedRocket.PayloadMass, retrievedRocket.PayloadMass, "Payload mass does not match");
            Assert.IsTrue(retrievedRocket.IsCustom);
            CheckRocketStages(retrievedRocket, stages);

            var retrievedStages = await db.GetAllStagesAsync();
            Assert.IsTrue(retrievedStages.Exists(s => s.Name == "Custom Stage"), "Could not find custom stage");

            var retrivedEngines = await db.GetAllEnginesAsync();
            Assert.IsTrue(retrivedEngines.Exists(engine => engine.Name == "Custom Engine"), "Could not find custom engine");
        }

        [TestMethod]
        public async Task SavingRocketTwice_ShouldFailAsync()
        {
            var savedRocket = new Rocket { Name = "TestRocket 3" };
            savedRocket.SetStages(AtlasVStages);
            Func<Task> saveTask = async () => await db.SaveRocketAsync(savedRocket);
            await saveTask();
            await Assert.ThrowsExceptionAsync<RecordAlreadyExistsException>(saveTask);
        }

    }
}
