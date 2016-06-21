using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StockBuddy.Shared.Utilities.AppSettings
{
    public sealed class SqlServerSettingsStore : ISettingsStore
    {
        private readonly string _connectionString;
        private readonly DbProviderFactory _dbProviderFactory;
        private readonly string _tableName;
        private readonly int _settingsId;

        public SqlServerSettingsStore(
            string connectionString, string dbProviderName = null, string tableName = null, int settingsId = 1)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Value must be set", nameof(connectionString));

            if (tableName?.Trim().Length == 0)
            {
                throw new ArgumentException("Table name can't be empty", nameof(tableName));
            }

            _connectionString = connectionString;
            _dbProviderFactory = DbProviderFactories.GetFactory(dbProviderName ?? "System.Data.SqlClient");
            _tableName = tableName ?? "AppSettings";
            _settingsId = settingsId;
        }

        public TSettings Load<TSettings>()
        {
            var paramID = CreateParameter("@ID", _settingsId);

            var readerResult = ExecuteReader(
                $"SELECT SettingsData FROM {_tableName} WHERE ID = {paramID.ParameterName}",
                reader => reader.GetString(0),
                paramID);

            string settingsData = readerResult.Single();

            TSettings settings = JsonConvert.DeserializeObject<TSettings>(settingsData);
            return settings;
        }

        public void Save<TSettings>(TSettings appSettings)
        {
            var settingsData = JsonConvert.SerializeObject(appSettings, Formatting.Indented);

            var paramSettingsData = CreateParameter("@SettingsData", settingsData);
            var paramID = CreateParameter("@ID", _settingsId);

            ExecuteCommand($@"
                UPDATE {_tableName} 
                SET SettingsData = {paramSettingsData.ParameterName}, LastEdited = GETDATE()
                WHERE ID = {paramID.ParameterName}",
                paramSettingsData, paramID);
        }

        public bool IsStoreCreated()
        {
            var paramTableName = CreateParameter("@TableName", _tableName);
            var paramID = CreateParameter("@ID", _settingsId);

            string sqlCommand = GenerateTableExistsSqlCommand(
                paramTableName.ParameterName,
                $"SELECT ID FROM {_tableName} WHERE ID = {paramID.ParameterName}");

            return ExecuteReader(sqlCommand, reader => true, paramTableName, paramID).Any();
        }

        public void CreateStore<TSettings>(TSettings settings)
        {
            var settingsData = JsonConvert.SerializeObject(settings, Formatting.Indented);

            var paramSettingsData = CreateParameter("@SettingsData", settingsData);
            var paramTableName = CreateParameter("@TableName", _tableName);
            var paramID = CreateParameter("@ID", _settingsId);

            string createTableSqlCommand = $@"
                CREATE TABLE {_tableName}
                (
                    ID INT NOT NULL PRIMARY KEY,
                    SettingsData NVARCHAR(MAX) NOT NULL,
                    Created DATETIME NOT NULL,
                    LastEdited DATETIME NOT NULL
                );";

            string sqlCommand = GenerateTableExistsSqlCommand(
                paramTableName.ParameterName,
                createTableSqlCommand,
                $@"INSERT INTO { _tableName} VALUES({paramID.ParameterName}, {paramSettingsData.ParameterName}, GETDATE(), GETDATE())",
                whenNotExists: true);

            ExecuteCommand(sqlCommand, paramSettingsData, paramTableName, paramID);
        }

        private string GenerateTableExistsSqlCommand(
            string paramTableName, string sqlCommandConditional, string sqlCommand = null, bool whenNotExists = false)
        {
            return $@"
                IF {(whenNotExists ? "NOT" : "")} EXISTS(
                    SELECT TOP 1 TABLE_NAME
                    FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_SCHEMA = 'dbo'
                    AND TABLE_NAME = {paramTableName}
                )
                BEGIN
                    {sqlCommandConditional}
                END

                {sqlCommand}";
        }

        private int ExecuteCommand(string sqlCommand, params DbParameter[] parameters)
        {
            using (var dbCommand = CreateDbCommand(sqlCommand, parameters))
            using (dbCommand.Connection)
            {
                return dbCommand.ExecuteNonQuery();
            }
        }

        private IEnumerable<TElement> ExecuteReader<TElement>(
            string sqlCommand, Func<DbDataReader, TElement> selector, params DbParameter[] parameters)
        {
            using (var dbCommand = CreateDbCommand(sqlCommand, parameters))
            using (dbCommand.Connection)
            using (var reader = dbCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    yield return selector(reader);
                }
            }
        }

        private DbCommand CreateDbCommand(string sqlCommand, params DbParameter[] parameters)
        {
            var connection = _dbProviderFactory.CreateConnection();
            connection.ConnectionString = _connectionString;

            var command = _dbProviderFactory.CreateCommand();
            command.Connection = connection;
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = sqlCommand;

            foreach (var parameter in parameters)
                command.Parameters.Add(parameter);

            connection.Open();
            return command;
        }

        private DbParameter CreateParameter(string name, object value)
        {
            var param = _dbProviderFactory.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            return param;
        }
    }
}
