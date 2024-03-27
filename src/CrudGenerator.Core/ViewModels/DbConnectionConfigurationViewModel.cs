using Database.DataAccess;
using DependencyInversion;
using Framework.NotifyChanges;
using System.Threading.Tasks;
using View.Abstractions;
using View.Abstractions.UseCase;

namespace CrudGenerator.Core.ViewModels
{
    public abstract class DbConnectionConfigurationViewModel : ViewModelBase
    {
        private ConnectionManager _connectionManager;
        private IMessageDialog _messageDialog;
        public bool? _connectionState;

        public DbConnectionConfigurationViewModel()
        {
        }

        [Injectable]
        public DbConnectionConfigurationViewModel(
            ConnectionManager connectionManager,
            IMessageDialog messageDialog)
            : base()
        {
            PresenterTitle = Messages.DatabaseConnectionConfiguration;

            _connectionManager = connectionManager;
            _messageDialog = messageDialog;
        }

        public ConnectionManager ConnectionManager
        {
            get => _connectionManager;
            set => PropertyChangedDispatcher.SetProperty(ref _connectionManager, value);
        }

        public IMessageDialog MessageDialog
        {
            get => _messageDialog;
            set => PropertyChangedDispatcher.SetProperty(ref _messageDialog, value);
        }

        public bool? ConnectionState
        {
            get => _connectionState;
            set => PropertyChangedDispatcher.SetProperty(ref _connectionState, value);
        }

        public abstract void UpdateConnection();

        public abstract Task<bool> ValidateConnectionInformations();

        public virtual async Task TestConnection()
        {
            ConnectionState = null;

            if (await ValidateConnectionInformations())
            {
                UpdateConnection();
                ConnectionState = _connectionManager.TestConnection();
            }
        }
    }
}
