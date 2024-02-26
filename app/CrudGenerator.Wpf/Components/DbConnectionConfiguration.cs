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

namespace CrudGenerator.Components
{
    public abstract class DbConnectionConfiguration : UserControl, IPresenterHandle, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SchemaInformationProperty =
            DependencyProperty.Register(
                nameof(SchemaInformation),
                typeof(ISchemaInformation),
                typeof(DbConnectionConfiguration),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSchemaInformationChanged)));

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
            ISchemaInformation schemaInformation,
            IMessageDialog messageDialog)
            : this()
        {
            Requires.NotNull(schemaInformation, nameof(schemaInformation));
            Requires.NotNull(messageDialog, nameof(messageDialog));

            SchemaInformation = schemaInformation;
            MessageDialog = messageDialog;

            PropertyChanged += DbConnectionConfigurationPropertyChanged;
        }

        [Injectable]
        protected DbConnectionConfiguration(DbConnectionConfigurationViewModel dbConnectionConfigurationViewModel)
            : this()
        {
            Requires.NotNull(dbConnectionConfigurationViewModel, nameof(dbConnectionConfigurationViewModel));

            DbConnectionConfigurationViewModel = dbConnectionConfigurationViewModel;

            SchemaInformation = dbConnectionConfigurationViewModel.SchemaInformation;
            MessageDialog = dbConnectionConfigurationViewModel.MessageDialog;

            PropertyChanged += DbConnectionConfigurationPropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => _propertyChangedDispatcher.AddHandler(value);
            remove => _propertyChangedDispatcher.RemoveHandler(value);
        }

        public ISchemaInformation SchemaInformation
        {
            get { return GetValue(SchemaInformationProperty) as ISchemaInformation; }
            set { SetValue(SchemaInformationProperty, value); }
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

        public static void OnSchemaInformationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is DbConnectionConfiguration dbConnectionConfiguration)
            {
                if (dbConnectionConfiguration.DbConnectionConfigurationViewModel != null)
                    dbConnectionConfiguration.DbConnectionConfigurationViewModel.SchemaInformation = dbConnectionConfiguration.SchemaInformation;

                dbConnectionConfiguration.PropertyChangedDispatcher.Notify(nameof(SchemaInformation));
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
                    dbConnectionConfiguration.SchemaInformation = newDbConnectionConfigurationViewModel.SchemaInformation;
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
            if ((e.PropertyName == nameof(DbConnectionConfigurationViewModel)) && (DbConnectionConfigurationViewModel is DbConnectionConfigurationViewModel dbConnectionConfigurationViewModel))
            {
                MessageDialog = dbConnectionConfigurationViewModel.MessageDialog;
                SchemaInformation = dbConnectionConfigurationViewModel.SchemaInformation;
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
