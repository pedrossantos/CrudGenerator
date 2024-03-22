using Database.Sqlite.DataAccess;
using DependencyInversion;
using Framework.NotifyChanges;
using System.Threading.Tasks;
using View.Abstractions;

namespace CrudGenerator.Core.ViewModels
{
    public class SqliteConnectionConfigurationViewModel : DbConnectionConfigurationViewModel
    {
        private IOpenFileDialog _openFileDialog;
        private string _sqliteDatabasePath;

        public SqliteConnectionConfigurationViewModel()
            : base()
        {
        }

        [Injectable]
        public SqliteConnectionConfigurationViewModel(
            SqliteSchemaInformation sqliteSchemaInformation,
            IMessageDialog messageDialog,
            IOpenFileDialog openFileDialog)
            : base(sqliteSchemaInformation, messageDialog)
        {
            PresenterTitle = Messages.SqliteDatabaseConnectionConfiguration;

            _openFileDialog = openFileDialog;
        }

        public IOpenFileDialog OpenFileDialog
        {
            get => _openFileDialog;
            set => PropertyChangedDispatcher.SetProperty(ref _openFileDialog, value);
        }

        public string SqliteDatabasePath
        {
            get
            {
                return _sqliteDatabasePath;
            }

            set
            {
                if (_sqliteDatabasePath != value)
                {
                    PropertyChangedDispatcher.SetProperty(ref _sqliteDatabasePath, value);

                    ConnectionState = null;

                    SchemaInformation.Clear();
                    PropertyChangedDispatcher.Notify(nameof(SchemaInformation));
                }
            }
        }

        public override void UpdateConnection()
        {
            SchemaInformation.UpdateConnection(new System.Data.SQLite.SQLiteConnectionStringBuilder($"Data Source={SqliteDatabasePath};Version=3;DateTimeFormat=Ticks;foreign keys=false;"));
        }

        public override async Task<bool> ValidateConnectionInformations()
        {
            if (string.IsNullOrEmpty(SqliteDatabasePath))
            {
                await MessageDialog.ShowAsync(Messages.SelectFilePath, Messages.ErrorTitle, EventLevel.Error);
                return false;
            }

            return true;
        }
    }
}
