using CrudGenerator.Core.ViewModels;
using Database.DataMapping;
using Database.MySql.DataAccess;
using Database.Sqlite.DataAccess;
using Database.SqlServer.DataAccess;
using Framework.NotifyChanges;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using View.Abstractions;

namespace CrudGenerator.Components
{
    public partial class SchemaInformationGenetator : UserControl, IPresenterHandle, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SchemaInformationGenetatorViewModelProperty =
            DependencyProperty.Register(
                nameof(SchemaInformationGenetatorViewModel),
                typeof(SchemaInformationGenetatorViewModel),
                typeof(SchemaInformationGenetator),
                new FrameworkPropertyMetadata(null, OnSchemaInformationGenetatorViewModelChanged));

        public static readonly DependencyProperty MySqlSchemaInformationProperty =
            DependencyProperty.Register(
                nameof(MySqlSchemaInformation),
                typeof(MySqlSchemaInformation),
                typeof(SchemaInformationGenetator),
                new FrameworkPropertyMetadata(null, OnMySqlSchemaInformationChanged));

        public static readonly DependencyProperty SqliteSchemaInformationProperty =
            DependencyProperty.Register(
                nameof(SqliteSchemaInformation),
                typeof(SqliteSchemaInformation),
                typeof(SchemaInformationGenetator),
                new FrameworkPropertyMetadata(null, OnSqliteSchemaInformationChanged));

        public static readonly DependencyProperty SqlServerSchemaInformationProperty =
            DependencyProperty.Register(
                nameof(SqlServerSchemaInformation),
                typeof(SqlServerSchemaInformation),
                typeof(SchemaInformationGenetator),
                new FrameworkPropertyMetadata(null, OnSqlServerSchemaInformationChanged));

        public static readonly DependencyProperty SelectedDatabaseTypeProperty =
            DependencyProperty.Register(
                nameof(SelectedDatabaseType),
                typeof(DatabaseTypes),
                typeof(SchemaInformationGenetator),
                new FrameworkPropertyMetadata(DatabaseTypes.None, OnSelectedDatabaseTypeChanged));

