using CrudGenerator.Core.ViewModels;
using Database.DataMapping;
using DependencyInversion;
using Framework.NotifyChanges;
using Framework.Validation;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using View.Abstractions;

namespace CrudGenerator.Core.Wpf.Components
{
    /// <summary>
    /// Interaction logic for GeneralDatabaseConfiguration.xaml
    /// </summary>
    public partial class GeneralDatabaseConfiguration : UserControl, IPresenterHandle, INotifyPropertyChanged
    {
        public static readonly DependencyProperty GeneralDatabaseConfigurationViewModelProperty =
            DependencyProperty.Register(
                nameof(GeneralDatabaseConfigurationViewModel),
                typeof(GeneralDatabaseConfigurationViewModel),
                typeof(GeneralDatabaseConfiguration),
                new FrameworkPropertyMetadata(null, OnGeneralDatabaseConfigurationViewModelChanged));

        public static readonly DependencyProperty NullSchemaInformationProperty =
            DependencyProperty.Register(
                nameof(NullSchemaInformation),
                typeof(NullSchemaInformation),
                typeof(GeneralDatabaseConfiguration),
                new FrameworkPropertyMetadata(null, OnNullSchemaInformationChanged));

        public static readonly DependencyProperty MySqlConnectionConfigurationViewModelProperty =
            DependencyProperty.Register(
                nameof(MySqlConnectionConfigurationViewModel),
                typeof(MySqlConnectionConfigurationViewModel),
                typeof(GeneralDatabaseConfiguration),
                new FrameworkPropertyMetadata(null, OnMySqlConnectionConfigurationViewModelChanged));

        public static readonly DependencyProperty PostgreSqlConnectionConfigurationViewModelProperty =
            DependencyProperty.Register(
                nameof(PostgreSqlConnectionConfigurationViewModel),
                typeof(PostgreSqlConnectionConfigurationViewModel),
                typeof(GeneralDatabaseConfiguration),
                new FrameworkPropertyMetadata(null, OnPostgreSqlConnectionConfigurationViewModelChanged));

        public static readonly DependencyProperty SqliteConnectionConfigurationViewModelProperty =
            DependencyProperty.Register(
                nameof(SqliteConnectionConfigurationViewModel),
                typeof(SqliteConnectionConfigurationViewModel),
                typeof(GeneralDatabaseConfiguration),
                new FrameworkPropertyMetadata(null, OnSqliteConnectionConfigurationViewModelChanged));

        public static readonly DependencyProperty SqlServerConnectionConfigurationViewModelProperty =
            DependencyProperty.Register(
                nameof(SqlServerConnectionConfigurationViewModel),
                typeof(SqlServerConnectionConfigurationViewModel),
                typeof(GeneralDatabaseConfiguration),
                new FrameworkPropertyMetadata(null, OnSqlServerConnectionConfigurationViewModelChanged));

        public static readonly DependencyProperty MessageDialogProperty =
            DependencyProperty.Register(
                nameof(MessageDialog),
                typeof(IMessageDialog),
                typeof(GeneralDatabaseConfiguration),
                new FrameworkPropertyMetadata(null, OnMessageDialogChanged));

        public static readonly DependencyProperty OpenFileDialogProperty =
            DependencyProperty.Register(
                nameof(OpenFileDialog),
                typeof(IOpenFileDialog),
                typeof(GeneralDatabaseConfiguration),
                new FrameworkPropertyMetadata(null, OnOpenFileDialogChanged));

        public static readonly DependencyProperty AvaiableDatabaseTypesProperty =
            DependencyProperty.Register(
                nameof(AvaiableDatabaseTypes),
                typeof(IEnumerable<DatabaseTypes>),
                typeof(GeneralDatabaseConfiguration),
                new FrameworkPropertyMetadata(null, OnAvaiableDatabaseTypesChanged));

        public static readonly DependencyProperty SelectedDatabaseTypeProperty =
            DependencyProperty.Register(
                nameof(SelectedDatabaseType),
                typeof(DatabaseTypes),
                typeof(GeneralDatabaseConfiguration),
                new FrameworkPropertyMetadata(DatabaseTypes.None, OnSelectedDatabaseTypeChanged));

        private PropertyChangedDispatcher _propertyChangedDispatcher;

