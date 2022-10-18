using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using OrbitSimulator.DataService;
using OrbitSimulator.Factory;

namespace OrbitSimulator
{
    public class RocketBuildViewModel : BindingSourceBase
    {
        readonly RocketDatabase database = new RocketDatabase();
        readonly string dbConnPath;
        double targetHeight;
        private BindableTask<List<Rocket>> getRocketsTask;
        private BindableTask<List<Stage>> getStagesTask;
        private BindableTask<List<Engine>> getEnginesTask;
        private BindableTask<List<Stage>> getStagesWithCustomTask;
        private Rocket selectedRocketData = new Rocket();

        public BindableTask<List<Rocket>> GetRocketsTask
        {
            get => getRocketsTask;
            private set => SetAndNotifyField(ref getRocketsTask, value);
        }
        public BindableTask<List<Stage>> GetStagesTask
        {
            get => getStagesTask;
            private set => SetAndNotifyField(ref getStagesTask, value);
        }
        public BindableTask<List<Engine>> GetEnginesTask
        {
            get => getEnginesTask;
            private set => SetAndNotifyField(ref getEnginesTask, value);
        }

        /// <summary>
        /// A <see cref="BindableTask{T}"/> that represents the task of retrieving the stages and adding another stage at the start to represent the custom stage
        /// </summary>
        public BindableTask<List<Stage>> GetStagesWithCustomTask
        {
            get => getStagesWithCustomTask;
            private set => SetAndNotifyField(ref getStagesWithCustomTask, value);
        }

        /// <summary>
        /// The chosen rocket data
        /// </summary>
        public Rocket SelectedRocket
        {
            get => selectedRocketData;
            set => SetAndNotifyField(ref selectedRocketData, value);
        }

        public double PayloadMass { get; set; }

        public RocketBuildViewModel(string dbConnPath, double targetHeight,double payloadMass = 0)
        {
            this.dbConnPath = dbConnPath;
            LoadData();
            this.targetHeight = targetHeight;
            this.PayloadMass = payloadMass;
        }


        #region Loading Data

        /// <summary>
        /// Sets all the data variables
        /// </summary>
        protected void LoadData()
        {
            //Set the bindable tasks
            GetRocketsTask = new BindableTask<List<Rocket>>(LoadRocketsAsync(), new List<Rocket>());
            GetStagesTask = new BindableTask<List<Stage>>(LoadStagesAsync(), new List<Stage>());
            GetEnginesTask = new BindableTask<List<Engine>>(LoadEnginesAsync(), new List<Engine>());
            var emptyStageData = new Stage { Name = "Custom" };
            GetStagesWithCustomTask = new BindableTask<List<Stage>>
                (
                    Task.Run(async () =>
                    {
                        var stages = new List<Stage>(await GetStagesTask.ResultTask); //Creates a new list from the old
                        stages.Insert(0, emptyStageData); //Add the extra stage to represent custom
                        return stages;
                    }),
                    new List<Stage> { emptyStageData } //The default value is a list with just the custom stage in it
                );
        }

        protected async Task<List<Rocket>> LoadRocketsAsync()
        {
            if (!database.IsConnectionOpen)
            { //Connection is not open
                await database.InitialiseConnectionAsync(dbConnPath);
            }
            //Do not need to get fully initialised rockets as it is not necessary
            return await database.GetAllRocketsAsync(fullyInitialised: false);
        }

        protected async Task<List<Stage>> LoadStagesAsync()
        {
            if (!database.IsConnectionOpen) //Connection is not open
            {
                await database.InitialiseConnectionAsync(dbConnPath);
            }
            return await database.GetAllStagesAsync(fullyInitialised: true); //The stages should be fully initialised

        }
        protected async Task<List<Engine>> LoadEnginesAsync()
        {
            if (!database.IsConnectionOpen) //Connection is not open
            {
                await database.InitialiseConnectionAsync(dbConnPath);
            }
            return await database.GetAllEnginesAsync();
        }
        #endregion

