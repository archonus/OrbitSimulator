namespace OrbitSimulator.Core
{
    public interface IRocket
    {
        /// <summary>
        /// The current position of the rocket
        /// </summary>
        Coord Position { get; }

        /// <summary>
        /// The current velocity of the rocket
        /// </summary>
        VectorPolar Velocity { get; }

        /// <summary>
        /// The current thrust level of the rocket
        /// </summary>
        VectorPolar Thrust { get; }

        /// <summary>
        /// The throttle of the rocket, which is a number between 0 and 1
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">Value of throttle set out of 0 to 1</exception> 
        double Throttle { get; set; }

        /// <summary>
        /// The amount of fuel available to the rocket
        /// </summary>
        double FuelLevel { get; }

        /// <summary>
        /// The current mass of the rocket
        /// </summary>
        /// <remarks>The sum of the engine mass, fuel, payload and structural mass</remarks>
        double CurrentMass { get; }

        /// <summary>
        /// The time that has passed since launch
        /// </summary>
        double CurrentTime { get; }

        /// <summary>
        /// Whether the rocket can eject its current stage
        /// </summary>
        /// <remarks>A value of false means that the rocket is in its final stage</remarks>
        bool CanEject { get; }

        /// <summary>
        /// The index of the active stage from the list of stages
        /// </summary>
        /// <remarks>Zero indexed</remarks>
        int CurrentStageIndex { get; }

        /// <summary>
        /// The total number of stages
        /// </summary>
        int NumStages { get; }

        /// <summary>
        /// Whether the rocket has crashed
        /// </summary>
        bool IsCrashed { get; }

        /// <summary>
        /// Update the time value by dt seconds
        /// </summary>
        /// <param name="dt">The change in time; must be positive</param>
        void UpdateTime(double dt);

        /// <summary>
        /// Change the direction of thrust
        /// </summary>
        /// <param name="angle">The change in angle of the thrust</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the value is out of range</exception>
        void ChangeThrustDirection(double angle);

        /// <summary>
        /// Eject a stage of the rocket.
        /// </summary>
        /// <returns>True if successful, false is not</returns>
        bool EjectStage();
    }
}
