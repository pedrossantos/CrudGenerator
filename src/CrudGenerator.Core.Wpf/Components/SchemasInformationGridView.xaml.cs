using Database.DataMapping;
using Framework.NotifyChanges;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using View.Abstractions;

namespace CrudGenerator.Core.Wpf.Components
{
    public partial class SchemasInformationGridView : UserControl, IPresenterHandle, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SchemaTableMappingsObservableCollectionProperty =
            DependencyProperty.Register(
                nameof(SchemaTableMappingsObservableCollection),
                typeof(ObservableCollection<SchemaInformationTableMapping>),
                typeof(SchemasInformationGridView),
                new FrameworkPropertyMetadata(null, OnSchemaTableMappingsObservableCollectionChanged));

        private PropertyChangedDispatcher _propertyChangedDispatcher;

        public SchemasInformationGridView()
        {
            _propertyChangedDispatcher = new PropertyChangedDispatcher(this, true);

            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => _propertyChangedDispatcher.AddHandler(value);
            remove => _propertyChangedDispatcher.RemoveHandler(value);
        }

        public string Title => nameof(SchemaInformationGenetator);

        public ObservableCollection<SchemaInformationTableMapping> SchemaTableMappingsObservableCollection
        {
            get { return GetValue(SchemaTableMappingsObservableCollectionProperty) as ObservableCollection<SchemaInformationTableMapping>; }
            set { SetValue(SchemaTableMappingsObservableCollectionProperty, value); }
        }

        private static void OnSchemaTableMappingsObservableCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SchemasInformationGridView schemasInformationGridView)
            {
                if (e.OldValue is ObservableCollection<SchemaInformationTableMapping> oldSchemaTableMappingsObservableCollection)
                    oldSchemaTableMappingsObservableCollection.CollectionChanged -= schemasInformationGridView.SchemaTableMappingsObservableCollectionCollectionChanged;

                schemasInformationGridView._propertyChangedDispatcher.Notify(nameof(SchemaTableMappingsObservableCollection));

                if (e.NewValue is ObservableCollection<SchemaInformationTableMapping> newSchemaTableMappingsObservableCollection)
                    newSchemaTableMappingsObservableCollection.CollectionChanged += schemasInformationGridView.SchemaTableMappingsObservableCollectionCollectionChanged;
            }
        }

        public void Dispose()
        {
        }

        private void SchemaTableMappingsObservableCollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _propertyChangedDispatcher.Notify(nameof(SchemaTableMappingsObservableCollection));
        }
    }
}
