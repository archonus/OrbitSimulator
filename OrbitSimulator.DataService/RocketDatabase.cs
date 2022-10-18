using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SQLite;

namespace OrbitSimulator.DataService
{
    //REVIEW Add update methods
    #region Exceptions
    public class DataServiceException : Exception
    {
        public DataServiceException() { }

        public DataServiceException(string message) : base(message) { }

        public DataServiceException(string message, Exception innerException) : base(message, innerException) { }

        public DatabaseEntity Entity { get; set; }
    }
    public class RecordAlreadyExistsException : DataServiceException { }
    public class NameNotUniqueException : DataServiceException
    {
        public string Name { get; internal set; }
    }
    #endregion
    public class RocketDatabase
    {
        SQLiteAsyncConnection connection;

        /// <summary>
        /// Whether the connection is open
        /// </summary>
        public virtual bool IsConnectionOpen { get; protected set; }

        [Conditional("DEBUG")]
        private void DebugWriteException(SQLiteException e, [CallerMemberName] string callerName = null, string extraMessage = null)
        {
            Debug.WriteLine($"Sqlite exception thrown from {callerName}");
            Debug.Indent();
            Debug.WriteLine(extraMessage);
            Debug.WriteLine(e.Message);
            Debug.WriteLine(e.Result);
            Debug.Unindent();
        }

        /// <summary>
        /// Initialises the database connection
        /// </summary>
        /// <param name="dbPath">The path to the database</param>
        /// <remarks>Creates the tables if they don't already exist</remarks>
        public async Task InitialiseConnectionAsync(string dbPath)
        {
            try
            {
                //If there is already a connection open, it should be closed before a new one is created.
                if (connection is null)
                {
                    connection = new SQLiteAsyncConnection(dbPath);
#if DEBUG
                    connection.Trace = true;
                    connection.Tracer = s => Debug.WriteLine(s); //Writes the queries being executed to the debug output
#endif
                    await CreateEngineTableAsync();
                    await CreateStageTableAsync();
                    await CreateRocketTableAsync();
                    await CreateRocketStageTableAsync();
                }
                IsConnectionOpen = true;
            }
            catch (SQLiteException e)
            {
                DebugWriteException(e);
                await CloseConnectionAsync();
                IsConnectionOpen = false;
            }
        }

        /// <summary>
        /// Closes the connection
        /// </summary>
        public async Task CloseConnectionAsync()
        {
            if (!(connection is null))
            {
                await connection?.CloseAsync();
            }
            IsConnectionOpen = false;
        }

        #region Table Creation Methods
        Task CreateRocketTableAsync()
        {
            return connection.ExecuteAsync(Rocket.CreateStatement);
        }

        Task CreateEngineTableAsync()
        {
            return connection.ExecuteAsync(Engine.CreateStatement);
        }

        Task CreateStageTableAsync()
        {
            return connection.ExecuteAsync(Stage.CreateStatement);
        }

        /// <summary>
        /// Create the rocket stage link table
        /// </summary>
        async Task CreateRocketStageTableAsync()
        {
            await connection.ExecuteAsync(RocketStage.CreateStatement);
        }
        #endregion

        #region Existence Check Methods

        async Task<bool> CheckExistenceFromIdAsync<T>(T entity) where T : DatabaseEntity, new()
        {
            try
            {
                return await CheckExistenceFromIdAsync<T>(entity.Id);
            }
            catch (DataServiceException e)
            {
                e.Entity = entity;
                throw;
            }
        }
        async Task<bool> CheckExistenceFromIdAsync<T>(int id) where T : DatabaseEntity, new()
        {
            var query = DatabaseEntity.GetExistencefromIdStatement<T>(); //Get the query
            try
            {
                var result = await connection.ExecuteScalarAsync<int>(query, id);
                return Convert.ToBoolean(result); //Convert from an integer (1 or 0) to bool
            }
            catch (SQLiteException e)
            {
                DebugWriteException(e);
                throw new DataServiceException(e.Message, e) { Entity = new T { Id = id } };
            }

        }
        #endregion

        #region Entity Retrieval Methods

