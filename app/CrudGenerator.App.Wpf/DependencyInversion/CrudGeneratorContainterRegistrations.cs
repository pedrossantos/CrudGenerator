using Database.MySql.DataAccess;
using Database.MySql.Sql;
using Database.PostgreSql.DataAccess;
using Database.PostgreSql.Sql;
using Database.Sqlite.DataAccess;
using Database.Sqlite.Sql;
using Database.SqlServer.DataAccess;
using Database.SqlServer.Sql;
using DependencyInversion;
using Framework;
using View.Abstractions.IO;
using View.Abstractions.Wpf;

namespace CrudGenerator.App.Wpf.DependencyInversion
{
    public sealed class CrudGeneratorContainterRegistrations : ImmutableContainerBuilder
    {
        public CrudGeneratorContainterRegistrations()
            : base(GetRegistrations())
        {
        }

        private static IEnumerable<ContainerRegistration> GetRegistrations()
        {
            // TODO: Caso necessário utilizar banco de dados fixo, alterar códigos abaixo
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
                IApplicationMetadata applicationMetadata = container.Resolve<IApplicationMetadata>();
                IApplicationPaths applicationPaths = container.Resolve<IApplicationPaths>();
                EnvironmentBasedSqliteFilePath filePath = new EnvironmentBasedSqliteFilePath("teste.db3", applicationPaths.ApplicationData);
                return new System.Data.SQLite.SQLiteConnectionStringBuilder($"Data Source={filePath.FullPath};Version=3;DateTimeFormat=Ticks;foreign keys=false;");
            });

            yield return CreateSingleton(container => new Npgsql.NpgsqlConnectionStringBuilder("server=localhost;user id=teste;password=teste;CommandTimeout=3600;Timeout=120;Pooling=True;") { Database = "testedatabaselocal" });

            yield return CreateSingleton(container =>
            {
                return new System.Data.SqlClient.SqlConnectionStringBuilder("Server=tcp:localhost,1433;Initial Catalog=TesteDatabaseLocal")
                {
                    UserID = "teste",
                    Password = "teste",
                    MultipleActiveResultSets = false,
                    Encrypt = true,
                    TrustServerCertificate = true,
                    ConnectTimeout = 30,
                };
            });

            #region MySql
            yield return CreateSingleton(new MySqlBuilderTemplate());
            yield return CreateSingleton(c => new MySqlNativeCommandBuilder(c.Resolve<MySqlBuilderTemplate>()));
            yield return CreateTransient<MySqlConnectionManagerBuilder>();
            yield return CreateSingleton<MySqlConnectionManager>();
            yield return CreateSingleton<MySqlSchemaInformation>();
            #endregion MySql

            #region PostgreSql
            yield return CreateSingleton(new PostgreSqlBuilderTemplate());
            yield return CreateSingleton(c => new PostgreSqlNativeCommandBuilder(c.Resolve<PostgreSqlBuilderTemplate>()));
            yield return CreateTransient<PostgreSqlConnectionManagerBuilder>();
            yield return CreateSingleton<PostgreSqlConnectionManager>();
            yield return CreateSingleton<PostgreSqlSchemaInformation>();
            #endregion PostgreSql

            #region Sqlite
            yield return CreateSingleton(new SqliteBuilderTemplate());
            yield return CreateSingleton(c => new SqliteNativeCommandBuilder(c.Resolve<SqliteBuilderTemplate>()));
            yield return CreateTransient<SqliteConnectionManagerBuilder>();
            yield return CreateSingleton<FileSqliteConnectionManager>();
            yield return CreateSingleton<SqliteSchemaInformation>();
            #endregion Sqlite

            #region SqlServer
            yield return CreateSingleton(new SqlServerBuilderTemplate());
            yield return CreateSingleton(c => new SqlServerNativeCommandBuilder(c.Resolve<SqlServerBuilderTemplate>()));
            yield return CreateTransient<SqlServerConnectionManagerBuilder>();
            yield return CreateSingleton<SqlServerConnectionManager>();
            yield return CreateSingleton<SqlServerSchemaInformation>();
            #endregion SqlServer

            yield return CreateSingleton<INavigationControllerConfiguration, CrudGeneratorWpfNavigationControllerConfiguration>();
            yield return CreateSingleton<MainWindow>();
        }
    }
}
