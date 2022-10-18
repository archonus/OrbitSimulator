using System;
using static OrbitSimulator.Core.PhysicsUtils;

namespace OrbitSimulator.Core
{
    /// <summary>
    /// Models a single stage rocket using the ideal rocket equations, including gravity but not air resitstance; launched in vacuum with constant gravity field
    /// </summary>
    public class IdealSingleStageRocket : IRocket
    {
        #region IRocket Properties
        public virtual Coord Position
        {
            get { return new Coord(xposition, height); }
            protected set
            {
                xposition = value.X;
                height = value.Y;
                if (height > maxHeight) //To record the max height
                {
                    maxHeight = height;
                }
            }
        }

        public int CurrentStageIndex => 0; //There is only one stage so it is always 0
        public int NumStages => 1;

        public VectorPolar Velocity
        {
            get; protected set;
        }

        public virtual VectorPolar Thrust
        {
            get
            {
                double phi = engine.GimbalAngle + Velocity.Direction;
                if (FuelLevel > 0)
                {
                    return new VectorPolar(Mdot * Ve, phi); //The direction of thrust is the sum of the pitch and the direction of the gimballed engines
                }
                return new VectorPolar(0, phi); //Zero thrust if there is no fuel left
            }
        }

        public double FuelLevel
        {
            get { return currentFuel; }
            protected set
            {
                if (value > 0)
                {
                    currentFuel = value;
                }
                else
                    currentFuel = 0; //Cannot have negative amount of fuel
            }
        }

        public virtual double CurrentMass => payloadMass + structuralMass + FuelLevel + engine.Mass;

        public virtual double Throttle
        {
            get { return throttle; }
            set
            {
                if (value <= 1 && value >= 0) //Value is within range
                {
                    throttle = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(paramName: nameof(value), message: "Value for throttle must be between 0 and 1");
                }
            }
        }

        public bool CanEject => false; //A single stage rocket cannot eject

        public double CurrentTime
        {
            get;
            protected set;
        }

        public bool IsCrashed => Position.Y <= 0 && CurrentTime > 0;

        #endregion

        /// <summary>
        /// The mass flow rate of the rocket, which is the engine's mass flow rate * throttle. It is the magnitude of the derivative of mass with respect to time
        /// </summary>
        protected double Mdot => engine.MassFlowRate * Throttle;
        /// <summary>
        /// The effective exhaust velocity, given by specific impulse * g0
        /// </summary>
        protected double Ve => engine.SpecificImpulse * g0;


        #region Structural fields
        protected RocketEngine engine;
        protected double structuralMass; //Mass of the rocket structure, excluding engine
        protected readonly double payloadMass; //Mass of the payload being carried
        protected readonly double initialMass; //Initial mass of rocket
        #endregion

        #region Backing Fields
        double height;
        double xposition;
        double throttle = 1;
        double currentFuel;
        protected double maxHeight;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs an IdealRocket at rest state
        /// </summary>
        /// <param name="engine">A RocketEngine, with a mass and specific impulse</param>
        /// <param name="payloadMass">The mass of the payload</param>
        /// <param name="fuel">The initial amount of fuel</param>
        /// <param name="structureMass">The mass of the rocket structure, excluding the mass of the engine</param>
        public IdealSingleStageRocket(RocketEngine engine, double payloadMass, double fuel, double structureMass)
        {
            this.payloadMass = payloadMass >= 0 ? payloadMass : 0;
            this.engine = engine;
            this.structuralMass = structureMass >= 0 ? structureMass : 0;
            FuelLevel = fuel >= 0 ? fuel : 0;
            maxHeight = 0;
            CurrentTime = 0;
            initialMass = CurrentMass;
        }

        /// <summary>
        /// Constructs an IdealRocket at a specified state
        /// </summary>
        /// <param name="engine">A RocketEngine, with a mass and specific impulse</param>
        /// <param name="payloadMass">The mass of the payload</param>
        /// <param name="fuel">The initial amount of fuel</param>
        /// <param name="structureMass">The mass of the rocket structure, excluding the mass of the engine</param>
        /// <param name="height">The height at which to start</param>
        /// <param name="xposition">The horizontal position to start in</param>
        /// <param name="currentVelocity">The current velocity of the rocket</param>
        public IdealSingleStageRocket(RocketEngine engine, double payloadMass, double fuel, double structureMass, double height, double xposition, VectorPolar currentVelocity) : this(engine, payloadMass, fuel, structureMass)
        {
            Velocity = currentVelocity;
            maxHeight = height;
            Position = new Coord(xposition, height);
        }
        #endregion

        #region IRocket Methods

        public void UpdateTime(double dt)
        {
            //Validate value of dt
            if (dt <= 0)
            { return; }//No change in time or an invalid input for dt will mean that nothing occurs
            else //Valid dt and not crashed
            {
                IncrementTime(dt);
            }
        }

        public void ChangeThrustDirection(double angle)
        {
            engine.GimbalAngle = angle;
        }

        public virtual bool EjectStage()
        {
            return false; //Single stage rocket cannot eject
        }
        #endregion