        /// <summary>
        /// Get a fully initialised rocket object from the id
        /// </summary>
        /// <param name="id">The id of the rocket</param>
        /// <returns>A <see cref="Rocket"/> object which is fully initialised, or null if there is an error</returns>
        public async Task<Rocket> GetRocketFromIdAsync(int id)
        {
            var rocketSelectStmt = DatabaseEntity.GetSelectFromIdStatement<Rocket>();
            try
            {
                Rocket rocketData = (await connection.QueryAsync<Rocket>(rocketSelectStmt, id)).Single();
                return await InitialiseRocket(rocketData); //Initialise the rocket's stages
            }
            catch (SQLiteException e)
            {
                DebugWriteException(e);
                return null;
            }
            catch (InvalidOperationException)
            { //Single() can throw an InvalidOperationException if the list is empty
                Debug.WriteLine($"No rocket found that matches id = {id}");
                return null;
            }
        }

        /// <summary>
        /// Initialises a <see cref="Rocket"/> object by initialising the stages
        /// </summary>
        /// <param name="rocketData">The object that only has the data from the database</param>
        /// <returns>The initialised object</returns>
        public async Task<Rocket> InitialiseRocket(Rocket rocketData)
        {
            try
            {
                var getStagesStmt = //SQL query to get all the stages that are associated with a particular rocket
                    $@"SELECT Stage.Id, Stage.Name, Stage.PropellantMass, Stage.StructuralMass, Stage.EngineId, RocketStage.StageNumber
                    FROM Stage 
                        INNER JOIN RocketStage ON RocketStage.StageId = Stage.Id
                    WHERE RocketStage.RocketId = ?
                    ORDER BY RocketStage.StageNumber ASC"; //Ensure that the stages are in the right order
                List<Stage> stagesData = await connection.QueryAsync<Stage>(getStagesStmt, rocketData.Id); //Get the data from db
                if (stagesData.Count == 0)
                { //No stages could be found for the given rocket
                    return null;
                }
                await InitialiseStages(stagesData);
                if (stagesData.Any(x => x is null)) //If any of the stages failed to initialise, the entire rocket fails.
                {
                    return null;
                }
                rocketData.SetStages(stagesData);
                return rocketData;
            }
            catch (SQLiteException e)
            {
                DebugWriteException(e);
                return null;
            }
        }

        /// <summary>
        /// Get a fully initialised stage from its id
        /// </summary>
        /// <param name="id">The id of the stage in the database</param>
        /// <returns>The initialised object, or null if there is an error</returns>
        public async Task<Stage> GetStageFromIdAsync(int id)
        {
            var selectStmt = DatabaseEntity.GetSelectFromIdStatement<Stage>();
            try
            {
                Stage stageData = (await connection.QueryAsync<Stage>(selectStmt, id)).FirstOrDefault();
                if (stageData is null)
                    return null; //REVIEW Consider this?
                return await InitialiseStage(stageData);
            }
            catch (SQLiteException e)
            {
                DebugWriteException(e);
                return null;
            }
        }

        /// <summary>
        /// Initialises a <see cref="Stage"/> object by initialising the engine
        /// </summary>
        /// <param name="stageData">The object that only has the data from the database</param>
        /// <returns>The initialised object, or null if it failed</returns>
        public async Task<Stage> InitialiseStage(Stage stageData)
        {
            var engineData = await GetEngineFromIdAsync(stageData.EngineId);
            if (engineData is null)
            {
                return null;
            }
            else
            {
                stageData.EngineData = engineData;
                return stageData;
            }
        }

        /// <summary>
        /// Gets the <see cref="Engine"/> object from its id
        /// </summary>
        /// <returns>The object, or null if there was a failure</returns>
        public async Task<Engine> GetEngineFromIdAsync(int id)
        {
            try
            {
                var engineSelectStmt = DatabaseEntity.GetSelectFromIdStatement<Engine>(); //Get SQL statement for Engine table
                Engine engineData = (await connection.QueryAsync<Engine>(engineSelectStmt, id)).Single(); //There should only be one result, so select the first
                return engineData;
            }
            catch (SQLiteException e)
            {
                DebugWriteException(e);
                return null;
            }

        }

