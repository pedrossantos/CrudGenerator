using CrudGenerator.Core.ViewModels;
using Database.DataAccess;
using DependencyInversion;
using Framework.NotifyChanges;
using Framework.Validation;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using View.Abstractions;

namespace CrudGenerator.Core.Wpf.Components
{
    public abstract class DbConnectionConfiguration : UserControl, IPresenterHandle, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ConnectionManagerProperty =
            DependencyProperty.Register(
                nameof(ConnectionManager),
                typeof(ConnectionManager),
                typeof(DbConnectionConfiguration),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnConnectionManagerChanged)));

        public static readonly DependencyProperty MessageDialogProperty =
            DependencyProperty.Register(
                nameof(MessageDialog),
                typeof(IMessageDialog),
                typeof(DbConnectionConfiguration),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnMessageDialogChanged)));

        public static readonly DependencyProperty DbConnectionConfigurationViewModelProperty =
            DependencyProperty.Register(
                nameof(DbConnectionConfigurationViewModel),
                typeof(DbConnectionConfigurationViewModel),
                typeof(DbConnectionConfiguration),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnDbConnectionConfigurationViewModelChanged)));

        private PropertyChangedDispatcher _propertyChangedDispatcher;
        private Brush _connectionStateBrush;

        protected DbConnectionConfiguration()
        {
            _connectionStateBrush = Brushes.Yellow;
            _propertyChangedDispatcher = new PropertyChangedDispatcher(this, true);
        }

        protected DbConnectionConfiguration(
            ConnectionManager connectionManager,
            IMessageDialog messageDialog)
            : this()
        {
            Requires.NotNull(connectionManager, nameof(connectionManager));
            Requires.NotNull(messageDialog, nameof(messageDialog));

            ConnectionManager = connectionManager;
            MessageDialog = messageDialog;

            PropertyChanged += DbConnectionConfigurationPropertyChanged;
        }

        [Injectable]
        protected DbConnectionConfiguration(DbConnectionConfigurationViewModel dbConnectionConfigurationViewModel)
            : this()
        {
            Requires.NotNull(dbConnectionConfigurationViewModel, nameof(dbConnectionConfigurationViewModel));

            DbConnectionConfigurationViewModel = dbConnectionConfigurationViewModel;

            ConnectionManager = dbConnectionConfigurationViewModel.ConnectionManager;
            MessageDialog = dbConnectionConfigurationViewModel.MessageDialog;

            PropertyChanged += DbConnectionConfigurationPropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => _propertyChangedDispatcher.AddHandler(value);
            remove => _propertyChangedDispatcher.RemoveHandler(value);
        }

        public ConnectionManager ConnectionManager
        {
            get { return GetValue(ConnectionManagerProperty) as ConnectionManager; }
            set { SetValue(ConnectionManagerProperty, value); }
        }

        public IMessageDialog MessageDialog
        {
            get { return GetValue(MessageDialogProperty) as IMessageDialog; }
            set { SetValue(MessageDialogProperty, value); }
        }

        public DbConnectionConfigurationViewModel DbConnectionConfigurationViewModel
        {
            get { return GetValue(DbConnectionConfigurationViewModelProperty) as DbConnectionConfigurationViewModel; }
            set { SetValue(DbConnectionConfigurationViewModelProperty, value); }
        }

        public string Title => DbConnectionConfigurationViewModel.PresenterTitle;

        public PropertyChangedDispatcher PropertyChangedDispatcher => _propertyChangedDispatcher;

        public Brush ConnectionStateBrush
        {
            get => _connectionStateBrush;
            set => _propertyChangedDispatcher.SetProperty(ref _connectionStateBrush, value);
        }

        public static void OnConnectionManagerChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is DbConnectionConfiguration dbConnectionConfiguration)
            {
                if (dbConnectionConfiguration.DbConnectionConfigurationViewModel != null)
                    dbConnectionConfiguration.DbConnectionConfigurationViewModel.ConnectionManager = dbConnectionConfiguration.ConnectionManager;

                dbConnectionConfiguration.PropertyChangedDispatcher.Notify(nameof(ConnectionManager));
            }
        }

        public static void OnMessageDialogChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is DbConnectionConfiguration dbConnectionConfiguration)
            {
                if (dbConnectionConfiguration.DbConnectionConfigurationViewModel != null)
                    dbConnectionConfiguration.DbConnectionConfigurationViewModel.MessageDialog = dbConnectionConfiguration.MessageDialog;

                dbConnectionConfiguration.PropertyChangedDispatcher.Notify(nameof(MessageDialog));
            }
        }

        public static void OnDbConnectionConfigurationViewModelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is DbConnectionConfiguration dbConnectionConfiguration)
            {
                if (args.OldValue is DbConnectionConfigurationViewModel oldDbConnectionConfigurationViewModel)
                {
                    oldDbConnectionConfigurationViewModel.PropertyChanged -= dbConnectionConfiguration.DbConnectionConfigurationPropertyChanged;
                }

                if (args.NewValue is DbConnectionConfigurationViewModel newDbConnectionConfigurationViewModel)
                {
                    dbConnectionConfiguration.ConnectionManager = newDbConnectionConfigurationViewModel.ConnectionManager;
                    dbConnectionConfiguration.MessageDialog = newDbConnectionConfigurationViewModel.MessageDialog;

                    dbConnectionConfiguration.PropertyChangedDispatcher.Notify(nameof(DbConnectionConfigurationViewModel));

                    newDbConnectionConfigurationViewModel.PropertyChanged += dbConnectionConfiguration.DbConnectionConfigurationPropertyChanged;
                }
            }
        }

        public abstract void Dispose();

        protected async void TestConnectionPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            await DbConnectionConfigurationViewModel.TestConnection();
        }

        private void DbConnectionConfigurationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DbConnectionConfigurationViewModel) && DbConnectionConfigurationViewModel is DbConnectionConfigurationViewModel dbConnectionConfigurationViewModel)
            {
                MessageDialog = dbConnectionConfigurationViewModel.MessageDialog;
                ConnectionManager = dbConnectionConfigurationViewModel.ConnectionManager;
            }
            else if (e.PropertyName == nameof(DbConnectionConfigurationViewModel.ConnectionState))
            {
                if (DbConnectionConfigurationViewModel.ConnectionState == null)
                    ConnectionStateBrush = Brushes.Yellow;
                else if (DbConnectionConfigurationViewModel.ConnectionState == false)
                    ConnectionStateBrush = Brushes.Red;
                else if (DbConnectionConfigurationViewModel.ConnectionState == true)
                    ConnectionStateBrush = Brushes.Green;

                _propertyChangedDispatcher.Notify(nameof(ConnectionStateBrush));
            }
        }
    }
}
