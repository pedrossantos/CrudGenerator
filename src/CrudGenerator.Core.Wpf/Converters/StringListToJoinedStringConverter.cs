using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace CrudGenerator.Core.Wpf.Converters
{
    public class StringListToJoinedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IList<string> stringList)
            {
                if (parameter is string separator)
                    return string.Join(separator, stringList);

                return string.Join("; ", stringList);
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