        /// <summary>
        /// Initialise all the rockets in a list of rockets
        /// </summary>
        /// <param name="rockets">The list of rockets to be initialised</param>
        private async Task InitialiseRockets(List<Rocket> rockets)
        {
            List<Task> rocketRetrievalTasks = new List<Task>(rockets.Count); //List to store all the retrieval tasks, which can then be executed in one go
            for (int i = 0; i < rockets.Count; i++)
            {
                rocketRetrievalTasks.Add(GetInitialiseRocketTask(i));
            }
            await Task.WhenAll(rocketRetrievalTasks); //Run all the tasks concurrently

            async Task GetInitialiseRocketTask(int index)
            {
                var rocket = await InitialiseRocket(rockets[index]);
                rockets[index] = rocket; //Overwrite the data with the initialised data
            }
        }

        /// <summary>
        /// Initialise all the stages in-place
        /// </summary>
        /// <param name="stages">The list of stages</param>
        private async Task InitialiseStages(List<Stage> stages)
        {
            List<Task> stageInitialisationTasks = new List<Task>(stages.Count);
            for (int i = 0; i < stages.Count; i++)
            {
                stageInitialisationTasks.Add(GetStageTask(i));
            }
            await Task.WhenAll(stageInitialisationTasks);

            async Task GetStageTask(int index)
            {
                var stageData = await InitialiseStage(stages[index]);
                stages[index] = stageData;
            }
        }

        /// <summary>
        /// Gets all the saved rockets in the database.
        /// </summary>
        /// <param name="fullyInitialised">Whether the rockets should be fully initialised, or just with data from the table</param>
        /// <returns>A list of <see cref="Rocket"/>, or empty list if query fails</returns>
        /// <remarks>If a particular rocket fails to load, it will silently not be included</remarks>
        public async Task<List<Rocket>> GetAllRocketsAsync(bool fullyInitialised = false)
        {
            string query = DatabaseEntity.GetSelectAllStatement<Rocket>();
            try
            {
                List<Rocket> rockets = await connection.QueryAsync<Rocket>(query);
                if (fullyInitialised)
                {
                    await InitialiseRockets(rockets);
                }
                return rockets.OrderBy(r => r.Id) //Ensure they are in the correct order
                              .Where(x => x != null) //Remove null values
                              .ToList(); //Convert to list
            }
            catch (SQLiteException e)
            { //If there is an error, return empty list
                DebugWriteException(e);
                return new List<Rocket>();
            }
        }

        /// <summary>
        /// Get all the saved stages in the database, by default fully initialised
        /// </summary>
        /// <param name="fullyInitialised">Whether the stages should be fully initialised, or just with data from the table</param>
        /// <returns>A list of <see cref="Stage"/>, or empty list if query fails</returns>
        /// <remarks>If a particular stage fails to load, it will silently not be included</remarks>
        public async Task<List<Stage>> GetAllStagesAsync(bool fullyInitialised = true)
        {
            string query = DatabaseEntity.GetSelectAllStatement<Stage>();
            try
            {
                List<Stage> stages = await connection.QueryAsync<Stage>(query); //List to be returned, with initial capacity specified to be number of stages (since it is the same as number of ids)
                if (fullyInitialised)
                {
                    await InitialiseStages(stages);
                }
                return stages.OrderBy(s => s.Id)
                             .Where(x => x != null)
                             .ToList();
            }
            catch (SQLiteException e)
            {
                DebugWriteException(e);
                return new List<Stage>();
            }
        }

        /// <summary>
        /// Gets a list of all the engines stored in the database
        /// </summary>
        /// <returns>An empty list if fails</returns>
        public async Task<List<Engine>> GetAllEnginesAsync()
        {
            string query = DatabaseEntity.GetSelectAllStatement<Engine>();
            try
            {
                return await connection.QueryAsync<Engine>(query);
            }
            catch (SQLiteException e)
            {
                DebugWriteException(e);
                return new List<Engine>();
            }
        }

