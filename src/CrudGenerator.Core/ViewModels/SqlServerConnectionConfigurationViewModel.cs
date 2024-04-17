using Database.SqlServer.DataAccess;
using DependencyInversion;
using Framework.NotifyChanges;
using System.Threading.Tasks;
using View.Abstractions;

namespace CrudGenerator.Core.ViewModels
{
    public class SqlServerConnectionConfigurationViewModel : DbConnectionConfigurationViewModel
    {
        private string _sqlServerServerNameOrIpAddress;
        private string _sqlServerDatabaseName;
        private string _sqlServerUserId;
        private string _sqlServerPassword;

        public SqlServerConnectionConfigurationViewModel()
            : base()
        {
        }

        [Injectable]
        public SqlServerConnectionConfigurationViewModel(
            SqlServerConnectionManager sqlServerConnectionManager,
            SqlServerSchemaInformation sqlServerSchemaInformation,
            IMessageDialog messageDialog)
            : base(sqlServerConnectionManager, sqlServerSchemaInformation, messageDialog)
        {
            PresenterTitle = Messages.SqlServerDatabaseConnectionConfiguration;
        }

        public string SqlServerServerNameOrIpAddress
        {
            get => _sqlServerServerNameOrIpAddress;
            set => PropertyChangedDispatcher.SetProperty(ref _sqlServerServerNameOrIpAddress, value);
        }

        public string SqlServerDatabaseName
        {
            get => _sqlServerDatabaseName;
            set => PropertyChangedDispatcher.SetProperty(ref _sqlServerDatabaseName, value);
        }

        public string SqlServerUserId
        {
            get => _sqlServerUserId;
            set => PropertyChangedDispatcher.SetProperty(ref _sqlServerUserId, value);
        }

        public string SqlServerPassword
        {
            get => _sqlServerPassword;
            set => PropertyChangedDispatcher.SetProperty(ref _sqlServerPassword, value);
        }

        public override void UpdateConnection()
        {
            ConnectionManager.UpdateConnectionStringBuilder(
                new System.Data.SqlClient.SqlConnectionStringBuilder($"Server=tcp:{SqlServerServerNameOrIpAddress},1433;Initial Catalog={SqlServerDatabaseName}")
                {
                    UserID = SqlServerUserId,
                    Password = SqlServerPassword,
                    MultipleActiveResultSets = false,
                    Encrypt = true,
                    TrustServerCertificate = true,
                    ConnectTimeout = 30,
                });
        }

        public override void UpdateSchemaInformationSchemaName()
        {
            if (ConnectionState == true)
                SchemaInformationBase.SchemaName = SqlServerDatabaseName;
            else
                SchemaInformationBase.SchemaName = string.Empty;
        }

        public override async Task<bool> ValidateConnectionInformations()
        {
            if (string.IsNullOrEmpty(SqlServerServerNameOrIpAddress))
            {
                await MessageDialog.ShowAsync(Messages.EnterServerNameOrIpAddres, Messages.ErrorTitle, EventLevel.Error);
                return false;
            }

            if (string.IsNullOrEmpty(SqlServerDatabaseName))
            {
                await MessageDialog.ShowAsync(Messages.EnterDatabaseName, Messages.ErrorTitle, EventLevel.Error);
                return false;
            }

            if (string.IsNullOrEmpty(SqlServerUserId))
            {
                await MessageDialog.ShowAsync(Messages.EnterUserId, Messages.ErrorTitle, EventLevel.Error);
                return false;
            }

            return true;
        }
    }
}
