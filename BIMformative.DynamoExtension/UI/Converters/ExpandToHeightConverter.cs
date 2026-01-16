using System;
using System.Globalization;
using System.Windows.Data;

namespace BIMformative.DynamoExtension.UI.Converters
{
    public class ExpandToHeightConverter : IValueConverter
    {
        // Example: If expanded, return a large value; otherwise, return a default height.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isExpanded = value is bool b && b;
            return isExpanded ? double.PositiveInfinity : 60.0; // 60.0 is a sample collapsed height
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