        #endregion

        #region Entity Insertion Methods

        /// <summary>
        /// Inserts the entity into the database, and returns the id generated
        /// </summary>
        /// <typeparam name="T">The type of the entity - must be a <see cref="DatabaseEntity"/> </typeparam>
        /// <param name="entity">The object containing the data</param>
        /// <returns>The id of the item generated</returns>
        /// <remarks>No validation performed</remarks>
        protected async Task<int> InsertEntityAsync<T>(T entity) where T : DatabaseEntity
        {
            (string query, object[] parameters) = entity.GetInsertStatement();
            var id_query = "SELECT last_insert_rowid();";

            await connection.ExecuteAsync(query, parameters);
            int id = await connection.ExecuteScalarAsync<int>(id_query); //Get the id
            return id;
        }


        /// <summary>
        /// Saves an engine to the database and returns the id of the record.
        /// </summary>
        /// <param name="engineData">The data used to store it in the database</param>
        /// <returns>The id of the record</returns>
        /// <exception cref="RecordAlreadyExistsException">If the record already exists, then it will throw an exception</exception>
        /// <exception cref="DataServiceException"/>
        private async Task<int> SaveEngineAsync(Engine engineData)
        {
            try
            {
                int id = await InsertEntityAsync(engineData);
                return id;
            }
            catch (SQLiteException e)
            {
                DebugWriteException(e);
                if (e.Result == SQLite3.Result.Constraint)
                { //The unique constraint has been broken
                    throw new RecordAlreadyExistsException { Entity = engineData };
                }
                else
                {
                    DebugWriteException(e);
                    throw new DataServiceException(e.Message, e) { Entity = engineData };
                }
            }
        }

        /// <summary>
        /// Saves a stage to the database and returns the id of the record.
        /// </summary>
        /// <param name="stageData">The data used to store it in the database</param>
        /// <returns>The id of the record</returns>
        /// <remarks>If the record already exists, then it will do nothing and simply return the id</remarks>
        /// <exception cref="NameNotUniqueException"/>
        /// <exception cref="DataServiceException"/>
        private async Task<int> SaveStageAsync(Stage stageData)
        {
            var idExists = await CheckExistenceFromIdAsync(stageData);
            if (idExists)
            {
                return stageData.Id;
            }
            else
            {
                var engineExists = await CheckExistenceFromIdAsync(stageData.EngineData); //Check if EngineId is valid
                if (!engineExists) //Engine doesn't already exist in the database
                { //This branch should not occur since user currently cannot create custom engines
                    var id = await SaveEngineAsync(stageData.EngineData);
                    stageData.EngineId = id;
                }
                try
                {
                    return await InsertEntityAsync(stageData);
                }
                catch (SQLiteException e)
                {
                    if (e.Result == SQLite3.Result.Constraint)
                    {
                        throw new NameNotUniqueException() { Entity = stageData };
                    }
                    else
                    {
                        DebugWriteException(e);
                        throw new DataServiceException(e.Message, e) { Entity = stageData };
                    }
                }

            }
        }

        /// <summary>
        /// Saves <see cref="RocketStage"/> to the database
        /// </summary>
        /// <param name="rocketStage">The data about the rocket stage to be saved</param>
        /// <remarks>If rocket does not exist, the <see cref="InvalidOperationException"/> thrown</remarks>
        /// <exception cref="DataServiceException"/>
        /// <exception cref="InvalidOperationException"/>
        private async Task SaveRocketStageAsync(RocketStage rocketStage)
        {
            if (rocketStage.StageData.IsCustom || rocketStage.StageId <= 0) //Seconds condition *should* not occur independently of first, but just in case
            { //It may need saving, or could already be there
                int stageId;
                try
                {
                    stageId = await SaveStageAsync(rocketStage.StageData);
                }
                catch (NameNotUniqueException)
                { //REVIEW This is an example of recursion in the algorithm
                    rocketStage.StageData.Name += "(1)"; //Recursively add (1) to the name until it works - not expected to go more than one deep due to higher level restriction on rocket name
                    await SaveRocketStageAsync(rocketStage);
                    return;
                }
                rocketStage.StageId = stageId;
            }
            try
            {
                await InsertEntityAsync(rocketStage);
            }
            catch (SQLiteException e)
            {
                DebugWriteException(e);
                if (e.Result == SQLite3.Result.Constraint)
                { //Foreign key constraint failed, so the rocket does not exist
                    throw new InvalidOperationException($"A rocket with id {rocketStage.RocketId} does not exist");
                }
                throw new DataServiceException(e.Message, e) { Entity = rocketStage };
            }
        }

