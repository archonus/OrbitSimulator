using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrbitSimulator;
using OrbitSimulator.Core;
using static OrbitSimulatorTests.TestingUtils;

namespace OrbitSimulatorTests.ViewModelTesting
{
    [TestClass]
    public class SimulationViewModelTests
    {
        static double targetHeight = 500;

        private static SimulationViewModel GetSimViewModel()
        {
            var r = GetMultiStageRocket(AtlasVStages);
            return GetSimViewModel(r);
        }

        private static SimulationViewModel GetSimViewModel(MultiStageRocket r)
        {
            return new SimulationViewModel(r, targetHeight);
        }

        [TestMethod]
        public void CurrentTime_ShouldBeZero()
        {
            var simviewModel = GetSimViewModel();
            Assert.AreEqual(0, simviewModel.CurrentTime);
        }

        [TestMethod]
        public void InitialTimeRatio_ShouldBeOne()
        {
            var simViewModel = GetSimViewModel();
            Assert.AreEqual(1, simViewModel.Timer.TimeRatio);
        }

        [TestMethod]
        public void ChangingTimeRatio_ShouldInvokePropertyChanged()
        {
            var simViewModel = GetSimViewModel();
            simViewModel.PropertyChanged += (obj, e) => Assert.AreEqual(nameof(simViewModel.Timer.TimeRatio), e.PropertyName);
            simViewModel.Timer.ChangeTimeRatio(1);
        }

        [TestMethod]
        public void EjectingStage_ShouldInvokeRocketStateChanged()
        {
            var simViewModel = GetSimViewModel();
            simViewModel.RocketStateChanged += (obj, e) => Assert.IsTrue(e.StageEjected, "StageEjected should be true");
            simViewModel.EjectStage();
            Assert.IsFalse(simViewModel.IsEjectEnabled);
            Assert.AreEqual(2, simViewModel.StageNumber);
        }

    }
}
