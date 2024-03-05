using Database.MySql.DataAccess;
using Database.MySql.Sql;
using Database.Sqlite.DataAccess;
using Database.Sqlite.Sql;
using Database.SqlServer.DataAccess;
using Database.SqlServer.Sql;
using DependencyInversion;
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
            #region MySql
            yield return CreateSingleton(new MySqlBuilderTemplate());
            yield return CreateSingleton(c => new MySqlNativeCommandBuilder(c.Resolve<MySqlBuilderTemplate>()));
            yield return CreateTransient<MySqlConnectionManagerBuilder>();
            yield return CreateSingleton<MySqlConnectionManager>();
            yield return CreateSingleton<MySqlSchemaInformation>();
            #endregion MySql

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
