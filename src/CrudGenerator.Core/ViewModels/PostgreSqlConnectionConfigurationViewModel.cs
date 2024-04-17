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
            PostgreSqlConnectionManager sqlServerConnectionManager,
            PostgreSqlSchemaInformation postgreSqlSchemaInformation,
            IMessageDialog messageDialog)
            : base(sqlServerConnectionManager, postgreSqlSchemaInformation, messageDialog)
        {
            PresenterTitle = Messages.PostgreSqlDatabaseConnectionConfiguration;
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
            ConnectionManager.UpdateConnectionStringBuilder(
                new Npgsql.NpgsqlConnectionStringBuilder($"server={PostgreSqlServerNameOrIpAddress};Port=5432;user id={PostgreSqlUserId};password={PostgreSqlPassword};CommandTimeout=3600;Timeout=120;Pooling=True;")
                {
                    Database = $"{PostgreSqlDatabaseName}"
                });
        }

        public override void UpdateSchemaInformationSchemaName()
        {
            if (ConnectionState == true)
                SchemaInformationBase.SchemaName = PostgreSqlDatabaseName;
            else
                SchemaInformationBase.SchemaName = string.Empty;
        }

        public override async Task<bool> ValidateConnectionInformations()
        {
            if (string.IsNullOrEmpty(PostgreSqlServerNameOrIpAddress))
            {
                await MessageDialog.ShowAsync(Messages.EnterServerNameOrIpAddres, Messages.ErrorTitle, EventLevel.Error);
                return false;
            }

            if (string.IsNullOrEmpty(PostgreSqlDatabaseName))
            {
                await MessageDialog.ShowAsync(Messages.EnterDatabaseName, Messages.ErrorTitle, EventLevel.Error);
                return false;
            }

            if (string.IsNullOrEmpty(PostgreSqlUserId))
            {
                await MessageDialog.ShowAsync(Messages.EnterUserId, Messages.ErrorTitle, EventLevel.Error);
                return false;
            }

            return true;
        }
    }
}