        /// <summary>
        /// Calculates the change in velocity given an initial and final mass, using the ideal rocket equation.
        /// </summary>
        /// <param name="m_i">The mass at time = t</param>
        /// <param name="m_f">The mass at time = t + dt; equal to inital_mass - dm</param>
        /// <returns>The vector representing the change in velocity</returns>
        protected virtual VectorPolar CalculateVelocityChange(double m_i, double m_f)
        {
            var dv = Ve * Math.Log(m_i / m_f); //The magnitude of the change in velocity from rocket equation
            var dV = new VectorPolar(dv, Thrust.Direction);
            var dt = (m_i - m_f) / Mdot;
            VectorPolar gEffect = new VectorPolar(-g0 * dt, 0); //The effect of gravity is straight down (phi = 0)
            return dV + gEffect;
        }

        /// <summary>
        /// Calculates the change in position when the rocket ejects propellant, from integrating the ideal rocket equation
        /// </summary>
        /// <param name="m_i">The mass at time = t</param>
        /// <param name="m_f">The mass at time = t + dt</param>
        /// <returns>A VectorPolar representing the change in position</returns>
        protected virtual VectorPolar CalculatePositionChange(double m_i, double m_f)
        {
            var dt = (m_i - m_f) / Mdot; //Calculate the change in time
            var u = Velocity.Magnitude;
            var ds = u * dt +
                    Ve * (
                         dt + (m_f * Math.Log(m_f / m_i)) / Mdot
                         ); //The integral of the ideal rocket equation from t = 0 to t = dt
            VectorPolar gEffect = new VectorPolar(-0.5 * g0 * dt * dt, 0); //The integral of the effect from gravity, only along the y-axis (phi = 0)
            return new VectorPolar(ds, Thrust.Direction) + gEffect;
        }

        /// <summary>
        /// Implements the burning of an amount of fuel and changes the velocity and position of the rocket correspondingly
        /// </summary>
        /// <param name="dm">The amount of fuel burnt</param>
        protected virtual void BurnFuel(double dm)
        {
            if (FuelLevel >= dm) //Fuel cannot be burnt if there is not enough
            {
                double m_i = CurrentMass;
                double m_f = m_i - dm;
                VectorPolar dV = CalculateVelocityChange(m_i, m_f);
                VectorPolar dS = CalculatePositionChange(m_i, m_f);
                //Only change after both velocity and height have been calculated
                Velocity += dV;
                Position += dS; //Translate position by position change vector
                FuelLevel -= dm; //Change the fuel
            }
            else
            {
                throw new ArgumentException("dm cannot be greater than current FuelLevel");
            }

        }

        /// <summary>
        /// Implements the changes in velocity and position when the rocket is falling under the influence of gravity
        /// </summary>
        /// <param name="dt">The time interval</param>
        protected virtual void Fall(double dt)
        {
            if (Thrust.Magnitude == 0) //The rocket should be falling
            {
                var dv = -g0 * dt; //The magnitude of the change in velocity;
                var v0 = Velocity; //Initial velocity
                VectorPolar dV = VectorPolar.FromCartesian(0, dv); //The change in velocity acts only in y direction
                var dy = v0.Y * dt - (0.5 * g0 * dt * dt); //The change in height
                var dx = v0.X * dt; //The change in x position
                VectorPolar dS = VectorPolar.FromCartesian(dx, dy); //Vector representing change in position
                Velocity += dV;
                Position += dS;
            }
        }

        protected virtual void IncrementTime(double dt)
        {
            if (dt < 0) //If there is invalid dt, but should not be needed
            {
                throw new ArgumentOutOfRangeException(nameof(dt), "dt cannot be negative");
            }
            else //Valid dt
            {
                if (currentFuel > 0 && Mdot != 0) //Thrust is provided
                {
                    var dm = Mdot * dt; //Change in the fuel
                    if (FuelLevel >= dm) //There is enough fuel in time dt
                    {
                        BurnFuel(dm);
                    }
                    else //The rocket will burn fuel for only some of the time
                    {
                        var new_dm = FuelLevel; //The change in fuel is how much fuel is left
                        var burnTime = new_dm / Mdot; //The time that is spent burning fuel
                        BurnFuel(new_dm);
                        var timeAfterBurn = dt - burnTime;
                        Fall(timeAfterBurn);
                    }
                }
                else //No fuel burnt
                {
                    Fall(dt);
                }
                CurrentTime += dt; //Increment the time
            }
        }

        /// <summary>
        /// Set the state of the rocket
        /// </summary>
        /// <param name="position">The position</param>
        /// <param name="velocity">The velocity</param>
        /// <param name="time">The time</param>
        internal void SetState(Coord position, VectorPolar velocity, double time = 0)
        {
            Position = position;
            Velocity = velocity;
            CurrentTime = time;
        }

        /// <summary>
        /// Sets the state of this rocket to match the state of another
        /// </summary>
        /// <param name="rocket">The rocket whose state is being copied</param>
        internal void SetState(IRocket rocket)
        {
            SetState(rocket.Position, rocket.Velocity, rocket.CurrentTime);
        }

        public override string ToString()
        {
            string displayString = $"Time:{CurrentTime:0.##}, Height:{Position.Y:0.##}, Speed:{Velocity.Magnitude:0.##}, Pitch:{Velocity.Direction:0.##}, FuelLevel:{FuelLevel:0.##}, Throttle:{Throttle * 100:0.##}%, Mass:{CurrentMass:0.##}, Thrust:{Thrust.Magnitude:0.##}";
            return displayString;
        }
    }
}
