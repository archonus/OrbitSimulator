namespace OrbitSimulator.DataService
{
    /// <summary>
    /// Represents a stage linked to a particular rocket
    /// </summary>
    public class RocketStage : DatabaseEntity
    {
        private Stage stageData = new Stage();

        #region Database Properties
        public int RocketId { get; set; }
        public int StageId { get => stageData.Id; set => stageData.Id = value; }
        public int StageNumber { get; set; }
        #endregion
        /// <summary>
        /// The data of the stage
        /// </summary>
        /// <remarks>Setting the property will change the value of <see cref="StageId"/> as well</remarks>
        public Stage StageData
        {
            get => stageData;
            set => stageData = value;
        }

        internal override string InsertStatement => "INSERT INTO RocketStage (RocketId,StageId,StageNumber) VALUES (?,?,?)";

        internal override (string query, object[] parameters) GetInsertStatement()
        {
            var query = InsertStatement;
            var parameters = new object[] { RocketId, StageId, StageNumber };
            return (query, parameters);
        }

        static readonly string createStatement =
            @"CREATE TABLE IF NOT EXISTS RocketStage(
                Id INTEGER PRIMARY KEY,
                RocketId INTEGER,
                StageId INTEGER,
                StageNumber INTEGER,
                FOREIGN KEY (RocketId) REFERENCES Rocket(Id),
                FOREIGN KEY (StageId) REFERENCES Stage(Id))";


        internal static string CreateStatement => createStatement;

    }
}
