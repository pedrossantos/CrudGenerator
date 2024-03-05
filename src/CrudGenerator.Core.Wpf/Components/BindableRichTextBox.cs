using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CrudGenerator.Core.Wpf.Components
{
    public class BindableRichTextBox : RichTextBox
    {
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register(
                nameof(Document),
                typeof(FlowDocument),
                typeof(BindableRichTextBox),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnDocumentChanged)));

        public new FlowDocument Document
        {
            get => GetValue(DocumentProperty) as FlowDocument;
            set => SetValue(DocumentProperty, value);
        }

        public static void OnDocumentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            RichTextBox rtb = (RichTextBox)obj;
            rtb.Document = (FlowDocument)args.NewValue;
        }
    }
}
