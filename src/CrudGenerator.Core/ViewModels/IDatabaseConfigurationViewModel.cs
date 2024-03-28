using Database.DataMapping;
using System.Collections.Generic;
using System.ComponentModel;
using View.Abstractions;
using View.Abstractions.UseCase;

namespace CrudGenerator.Core.ViewModels
{
    public interface IDatabaseConfigurationViewModel : INotifyPropertyChanged
    {
        public IMessageDialog MessageDialog { get; set; }

        public IOpenFileDialog OpenFileDialog { get; set; }

        public NullSchemaInformation NullSchemaInformation { get; set; }

        public MySqlConnectionConfigurationViewModel MySqlConnectionConfigurationViewModel { get; set; }

        public PostgreSqlConnectionConfigurationViewModel PostgreSqlConnectionConfigurationViewModel { get; set; }

        public SqliteConnectionConfigurationViewModel SqliteConnectionConfigurationViewModel { get; set; }

        public SqlServerConnectionConfigurationViewModel SqlServerConnectionConfigurationViewModel { get; set; }

        public IEnumerable<DatabaseTypes> AvaiableDatabaseTypes { get; set; }

        public DatabaseTypes SelectedDatabaseType { get; set; }

        public bool CanReadDatabaseSchema { get; }
    }

    public interface IDatabaseConfigurationViewModel<TInput, TResult> : IViewModel<TInput, TResult>, IDatabaseConfigurationViewModel
    {

    }
}