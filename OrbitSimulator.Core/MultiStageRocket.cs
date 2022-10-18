using System.Collections.Generic;
using System.Linq;

namespace OrbitSimulator.Core
{
    public class MultiStageRocket : IRocket
    {
        protected readonly IdealSingleStageRocket[] Stages; //REVIEW Should abstract this

        /// <summary>
        /// The index of the current stage
        /// </summary>
        /// <remarks>Zero Indexed</remarks>
        protected int stageIndex;

        /// <summary>
        /// The number of stages left
        /// </summary>
        public int NumStagesLeft => NumStages - stageIndex;

        public double TotalFuel
        {
            get
            {
                var slice = Stages.Skip(stageIndex); //Only get the remaining stages
                return slice.Sum(r => r.FuelLevel);
            }
        }

        protected virtual IdealSingleStageRocket ActiveStage => Stages[stageIndex];

        #region IRocket Properties
        public Coord Position => ActiveStage.Position;

        public VectorPolar Velocity => ActiveStage.Velocity;

        public VectorPolar Thrust => ActiveStage.Thrust;

        public double Throttle
        {
            get => ActiveStage.Throttle;
            set => ActiveStage.Throttle = value;
        }

        public virtual double CurrentMass
        {
            get => ActiveStage.CurrentMass;
        }

        public double CurrentTime => ActiveStage.CurrentTime;  //The time that has passed in total

        public virtual double FuelLevel => ActiveStage.FuelLevel;

        public int CurrentStageIndex => stageIndex;

        public int NumStages => Stages.Length;

        public virtual bool CanEject => stageIndex != Stages.Length - 1; //The current stage is not the last one

        public bool IsCrashed => Position.Y <= 0 && CurrentTime > 0;

        #endregion

        #region Constructors
        public MultiStageRocket(IEnumerable<IdealSingleStageRocket> stages)
        {
            Stages = stages.ToArray();
        }
        public MultiStageRocket(params IdealSingleStageRocket[] stages)
        {
            Stages = stages;
        }

        #endregion

        #region IRocket Methods
        public void ChangeThrustDirection(double angle)
        {
            ActiveStage.ChangeThrustDirection(angle);
        }

        public virtual bool EjectStage()
        {
            if (CanEject)
            {
                var NextStage = Stages[stageIndex + 1];
                NextStage.SetState(ActiveStage);
                stageIndex++; //Increment the current stage index
                return true;
            }
            else
            {
                return false; //Did not eject
            }
        }
        public virtual void UpdateTime(double dt)
        {
            ActiveStage.UpdateTime(dt);
        }
        #endregion
        public override string ToString()
        {
            string displayString = $"Time:{CurrentTime:F}, Stage:{stageIndex + 1}, Height:{Position.Y:F}, Speed:{Velocity.Magnitude:F}, Total Fuel:{TotalFuel:F}, Active Fuel:{ActiveStage.FuelLevel:F} Throttle:{Throttle:P}, Mass:{CurrentMass:F}, Thrust:{ActiveStage.Thrust.Magnitude:F}";
            return displayString;
        }
    }
}