        public GeneralDatabaseConfiguration()
        {
            _propertyChangedDispatcher = new PropertyChangedDispatcher(this, true);

            AvaiableDatabaseTypes = new DatabaseTypes[]
            {
                DatabaseTypes.None,
                DatabaseTypes.MySql,
                DatabaseTypes.Sqlite,
                DatabaseTypes.SqlServer,
            };

            InitializeComponent();
        }

        public GeneralDatabaseConfiguration(
            MySqlConnectionConfigurationViewModel mySqlConnectionConfigurationViewModel,
            PostgreSqlConnectionConfigurationViewModel postgreSqlConnectionConfigurationViewModel,
            SqliteConnectionConfigurationViewModel sqliteConnectionConfigurationViewModel,
            SqlServerConnectionConfigurationViewModel sqlServerConnectionConfigurationViewModel,
            IMessageDialog messageDialog,
            IOpenFileDialog openFileDialog)
        {
            Requires.NotNull(mySqlConnectionConfigurationViewModel, nameof(mySqlConnectionConfigurationViewModel));
            Requires.NotNull(postgreSqlConnectionConfigurationViewModel, nameof(postgreSqlConnectionConfigurationViewModel));
            Requires.NotNull(sqliteConnectionConfigurationViewModel, nameof(sqliteConnectionConfigurationViewModel));
            Requires.NotNull(sqlServerConnectionConfigurationViewModel, nameof(sqlServerConnectionConfigurationViewModel));
            Requires.NotNull(messageDialog, nameof(messageDialog));
            Requires.NotNull(openFileDialog, nameof(openFileDialog));

            _propertyChangedDispatcher = new PropertyChangedDispatcher(this, true);

            MessageDialog = messageDialog;
            OpenFileDialog = openFileDialog;

            GeneralDatabaseConfigurationViewModel = new GeneralDatabaseConfigurationViewModel(
                mySqlConnectionConfigurationViewModel,
                postgreSqlConnectionConfigurationViewModel,
                sqliteConnectionConfigurationViewModel,
                sqlServerConnectionConfigurationViewModel,
                messageDialog,
                OpenFileDialog);

            InitializeComponent();
        }

        [Injectable]
        public GeneralDatabaseConfiguration(GeneralDatabaseConfigurationViewModel generalDatabaseConfigurationViewModel)
        {
            Requires.NotNull(generalDatabaseConfigurationViewModel, nameof(generalDatabaseConfigurationViewModel));

            _propertyChangedDispatcher = new PropertyChangedDispatcher(this, true);

            GeneralDatabaseConfigurationViewModel = generalDatabaseConfigurationViewModel;

            NullSchemaInformation = GeneralDatabaseConfigurationViewModel.NullSchemaInformation;
            MySqlConnectionConfigurationViewModel = GeneralDatabaseConfigurationViewModel.MySqlConnectionConfigurationViewModel;
            SqliteConnectionConfigurationViewModel = GeneralDatabaseConfigurationViewModel.SqliteConnectionConfigurationViewModel;
            SqlServerConnectionConfigurationViewModel = GeneralDatabaseConfigurationViewModel.SqlServerConnectionConfigurationViewModel;
            MessageDialog = GeneralDatabaseConfigurationViewModel.MessageDialog;
            OpenFileDialog = GeneralDatabaseConfigurationViewModel.OpenFileDialog;

            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => _propertyChangedDispatcher.AddHandler(value);
            remove => _propertyChangedDispatcher.RemoveHandler(value);
        }

        public string Title => GeneralDatabaseConfigurationViewModel.PresenterTitle;

        public GeneralDatabaseConfigurationViewModel GeneralDatabaseConfigurationViewModel
        {
            get { return GetValue(GeneralDatabaseConfigurationViewModelProperty) as GeneralDatabaseConfigurationViewModel; }
            set { SetValue(GeneralDatabaseConfigurationViewModelProperty, value); }
        }

        public NullSchemaInformation NullSchemaInformation
        {
            get { return GetValue(NullSchemaInformationProperty) as NullSchemaInformation; }
            set { SetValue(NullSchemaInformationProperty, value); }
        }

        public MySqlConnectionConfigurationViewModel MySqlConnectionConfigurationViewModel
        {
            get { return GetValue(MySqlConnectionConfigurationViewModelProperty) as MySqlConnectionConfigurationViewModel; }
            set { SetValue(MySqlConnectionConfigurationViewModelProperty, value); }
        }

