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

        public string GetCheckIfDatabaseExistsSql()
            => "SELECT 1 from pg_database WHERE datname = '{0}';";

        public string GetCreateDatabaseSql()
            => "CREATE DATABASE {0};";

        public string GetCheckIfDatabaseConfiguredSql()
            => "SELECT 1 FROM pg_tables WHERE  tablename = '__yuniqldbversion'";

        public string GetConfigureDatabaseSql()
            => @"CREATE TABLE __YuniqlDbVersion(
                    Id SMALLSERIAL PRIMARY KEY NOT NULL,
                    Version VARCHAR(32) NOT NULL,
                    DateInsertedUtc TIMESTAMP NOT NULL,
		            LastUpdatedUtc TIMESTAMP NOT NULL,
                    LastUserId VARCHAR(128) NOT NULL,
                    Artifact BYTEA NULL,
                    CONSTRAINT IX___YuniqlDbVersion UNIQUE(Version)
	            );";

        public string GetGetCurrentVersionSql()
            => "SELECT Version FROM __yuniqldbversion ORDER BY Id DESC LIMIT 1;";

        public string GetGetAllVersionsSql()
            => "SELECT Id, Version, DateInsertedUtc, LastUserId FROM __yuniqldbversion ORDER BY Version ASC;";

        public string GetUpdateVersionSql()
            => "INSERT INTO __yuniqldbversion (Version, DateInsertedUtc, LastUpdatedUtc, LastUserId) VALUES ('{0}', NOW(), NOW(), user);";
    }
}