        /// <summary>
        /// Constructs a <see cref="Core.MultiStageRocket"/> from the provided data.
        /// </summary>
        /// <param name="rocketData">The data about the rocket</param>
        /// <param name="stagesSet">Whether the rocket has its stages list initialised or not</param>
        /// <returns>A fully initialised <see cref="Core.MultiStageRocket"/></returns>
        protected async Task<Core.MultiStageRocket> ConstructRocketAsync(Rocket rocketData, bool stagesSet)
        {
            if (!stagesSet)
            { //First set the stages with the correct data
                rocketData = await database.InitialiseRocket(rocketData);
            }
            //If there was an error, rocketData will be null, and null will be returned
            return rocketData is null ? null : MultiStageRocketFactory.ConstructMultiStageRocket(rocketData);
        }

        /// <summary>
        /// Sets the list of stages of <see cref="SelectedRocket"/>
        /// </summary>
        /// <param name="stages">The list of stages to be stored</param>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="SelectedRocket"/> is marked as non-custom</exception>
        public void SetRocketStages(IList<Stage> stages)
        {
            if (SelectedRocket.IsCustom)
            {
                SelectedRocket.SetStages(stages);
            }
            else
            {
                throw new InvalidOperationException("Selected rocket is non-editable");
            }
        }

        /// <summary>
        /// Saves <see cref="SelectedRocket"/> to the database
        /// </summary>
        /// <param name="name">The name of the rocket to be saved</param>
        /// <exception cref="ArgumentException">Thrown if the name provided is null</exception>
        public Task SaveRocketAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty", nameof(name));
            }
            SelectedRocket.Name = name;
            SelectedRocket.PayloadMass = PayloadMass;
            return SaveRocketAsync(SelectedRocket);
        }

        /// <summary>
        /// Saves a rocket to the database
        /// </summary>
        /// <param name="rocketData">The data to be inserted into the database</param>
        /// <exception cref="ArgumentNullException">Thrown if rocketData is null</exception>
        public async Task SaveRocketAsync(Rocket rocketData)
        {
            if (rocketData is null)
            {
                throw new ArgumentNullException(nameof(rocketData));
            }

            Debug.Assert(rocketData.IsCustom, "The rocket being saved is not marked as custom");
            Debug.Assert(!string.IsNullOrEmpty(rocketData.Name), "The rocket name is null");
            if (!database.IsConnectionOpen) //If the connection is closed
            {
                await database.InitialiseConnectionAsync(dbConnPath);
            }
            await database.SaveRocketAsync(rocketData);
        }

        /// <summary>
        /// Initialises a <see cref="Stage"/> object by initialising its <see cref="Stage.EngineData"/>
        /// </summary>
        /// <param name="stageData">The object to be initialised</param>
        public Task InitialiseStage(Stage stageData)
        {
            return database.InitialiseStage(stageData);
        }

        /// <summary>
        /// Constructs an initialised <see cref="SimulationViewModel"/> using the data about the rocket
        /// </summary>
        /// <param name="rocketData">The data about the rocket to be used in the <see cref="SimulationViewModel"/> </param>
        /// <param name="stagesSet">Whether the rocket has its stages set or not</param>
        /// <returns>An initialised <see cref="SimulationViewModel"/> with its <see cref="SimulationViewModel.Rocket"/> initialised.</returns>
        public async Task<SimulationViewModel> GetSimulationViewModelAsync(Rocket rocketData, bool stagesSet = false)
        {
            if (rocketData is null)
            {
                throw new ArgumentNullException(nameof(rocketData));
            }

            var r = await ConstructRocketAsync(rocketData, stagesSet);
            return r is null ? null : new SimulationViewModel(r, targetHeight);
        }

        /// <summary>
        /// Constructs an initialised <see cref="SimulationViewModel"/> using data from <see cref="SelectedRocket"/>
        /// </summary>
        /// <param name="stagesSet">Whether the rocket has its stages fully initialised or not</param>
        /// <returns>An initialised <see cref="SimulationViewModel"/> with its <see cref="SimulationViewModel.Rocket"/> initialised from <see cref="SelectedRocket"/>. If the rocket is null, then null is returned.</returns>
        public Task<SimulationViewModel> GetSimulationViewModelAsync(bool stagesSet = false)
        {
            return GetSimulationViewModelAsync(SelectedRocket, stagesSet);
        }

    }
}