        /// <summary>
        /// Save a rocket to the database and returns the id generated
        /// </summary>
        /// <param name="rocketData">The object containing all the data to be stored</param>
        /// <exception cref="RecordAlreadyExistsException"/>
        /// <exception cref="NameNotUniqueException"/>
        /// <exception cref="DataServiceException">Thrown if there was an error accessing the database</exception>
        /// <exception cref="ArgumentException">Thrown if the properties of the argument are invalid</exception>
        public async Task<int> SaveRocketAsync(Rocket rocketData)
        {
            if (rocketData is null)
            {
                throw new ArgumentNullException(nameof(rocketData));
            }

            var idExists = await CheckExistenceFromIdAsync(rocketData);
            if (idExists) //This is not an update method
            {
                throw new RecordAlreadyExistsException { Entity = rocketData };
            }
            if (string.IsNullOrEmpty(rocketData.Name)) //Rocket with no name should not be saved
            {
                throw new ArgumentException("The name of the rocket cannot be null or empty", nameof(rocketData.Name));
            }
            if (rocketData.NumStages < 1)
            {
                throw new ArgumentException("The rocket must have at least 1 stage", nameof(rocketData.RocketStages));
            }
            //The rocket can be saved
            int rocketId;
            try
            {
                Debug.WriteLineIf(rocketData.PayloadMass <= 0, $"WARNING: Rocket is being saved with payload of {rocketData.PayloadMass}");
                rocketId = await InsertEntityAsync(rocketData); //Try inserting into the rocket table
            }
            catch (SQLiteException e)
            { //Convert to meaningful exceptions
                DebugWriteException(e);
                if (e.Result == SQLite3.Result.Constraint) //The name is not unique
                {
                    throw new NameNotUniqueException { Entity = rocketData, Name = rocketData.Name };
                }
                else
                {
                    await CloseConnectionAsync();
                    DebugWriteException(e);
                    throw new DataServiceException(e.Message, e) { Entity = rocketData };
                }
            }
            //The rocket was successfully saved
            rocketData.Id = rocketId; //Set the id correctly
            try
            { //Insert the stages
                foreach (var rocketStage in rocketData.RocketStages)
                {
                    Debug.Assert(rocketStage.RocketId == rocketId, "Rocket id of the stage does not match the rocket parent"); //The rocket id for each stage should be correctly set
                    await SaveRocketStageAsync(rocketStage);
                }
                return rocketId;
            }
            catch (DataServiceException)
            { //Undo the addition to the database
                var success = await RemoveRocketAsync(rocketId); //Try to remove rocket and its stages from database
                Debug.WriteLineIf(!success, "Failed to remove rocket"); //If there was an error
                throw; //Throw the original exception
            }
        }

        #endregion

        /// <summary>
        /// Removes all data stored in the database associated with a particular rocket
        /// </summary>
        /// <param name="rocketId">The id of the rocket whose data is to be removed</param>
        /// <returns>Whether the operation was a success</returns>
        public async Task<bool> RemoveRocketAsync(int rocketId)
        {
            try
            {
                string del_RocketStage = "DELETE FROM RocketStage WHERE RocketId = ?";
                await connection.ExecuteAsync(del_RocketStage, rocketId); //Remove from RocketStage table

                string del_Rocket = "DELETE FROM Rocket WHERE Id = ?"; //Remove from the Rocket table
                await connection.ExecuteAsync(del_Rocket, rocketId);
                return true;
            }
            catch (SQLiteException e)
            {
                DebugWriteException(e);
                return false;
            }
        }



    }
}
