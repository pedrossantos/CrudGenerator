using CrudGenerator.Core.ViewModels;
using Database.PostgreSql.DataAccess;
using DependencyInversion;
using Framework.Validation;
using System.Windows;
using View.Abstractions;

namespace CrudGenerator.Core.Wpf.Components
{
    public partial class PostgreSqlConnectionConfiguration : DbConnectionConfiguration
    {
        public static readonly DependencyProperty PostgreSqlConnectionConfigurationViewModelProperty =
            DependencyProperty.Register(
                nameof(PostgreSqlConnectionConfigurationViewModel),
                typeof(PostgreSqlConnectionConfigurationViewModel),
                typeof(PostgreSqlConnectionConfiguration),
                new FrameworkPropertyMetadata(null, OnPostgreSqlConnectionConfigurationViewModelChanged));

        public static readonly DependencyProperty PostgreSqlServerNameOrIpAddressProperty =
            DependencyProperty.Register(
                nameof(PostgreSqlServerNameOrIpAddress),
                typeof(string),
                typeof(PostgreSqlConnectionConfiguration),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPostgreSqlServerNameOrIpAddressChanged)));

        public static readonly DependencyProperty PostgreSqlDatabaseNameProperty =
            DependencyProperty.Register(
                nameof(PostgreSqlDatabaseName),
                typeof(string),
                typeof(PostgreSqlConnectionConfiguration),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPostgreSqlDatabaseNameChanged)));

        public static readonly DependencyProperty PostgreSqlUserIdProperty =
            DependencyProperty.Register(
                nameof(PostgreSqlUserId),
                typeof(string),
                typeof(PostgreSqlConnectionConfiguration),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPostgreSqlUserIdChanged)));

        public static readonly DependencyProperty PostgreSqlPasswordProperty =
            DependencyProperty.Register(
                nameof(PostgreSqlPassword),
                typeof(string),
                typeof(PostgreSqlConnectionConfiguration),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPostgreSqlPasswordChanged)));

        public PostgreSqlConnectionConfiguration()
            : base()
        {
            PropertyChanged += PostgreSqlConnectionConfigurationPropertyChanged;

            InitializeComponent();
        }

        public PostgreSqlConnectionConfiguration(
            PostgreSqlSchemaInformation mySqlSchemaInformation,
            IMessageDialog messageDialog)
            : base(mySqlSchemaInformation, messageDialog)
        {
            DbConnectionConfigurationViewModel = PostgreSqlConnectionConfigurationViewModel = new PostgreSqlConnectionConfigurationViewModel(
                mySqlSchemaInformation,
                MessageDialog);

            PropertyChanged += PostgreSqlConnectionConfigurationPropertyChanged;

            InitializeComponent();
        }

        [Injectable]
        public PostgreSqlConnectionConfiguration(PostgreSqlConnectionConfigurationViewModel mySqlConnectionConfigurationViewModel)
            : base(mySqlConnectionConfigurationViewModel)
        {
            Requires.NotNull(mySqlConnectionConfigurationViewModel, nameof(mySqlConnectionConfigurationViewModel));

            DbConnectionConfigurationViewModel = PostgreSqlConnectionConfigurationViewModel = mySqlConnectionConfigurationViewModel;
            
            PostgreSqlServerNameOrIpAddress = mySqlConnectionConfigurationViewModel.PostgreSqlServerNameOrIpAddress;
            PostgreSqlDatabaseName = mySqlConnectionConfigurationViewModel.PostgreSqlDatabaseName;
            PostgreSqlUserId = mySqlConnectionConfigurationViewModel.PostgreSqlUserId;
            PostgreSqlPassword = mySqlConnectionConfigurationViewModel.PostgreSqlPassword;

            PropertyChanged += PostgreSqlConnectionConfigurationPropertyChanged;

            InitializeComponent();
        }

        public PostgreSqlConnectionConfigurationViewModel PostgreSqlConnectionConfigurationViewModel
        {
            get { return GetValue(PostgreSqlConnectionConfigurationViewModelProperty) as PostgreSqlConnectionConfigurationViewModel; }
            set { SetValue(PostgreSqlConnectionConfigurationViewModelProperty, value); }
        }

        public string PostgreSqlServerNameOrIpAddress
        {
            get { return GetValue(PostgreSqlServerNameOrIpAddressProperty)?.ToString(); }
            set { SetValue(PostgreSqlServerNameOrIpAddressProperty, value); }
        }

        public string PostgreSqlDatabaseName
        {
            get { return GetValue(PostgreSqlDatabaseNameProperty)?.ToString(); }
            set { SetValue(PostgreSqlDatabaseNameProperty, value); }
        }

        public string PostgreSqlUserId
        {
            get { return GetValue(PostgreSqlUserIdProperty)?.ToString(); }
            set { SetValue(PostgreSqlUserIdProperty, value); }
        }

        public string PostgreSqlPassword
        {
            get { return GetValue(PostgreSqlPasswordProperty)?.ToString(); }
            set { SetValue(PostgreSqlPasswordProperty, value); }
        }

        private static void OnPostgreSqlConnectionConfigurationViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is PostgreSqlConnectionConfiguration mySqlConnectionConfiguration) && (e.OldValue != e.NewValue))
            {
                if (e.NewValue is PostgreSqlConnectionConfigurationViewModel newPostgreSqlConnectionConfigurationViewModel)
                {
                    mySqlConnectionConfiguration.DbConnectionConfigurationViewModel = newPostgreSqlConnectionConfigurationViewModel;

                    mySqlConnectionConfiguration.PostgreSqlServerNameOrIpAddress = newPostgreSqlConnectionConfigurationViewModel.PostgreSqlServerNameOrIpAddress;
                    mySqlConnectionConfiguration.PostgreSqlDatabaseName = newPostgreSqlConnectionConfigurationViewModel.PostgreSqlDatabaseName;
                    mySqlConnectionConfiguration.PostgreSqlUserId = newPostgreSqlConnectionConfigurationViewModel.PostgreSqlUserId;
                    mySqlConnectionConfiguration.PostgreSqlPassword = newPostgreSqlConnectionConfigurationViewModel.PostgreSqlPassword;
                }
            }
        }

        public static void OnPostgreSqlServerNameOrIpAddressChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is PostgreSqlConnectionConfiguration mySqlConnectionConfiguration)
            {
                if (args.NewValue is string newPostgreSqlServerNameOrIpAddress)
                    mySqlConnectionConfiguration.PostgreSqlConnectionConfigurationViewModel.PostgreSqlServerNameOrIpAddress = newPostgreSqlServerNameOrIpAddress;

                mySqlConnectionConfiguration.PropertyChangedDispatcher.Notify(nameof(PostgreSqlServerNameOrIpAddress));
            }
        }

        public static void OnPostgreSqlDatabaseNameChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is PostgreSqlConnectionConfiguration mySqlConnectionConfiguration)
            {
                if (args.NewValue is string newPostgreSqlDatabaseName)
                    mySqlConnectionConfiguration.PostgreSqlConnectionConfigurationViewModel.PostgreSqlDatabaseName = newPostgreSqlDatabaseName;

                mySqlConnectionConfiguration.PropertyChangedDispatcher.Notify(nameof(PostgreSqlDatabaseName));
            }
        }

        public static void OnPostgreSqlUserIdChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is PostgreSqlConnectionConfiguration mySqlConnectionConfiguration)
            {
                if (args.NewValue is string newPostgreSqlUserId)
                    mySqlConnectionConfiguration.PostgreSqlConnectionConfigurationViewModel.PostgreSqlUserId = newPostgreSqlUserId;

                mySqlConnectionConfiguration.PropertyChangedDispatcher.Notify(nameof(PostgreSqlUserId));
            }
        }

        public static void OnPostgreSqlPasswordChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is PostgreSqlConnectionConfiguration mySqlConnectionConfiguration)
            {
                if (args.NewValue is string newPostgreSqlPassword)
                    mySqlConnectionConfiguration.PostgreSqlConnectionConfigurationViewModel.PostgreSqlPassword = newPostgreSqlPassword;

                mySqlConnectionConfiguration.PropertyChangedDispatcher.Notify(nameof(PostgreSqlPassword));
            }
        }

        public override void Dispose()
        {
        }

        private void PostgreSqlConnectionConfigurationPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == nameof(DbConnectionConfigurationViewModel)) && (DbConnectionConfigurationViewModel is PostgreSqlConnectionConfigurationViewModel mySqlConnectionConfigurationViewModel))
            {
                PostgreSqlServerNameOrIpAddress = mySqlConnectionConfigurationViewModel.PostgreSqlServerNameOrIpAddress;
                PostgreSqlDatabaseName = mySqlConnectionConfigurationViewModel.PostgreSqlDatabaseName;
                PostgreSqlUserId = mySqlConnectionConfigurationViewModel.PostgreSqlUserId;
                PostgreSqlPassword = mySqlConnectionConfigurationViewModel.PostgreSqlPassword;
            }
        }
    }
}
