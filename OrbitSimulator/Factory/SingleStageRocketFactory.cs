using OrbitSimulator.Core;

namespace OrbitSimulator.Factory
{
    public static class SingleStageRocketFactory
    {
        /// <summary>
        /// Constructs a <see cref="RocketEngine"/> object from the data provided
        /// </summary>
        /// <param name="engineData">The specifications of the engine</param>
        public static RocketEngine ConstructEngine(DataService.Engine engineData)
        { //Calls the constructor with the correct arguments
            return new RocketEngine(engineData.Mass, engineData.SpecificImpulse, engineData.MassFlowRate);
        }

        /// <summary>
        /// Constructs a <see cref="IdealSingleStageRocket"/> object from the data provided
        /// </summary>
        /// <param name="stageData">The specifications of the rocket</param>
        /// <param name="payloadMass">The mass of the payload</param>
        public static IdealSingleStageRocket ConstructIdealSingleStageRocket(DataService.Stage stageData, double payloadMass)
        {
            var engine = ConstructEngine(stageData.EngineData); //Create the engine
            return new IdealSingleStageRocket(engine, payloadMass, stageData.PropellantMass, stageData.StructuralMass);
        }


    }
}
