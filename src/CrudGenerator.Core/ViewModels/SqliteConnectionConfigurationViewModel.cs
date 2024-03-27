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
            FileSqliteConnectionManager sqliteConnectionManager,
            IMessageDialog messageDialog,
            IOpenFileDialog openFileDialog)
            : base(sqliteConnectionManager, messageDialog)
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

                    PropertyChangedDispatcher.Notify(nameof(ConnectionManager));
                }
            }
        }

        public override void UpdateConnection()
        {
            ConnectionManager.UpdateConnectionStringBuilder(new System.Data.SQLite.SQLiteConnectionStringBuilder($"Data Source={SqliteDatabasePath};Version=3;DateTimeFormat=Ticks;foreign keys=false;"));
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
