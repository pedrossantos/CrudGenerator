using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace CrudGenerator.Core.Wpf.Components
{
    public class StringToFlowDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FlowDocument doc = new FlowDocument();
            doc.FontSize = 16d;

            string s = value as string;
            if (s != null)
            {
                using (StringReader reader = new StringReader(s))
                {
                    string newLine;
                    while ((newLine = reader.ReadLine()) != null)
                    {
                        Paragraph paragraph;
                        if (newLine.EndsWith(":."))
                        {
                            paragraph = new Paragraph(new Run(newLine.Replace(":.", string.Empty)));
                            paragraph.Foreground = new SolidColorBrush(Colors.Blue);
                            paragraph.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            paragraph = new Paragraph(new Run(newLine));
                        }

                        paragraph.Margin = new Thickness(0);
                        doc.Blocks.Add(paragraph);
                    }
                }
            }

            return doc;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FlowDocument flowDocument)
                return flowDocument.ToString();

            return string.Empty;
        }
    }
}
