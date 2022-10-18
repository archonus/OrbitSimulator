using System;
using System.Windows.Input;
using OrbitSimulator.Core;
using Xamarin.Forms;

namespace OrbitSimulator
{

    public class RocketStateChangedArgs : EventArgs
    {
        public bool StageEjected = false;
        public string Message = null;
        public bool FullBurnReached = false;
        public bool CanEject;
    }

    /// <summary>
    /// The view model for an IRocket simulation
    /// </summary>
    public class SimulationViewModel : BindingSourceBase
    {

        #region Events

        /// <summary>
        /// Occurs when the state of the rocket is changed
        /// </summary>
        public event EventHandler<RocketStateChangedArgs> RocketStateChanged;

        /// <summary>
        /// Occurs when the simulation ends
        /// </summary>
        public event EventHandler SimulationOver;
        #endregion

        #region Private Fields
        double targetHeight;
        bool simulationOver = false;
        bool isPoweredFlight;

        bool notifiedFullBurn = false;
        bool notifiedEnteredUpperAtmosphere = false;
        bool notifiedEscapedAtmosphere = false;

        Color skyColor = SkyColourHelper.StartingColour;
        #endregion

        #region Rocket Properties

        /// <summary>
        /// The height of the rocket, in kilometres
        /// </summary>
        /// <remarks>Never negative</remarks>
        public double Height => Rocket.Position.Y >= 0 ? PhysicsUtils.ConvertMetresToKm(Rocket.Position.Y) : 0;

        /// <summary>
        /// The speed of the rocket in km/hour
        /// </summary>
        /// <remarks>Signed speed - if the rocket is moving downwards then it is negative</remarks>
        public double Speed
        {
            get
            {
                var velocity = Rocket.Velocity;
                var speed = PhysicsUtils.ConvertMpsToKmph(Rocket.Velocity.Magnitude);
                return velocity.IsDownwards ? -speed : speed;
            }
        }

        public double Fuel => Rocket.FuelLevel; //REVIEW Fuel level as percentage
        public string RocketString => Rocket.ToString();
        public double CurrentTime => Rocket.CurrentTime;

        /// <summary>
        /// The direction of the rocket in degrees
        /// </summary>
        public double Direction => PhysicsUtils.ConvertRadiansToDegrees(Rocket.Velocity.Direction);

        public double Throttle
        {
            get => Rocket.Throttle;
            set
            {
                Rocket.Throttle = value;
                OnPropertyChanged();
                if (value == 0)
                {
                    InPoweredFlight = false; //The rocket is not in powered flight
                }
            }
        }

        public double GimbalAngle
        {
            get => PhysicsUtils.ConvertRadiansToDegrees(Rocket.Thrust.Direction - Rocket.Velocity.Direction);
        }

        /// <summary>
        /// Which stage is currently active
        /// </summary>
        /// <remarks>Adjusting the zero index to start at 1</remarks>
        public int StageNumber { get => Rocket.CurrentStageIndex + 1; }

        /// <summary>
        /// The total number of stages
        /// </summary>
        public int NumStages { get => Rocket.NumStages; }

        public bool IsCrashed => Rocket.IsCrashed;

        public bool IsEjectEnabled => Rocket.CanEject && !IsSimulationOver;
        #endregion

        public double TargetHeight
        {
            get => targetHeight;
            set => SetAndNotifyField(ref targetHeight, value);
        }

        public bool InPoweredFlight
        {
            get => isPoweredFlight;
            set => SetAndNotifyField(ref isPoweredFlight, value);
        }

        public TimerViewModel Timer { get; }

        public IRocket Rocket
        {
            protected get;
            set;
        } //Effectively a write-only property from the outside

        /// <summary>
        /// The average length of a rocket, in kilometres
        /// </summary>
        /// <remarks>For scaling purposes</remarks>
        public double RocketLength => 0.025 * Rocket.NumStages; // An approximation of the height of the rocket based on the number of stages - most rockets are 2 stage and ~ 50 m

        public Color SkyColour
        {
            get => skyColor;
            protected set => SetAndNotifyField(ref skyColor, value);
        }

        public double EffectiveOrbitalSpeed => PhysicsUtils.ConvertMpsToKmph(Rocket.Velocity.Magnitude) + PhysicsUtils.GetTangentialVelocity();

        /// <summary>
        /// Whether the simulation is finished
        /// </summary>
        /// <remarks>If true, then it cannot be started again</remarks>
        public bool IsSimulationOver
        {
            get => simulationOver;
            set => SetAndNotifyField(ref simulationOver, value);
        }

        #region Commands

        public ICommand PlayPauseCommand { get; }

        public ICommand EjectCommand { get; }

        #endregion

        #region Constructors
        /// <summary>
        /// Initialises a <see cref="SimulationViewModel"/> without a rocket specified. 
        /// </summary>
        private SimulationViewModel()
        {
            PlayPauseCommand = new Command(PlayPause);
            EjectCommand = new Command(
                execute: EjectStage,
                canExecute: () => Rocket.CanEject);
            Timer = new TimerViewModel();
        }

        /// <summary>
        /// Initialises a <see cref="SimulationViewModel"/> with specified rocket as its source 
        /// </summary>
        /// <param name="rocket">The rocket being used</param>
        /// <param name="timerInterval">The interval for the </param>
        /// <param name="targetHeight">The target height for orbit in metres</param>
        public SimulationViewModel(IRocket rocket, double targetHeight, double timerInterval = 0.5) : this()
        {
            Timer.TimerInterval = timerInterval;
            this.Rocket = rocket;
            this.targetHeight = targetHeight;
        }
        #endregion

