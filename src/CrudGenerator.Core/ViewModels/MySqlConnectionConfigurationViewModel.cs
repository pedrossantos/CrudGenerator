using Database.MySql.DataAccess;
using DependencyInversion;
using Framework.NotifyChanges;
using System.Threading.Tasks;
using View.Abstractions;

namespace CrudGenerator.Core.ViewModels
{
    public class MySqlConnectionConfigurationViewModel : DbConnectionConfigurationViewModel
    {
        private string _mySqlServerNameOrIpAddress;
        private string _mySqlDatabaseName;
        private string _mySqlUserId;
        private string _mySqlPassword;

        public MySqlConnectionConfigurationViewModel()
        {
        }

        [Injectable]
        public MySqlConnectionConfigurationViewModel(
            MySqlConnectionManager mysSqlConnectionManager,
            IMessageDialog messageDialog)
            : base(mysSqlConnectionManager, messageDialog)
        {
            PresenterTitle = Messages.MySqlDatabaseConnectionConfiguration;
        }

        public string MySqlServerNameOrIpAddress
        {
            get => _mySqlServerNameOrIpAddress;
            set => PropertyChangedDispatcher.SetProperty(ref _mySqlServerNameOrIpAddress, value);
        }

        public string MySqlDatabaseName
        {
            get => _mySqlDatabaseName;
            set => PropertyChangedDispatcher.SetProperty(ref _mySqlDatabaseName, value);
        }

        public string MySqlUserId
        {
            get => _mySqlUserId;
            set => PropertyChangedDispatcher.SetProperty(ref _mySqlUserId, value);
        }

        public string MySqlPassword
        {
            get => _mySqlPassword;
            set => PropertyChangedDispatcher.SetProperty(ref _mySqlPassword, value);
        }

        public override void UpdateConnection()
        {
            ConnectionManager.UpdateConnectionStringBuilder(
                new MySql.Data.MySqlClient.MySqlConnectionStringBuilder($"Server={MySqlServerNameOrIpAddress};Database={MySqlDatabaseName}")
                {
                    UserID = MySqlUserId,
                    Password = MySqlPassword,
                });
        }

        public override async Task<bool> ValidateConnectionInformations()
        {
            if (string.IsNullOrEmpty(MySqlServerNameOrIpAddress))
            {
                await MessageDialog.ShowAsync(Messages.EnterServerNameOrIpAddres, Messages.ErrorTitle, EventLevel.Error);
                return false;
            }

            if (string.IsNullOrEmpty(MySqlDatabaseName))
            {
                await MessageDialog.ShowAsync(Messages.EnterDatabaseName, Messages.ErrorTitle, EventLevel.Error);
                return false;
            }

            if (string.IsNullOrEmpty(MySqlUserId))
            {
                await MessageDialog.ShowAsync(Messages.EnterUserId, Messages.ErrorTitle, EventLevel.Error);
                return false;
            }

            return true;
        }
    }
}
