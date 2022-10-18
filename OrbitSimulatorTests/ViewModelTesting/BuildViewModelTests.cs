using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrbitSimulator;
using static OrbitSimulatorTests.TestingUtils;
using DataService = OrbitSimulator.DataService;

namespace OrbitSimulatorTests.ViewModelTesting
{
    [TestClass]
    public class BuildViewModelTests
    {
        private RocketBuildViewModel buildViewModel;
        private double targetHeight = 1000;

        [ClassInitialize]
        public static async Task TestsSetupAsync(TestContext testContext)
        {
            await GenerateDBFile();
        }

        [TestInitialize]
        public void TestInitialise()
        {
            buildViewModel = new RocketBuildViewModel(DBPath, targetHeight);
        }

        [TestMethod]
        public async Task CustomStages_ShouldIncludeExtraOptionOfCustomAsync()
        {
            var stagesList = await buildViewModel.GetStagesTask.ResultTask;
            var stagesListWithCustom = await buildViewModel.GetStagesWithCustomTask.ResultTask;
            Assert.AreEqual(stagesList.Count + 1, stagesListWithCustom.Count);
            Assert.AreEqual("Custom", stagesListWithCustom[0].Name);
        }

        [TestMethod]
        public void NonCustomRocketSettingStages_ShouldFail()
        {
            buildViewModel.SelectedRocket.IsCustom = false;
            Action invalidSetStages = () => buildViewModel.SetRocketStages(new List<DataService.Stage>());
            Assert.ThrowsException<InvalidOperationException>(invalidSetStages);
        }

        [TestMethod]
        public void SaveRocketWithoutName_ShouldFail()
        {
            Action invalidSave = () => buildViewModel.SaveRocketAsync("");
            Assert.ThrowsException<ArgumentException>(invalidSave);
        }

        [TestMethod]
        public async Task CreatingSimulationViewModel_ShouldHaveSameTargetHeightAsync()
        {
            buildViewModel.SelectedRocket = new DataService.Rocket { PayloadMass = 50 };
            buildViewModel.SetRocketStages(AtlasVStages);
            var simViewModel = await buildViewModel.GetSimulationViewModelAsync(stagesSet: true);
            Assert.AreEqual(targetHeight, simViewModel.TargetHeight);
        }

        [TestMethod]
        public async Task SimulationViewModel_ShouldHaveSameRocketDataAsync()
        {
            buildViewModel.SelectedRocket = new DataService.Rocket { PayloadMass = 50 };
            buildViewModel.SetRocketStages(AtlasVStages);
            var simViewModel = await buildViewModel.GetSimulationViewModelAsync(stagesSet: true);
            Assert.AreEqual(2, simViewModel.NumStages, "NumStages is incorrect");
            Assert.AreEqual(AtlasV_stage1.PropellantMass, simViewModel.Fuel);
        }
    }
}
