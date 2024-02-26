using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CrudGenerator.Converters
{
    public class IEnumerableToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value is IEnumerable enumerableValue) && enumerableValue.Cast<object>().Any())
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
