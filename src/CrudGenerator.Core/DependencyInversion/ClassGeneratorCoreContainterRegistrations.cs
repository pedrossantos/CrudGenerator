﻿using CrudGenerator.Core.ViewModels;
using DependencyInversion;
using Framework;
using System.Collections.Generic;

namespace CrudGenerator.Core.DependencyInversion
{
    public sealed class CrudGeneratorCoreContainterRegistrations : ImmutableContainerBuilder
    {
        public CrudGeneratorCoreContainterRegistrations()
            : base(GetRegistrations())
        {
        }

        private static IEnumerable<ContainerRegistration> GetRegistrations()
        {
            yield return CreateSingleton<IApplicationMetadata, CrudGeneratorApplicationMetadata>();

            yield return CreateTransient<GeneralDatabaseConfigurationViewModel>();
            yield return CreateTransient<MySqlConnectionConfigurationViewModel>();
            yield return CreateTransient<SqliteConnectionConfigurationViewModel>();
            yield return CreateTransient<SqlServerConnectionConfigurationViewModel>();
            yield return CreateTransient<SchemaInformationGenetatorViewModel>();
            yield return CreateTransient<ProjectGeneratorViewModel>();

            // TODO: Caso necessário utilizar banco de dados fixo, alterar códigos abaixo
            yield return CreateSingleton(container =>
            {
                Database.Sqlite.DataAccess.EnvironmentBasedSqliteFilePath filePath = new Database.Sqlite.DataAccess.EnvironmentBasedSqliteFilePath("teste.db3", "C:\\Meu GitHub\\Database\\examples");
                return new System.Data.SQLite.SQLiteConnectionStringBuilder($"Data Source={filePath.FullPath};Version=3;DateTimeFormat=Ticks;foreign keys=false;");
            });

            yield return CreateSingleton(container =>
            {
                return new MySql.Data.MySqlClient.MySqlConnectionStringBuilder("Server=localhost;Database=testedatabaselocal")
                {
                    UserID = "teste",
                    Password = "teste",
                };
            });

            yield return CreateSingleton(container =>
            {
                return new System.Data.SqlClient.SqlConnectionStringBuilder("Server=tcp:DESKTOP-CO383P2,1433;Initial Catalog=TesteDatabaseLocal")
                {
                    UserID = "teste",
                    Password = "teste",
                    MultipleActiveResultSets = false,
                    Encrypt = true,
                    TrustServerCertificate = true,
                    ConnectTimeout = 30,
                };
            });
        }
    }
}