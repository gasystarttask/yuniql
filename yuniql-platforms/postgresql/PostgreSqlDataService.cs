﻿using System;
using System.Collections.Generic;
using System.Data;
using Yuniql.Extensibility;
using Npgsql;

namespace Yuniql.PostgreSql
{
    public class PostgreSqlDataService : IDataService
    {
        private string _connectionString;
        private readonly ITraceService _traceService;

        public PostgreSqlDataService(ITraceService traceService)
        {
            this._traceService = traceService;
        }

        public bool IsAtomicDDLSupported => true;

        public bool IsSchemaSupported { get; } = true;

        public bool IsBatchSqlSupported { get; } = false;

        public string TableName { get; set; } = "__yuniqldbversion";

        public string SchemaName { get; set; } = "public";

        public void Initialize(string connectionString)
        {
            this._connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public IDbConnection CreateMasterConnection()
        {
            var masterConnectionStringBuilder = new NpgsqlConnectionStringBuilder(_connectionString);
            masterConnectionStringBuilder.Database = "postgres";

            return new NpgsqlConnection(masterConnectionStringBuilder.ConnectionString);
        }

        public List<string> BreakStatements(string sqlStatementRaw)
        {
            return new List<string> { sqlStatementRaw };
        }

        public ConnectionInfo GetConnectionInfo()
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_connectionString);
            return new ConnectionInfo { DataSource = connectionStringBuilder.Host, Database = connectionStringBuilder.Database };
        }

        public string GetSqlForCheckIfDatabaseExists()
            => @"SELECT 1 from pg_database WHERE datname = '${YUNIQL_DB_NAME}';";

        public string GetSqlForCreateDatabase()
            => "CREATE DATABASE \"${YUNIQL_DB_NAME}\";";

        public string GetSqlForCreateSchema()
            => "CREATE SCHEMA \"${YUNIQL_SCHEMA_NAME}\";";

        public string GetSqlForCheckIfDatabaseConfigured()
            => @"SELECT 1 FROM pg_tables WHERE  tablename = '${YUNIQL_TABLE_NAME}'";

        public string GetSqlForConfigureDatabase()
            => @"CREATE TABLE ${YUNIQL_SCHEMA_NAME}.${YUNIQL_TABLE_NAME}(
                    sequence_id  SMALLSERIAL PRIMARY KEY NOT NULL,
                    version VARCHAR(512) NOT NULL,
                    applied_on_utc TIMESTAMP NOT NULL DEFAULT(current_timestamp AT TIME ZONE 'UTC'),
                    applied_by_user VARCHAR(32) NOT NULL DEFAULT(user),
                    applied_by_tool VARCHAR(32) NULL,
                    applied_by_tool_version VARCHAR(16) NULL,
                    additional_artifacts BYTEA NULL,
                    CONSTRAINT ix___yuniqldbversion UNIQUE(version)
	            );";

        public string GetSqlForGetCurrentVersion()
            => @"SELECT version FROM ${YUNIQL_SCHEMA_NAME}.${YUNIQL_TABLE_NAME} ORDER BY sequence_id DESC LIMIT 1;";

        public string GetSqlForGetAllVersions()
            => @"SELECT sequence_id, version, applied_on_utc, applied_by_user, applied_by_tool, applied_by_tool_version FROM ${YUNIQL_SCHEMA_NAME}.${YUNIQL_TABLE_NAME} ORDER BY version ASC;";

        public string GetSqlForInsertVersion()
            => @"INSERT INTO ${YUNIQL_SCHEMA_NAME}.${YUNIQL_TABLE_NAME} (version, applied_by_tool, applied_by_tool_version) VALUES ('{0}', '{1}', '{2}');";
    }
}