        public static readonly RoutedEvent GenerateSchemaInformationInitializedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(GenerateSchemaInformationInitialized),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(SchemaInformationGenetator));

        public static readonly RoutedEvent GenerateSchemaInformationFinalizedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(GenerateSchemaInformationFinalized),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(SchemaInformationGenetator));

        private PropertyChangedDispatcher _propertyChangedDispatcher;

        public SchemaInformationGenetator()
        {
            _propertyChangedDispatcher = new PropertyChangedDispatcher(this, true);

            InitializeComponent();
        }

        public SchemaInformationGenetator(
            MySqlSchemaInformation mySqlSchemaInformation,
            SqliteSchemaInformation sqliteSchemaInformation,
            SqlServerSchemaInformation sqlServerSchemaInformation)
        {
            _propertyChangedDispatcher = new PropertyChangedDispatcher(this, true);

            SchemaInformationGenetatorViewModel = new SchemaInformationGenetatorViewModel(
                mySqlSchemaInformation,
                sqliteSchemaInformation,
                sqlServerSchemaInformation);

            InitializeComponent();
        }

        public SchemaInformationGenetator(SchemaInformationGenetatorViewModel schemaInformationGenetatorViewModel)
        {
            _propertyChangedDispatcher = new PropertyChangedDispatcher(this, true);

            SchemaInformationGenetatorViewModel = schemaInformationGenetatorViewModel;

            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => _propertyChangedDispatcher.AddHandler(value);
            remove => _propertyChangedDispatcher.RemoveHandler(value);
        }

        public event RoutedEventHandler GenerateSchemaInformationInitialized
        {
            add => AddHandler(GenerateSchemaInformationInitializedEvent, value);
            remove => RemoveHandler(GenerateSchemaInformationInitializedEvent, value);
        }

        public event RoutedEventHandler GenerateSchemaInformationFinalized
        {
            add => AddHandler(GenerateSchemaInformationFinalizedEvent, value);
            remove => RemoveHandler(GenerateSchemaInformationFinalizedEvent, value);
        }

        public SchemaInformationGenetatorViewModel SchemaInformationGenetatorViewModel
        {
            get { return GetValue(SchemaInformationGenetatorViewModelProperty) as SchemaInformationGenetatorViewModel; }
            set { SetValue(SchemaInformationGenetatorViewModelProperty, value); }
        }

        public MySqlSchemaInformation MySqlSchemaInformation
        {
            get { return GetValue(MySqlSchemaInformationProperty) as MySqlSchemaInformation; }
            set { SetValue(MySqlSchemaInformationProperty, value); }
        }

        public SqliteSchemaInformation SqliteSchemaInformation
        {
            get { return GetValue(SqliteSchemaInformationProperty) as SqliteSchemaInformation; }
            set { SetValue(SqliteSchemaInformationProperty, value); }
        }

        public SqlServerSchemaInformation SqlServerSchemaInformation
        {
            get { return GetValue(SqlServerSchemaInformationProperty) as SqlServerSchemaInformation; }
            set { SetValue(SqlServerSchemaInformationProperty, value); }
        }

        public DatabaseTypes SelectedDatabaseType
        {
            get { return (DatabaseTypes)GetValue(SelectedDatabaseTypeProperty); }
            set { SetValue(SelectedDatabaseTypeProperty, value); }
        }

        public string Title => nameof(SchemaInformationGenetator);

        private static void OnSchemaInformationGenetatorViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is SchemaInformationGenetator schemaInformationGenetator) && (e.OldValue != e.NewValue))
            {
                if (e.OldValue is SchemaInformationGenetatorViewModel oldSchemaInformationGenetatorViewModel)
                    oldSchemaInformationGenetatorViewModel.PropertyChanged -= schemaInformationGenetator.SchemaInformationGeneratorPropertyChanged;

                if (e.NewValue is SchemaInformationGenetatorViewModel newSchemaInformationGenetatorViewModel)
                {
                    newSchemaInformationGenetatorViewModel.PropertyChanged += schemaInformationGenetator.SchemaInformationGeneratorPropertyChanged;

                    schemaInformationGenetator.MySqlSchemaInformation = newSchemaInformationGenetatorViewModel.MySqlSchemaInformation;
                    schemaInformationGenetator.SqliteSchemaInformation = newSchemaInformationGenetatorViewModel.SqliteSchemaInformation;
                    schemaInformationGenetator.SqlServerSchemaInformation = newSchemaInformationGenetatorViewModel.SqlServerSchemaInformation;
                    schemaInformationGenetator.SelectedDatabaseType = newSchemaInformationGenetatorViewModel.SelectedDatabaseType;
                }
            }
        }

        private static void OnMySqlSchemaInformationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is SchemaInformationGenetator schemaInformationGenetator) && (e.OldValue != e.NewValue))
            {
                if (schemaInformationGenetator.SchemaInformationGenetatorViewModel != null)
                    schemaInformationGenetator.SchemaInformationGenetatorViewModel.MySqlSchemaInformation = schemaInformationGenetator.MySqlSchemaInformation;

                schemaInformationGenetator._propertyChangedDispatcher.Notify(nameof(MySqlSchemaInformation));
            }
        }

        private static void OnSqliteSchemaInformationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is SchemaInformationGenetator schemaInformationGenetator) && (e.OldValue != e.NewValue))
            {
                if (schemaInformationGenetator.SchemaInformationGenetatorViewModel != null)
                    schemaInformationGenetator.SchemaInformationGenetatorViewModel.SqliteSchemaInformation = schemaInformationGenetator.SqliteSchemaInformation;

                schemaInformationGenetator._propertyChangedDispatcher.Notify(nameof(SqliteSchemaInformation));
            }
        }

        private static void OnSqlServerSchemaInformationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is SchemaInformationGenetator schemaInformationGenetator) && (e.OldValue != e.NewValue))
            {
                if (schemaInformationGenetator.SchemaInformationGenetatorViewModel != null)
                    schemaInformationGenetator.SchemaInformationGenetatorViewModel.SqlServerSchemaInformation = schemaInformationGenetator.SqlServerSchemaInformation;

                schemaInformationGenetator._propertyChangedDispatcher.Notify(nameof(SqlServerSchemaInformation));
            }
        }

        private static void OnSelectedDatabaseTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is SchemaInformationGenetator schemaInformationGenetator) && (e.OldValue != e.NewValue))
            {
                if (schemaInformationGenetator.SchemaInformationGenetatorViewModel != null)
                    schemaInformationGenetator.SchemaInformationGenetatorViewModel.SelectedDatabaseType = schemaInformationGenetator.SelectedDatabaseType;

                schemaInformationGenetator._propertyChangedDispatcher.Notify(nameof(SelectedDatabaseType));
            }
        }

        private void SchemaInformationGeneratorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SchemaInformationGenetatorViewModel.GeneratingSchemaInformations))
            {
                Dispatcher.BeginInvoke(() =>
                {
                    if (SchemaInformationGenetatorViewModel.GeneratingSchemaInformations)
                        RaiseEvent(new RoutedEventArgs(GenerateSchemaInformationInitializedEvent));
                    else
                        RaiseEvent(new RoutedEventArgs(GenerateSchemaInformationFinalizedEvent));
                });
            }
        }

        public void Dispose()
        {
        }

        private void Button_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SchemaInformationGenetatorViewModel != null)
                SchemaInformationGenetatorViewModel.GenerateSchemaInformations();
        }
    }
}