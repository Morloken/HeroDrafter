using System;
using System.Globalization;
using System.Windows.Data;

namespace HeroDrafter.Views
{
    public class IsSlotSelectedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is int slotIndex && values[1] is int selectedIndex)
            {
                return slotIndex == selectedIndex;
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