        public PostgreSqlConnectionConfigurationViewModel PostgreSqlConnectionConfigurationViewModel
        {
            get { return GetValue(PostgreSqlConnectionConfigurationViewModelProperty) as PostgreSqlConnectionConfigurationViewModel; }
            set { SetValue(PostgreSqlConnectionConfigurationViewModelProperty, value); }
        }

        public SqliteConnectionConfigurationViewModel SqliteConnectionConfigurationViewModel
        {
            get { return GetValue(SqliteConnectionConfigurationViewModelProperty) as SqliteConnectionConfigurationViewModel; }
            set { SetValue(SqliteConnectionConfigurationViewModelProperty, value); }
        }

        public SqlServerConnectionConfigurationViewModel SqlServerConnectionConfigurationViewModel
        {
            get { return GetValue(SqlServerConnectionConfigurationViewModelProperty) as SqlServerConnectionConfigurationViewModel; }
            set { SetValue(SqlServerConnectionConfigurationViewModelProperty, value); }
        }

        public IMessageDialog MessageDialog
        {
            get { return GetValue(MessageDialogProperty) as IMessageDialog; }
            set { SetValue(MessageDialogProperty, value); }
        }

        public IOpenFileDialog OpenFileDialog
        {
            get { return GetValue(OpenFileDialogProperty) as IOpenFileDialog; }
            set { SetValue(OpenFileDialogProperty, value); }
        }

        public IEnumerable<DatabaseTypes> AvaiableDatabaseTypes
        {
            get { return GetValue(AvaiableDatabaseTypesProperty) as IEnumerable<DatabaseTypes>; }
            set { SetValue(AvaiableDatabaseTypesProperty, value); }
        }

        public DatabaseTypes SelectedDatabaseType
        {
            get { return (DatabaseTypes)GetValue(SelectedDatabaseTypeProperty); }
            set { SetValue(SelectedDatabaseTypeProperty, value); }
        }

        private static void OnGeneralDatabaseConfigurationViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is GeneralDatabaseConfiguration generalDatabaseConfiguration) && (e.OldValue != e.NewValue))
            {
                if (e.NewValue is GeneralDatabaseConfigurationViewModel newGeneralDatabaseConfigurationViewModel)
                {
                    generalDatabaseConfiguration.NullSchemaInformation = newGeneralDatabaseConfigurationViewModel.NullSchemaInformation;
                    generalDatabaseConfiguration.MySqlConnectionConfigurationViewModel = newGeneralDatabaseConfigurationViewModel.MySqlConnectionConfigurationViewModel;
                    generalDatabaseConfiguration.SqliteConnectionConfigurationViewModel = newGeneralDatabaseConfigurationViewModel.SqliteConnectionConfigurationViewModel;
                    generalDatabaseConfiguration.SqlServerConnectionConfigurationViewModel = newGeneralDatabaseConfigurationViewModel.SqlServerConnectionConfigurationViewModel;
                    generalDatabaseConfiguration.SelectedDatabaseType = newGeneralDatabaseConfigurationViewModel.SelectedDatabaseType;
                    generalDatabaseConfiguration.MessageDialog = newGeneralDatabaseConfigurationViewModel.MessageDialog;
                    generalDatabaseConfiguration.OpenFileDialog = newGeneralDatabaseConfigurationViewModel.OpenFileDialog;
                }
            }
        }

        private static void OnNullSchemaInformationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is GeneralDatabaseConfiguration generalDatabaseConfiguration) && (e.OldValue != e.NewValue))
            {
                if (generalDatabaseConfiguration.GeneralDatabaseConfigurationViewModel != null)
                    generalDatabaseConfiguration.GeneralDatabaseConfigurationViewModel.NullSchemaInformation = generalDatabaseConfiguration.NullSchemaInformation;

                generalDatabaseConfiguration._propertyChangedDispatcher.Notify(nameof(NullSchemaInformation));
            }
        }

        private static void OnMySqlConnectionConfigurationViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is GeneralDatabaseConfiguration generalDatabaseConfiguration) && (e.OldValue != e.NewValue))
            {
                if (generalDatabaseConfiguration.GeneralDatabaseConfigurationViewModel != null)
                    generalDatabaseConfiguration.GeneralDatabaseConfigurationViewModel.MySqlConnectionConfigurationViewModel = generalDatabaseConfiguration.MySqlConnectionConfigurationViewModel;

                generalDatabaseConfiguration._propertyChangedDispatcher.Notify(nameof(MySqlConnectionConfigurationViewModel));
            }
        }

        private static void OnPostgreSqlConnectionConfigurationViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is GeneralDatabaseConfiguration generalDatabaseConfiguration) && (e.OldValue != e.NewValue))
            {
                if (generalDatabaseConfiguration.GeneralDatabaseConfigurationViewModel != null)
                    generalDatabaseConfiguration.GeneralDatabaseConfigurationViewModel.PostgreSqlConnectionConfigurationViewModel = generalDatabaseConfiguration.PostgreSqlConnectionConfigurationViewModel;

                generalDatabaseConfiguration._propertyChangedDispatcher.Notify(nameof(PostgreSqlConnectionConfigurationViewModel));
            }
        }

        private static void OnSqliteConnectionConfigurationViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is GeneralDatabaseConfiguration generalDatabaseConfiguration) && (e.OldValue != e.NewValue))
            {
                if (generalDatabaseConfiguration.GeneralDatabaseConfigurationViewModel != null)
                    generalDatabaseConfiguration.GeneralDatabaseConfigurationViewModel.SqliteConnectionConfigurationViewModel = generalDatabaseConfiguration.SqliteConnectionConfigurationViewModel;

                generalDatabaseConfiguration._propertyChangedDispatcher.Notify(nameof(SqliteConnectionConfigurationViewModel));
            }
        }

        private static void OnSqlServerConnectionConfigurationViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is GeneralDatabaseConfiguration generalDatabaseConfiguration) && (e.OldValue != e.NewValue))
            {
                if (generalDatabaseConfiguration.GeneralDatabaseConfigurationViewModel != null)
                    generalDatabaseConfiguration.GeneralDatabaseConfigurationViewModel.SqlServerConnectionConfigurationViewModel = generalDatabaseConfiguration.SqlServerConnectionConfigurationViewModel;

                generalDatabaseConfiguration._propertyChangedDispatcher.Notify(nameof(SqlServerConnectionConfigurationViewModel));
            }
        }

        private static void OnMessageDialogChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is GeneralDatabaseConfiguration generalDatabaseConfiguration) && (e.OldValue != e.NewValue))
            {
                if (generalDatabaseConfiguration.GeneralDatabaseConfigurationViewModel != null)
                    generalDatabaseConfiguration.GeneralDatabaseConfigurationViewModel.MessageDialog = generalDatabaseConfiguration.MessageDialog;

                generalDatabaseConfiguration._propertyChangedDispatcher.Notify(nameof(MessageDialog));
            }
        }

        private static void OnOpenFileDialogChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is GeneralDatabaseConfiguration generalDatabaseConfiguration) && (e.OldValue != e.NewValue))
            {
                if (generalDatabaseConfiguration.GeneralDatabaseConfigurationViewModel != null)
                    generalDatabaseConfiguration.GeneralDatabaseConfigurationViewModel.OpenFileDialog = generalDatabaseConfiguration.OpenFileDialog;

                generalDatabaseConfiguration._propertyChangedDispatcher.Notify(nameof(OpenFileDialog));
            }
        }

        private static void OnAvaiableDatabaseTypesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is GeneralDatabaseConfiguration generalDatabaseConfiguration) && (e.OldValue != e.NewValue))
            {
                if (generalDatabaseConfiguration.GeneralDatabaseConfigurationViewModel != null)
                    generalDatabaseConfiguration.GeneralDatabaseConfigurationViewModel.AvaiableDatabaseTypes = generalDatabaseConfiguration.AvaiableDatabaseTypes;

                generalDatabaseConfiguration._propertyChangedDispatcher.Notify(nameof(AvaiableDatabaseTypes));
            }
        }

        private static void OnSelectedDatabaseTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is GeneralDatabaseConfiguration generalDatabaseConfiguration) && (e.OldValue != e.NewValue))
            {
                if (generalDatabaseConfiguration.GeneralDatabaseConfigurationViewModel != null)
                    generalDatabaseConfiguration.GeneralDatabaseConfigurationViewModel.SelectedDatabaseType = generalDatabaseConfiguration.SelectedDatabaseType;

                generalDatabaseConfiguration._propertyChangedDispatcher.Notify(nameof(SelectedDatabaseType));
            }
        }

        public void Dispose()
        {
        }
    }
}
