using Database.PostgreSql.DataAccess;
using DependencyInversion;
using Framework.NotifyChanges;
using System.Threading.Tasks;
using View.Abstractions;

namespace CrudGenerator.Core.ViewModels
{
    public class PostgreSqlConnectionConfigurationViewModel : DbConnectionConfigurationViewModel
    {
        private string _sqlServerServerNameOrIpAddress;
        private string _sqlServerDatabaseName;
        private string _sqlServerUserId;
        private string _sqlServerPassword;

        public PostgreSqlConnectionConfigurationViewModel()
            : base()
        {
        }

        [Injectable]
        public PostgreSqlConnectionConfigurationViewModel(
            PostgreSqlSchemaInformation sqlServerSchemaInformation,
            IMessageDialog messageDialog)
            : base(sqlServerSchemaInformation, messageDialog)
        {
            PresenterTitle = "PostgreSql Connection Configuration";
        }

        public string PostgreSqlServerNameOrIpAddress
        {
            get => _sqlServerServerNameOrIpAddress;
            set => PropertyChangedDispatcher.SetProperty(ref _sqlServerServerNameOrIpAddress, value);
        }

        public string PostgreSqlDatabaseName
        {
            get => _sqlServerDatabaseName;
            set => PropertyChangedDispatcher.SetProperty(ref _sqlServerDatabaseName, value);
        }

        public string PostgreSqlUserId
        {
            get => _sqlServerUserId;
            set => PropertyChangedDispatcher.SetProperty(ref _sqlServerUserId, value);
        }

        public string PostgreSqlPassword
        {
            get => _sqlServerPassword;
            set => PropertyChangedDispatcher.SetProperty(ref _sqlServerPassword, value);
        }

        public override void UpdateConnection()
        {
            SchemaInformation.UpdateConnection(
                new Npgsql.NpgsqlConnectionStringBuilder($"server={PostgreSqlServerNameOrIpAddress};Port=5432;user id={PostgreSqlUserId};password={PostgreSqlPassword};CommandTimeout=3600;Timeout=120;Pooling=True;")
                {
                    Database = $"{PostgreSqlDatabaseName}"
                });
        }

        public override async Task<bool> ValidateConnectionInformations()
        {
            if (string.IsNullOrEmpty(PostgreSqlServerNameOrIpAddress))
            {
                await MessageDialog.ShowAsync("Please enter the Server Name or Server IP Address!", "Error", EventLevel.Error);
                return false;
            }

            if (string.IsNullOrEmpty(PostgreSqlDatabaseName))
            {
                await MessageDialog.ShowAsync("Please enter the Database Name!", "Error", EventLevel.Error);
                return false;
            }

            if (string.IsNullOrEmpty(PostgreSqlUserId))
            {
                await MessageDialog.ShowAsync("Please enter the User ID!", "Error", EventLevel.Error);
                return false;
            }

            return true;
        }
    }
}
