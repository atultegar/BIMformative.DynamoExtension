using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BIMformative.DynamoExtension.UI.Converters
{
    public class EqualityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value?.ToString() == parameter?.ToString();


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value ? parameter! : Binding.DoNothing;
    }
}
