using CrudGenerator.Core.ViewModels;
using Database.DataMapping;
using Framework.NotifyChanges;
using Framework.Validation;
using System.ComponentModel;
using System.Windows.Controls;
using View.Abstractions;

namespace CrudGenerator
{
    public partial class MainWindow : UserControl, IPresenterHandle, INotifyPropertyChanged, IDisposable
    {
        private readonly GeneralDatabaseConfigurationViewModel _generalDatabaseConfigurationViewModel;
        private readonly SchemaInformationGenetatorViewModel _schemaInformationGenetatorViewModel;
        private readonly ProjectGeneratorViewModel _projectGeneratorViewModel;

        private bool _isLoading;

        private PropertyChangedDispatcher _propertyChangedDispatcher;

        public MainWindow(
            GeneralDatabaseConfigurationViewModel generalDatabaseConfigurationViewModel,
            SchemaInformationGenetatorViewModel schemaInformationGenetatorView,
            ProjectGeneratorViewModel projectGeneratorViewModel)
        {
            Requires.NotNull(generalDatabaseConfigurationViewModel, nameof(generalDatabaseConfigurationViewModel));
            Requires.NotNull(schemaInformationGenetatorView, nameof(schemaInformationGenetatorView));
            Requires.NotNull(projectGeneratorViewModel, nameof(projectGeneratorViewModel));

            _propertyChangedDispatcher = new PropertyChangedDispatcher(this, true);

            _generalDatabaseConfigurationViewModel = generalDatabaseConfigurationViewModel;
            _schemaInformationGenetatorViewModel = schemaInformationGenetatorView;
            _projectGeneratorViewModel = projectGeneratorViewModel;

            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => _propertyChangedDispatcher.AddHandler(value);
            remove => _propertyChangedDispatcher.RemoveHandler(value);
        }

        public string Title => nameof(MainWindow);

        public GeneralDatabaseConfigurationViewModel GeneralDatabaseConfigurationViewModel => _generalDatabaseConfigurationViewModel;

        public SchemaInformationGenetatorViewModel SchemaInformationGenetatorViewModel => _schemaInformationGenetatorViewModel;

        public ProjectGeneratorViewModel ProjectGeneratorViewModel => _projectGeneratorViewModel;

        public bool IsLoading
        {
            get => _isLoading;
            set => _propertyChangedDispatcher.SetProperty(ref _isLoading, value);
        }

        public void Dispose()
        {
            if (_generalDatabaseConfigurationViewModel != null)
                _generalDatabaseConfigurationViewModel?.Dispose();
        }

        private void SchemaInformationGenetatorGenerateSchemaInformationInitialized(object sender, System.Windows.RoutedEventArgs e)
        {
            IsLoading = true;
        }

        private void SchemaInformationGenetatorGenerateSchemaInformationFinalized(object sender, System.Windows.RoutedEventArgs e)
        {
            IsLoading = false;
        }

        private void ProjectGeneratorControlGenerateProjectInitialized(object sender, System.Windows.RoutedEventArgs e)
        {
            IsLoading = true;
        }

        private void ProjectGeneratorControlGenerateProjectFinalized(object sender, System.Windows.RoutedEventArgs e)
        {
            IsLoading = false;
        }

        private void ProjectGeneratorControl_GenerateClassesInitialized(object sender, System.Windows.RoutedEventArgs e)
        {
            IsLoading = true;
        }

        private void ProjectGeneratorControl_GenerateClassesFinalized(object sender, System.Windows.RoutedEventArgs e)
        {
            IsLoading = false;
        }
    }
}