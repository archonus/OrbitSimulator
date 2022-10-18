using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace OrbitSimulator
{
    /// <summary>
    /// View model for representing the timer in the simulation
    /// </summary>
    public class TimerViewModel : BindingSourceBase
    {
        private double timeRatio = 1;
        private readonly int[] timeRatioValues = new int[] { 1, 2, 5, 10, 25, 50, 100 };
        private int timeRatioIndex = 0;
        private bool isRunning;
        private double timerInterval;

        public double TimeRatio
        {
            get => timeRatio;
            private set => SetAndNotifyField(ref timeRatio, value);
        }

        public bool IsRunning
        {
            get => isRunning;
            set => SetAndNotifyField(ref isRunning, value);
        }

        public double TimerInterval
        {
            get => timerInterval;
            set => SetAndNotifyField(ref timerInterval, value);
        }

        public ICommand SpeedUpTimeCommand { get; }
        public ICommand SlowDowTimeCommand { get; }

        public TimerViewModel()
        {
            SpeedUpTimeCommand = new Command
                (
                () => ChangeTimeRatio(1)
                );
            SlowDowTimeCommand = new Command
                (
                () => ChangeTimeRatio(-1)
                );
        }

        public TimerViewModel(int[] timeRatioValues) : this()
        {
            this.timeRatioValues = timeRatioValues;
        }

        /// <summary>
        /// Change the value of the time ratio by incrementing the index by the value
        /// </summary>
        /// <param name="incrementValue">How much to move the pointer by</param>
        public void ChangeTimeRatio(int incrementValue)
        {
            var calculatedIndex = timeRatioIndex + incrementValue;

            var actualIndex = Math.Abs(calculatedIndex); //The actual index of the array must be positive
            if (actualIndex >= timeRatioValues.Length)
            { //If the index would have gone beyond the array's bounds, set it to the last value
                actualIndex = timeRatioValues.Length - 1;
            }

            int value = timeRatioValues[actualIndex];
            if (calculatedIndex >= 0)
            { //The index is positive
                TimeRatio = value;
                timeRatioIndex = actualIndex; //Update the value
            }
            else //The calculated index is negative
            { //Take the reciprocal. This effectively extends the possible range of values
                TimeRatio = 1.0 / value; //1.0 to ensure double division not integer
                timeRatioIndex = -actualIndex; //Update the value to the negative of actualIndex
            }
        }
    }
}
