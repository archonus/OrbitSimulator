using System.Collections.Generic;
using OrbitSimulator.Core;

namespace OrbitSimulator.Factory
{
    public static class MultiStageRocketFactory
    {
        /// <summary>
        /// Constructs a <see cref="MultiStageRocket"/> from the provided data
        /// </summary>
        /// <param name="rocketData">The data about the rocket</param>
        /// <returns>A fully initialised <see cref="MultiStageRocket"/></returns>
        public static MultiStageRocket ConstructMultiStageRocket(DataService.Rocket rocketData)
        {
            double current_payload = rocketData.PayloadMass; //The payload of the final stage is the final payload
            var stagesDataStack = new Stack<DataService.RocketStage>(rocketData.RocketStages); //Stack of the stages data. The final stage is at the top
            var stagesStack = new Stack<IdealSingleStageRocket>(rocketData.NumStages); //Stack for the actual stages of the rocket
            for (int i = 0; i < rocketData.NumStages; i++)
            { //Iterate through the stagesData stack, by popping from it and pushing a corressponding instance onto the stagesStack
                var rocketStage = stagesDataStack.Pop(); //Gets the top of the stack, and removes it
                var currentStage = SingleStageRocketFactory.ConstructIdealSingleStageRocket(rocketStage.StageData, current_payload);
                current_payload = currentStage.CurrentMass; //The payload of the next stage will be all of the current stage
                stagesStack.Push(currentStage); //Push the current stage onto the stack, so that the first stage is accessed first
            }
            return new MultiStageRocket(stagesStack.ToArray()); //Create the multi-stage rocket from the stack of stages.
        }


    }
}
