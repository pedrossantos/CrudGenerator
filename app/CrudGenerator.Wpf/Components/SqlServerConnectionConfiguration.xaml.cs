using CrudGenerator.Core.ViewModels;
using Database.SqlServer.DataAccess;
using DependencyInversion;
using Framework.Validation;
using System.Windows;
using View.Abstractions;

namespace CrudGenerator.Components
{
    public partial class SqlServerConnectionConfiguration : DbConnectionConfiguration
    {
        public static readonly DependencyProperty SqlServerConnectionConfigurationViewModelProperty =
            DependencyProperty.Register(
                nameof(SqlServerConnectionConfigurationViewModel),
                typeof(SqlServerConnectionConfigurationViewModel),
                typeof(SqlServerConnectionConfiguration),
                new FrameworkPropertyMetadata(null, OnSqlServerConnectionConfigurationViewModelChanged));

        public static readonly DependencyProperty SqlServerServerNameOrIpAddressProperty =
            DependencyProperty.Register(
                nameof(SqlServerServerNameOrIpAddress),
                typeof(string),
                typeof(SqlServerConnectionConfiguration),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSqlServerServerNameOrIpAddressChanged)));

        public static readonly DependencyProperty SqlServerDatabaseNameProperty =
            DependencyProperty.Register(
                nameof(SqlServerDatabaseName),
                typeof(string),
                typeof(SqlServerConnectionConfiguration),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSqlServerDatabaseNameChanged)));

        public static readonly DependencyProperty SqlServerUserIdProperty =
            DependencyProperty.Register(
                nameof(SqlServerUserId),
                typeof(string),
                typeof(SqlServerConnectionConfiguration),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSqlServerUserIdChanged)));

        public static readonly DependencyProperty SqlServerPasswordProperty =
            DependencyProperty.Register(
                nameof(SqlServerPassword),
                typeof(string),
                typeof(SqlServerConnectionConfiguration),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSqlServerPasswordChanged)));

        public SqlServerConnectionConfiguration()
            : base()
        {
            PropertyChanged += SqlSeverConnectionConfigurationPropertyChanged;

            InitializeComponent();
        }

        public SqlServerConnectionConfiguration(
            SqlServerSchemaInformation sqlServerSchemaInformation,
            IMessageDialog messageDialog)
            : base(sqlServerSchemaInformation, messageDialog)
        {
            DbConnectionConfigurationViewModel = SqlServerConnectionConfigurationViewModel = new SqlServerConnectionConfigurationViewModel(
                sqlServerSchemaInformation,
                MessageDialog);

            PropertyChanged += SqlSeverConnectionConfigurationPropertyChanged;

            InitializeComponent();
        }

        [Injectable]
        public SqlServerConnectionConfiguration(SqlServerConnectionConfigurationViewModel sqlServerConnectionConfigurationViewModel)
            : base(sqlServerConnectionConfigurationViewModel)
        {
            Requires.NotNull(sqlServerConnectionConfigurationViewModel, nameof(sqlServerConnectionConfigurationViewModel));

            DbConnectionConfigurationViewModel = SqlServerConnectionConfigurationViewModel = sqlServerConnectionConfigurationViewModel;

            SqlServerServerNameOrIpAddress = sqlServerConnectionConfigurationViewModel.SqlServerServerNameOrIpAddress;
            SqlServerDatabaseName = sqlServerConnectionConfigurationViewModel.SqlServerDatabaseName;
            SqlServerUserId = sqlServerConnectionConfigurationViewModel.SqlServerUserId;
            SqlServerPassword = sqlServerConnectionConfigurationViewModel.SqlServerPassword;

            PropertyChanged += SqlSeverConnectionConfigurationPropertyChanged;

            InitializeComponent();
        }

        public SqlServerConnectionConfigurationViewModel SqlServerConnectionConfigurationViewModel
        {
            get { return GetValue(SqlServerConnectionConfigurationViewModelProperty) as SqlServerConnectionConfigurationViewModel; }
            set { SetValue(SqlServerConnectionConfigurationViewModelProperty, value); }
        }

        public string SqlServerServerNameOrIpAddress
        {
            get { return GetValue(SqlServerServerNameOrIpAddressProperty)?.ToString(); }
            set { SetValue(SqlServerServerNameOrIpAddressProperty, value); }
        }

        public string SqlServerDatabaseName
        {
            get { return GetValue(SqlServerDatabaseNameProperty)?.ToString(); }
            set { SetValue(SqlServerDatabaseNameProperty, value); }
        }

        public string SqlServerUserId
        {
            get { return GetValue(SqlServerUserIdProperty)?.ToString(); }
            set { SetValue(SqlServerUserIdProperty, value); }
        }

        public string SqlServerPassword
        {
            get { return GetValue(SqlServerPasswordProperty)?.ToString(); }
            set { SetValue(SqlServerPasswordProperty, value); }
        }

        private static void OnSqlServerConnectionConfigurationViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is SqlServerConnectionConfiguration sqlServerConnectionConfiguration) && (e.OldValue != e.NewValue))
            {
                if (e.NewValue is SqlServerConnectionConfigurationViewModel newSqlServerConnectionConfigurationViewModel)
                {
                    sqlServerConnectionConfiguration.DbConnectionConfigurationViewModel = newSqlServerConnectionConfigurationViewModel;

                    sqlServerConnectionConfiguration.SqlServerServerNameOrIpAddress = newSqlServerConnectionConfigurationViewModel.SqlServerServerNameOrIpAddress;
                    sqlServerConnectionConfiguration.SqlServerDatabaseName = newSqlServerConnectionConfigurationViewModel.SqlServerDatabaseName;
                    sqlServerConnectionConfiguration.SqlServerUserId = newSqlServerConnectionConfigurationViewModel.SqlServerUserId;
                    sqlServerConnectionConfiguration.SqlServerPassword = newSqlServerConnectionConfigurationViewModel.SqlServerPassword;
                }
            }
        }

        public static void OnSqlServerServerNameOrIpAddressChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is SqlServerConnectionConfiguration sqlServerConnectionConfiguration)
            {
                if (args.NewValue is string newSqlServerServerNameOrIpAddress)
                    sqlServerConnectionConfiguration.SqlServerConnectionConfigurationViewModel.SqlServerServerNameOrIpAddress = newSqlServerServerNameOrIpAddress;

                sqlServerConnectionConfiguration.PropertyChangedDispatcher.Notify(nameof(SqlServerServerNameOrIpAddress));
            }
        }

        public static void OnSqlServerDatabaseNameChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is SqlServerConnectionConfiguration sqlServerConnectionConfiguration)
            {
                if (args.NewValue is string newSqlServerDatabaseName)
                    sqlServerConnectionConfiguration.SqlServerConnectionConfigurationViewModel.SqlServerDatabaseName = newSqlServerDatabaseName;

                sqlServerConnectionConfiguration.PropertyChangedDispatcher.Notify(nameof(SqlServerDatabaseName));
            }
        }

        public static void OnSqlServerUserIdChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is SqlServerConnectionConfiguration sqlServerConnectionConfiguration)
            {
                if (args.NewValue is string newSqlServerUserId)
                    sqlServerConnectionConfiguration.SqlServerConnectionConfigurationViewModel.SqlServerUserId = newSqlServerUserId;

                sqlServerConnectionConfiguration.PropertyChangedDispatcher.Notify(nameof(SqlServerUserId));
            }
        }

        public static void OnSqlServerPasswordChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is SqlServerConnectionConfiguration sqlServerConnectionConfiguration)
            {
                if (args.NewValue is string newSqlServerPassword)
                    sqlServerConnectionConfiguration.SqlServerConnectionConfigurationViewModel.SqlServerPassword = newSqlServerPassword;

                sqlServerConnectionConfiguration.PropertyChangedDispatcher.Notify(nameof(SqlServerPassword));
            }
        }

        public override void Dispose()
        {
        }

        private void SqlSeverConnectionConfigurationPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == nameof(DbConnectionConfigurationViewModel)) && (DbConnectionConfigurationViewModel is SqlServerConnectionConfigurationViewModel sqlServerConnectionConfigurationViewModel))
            {
                SqlServerServerNameOrIpAddress = sqlServerConnectionConfigurationViewModel.SqlServerServerNameOrIpAddress;
                SqlServerDatabaseName = sqlServerConnectionConfigurationViewModel.SqlServerDatabaseName;
                SqlServerUserId = sqlServerConnectionConfigurationViewModel.SqlServerUserId;
                SqlServerPassword = sqlServerConnectionConfigurationViewModel.SqlServerPassword;
            }
        }
    }
}
