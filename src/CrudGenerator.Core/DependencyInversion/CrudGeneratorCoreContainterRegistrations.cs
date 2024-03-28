using CrudGenerator.Core.ViewModels;
using DependencyInversion;
using Framework;
using System.Collections.Generic;
using System.Data.SQLite;
using View.Abstractions.IO;

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
            yield return CreateTransient<PostgreSqlConnectionConfigurationViewModel>();
            yield return CreateTransient<SqliteConnectionConfigurationViewModel>();
            yield return CreateTransient<SqlServerConnectionConfigurationViewModel>();
            yield return CreateTransient<SchemaInformationGenetatorViewModel>();
            yield return CreateTransient<ProjectGeneratorViewModel>();
        }
    }
}