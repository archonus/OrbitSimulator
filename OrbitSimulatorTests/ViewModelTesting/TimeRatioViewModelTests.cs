using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrbitSimulator;

namespace OrbitSimulatorTests.ViewModelTesting
{

    [TestClass]
    public class TimeRatioViewModelTests
    {
        TimerViewModel viewModel;
        readonly int[] timeRatioValues = new int[] { 1, 2, 5, 10, 25, 50, 100 };

        [TestInitialize]
        public void TestInitialise()
        {
            viewModel = new TimerViewModel(timeRatioValues);
            viewModel.PropertyChanged += (obj, e) => Assert.AreEqual(nameof(viewModel.TimeRatio), e.PropertyName);
        }

        [TestMethod]
        public void IncrementTimeRatio_ShouldIncrease()
        {
            viewModel.ChangeTimeRatio(2);
            Assert.AreEqual(5, viewModel.TimeRatio);
        }

        [TestMethod]
        public void DecrementTimeRatio_ShouldDecrease()
        {
            viewModel.ChangeTimeRatio(-3);
            Assert.AreEqual(0.1, viewModel.TimeRatio);
        }

        [TestMethod]
        public void IncrementBeyondLimit_ShouldDoNothing()
        {
            viewModel.ChangeTimeRatio(timeRatioValues.Length);
            Assert.AreEqual(100, viewModel.TimeRatio);
        }

        [TestMethod]
        public void DecrementBeyondLimit_ShouldDoNothing()
        {
            viewModel.ChangeTimeRatio(-timeRatioValues.Length);
            Assert.AreEqual(0.01, viewModel.TimeRatio);
        }
    }
}
