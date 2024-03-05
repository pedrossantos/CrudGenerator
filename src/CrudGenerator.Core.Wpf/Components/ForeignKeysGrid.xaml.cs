using Database.DataAccess;
using Framework.NotifyChanges;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CrudGenerator.Core.Wpf.Components
{
    public partial class ForeignKeysGrid : UserControl
    {
        public static readonly DependencyProperty ForeignKeysProperty =
            DependencyProperty.Register(
                nameof(ForeignKeys),
                typeof(IEnumerable<ForeignKeyValueColletion>),
                typeof(ForeignKeysGrid),
                new FrameworkPropertyMetadata(null, OnForeignKeysChanged));

        private PropertyChangedDispatcher _propertyChangedDispatcher;

        public ForeignKeysGrid()
        {
            _propertyChangedDispatcher = new PropertyChangedDispatcher(this, true);

            InitializeComponent();
        }

        public string Title => nameof(SchemaInformationGenetator);

        public IEnumerable<ForeignKeyValueColletion> ForeignKeys
        {
            get { return GetValue(ForeignKeysProperty) as IEnumerable<ForeignKeyValueColletion>; }
            set { SetValue(ForeignKeysProperty, value); }
        }

        private static void OnForeignKeysChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ForeignKeysGrid foreignKeysGrid)
                foreignKeysGrid._propertyChangedDispatcher.Notify(nameof(ForeignKeys));
        }

        public void Dispose()
        {
        }
    }
}
