using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace HeroDrafter.Views
{
    public class SlotBackgroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return new SolidColorBrush(Color.FromRgb(0x16, 0x21, 0x3E));
            
            int selectedIndex = -1;
            int slotIndex = -1;
            
            // Parse selected index
            if (values[0] is int si) selectedIndex = si;
            else if (values[0] is string s1 && int.TryParse(s1, out int parsed1)) selectedIndex = parsed1;
            
            // Parse slot index (might come from XAML as string)
            if (values[1] is int i) slotIndex = i;
            else if (values[1] is string s && int.TryParse(s, out int parsed)) slotIndex = parsed;
            
            if (selectedIndex == slotIndex && selectedIndex >= 0)
            {
                return new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x2E));
            }
            
            return new SolidColorBrush(Color.FromRgb(0x16, 0x21, 0x3E));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
