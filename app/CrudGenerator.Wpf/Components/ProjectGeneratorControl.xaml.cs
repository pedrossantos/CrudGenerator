using CrudGenerator.Core;
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
    public partial class ProjectGeneratorControl : UserControl, IPresenterHandle, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ProjectGeneratorViewModelProperty =
            DependencyProperty.Register(
                nameof(ProjectGeneratorViewModel),
                typeof(ProjectGeneratorViewModel),
                typeof(ProjectGeneratorControl),
                new FrameworkPropertyMetadata(null, OnProjectGeneratorViewModelChanged));

        public static readonly DependencyProperty MySqlSchemaInformationProperty =
            DependencyProperty.Register(
                nameof(MySqlSchemaInformation),
                typeof(MySqlSchemaInformation),
                typeof(ProjectGeneratorControl),
                new FrameworkPropertyMetadata(null, OnMySqlSchemaInformationChanged));

        public static readonly DependencyProperty SqliteSchemaInformationProperty =
            DependencyProperty.Register(
                nameof(SqliteSchemaInformation),
                typeof(SqliteSchemaInformation),
                typeof(ProjectGeneratorControl),
                new FrameworkPropertyMetadata(null, OnSqliteSchemaInformationChanged));

        public static readonly DependencyProperty SqlServerSchemaInformationProperty =
            DependencyProperty.Register(
                nameof(SqlServerSchemaInformation),
                typeof(SqlServerSchemaInformation),
                typeof(ProjectGeneratorControl),
                new FrameworkPropertyMetadata(null, OnSqlServerSchemaInformationChanged));

        public static readonly DependencyProperty SelectedDatabaseTypeProperty =
            DependencyProperty.Register(
                nameof(SelectedDatabaseType),
                typeof(DatabaseTypes),
                typeof(ProjectGeneratorControl),
                new FrameworkPropertyMetadata(DatabaseTypes.None, OnSelectedDatabaseTypeChanged));

        public static readonly DependencyProperty MessageDialogProperty =
            DependencyProperty.Register(
                nameof(MessageDialog),
                typeof(IMessageDialog),
                typeof(ProjectGeneratorControl),
                new FrameworkPropertyMetadata(null, OnMessageDialogChanged));

        public static readonly DependencyProperty SelectFolderDialogProperty =
            DependencyProperty.Register(
                nameof(SelectFolderDialog),
                typeof(ISelectFolderDialog),
                typeof(ProjectGeneratorControl),
                new FrameworkPropertyMetadata(null, OnSelectFolderDialogChanged));

        public static readonly DependencyProperty DefaultNamespaceProperty =
            DependencyProperty.Register(
                nameof(DefaultNamespace),
                typeof(string),
                typeof(ProjectGeneratorControl),
                new FrameworkPropertyMetadata(string.Empty, OnDefaultNamespaceChanged));

        public static readonly DependencyProperty ProjectFolderProperty =
            DependencyProperty.Register(
                nameof(ProjectFolder),
                typeof(string),
                typeof(ProjectGeneratorControl),
                new FrameworkPropertyMetadata(string.Empty, OnProjectFolderChanged));

        public static readonly DependencyProperty ProjectNameProperty =
            DependencyProperty.Register(
                nameof(ProjectName),
                typeof(string),
                typeof(ProjectGeneratorControl),
                new FrameworkPropertyMetadata(string.Empty, OnProjectNameChanged));

        public static readonly RoutedEvent GenerateClassesInitializedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(GenerateClassesInitialized),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(ProjectGeneratorControl));

        public static readonly RoutedEvent GenerateClassesFinalizedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(GenerateClassesFinalized),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(ProjectGeneratorControl));

        public static readonly RoutedEvent GenerateProjectInitializedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(GenerateProjectInitialized),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(ProjectGeneratorControl));

        public static readonly RoutedEvent GenerateProjectFinalizedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(GenerateProjectFinalized),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(ProjectGeneratorControl));

        private PropertyChangedDispatcher _propertyChangedDispatcher;

        public ProjectGeneratorControl()
        {
            _propertyChangedDispatcher = new PropertyChangedDispatcher(this, true);

            DefaultNamespace = string.Empty;

            InitializeComponent();
        }

        public ProjectGeneratorControl(
            MySqlSchemaInformation mySqlSchemaInformation,
            SqliteSchemaInformation sqliteSchemaInformation,
            SqlServerSchemaInformation sqlServerSchemaInformation,
            IMessageDialog messageDialog,
            ISelectFolderDialog selectFolderDialog)
        {
            _propertyChangedDispatcher = new PropertyChangedDispatcher(this, true);

            DefaultNamespace = string.Empty;

            ProjectGeneratorViewModel = new ProjectGeneratorViewModel(
                mySqlSchemaInformation,
                sqliteSchemaInformation,
                sqlServerSchemaInformation,
                messageDialog,
                selectFolderDialog);

            InitializeComponent();
        }

        public ProjectGeneratorControl(ProjectGeneratorViewModel projectGeneratorViewModel)
        {
            _propertyChangedDispatcher = new PropertyChangedDispatcher(this, true);

            DefaultNamespace = string.Empty;

            ProjectGeneratorViewModel = projectGeneratorViewModel;

            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => _propertyChangedDispatcher.AddHandler(value);
            remove => _propertyChangedDispatcher.RemoveHandler(value);
        }

        public event RoutedEventHandler GenerateClassesInitialized
        {
            add => AddHandler(GenerateClassesInitializedEvent, value);
            remove => RemoveHandler(GenerateClassesInitializedEvent, value);
        }

        public event RoutedEventHandler GenerateClassesFinalized
        {
            add => AddHandler(GenerateClassesFinalizedEvent, value);
            remove => RemoveHandler(GenerateClassesFinalizedEvent, value);
        }

        public event RoutedEventHandler GenerateProjectInitialized
        {
            add => AddHandler(GenerateProjectInitializedEvent, value);
            remove => RemoveHandler(GenerateProjectInitializedEvent, value);
        }

        public event RoutedEventHandler GenerateProjectFinalized
        {
            add => AddHandler(GenerateProjectFinalizedEvent, value);
            remove => RemoveHandler(GenerateProjectFinalizedEvent, value);
        }

        public ProjectGeneratorViewModel ProjectGeneratorViewModel
        {
            get { return GetValue(ProjectGeneratorViewModelProperty) as ProjectGeneratorViewModel; }
            set { SetValue(ProjectGeneratorViewModelProperty, value); }
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

        public IMessageDialog MessageDialog
        {
            get { return GetValue(MessageDialogProperty) as IMessageDialog; }
            set { SetValue(MessageDialogProperty, value); }
        }

        public ISelectFolderDialog SelectFolderDialog
        {
            get { return GetValue(SelectFolderDialogProperty) as ISelectFolderDialog; }
            set { SetValue(SelectFolderDialogProperty, value); }
        }

        public string DefaultNamespace
        {
            get { return GetValue(DefaultNamespaceProperty)?.ToString(); }
            set { SetValue(DefaultNamespaceProperty, value); }
        }

        public string ProjectFolder
        {
            get { return GetValue(ProjectFolderProperty)?.ToString(); }
            set { SetValue(ProjectFolderProperty, value); }
        }

        public string ProjectName
        {
            get { return GetValue(ProjectNameProperty)?.ToString(); }
            set { SetValue(ProjectNameProperty, value); }
        }

        public string Title => nameof(ProjectGeneratorControl);

        private static void OnProjectGeneratorViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is ProjectGeneratorControl projectGeneratorControl) && (e.OldValue != e.NewValue))
            {
                if (e.OldValue is ProjectGeneratorViewModel oldProjectGeneratorViewModel)
                    oldProjectGeneratorViewModel.PropertyChanged -= projectGeneratorControl.ProjectGeneratorViewModelPropertyChanged;

                if (e.NewValue is ProjectGeneratorViewModel newProjectGeneratorViewModel)
                {
                    newProjectGeneratorViewModel.PropertyChanged += projectGeneratorControl.ProjectGeneratorViewModelPropertyChanged;

                    projectGeneratorControl.MySqlSchemaInformation = newProjectGeneratorViewModel.MySqlSchemaInformation;
                    projectGeneratorControl.SqliteSchemaInformation = newProjectGeneratorViewModel.SqliteSchemaInformation;
                    projectGeneratorControl.SqlServerSchemaInformation = newProjectGeneratorViewModel.SqlServerSchemaInformation;
                    projectGeneratorControl.SelectedDatabaseType = newProjectGeneratorViewModel.SelectedDatabaseType;
                    projectGeneratorControl.MessageDialog = newProjectGeneratorViewModel.MessageDialog;
                    projectGeneratorControl.SelectFolderDialog = newProjectGeneratorViewModel.SelectFolderDialog;
                    projectGeneratorControl.DefaultNamespace = newProjectGeneratorViewModel.DefaultNamespace;
                    projectGeneratorControl.ProjectFolder = newProjectGeneratorViewModel.ProjectFolder;
                    projectGeneratorControl.ProjectName = newProjectGeneratorViewModel.ProjectName;
                }
            }
        }

        private static void OnMySqlSchemaInformationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is ProjectGeneratorControl projectGeneratorControl) && (e.OldValue != e.NewValue))
            {
                if (projectGeneratorControl.ProjectGeneratorViewModel != null)
                    projectGeneratorControl.ProjectGeneratorViewModel.MySqlSchemaInformation = projectGeneratorControl.MySqlSchemaInformation;

                projectGeneratorControl._propertyChangedDispatcher.Notify(nameof(MySqlSchemaInformation));
            }
        }

        private static void OnSqliteSchemaInformationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is ProjectGeneratorControl projectGeneratorControl) && (e.OldValue != e.NewValue))
            {
                if (projectGeneratorControl.ProjectGeneratorViewModel != null)
                    projectGeneratorControl.ProjectGeneratorViewModel.SqliteSchemaInformation = projectGeneratorControl.SqliteSchemaInformation;

                projectGeneratorControl._propertyChangedDispatcher.Notify(nameof(SqliteSchemaInformation));
            }
        }

        private static void OnSqlServerSchemaInformationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is ProjectGeneratorControl projectGeneratorControl) && (e.OldValue != e.NewValue))
            {
                if (projectGeneratorControl.ProjectGeneratorViewModel != null)
                    projectGeneratorControl.ProjectGeneratorViewModel.SqlServerSchemaInformation = projectGeneratorControl.SqlServerSchemaInformation;

                projectGeneratorControl._propertyChangedDispatcher.Notify(nameof(SqlServerSchemaInformation));
            }
        }

        private static void OnSelectedDatabaseTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is ProjectGeneratorControl projectGeneratorControl) && (e.OldValue != e.NewValue))
            {
                if (projectGeneratorControl.ProjectGeneratorViewModel != null)
                    projectGeneratorControl.ProjectGeneratorViewModel.SelectedDatabaseType = projectGeneratorControl.SelectedDatabaseType;

                projectGeneratorControl._propertyChangedDispatcher.Notify(nameof(SelectedDatabaseType));
            }
        }

        private static void OnMessageDialogChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is ProjectGeneratorControl projectGeneratorControl) && (e.OldValue != e.NewValue))
            {
                if (projectGeneratorControl.ProjectGeneratorViewModel != null)
                    projectGeneratorControl.ProjectGeneratorViewModel.MessageDialog = projectGeneratorControl.MessageDialog;

                projectGeneratorControl._propertyChangedDispatcher.Notify(nameof(MessageDialog));
            }
        }

        private static void OnSelectFolderDialogChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is ProjectGeneratorControl projectGeneratorControl) && (e.OldValue != e.NewValue))
            {
                if (projectGeneratorControl.ProjectGeneratorViewModel != null)
                    projectGeneratorControl.ProjectGeneratorViewModel.SelectFolderDialog = projectGeneratorControl.SelectFolderDialog;

                projectGeneratorControl._propertyChangedDispatcher.Notify(nameof(SelectFolderDialog));
            }
        }

        private static void OnDefaultNamespaceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is ProjectGeneratorControl projectGeneratorControl) && (e.OldValue != e.NewValue))
            {
                if (projectGeneratorControl.ProjectGeneratorViewModel != null)
                    projectGeneratorControl.ProjectGeneratorViewModel.DefaultNamespace = projectGeneratorControl.DefaultNamespace;

                projectGeneratorControl._propertyChangedDispatcher.Notify(nameof(DefaultNamespace));
            }
        }

        private static void OnProjectFolderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is ProjectGeneratorControl projectGeneratorControl) && (e.OldValue != e.NewValue))
            {
                if (projectGeneratorControl.ProjectGeneratorViewModel != null)
                    projectGeneratorControl.ProjectGeneratorViewModel.ProjectFolder = projectGeneratorControl.ProjectFolder;

                projectGeneratorControl._propertyChangedDispatcher.Notify(nameof(ProjectFolder));
            }
        }

        private static void OnProjectNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is ProjectGeneratorControl projectGeneratorControl) && (e.OldValue != e.NewValue))
            {
                if (projectGeneratorControl.ProjectGeneratorViewModel != null)
                    projectGeneratorControl.ProjectGeneratorViewModel.ProjectName = projectGeneratorControl.ProjectName;

                projectGeneratorControl._propertyChangedDispatcher.Notify(nameof(ProjectName));
            }
        }

        public void Dispose()
        {
        }

        private async void GenerateClassesPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            await ProjectGeneratorViewModel.GenerateClasses();
        }

        private void SelectProjectFolderPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ProjectGeneratorViewModel.SelectProjectFolder();
        }

        private async void GenerateProjectPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            await ProjectGeneratorViewModel.GenerateProject();
        }

        private void TreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is GeneratedClass generatedClass)
                ProjectGeneratorViewModel.SelectedGeneratedClass = generatedClass;
        }

        private void ProjectGeneratorViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProjectGeneratorViewModel.GeneratingProject))
            {
                Dispatcher.BeginInvoke(() =>
                {
                    if (ProjectGeneratorViewModel.GeneratingProject)
                        RaiseEvent(new RoutedEventArgs(GenerateClassesInitializedEvent));
                    else
                        RaiseEvent(new RoutedEventArgs(GenerateClassesFinalizedEvent));
                });
            }
        }
    }
}
