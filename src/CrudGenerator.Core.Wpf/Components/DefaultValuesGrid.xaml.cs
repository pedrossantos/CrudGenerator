using Database.DataMapping;
using Framework.NotifyChanges;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using View.Abstractions;

namespace CrudGenerator.Core.Wpf.Components
{
    public partial class DefaultValuesGrid : UserControl, IPresenterHandle, INotifyPropertyChanged
    {
        public static readonly DependencyProperty DefaultValuesProperty =
            DependencyProperty.Register(
                nameof(DefaultValues),
                typeof(IEnumerable<ColumnInfo>),
                typeof(DefaultValuesGrid),
                new FrameworkPropertyMetadata(null, OnDefaultValuesChanged));

        private PropertyChangedDispatcher _propertyChangedDispatcher;

        public DefaultValuesGrid()
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

        public IEnumerable<ColumnInfo> DefaultValues
        {
            get { return GetValue(DefaultValuesProperty) as IEnumerable<ColumnInfo>; }
            set { SetValue(DefaultValuesProperty, value); }
        }

        private static void OnDefaultValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DefaultValuesGrid defaultValuesGrid)
                defaultValuesGrid._propertyChangedDispatcher.Notify(nameof(DefaultValues));
        }

        public void Dispose()
        {
        }
    }
}
