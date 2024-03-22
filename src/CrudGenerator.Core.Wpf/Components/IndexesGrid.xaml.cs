using Database.DataMapping;
using Framework.NotifyChanges;
using System.Windows;
using System.Windows.Controls;

namespace CrudGenerator.Core.Wpf.Components
{
    public partial class IndexesGrid : UserControl
    {
        public static readonly DependencyProperty IndexesProperty =
            DependencyProperty.Register(
                nameof(Indexes),
                typeof(IEnumerable<IndexInfo>),
                typeof(IndexesGrid),
                new FrameworkPropertyMetadata(null, OnIndexesChanged));

        private PropertyChangedDispatcher _propertyChangedDispatcher;

        public IndexesGrid()
        {
            _propertyChangedDispatcher = new PropertyChangedDispatcher(this, true);

            InitializeComponent();
        }

        public string Title => nameof(SchemaInformationGenetator);

        public IEnumerable<IndexInfo> Indexes
        {
            get { return GetValue(IndexesProperty) as IEnumerable<IndexInfo>; }
            set { SetValue(IndexesProperty, value); }
        }

        private static void OnIndexesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is IndexesGrid foreignKeysGrid)
                foreignKeysGrid._propertyChangedDispatcher.Notify(nameof(Indexes));
        }

        public void Dispose()
        {
        }
    }
}
