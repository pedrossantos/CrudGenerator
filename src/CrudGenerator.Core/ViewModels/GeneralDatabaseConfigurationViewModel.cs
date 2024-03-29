﻿using Database.DataMapping;
using DependencyInversion;
using Framework.Validation;
using System.Collections.Generic;
using View.Abstractions;
using View.Abstractions.UseCase;

namespace CrudGenerator.Core.ViewModels
{
    public class GeneralDatabaseConfigurationViewModel : ViewModelBase, IDatabaseConfigurationViewModel<Framework.Void, Framework.Void>
    {
        private MySqlConnectionConfigurationViewModel _mySqlConnectionConfigurationViewModel;
        private PostgreSqlConnectionConfigurationViewModel _postgreSqlConnectionConfigurationViewModel;
        private SqliteConnectionConfigurationViewModel _sqliteConnectionConfigurationViewModel;
        private SqlServerConnectionConfigurationViewModel _sqlServerConnectionConfigurationViewModel;

        private IMessageDialog _messageDialog;
        private IOpenFileDialog _openFileDialog;

        private IEnumerable<DatabaseTypes> _avaiableDatabaseTypes;
        private DatabaseTypes _selectedDatabaseType;
        private NullSchemaInformation _nullSchemaInformation;
        private bool _canReadDatabaseSchema;

        public GeneralDatabaseConfigurationViewModel()
        {
        }

        [Injectable]
        public GeneralDatabaseConfigurationViewModel(
            MySqlConnectionConfigurationViewModel mySqlConnectionConfigurationViewModel,
            PostgreSqlConnectionConfigurationViewModel postgreSqlConnectionConfigurationViewModel,
            SqliteConnectionConfigurationViewModel sqliteConnectionConfigurationViewModel,
            SqlServerConnectionConfigurationViewModel sqlServerConnectionConfigurationViewModel,
            IMessageDialog messageDialog,
            IOpenFileDialog openFileDialog)
        {
            Requires.NotNull(mySqlConnectionConfigurationViewModel, nameof(mySqlConnectionConfigurationViewModel));
            Requires.NotNull(postgreSqlConnectionConfigurationViewModel, nameof(postgreSqlConnectionConfigurationViewModel));
            Requires.NotNull(sqliteConnectionConfigurationViewModel, nameof(sqliteConnectionConfigurationViewModel));
            Requires.NotNull(sqlServerConnectionConfigurationViewModel, nameof(sqlServerConnectionConfigurationViewModel));
            Requires.NotNull(messageDialog, nameof(messageDialog));
            Requires.NotNull(openFileDialog, nameof(openFileDialog));

            PresenterTitle = Messages.DatabaseConnectionConfiguration;

            _nullSchemaInformation = new NullSchemaInformation();

            _mySqlConnectionConfigurationViewModel = mySqlConnectionConfigurationViewModel;
            _mySqlConnectionConfigurationViewModel.PropertyChanged += ConnectionConfigurationViewModelPropertyChanged;

            _postgreSqlConnectionConfigurationViewModel = postgreSqlConnectionConfigurationViewModel;
            _postgreSqlConnectionConfigurationViewModel.PropertyChanged += ConnectionConfigurationViewModelPropertyChanged;

            _sqliteConnectionConfigurationViewModel = sqliteConnectionConfigurationViewModel;
            _sqliteConnectionConfigurationViewModel.PropertyChanged += ConnectionConfigurationViewModelPropertyChanged;

            _sqlServerConnectionConfigurationViewModel = sqlServerConnectionConfigurationViewModel;
            _sqlServerConnectionConfigurationViewModel.PropertyChanged += ConnectionConfigurationViewModelPropertyChanged;

            _messageDialog = messageDialog;
            _openFileDialog = openFileDialog;

            _avaiableDatabaseTypes = new DatabaseTypes[]
            {
                DatabaseTypes.None,
                DatabaseTypes.MySql,
                DatabaseTypes.PostgreSql,
                DatabaseTypes.Sqlite,
                DatabaseTypes.SqlServer,
            };

            _selectedDatabaseType = DatabaseTypes.None;
        }

        public IMessageDialog MessageDialog
        {
            get => _messageDialog;
            set => PropertyChangedDispatcher.SetProperty(ref _messageDialog, value);
        }

        public IOpenFileDialog OpenFileDialog
        {
            get => _openFileDialog;
            set => PropertyChangedDispatcher.SetProperty(ref _openFileDialog, value);
        }

        public NullSchemaInformation NullSchemaInformation
        {
            get => _nullSchemaInformation;
            set => PropertyChangedDispatcher.SetProperty(ref _nullSchemaInformation, value);
        }

        public MySqlConnectionConfigurationViewModel MySqlConnectionConfigurationViewModel
        {
            get => _mySqlConnectionConfigurationViewModel;
            set => PropertyChangedDispatcher.SetProperty(ref _mySqlConnectionConfigurationViewModel, value);
        }

        public PostgreSqlConnectionConfigurationViewModel PostgreSqlConnectionConfigurationViewModel
        {
            get => _postgreSqlConnectionConfigurationViewModel;
            set => PropertyChangedDispatcher.SetProperty(ref _postgreSqlConnectionConfigurationViewModel, value);
        }

        public SqliteConnectionConfigurationViewModel SqliteConnectionConfigurationViewModel
        {
            get => _sqliteConnectionConfigurationViewModel;
            set => PropertyChangedDispatcher.SetProperty(ref _sqliteConnectionConfigurationViewModel, value);
        }

        public SqlServerConnectionConfigurationViewModel SqlServerConnectionConfigurationViewModel
        {
            get => _sqlServerConnectionConfigurationViewModel;
            set => PropertyChangedDispatcher.SetProperty(ref _sqlServerConnectionConfigurationViewModel, value);
        }

        public IEnumerable<DatabaseTypes> AvaiableDatabaseTypes
        {
            get => _avaiableDatabaseTypes;
            set => PropertyChangedDispatcher.SetProperty(ref _avaiableDatabaseTypes, value);
        }

        public DatabaseTypes SelectedDatabaseType
        {
            get => _selectedDatabaseType;
            set
            {
                if (_selectedDatabaseType != value)
                {
                    PropertyChangedDispatcher.SetProperty(ref _selectedDatabaseType, value);

                    CanReadDatabaseSchema = false;

                    PropertyChangedDispatcher.Notify(nameof(MySqlConnectionConfigurationViewModel));
                    PropertyChangedDispatcher.Notify(nameof(PostgreSqlConnectionConfigurationViewModel));
                    PropertyChangedDispatcher.Notify(nameof(SqliteConnectionConfigurationViewModel));
                    PropertyChangedDispatcher.Notify(nameof(SqlServerConnectionConfigurationViewModel));
                }
            }
        }

        public bool CanReadDatabaseSchema
        {
            get => _canReadDatabaseSchema;
            private set => PropertyChangedDispatcher.SetProperty(ref _canReadDatabaseSchema, value);
        }

        private void ConnectionConfigurationViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == nameof(DbConnectionConfigurationViewModel.ConnectionState))
                && (sender is DbConnectionConfigurationViewModel dbConnectionConfigurationViewModel))
            {
                if (dbConnectionConfigurationViewModel.ConnectionState == true)
                    CanReadDatabaseSchema = true;
                else
                    CanReadDatabaseSchema = false;
            }
        }
    }
}
