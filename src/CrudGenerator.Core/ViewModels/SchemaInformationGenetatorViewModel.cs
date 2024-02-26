using Database.DataAccess;
using Database.DataMapping;
using Database.MySql.DataAccess;
using Database.Sqlite.DataAccess;
using Database.SqlServer.DataAccess;
using DependencyInversion;
using System.Threading;
using View.Abstractions.UseCase;

namespace CrudGenerator.Core.ViewModels
{
    public class SchemaInformationGenetatorViewModel : ViewModelBase
    {
        private DatabaseTypes _selectedDatabaseType;
        private MySqlSchemaInformation _mySqlSchemaInformation;
        private SqliteSchemaInformation _sqliteSchemaInformation;
        private SqlServerSchemaInformation _sqlServerSchemaInformation;

        private bool _generatingSchemaInformations;
        private Thread _threadGenerateEntities;

        public SchemaInformationGenetatorViewModel()
        {
        }

        [Injectable]
        public SchemaInformationGenetatorViewModel(
            MySqlSchemaInformation mySqlSchemaInformation,
            SqliteSchemaInformation sqliteSchemaInformation,
            SqlServerSchemaInformation sqlServerSchemaInformation)
        {
            _mySqlSchemaInformation = mySqlSchemaInformation;
            _sqliteSchemaInformation = sqliteSchemaInformation;
            _sqlServerSchemaInformation = sqlServerSchemaInformation;
        }

        public ISchemaInformation SchemaInformation
        {
            get
            {
                if (SelectedDatabaseType == DatabaseTypes.MySql)
                    return _mySqlSchemaInformation;
                else if (SelectedDatabaseType == DatabaseTypes.Sqlite)
                    return _sqliteSchemaInformation;
                else if (SelectedDatabaseType == DatabaseTypes.SqlServer)
                    return _sqlServerSchemaInformation;

                return null;
            }
        }

        public DatabaseTypes SelectedDatabaseType
        {
            get => _selectedDatabaseType;
            set => PropertyChangedDispatcher.SetProperty(ref _selectedDatabaseType, value);
        }

        public MySqlSchemaInformation MySqlSchemaInformation
        {
            get
            {
                return _mySqlSchemaInformation;
            }

            set
            {
                if (_mySqlSchemaInformation != value)
                {
                    PropertyChangedDispatcher.SetProperty(ref _mySqlSchemaInformation, value);
                    if (_selectedDatabaseType == DatabaseTypes.MySql)
                        PropertyChangedDispatcher.Notify(nameof(SchemaInformation));
                }
            }
        }

        public SqliteSchemaInformation SqliteSchemaInformation
        {
            get
            {
                return _sqliteSchemaInformation;
            }

            set
            {
                if (_sqliteSchemaInformation != value)
                {
                    PropertyChangedDispatcher.SetProperty(ref _sqliteSchemaInformation, value);
                    if (_selectedDatabaseType == DatabaseTypes.Sqlite)
                        PropertyChangedDispatcher.Notify(nameof(SchemaInformation));
                }
            }
        }

        public SqlServerSchemaInformation SqlServerSchemaInformation
        {
            get
            {
                return _sqlServerSchemaInformation;
            }

            set
            {
                if (_sqlServerSchemaInformation != value)
                {
                    PropertyChangedDispatcher.SetProperty(ref _sqlServerSchemaInformation, value);
                    if (_selectedDatabaseType == DatabaseTypes.SqlServer)
                        PropertyChangedDispatcher.Notify(nameof(SchemaInformation));
                }
            }
        }

        public bool GeneratingSchemaInformations
        {
            get => _generatingSchemaInformations;
            set => PropertyChangedDispatcher.SetProperty(ref _generatingSchemaInformations, value);
        }

        public void GenerateSchemaInformations()
        {
            if ((!_generatingSchemaInformations) && (_threadGenerateEntities != null) && (_threadGenerateEntities.IsAlive))
            {
                _threadGenerateEntities.Abort();
                _threadGenerateEntities = null;
            }

            GeneratingSchemaInformations = true;
            _threadGenerateEntities = new Thread(new ParameterizedThreadStart(DoGenerateEntities));
            _threadGenerateEntities.Start(new object[] { SchemaInformation });
        }

        private void DoGenerateEntities(object args)
        {
            ISchemaInformation schemaInformation;
            try
            {
                schemaInformation = ((object[])args)[0] as ISchemaInformation;
                if (schemaInformation != null)
                {
                    schemaInformation.PopulateSchemaTableMappings();
                    PropertyChangedDispatcher.Notify(nameof(SchemaInformation));
                }
            }
            finally
            {
                GeneratingSchemaInformations = false;
            }
        }
    }
}