        #region Simulation Methods

        /// <summary>
        /// Update the state of the rocket by specified amount of time
        /// </summary>
        /// <param name="dt">The time in seconds to be updated</param>
        /// <remarks>If the rocket crashed, will do nothing</remarks>
        public void UpdateRocketState(double dt)
        {
            if (IsSimulationOver || IsCrashed) //Do nothing if the rocket is crashed
            {
                return;
            }
            Rocket.UpdateTime(dt);
            var c = SkyColourHelper.GetColor(Height); //Calculate the new colour of the sky
            if (SkyColour != c) //Only set if it is different
            {
                SkyColour = c;
            }
            //As long as the thrust of the rocket is not zero (and it is actually in the air), the rocket is in powered flight
            InPoweredFlight = (Rocket.Thrust.Magnitude != 0) && (Height > 0);
            CheckStateAndNotify(); //Check the state of the rocket and invoke the appropriate events
        }

        /// <summary>
        /// Checks the state of the rocket and raises the appropriate events
        /// </summary>
        private void CheckStateAndNotify()
        {
            string message = null;
            bool fullBurn = false;
            if (Rocket.FuelLevel == 0 && !notifiedFullBurn)
            { //If the rocket is out of fuel, raise the event only if it hasn't already been raised
                message = "Full Burn Reached";
                fullBurn = true;
                notifiedFullBurn = true;
            }
            if (Rocket.IsCrashed)
            { //The simulation is over
                OnSimulationOver();
                return;
            }
            else if (Height >= TargetHeight)
            { //The rocket has reached the target height and the simulation is over
                OnSimulationOver();
                return;
            }
            else if (Height >= PhysicsUtils.TroposphereEdge && !notifiedEnteredUpperAtmosphere)
            { //If not already notified rocket has reached upper atmosphere, invoke event
                message = "Reached the upper atmosphere";
                notifiedEnteredUpperAtmosphere = true;
            }
            else if (Height >= PhysicsUtils.AtmosphereEdge && !notifiedEscapedAtmosphere)
            { //If not already notified rocket has escaped atmosphere
                message = "Escaped Earth's atmosphere";
                notifiedEscapedAtmosphere = true;
            }
            OnRocketStateChanged(message, fullBurn); //Invoke event
        }

        /// <summary>
        /// Starts the simulation timer
        /// </summary>
        /// <remarks>If either of <see cref="TimerInterval"/> or <see cref="TimeRatio"/> is zero, will not do anything</remarks>
        public void StartSimulation()
        {
            if (IsSimulationOver)
                return; //If the simulation is over, it cannot be restarted with the current instance
            if (Timer.TimerInterval != 0 && Timer.TimeRatio != 0)
            {
                Timer.IsRunning = true;
                Device.StartTimer(
                    TimeSpan.FromSeconds(Timer.TimerInterval), //The interval of the timer
                    () => //The callback function that is called at the interval specified
                    {
                        UpdateRocketState(Timer.TimerInterval * Timer.TimeRatio); //The time passed in the simulation
                        return Timer.IsRunning; //Returning false stops the timer
                    });
            }
        }

        /// <summary>
        /// Change the gimbal angle of the rocket
        /// </summary>
        /// <param name="angle">The angle in degrees to the vertical axis, in degrees</param>
        public void ChangeThrustDirection(double angle)
        {
            var radianValue = PhysicsUtils.ConvertDegreesToRadians(angle); //Convert to radians
            try
            {
                Rocket.ChangeThrustDirection(radianValue);
                OnPropertyChanged();
            }
            catch (ArgumentOutOfRangeException)
            { //The angle is out of range
                //Do nothing
            }
        }

        /// <summary>
        /// Stops the simulation timer
        /// </summary>
        public void StopSimulation()
        {
            Timer.IsRunning = false;
        }

        public void EjectStage()
        {
            var successfullyEjected = Rocket.EjectStage(); //Whether the rocket actually ejected
            if (successfullyEjected)
            { //If the stage was actually ejected
                notifiedFullBurn = false;
                OnRocketStateChanged(stageEjected: true); //The state of the rocket has changed, so the user interface should be updated
            }
        }

        public void PlayPause()
        {
            if (Timer.IsRunning)
            { //The timer is running
                StopSimulation();
            }
            else
            {
                StartSimulation();
            }
        }
        #endregion

        #region Invoking Events

        /// <summary>
        /// Method for when the state of the entire rocket has changed. Invokes <see cref="RocketStateChanged"/> 
        /// </summary>
        protected virtual void OnRocketStateChanged(string message = null, bool fullBurnReached = false, bool stageEjected = false)
        {
            var eventArgs = new RocketStateChangedArgs //Create the event arguments object
            {
                Message = message,
                CanEject = Rocket.CanEject,
                FullBurnReached = fullBurnReached,
                StageEjected = stageEjected
            };
            OnPropertyChanged(string.Empty); //For when all the bindings need to be refreshed - easier than invoking for all the properties separately
            RocketStateChanged?.Invoke(this, eventArgs); //For when being handled outside the INotify framework
        }

        /// <summary>
        /// When the simulation ends
        /// </summary>
        /// <remarks>Either the target height is achieved or the rocket has crashed</remarks>
        protected virtual void OnSimulationOver()
        {
            StopSimulation();
            SimulationOver?.Invoke(this, EventArgs.Empty);
            IsSimulationOver = true;
        }
        #endregion
    }
}
