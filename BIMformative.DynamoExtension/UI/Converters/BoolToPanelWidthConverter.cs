using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BIMformative.DynamoExtension.UI.Converters
{
    public class BoolToPanelWidthConverter : IValueConverter
    {
        public double OpenWidth { get; set; } = 450;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isOpen && isOpen)
                return OpenWidth;

            return 0d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
