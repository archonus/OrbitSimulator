using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OrbitSimulator.Core;
using OrbitSimulator.DataService;
using OrbitSimulator.Factory;

namespace OrbitSimulatorTests
{
    public static class TestingUtils
    {
        /// <summary>
        /// The tolerance for accuracy of double values - 3 d.p.
        /// </summary>
        public const double DecimalTolerance = 0.001;

        private const string dbName = "TestingRocketsDb.db";
        public static string DBPath
        {
            get
            {
                string dbFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(dbFolder, dbName);
            }
        }

        public static async Task GenerateDBFile()
        {
            var assembly = typeof(OrbitSimulator.App).Assembly;
            string resourceId = assembly.GetManifestResourceNames().Single(name => name.EndsWith("RocketsDB.db")); //Gets the resource name, no matter the folder
            Console.WriteLine($"Resource found at {resourceId}");
            using (Stream embeddedDbStream = assembly.GetManifestResourceStream(resourceId))
            {
                using (var fileStream = new FileStream(DBPath, FileMode.Create))
                {
                    await embeddedDbStream.CopyToAsync(fileStream); //Copy the data from the embedded resource to the file location
                }
            }
            Console.WriteLine($"Database file copied to {DBPath}");
        }

        public static Engine RD180_engine => new Engine
        {
            Id = 1,
            Name = "RD180",
            Mass = 5480,
            SpecificImpulse = 338,
            MassFlowRate = 1250
        };

        public static Engine RL10_engine => new Engine
        {
            Id = 4,
            Name = "RL10",
            Mass = 167.8,
            SpecificImpulse = 450.5,
            MassFlowRate = 22.45
        };

        public static Stage AtlasV_stage1 => new Stage
        {
            Name = "Atlas V Stage 1",
            EngineData = RD180_engine,
            PropellantMass = 284089,
            StructuralMass = 15570
        };

        public static Stage AtlasV_stage2 => new Stage
        {
            Name = "Atlas V Stage 2",
            EngineData = RL10_engine,
            PropellantMass = 20830,
            StructuralMass = 2079
        };

        public static Stage[] AtlasVStages => new Stage[2] { AtlasV_stage1, AtlasV_stage2 };

        public static IdealSingleStageRocket GetSingleStageRocket(Stage stageData, double payload = 400)
        {
            return SingleStageRocketFactory.ConstructIdealSingleStageRocket(stageData, payload);
        }

        public static MultiStageRocket GetMultiStageRocket(Stage[] stages, double payload = 400)
        {
            var rocketData = new Rocket { PayloadMass = payload };
            rocketData.SetStages(stages);
            return MultiStageRocketFactory.ConstructMultiStageRocket(rocketData);
        }

        public static RocketEngine GetRocketEngine(Engine engine)
        {
            return SingleStageRocketFactory.ConstructEngine(engine);
        }


    }
}
