using BIMformative.Core.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BIMformative.DynamoExtension.UI.Converters
{
    public class ViewStateToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            if (!Enum.TryParse(parameter.ToString(), out ViewState targetState))
                return Visibility.Collapsed;

            if (value is ViewState currentState)
                return currentState == targetState
                    ? Visibility.Visible 
                    : Visibility.Collapsed;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
