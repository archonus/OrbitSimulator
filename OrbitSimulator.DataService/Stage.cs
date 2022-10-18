namespace OrbitSimulator.DataService
{
    public class Stage : DatabaseEntity
    {
        #region Database Properties
        public double PropellantMass { get; set; }
        public double StructuralMass { get; set; }
        public string Name { get; set; }

        public int EngineId { get => EngineData.Id; set => EngineData.Id = value; }
        public bool IsCustom { get; set; } = true; //Defaults to true
        #endregion

        /// <summary>
        /// The entity object representing the engine
        /// </summary>
        public Engine EngineData
        {
            get => engineData;
            set => engineData = value;
        }


        static readonly string createStatement =
            @"CREATE TABLE IF NOT EXISTS Stage(
            Id INTEGER PRIMARY KEY,
            Name TEXT UNIQUE,
            PropellantMass REAL,
            StructuralMass REAL,
            EngineId INTEGER,
            IsCustom BOOLEAN DEFAULT 1,
            FOREIGN KEY (EngineId) REFERENCES Engine(id))";
        private Engine engineData = new Engine();

        internal static string CreateStatement => createStatement;

        internal override string InsertStatement
        {
            get
            {
                return
                    $@"INSERT INTO Stage(Name,PropellantMass,StructuralMass,EngineId)
                    VALUES(?,?,?,?)";
            }
        }

        internal override (string query, object[] parameters) GetInsertStatement()
        {
            var query = InsertStatement;
            object[] parameters = new object[] { Name, PropellantMass, StructuralMass, EngineId };
            return (query, parameters);
        }
    }
}
