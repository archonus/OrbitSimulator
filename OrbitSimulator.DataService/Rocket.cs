using System.Collections.Generic;

namespace OrbitSimulator.DataService
{
    public class Rocket : DatabaseEntity
    {
        readonly static string createStatement =
        @"CREATE TABLE IF NOT EXISTS Rocket(
            Id INTEGER PRIMARY KEY,
            Name TEXT UNIQUE,
            PayloadMass REAL DEFAULT 0,
            IsCustom BOOLEAN DEFAULT 1)";

        //REVIEW Make this immutable - use an array instead
        private List<RocketStage> rocketStages = new List<RocketStage>(); //To avoid null references

        #region Database Properties
        public double PayloadMass { get; set; } = 0;
        public string Name { get; set; }
        public bool IsCustom { get; set; } = true;

        /// <summary>
        /// The id of the rocket
        /// </summary>
        /// <remarks>Setting it will change the rocket id of all the stages in <see cref="RocketStages"/></remarks>
        public override int Id
        {
            get => base.Id;
            set
            {
                base.Id = value;
                foreach (var rocketStage in rocketStages)
                {
                    rocketStage.RocketId = value;
                }
            }
        }
        #endregion

        /// <summary>
        /// The stages of the rocket
        /// </summary>
        public List<RocketStage> RocketStages
        {
            get => rocketStages;
        }

        /// <summary>
        /// The number of stages, or 0 if not initialised
        /// </summary>
        public int NumStages { get => RocketStages?.Count ?? 0; }

        /// <summary>
        /// Sets <see cref="RocketStages"/> from a list of stages
        /// </summary>
        /// <param name="stagesData">The collection of stages from which the <see cref="RocketStage"/> objects will be constructed</param>
        public void SetStages(IList<Stage> stagesData)
        {
            if (stagesData is null)
            {
                throw new System.ArgumentNullException(nameof(stagesData));
            }

            rocketStages = new List<RocketStage>(stagesData.Count); //The size of the two lists will be the same
            for (int i = 0; i < stagesData.Count; i++)
            {
                rocketStages.Add(
                    new RocketStage
                    {
                        StageNumber = i + 1, //Since the stage number is not 0 indexed
                        StageData = stagesData[i],
                        RocketId = this.Id
                    }
                    );
            }
        }


        internal static string CreateStatement => createStatement;

        internal override string InsertStatement => $@"INSERT INTO Rocket(Name,PayloadMass,IsCustom)
                VALUES(?,?,?)";
        internal override (string query, object[] parameters) GetInsertStatement()
        {
            var query = InsertStatement;
            object[] parameters = new object[] { Name, PayloadMass, IsCustom };
            return (query, parameters);
        }
    }
}
