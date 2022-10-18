namespace OrbitSimulator.DataService
{
    public class Engine : DatabaseEntity
    {
        public double Mass { get; set; } = 0;
        public double SpecificImpulse { get; set; } = 0;
        public double MassFlowRate { get; set; } = 0;
        public string Name { get; set; }

        readonly static string createStatement =
            @"CREATE TABLE IF NOT EXISTS Engine(
            Id INTEGER PRIMARY KEY,
            Name TEXT UNIQUE,
            Mass REAL,
            SpecificImpulse REAL,
            MassFlowRate REAL)";

        internal static string CreateStatement => createStatement;
        internal override string InsertStatement => $@"INSERT INTO Engine(Name,Mass,SpecificImpulse,MassFlowRate)
                        VALUES (?,?,?,?)";

        internal override (string query, object[] parameters) GetInsertStatement()
        {
            var query = InsertStatement;
            object[] parameters = new object[] { Name, Mass, SpecificImpulse, MassFlowRate };
            return (query, parameters);
        }
    }
}
