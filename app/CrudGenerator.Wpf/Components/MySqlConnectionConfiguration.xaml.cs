using CrudGenerator.Core.ViewModels;
using Database.MySql.DataAccess;
using DependencyInversion;
using Framework.Validation;
using System.Windows;
using View.Abstractions;

namespace CrudGenerator.Components
{
    public partial class MySqlConnectionConfiguration : DbConnectionConfiguration
    {
        public static readonly DependencyProperty MySqlConnectionConfigurationViewModelProperty =
            DependencyProperty.Register(
                nameof(MySqlConnectionConfigurationViewModel),
                typeof(MySqlConnectionConfigurationViewModel),
                typeof(MySqlConnectionConfiguration),
                new FrameworkPropertyMetadata(null, OnMySqlConnectionConfigurationViewModelChanged));

        public static readonly DependencyProperty MySqlServerNameOrIpAddressProperty =
            DependencyProperty.Register(
                nameof(MySqlServerNameOrIpAddress),
                typeof(string),
                typeof(MySqlConnectionConfiguration),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnMySqlServerNameOrIpAddressChanged)));

        public static readonly DependencyProperty MySqlDatabaseNameProperty =
            DependencyProperty.Register(
                nameof(MySqlDatabaseName),
                typeof(string),
                typeof(MySqlConnectionConfiguration),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnMySqlDatabaseNameChanged)));

        public static readonly DependencyProperty MySqlUserIdProperty =
            DependencyProperty.Register(
                nameof(MySqlUserId),
                typeof(string),
                typeof(MySqlConnectionConfiguration),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnMySqlUserIdChanged)));

        public static readonly DependencyProperty MySqlPasswordProperty =
            DependencyProperty.Register(
                nameof(MySqlPassword),
                typeof(string),
                typeof(MySqlConnectionConfiguration),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnMySqlPasswordChanged)));

        public MySqlConnectionConfiguration()
            : base()
        {
            PropertyChanged += MySqlConnectionConfigurationPropertyChanged;

            InitializeComponent();
        }

        public MySqlConnectionConfiguration(
            MySqlSchemaInformation mySqlSchemaInformation,
            IMessageDialog messageDialog)
            : base(mySqlSchemaInformation, messageDialog)
        {
            DbConnectionConfigurationViewModel = MySqlConnectionConfigurationViewModel = new MySqlConnectionConfigurationViewModel(
                mySqlSchemaInformation,
                MessageDialog);

            PropertyChanged += MySqlConnectionConfigurationPropertyChanged;

            InitializeComponent();
        }

        [Injectable]
        public MySqlConnectionConfiguration(MySqlConnectionConfigurationViewModel mySqlConnectionConfigurationViewModel)
            : base(mySqlConnectionConfigurationViewModel)
        {
            Requires.NotNull(mySqlConnectionConfigurationViewModel, nameof(mySqlConnectionConfigurationViewModel));

            DbConnectionConfigurationViewModel = MySqlConnectionConfigurationViewModel = mySqlConnectionConfigurationViewModel;
            
            MySqlServerNameOrIpAddress = mySqlConnectionConfigurationViewModel.MySqlServerNameOrIpAddress;
            MySqlDatabaseName = mySqlConnectionConfigurationViewModel.MySqlDatabaseName;
            MySqlUserId = mySqlConnectionConfigurationViewModel.MySqlUserId;
            MySqlPassword = mySqlConnectionConfigurationViewModel.MySqlPassword;

            PropertyChanged += MySqlConnectionConfigurationPropertyChanged;

            InitializeComponent();
        }

        public MySqlConnectionConfigurationViewModel MySqlConnectionConfigurationViewModel
        {
            get { return GetValue(MySqlConnectionConfigurationViewModelProperty) as MySqlConnectionConfigurationViewModel; }
            set { SetValue(MySqlConnectionConfigurationViewModelProperty, value); }
        }

        public string MySqlServerNameOrIpAddress
        {
            get { return GetValue(MySqlServerNameOrIpAddressProperty)?.ToString(); }
            set { SetValue(MySqlServerNameOrIpAddressProperty, value); }
        }

        public string MySqlDatabaseName
        {
            get { return GetValue(MySqlDatabaseNameProperty)?.ToString(); }
            set { SetValue(MySqlDatabaseNameProperty, value); }
        }

        public string MySqlUserId
        {
            get { return GetValue(MySqlUserIdProperty)?.ToString(); }
            set { SetValue(MySqlUserIdProperty, value); }
        }

        public string MySqlPassword
        {
            get { return GetValue(MySqlPasswordProperty)?.ToString(); }
            set { SetValue(MySqlPasswordProperty, value); }
        }

        private static void OnMySqlConnectionConfigurationViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is MySqlConnectionConfiguration mySqlConnectionConfiguration) && (e.OldValue != e.NewValue))
            {
                if (e.NewValue is MySqlConnectionConfigurationViewModel newMySqlConnectionConfigurationViewModel)
                {
                    mySqlConnectionConfiguration.DbConnectionConfigurationViewModel = newMySqlConnectionConfigurationViewModel;

                    mySqlConnectionConfiguration.MySqlServerNameOrIpAddress = newMySqlConnectionConfigurationViewModel.MySqlServerNameOrIpAddress;
                    mySqlConnectionConfiguration.MySqlDatabaseName = newMySqlConnectionConfigurationViewModel.MySqlDatabaseName;
                    mySqlConnectionConfiguration.MySqlUserId = newMySqlConnectionConfigurationViewModel.MySqlUserId;
                    mySqlConnectionConfiguration.MySqlPassword = newMySqlConnectionConfigurationViewModel.MySqlPassword;
                }
            }
        }

        public static void OnMySqlServerNameOrIpAddressChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is MySqlConnectionConfiguration mySqlConnectionConfiguration)
            {
                if (args.NewValue is string newMySqlServerNameOrIpAddress)
                    mySqlConnectionConfiguration.MySqlConnectionConfigurationViewModel.MySqlServerNameOrIpAddress = newMySqlServerNameOrIpAddress;

                mySqlConnectionConfiguration.PropertyChangedDispatcher.Notify(nameof(MySqlServerNameOrIpAddress));
            }
        }

        public static void OnMySqlDatabaseNameChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is MySqlConnectionConfiguration mySqlConnectionConfiguration)
            {
                if (args.NewValue is string newMySqlDatabaseName)
                    mySqlConnectionConfiguration.MySqlConnectionConfigurationViewModel.MySqlDatabaseName = newMySqlDatabaseName;

                mySqlConnectionConfiguration.PropertyChangedDispatcher.Notify(nameof(MySqlDatabaseName));
            }
        }

        public static void OnMySqlUserIdChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is MySqlConnectionConfiguration mySqlConnectionConfiguration)
            {
                if (args.NewValue is string newMySqlUserId)
                    mySqlConnectionConfiguration.MySqlConnectionConfigurationViewModel.MySqlUserId = newMySqlUserId;

                mySqlConnectionConfiguration.PropertyChangedDispatcher.Notify(nameof(MySqlUserId));
            }
        }

        public static void OnMySqlPasswordChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is MySqlConnectionConfiguration mySqlConnectionConfiguration)
            {
                if (args.NewValue is string newMySqlPassword)
                    mySqlConnectionConfiguration.MySqlConnectionConfigurationViewModel.MySqlPassword = newMySqlPassword;

                mySqlConnectionConfiguration.PropertyChangedDispatcher.Notify(nameof(MySqlPassword));
            }
        }

        public override void Dispose()
        {
        }

        private void MySqlConnectionConfigurationPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == nameof(DbConnectionConfigurationViewModel)) && (DbConnectionConfigurationViewModel is MySqlConnectionConfigurationViewModel mySqlConnectionConfigurationViewModel))
            {
                MySqlServerNameOrIpAddress = mySqlConnectionConfigurationViewModel.MySqlServerNameOrIpAddress;
                MySqlDatabaseName = mySqlConnectionConfigurationViewModel.MySqlDatabaseName;
                MySqlUserId = mySqlConnectionConfigurationViewModel.MySqlUserId;
                MySqlPassword = mySqlConnectionConfigurationViewModel.MySqlPassword;
            }
        }
    }
}
