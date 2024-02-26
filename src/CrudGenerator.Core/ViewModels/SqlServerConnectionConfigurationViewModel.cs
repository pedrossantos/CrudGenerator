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
            SqlServerSchemaInformation sqlServerSchemaInformation,
            IMessageDialog messageDialog)
            : base(sqlServerSchemaInformation, messageDialog)
        {
            PresenterTitle = "SqlServer Connection Configuration";
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
            SchemaInformation.UpdateConnection(
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

        public override async Task<bool> ValidateConnectionInformations()
        {
            if (string.IsNullOrEmpty(SqlServerServerNameOrIpAddress))
            {
                await MessageDialog.ShowAsync("Please enter the Server Name or Server IP Address!", "Error", EventLevel.Error);
                return false;
            }

            if (string.IsNullOrEmpty(SqlServerDatabaseName))
            {
                await MessageDialog.ShowAsync("Please enter the Database Name!", "Error", EventLevel.Error);
                return false;
            }

            if (string.IsNullOrEmpty(SqlServerUserId))
            {
                await MessageDialog.ShowAsync("Please enter the User ID!", "Error", EventLevel.Error);
                return false;
            }

            return true;
        }
    }
}
