namespace OrbitSimulator.DataService
{
    public abstract class DatabaseEntity
    {

        public virtual int Id { get; set; } //All entities must have an id
        internal static string GetTableName<T>() where T : DatabaseEntity
        {
            return typeof(T).Name;
        }
        internal abstract string InsertStatement { get; }

        /// <summary>
        /// Generates a parameterised statement for inserting the entity into database
        /// </summary>
        /// <returns>A value tuple of the query string and its parameters</returns>
        internal abstract (string query, object[] parameters) GetInsertStatement();

        /// <summary>
        /// Generates a statement for selecting everything from a particular table
        /// </summary>
        /// <typeparam name="T">The table name</typeparam>
        internal static string GetSelectAllStatement<T>() where T : DatabaseEntity
        {
            var tableName = GetTableName<T>();
            return $"SELECT * FROM {tableName}";
        }

        /// <summary>
        /// Generates a statement for getting all the ids from a table
        /// </summary>
        /// <typeparam name="T">The table name</typeparam>
        internal static string GetSelectIdsStatement<T>() where T : DatabaseEntity
        {
            var tableName = GetTableName<T>();
            return $"SELECT Id FROM {tableName}";
        }

        /// <summary>
        /// Generates a parameterised statement for selecting an object from its id
        /// </summary>
        /// <typeparam name="T">The table type</typeparam>
        /// <returns>A statement containing ? for the id</returns>
        internal static string GetSelectFromIdStatement<T>() where T : DatabaseEntity
        {
            var tableName = GetTableName<T>();
            return $"SELECT * FROM {tableName} WHERE Id = ?";

        }

        /// <summary>
        /// Generates a parameterised statement for returning 1 if an object exists from its id
        /// </summary>
        /// <typeparam name="T">The table name</typeparam>
        /// <returns>A statement containing ? for the id</returns>
        internal static string GetExistencefromIdStatement<T>() where T : DatabaseEntity
        {
            var tableName = GetTableName<T>();
            return $"SELECT 1 FROM {tableName} WHERE Id = ?";
        }

        /// <summary>
        /// Generates a parameterised statement for returning 1 if a record exists from its name
        /// </summary>
        /// <typeparam name="T">The table name</typeparam>
        /// <returns>A statement containing ? for the name</returns>
        internal static string GetExistencefromNameStatement<T>() where T : DatabaseEntity
        {
            var tableName = GetTableName<T>();
            return $"SELECT 1 FROM {tableName} WHERE Name = ?";
        }


    }
}
