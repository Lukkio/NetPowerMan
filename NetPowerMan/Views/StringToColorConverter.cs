using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace NetPowerMan.Views
{
    internal class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = new BrushConverter();

            if (value == null) 
            {
                if (targetType.Name.Contains("Color"))
                {
                    return Color.FromArgb(0xff, 0xff, 0xff, 0xff);
                }
                else return new BrushConverter().ConvertFromString("White") as SolidColorBrush; 
            }

            if (color.IsValid(value.ToString())) 
            {
                var col = new BrushConverter().ConvertFromString(value.ToString()) as SolidColorBrush;
                if (targetType.Name.Contains("Color"))
                {
                    return Color.FromArgb(col.Color.A, col.Color.R, col.Color.G, col.Color.B);
                }
                else return new BrushConverter().ConvertFromString(value.ToString()) as SolidColorBrush;
            }
            else 
                if(targetType.Name.Contains("Color")) 
                   return Color.FromArgb(0xff, 0xff, 0xff, 0xff); 
                else return new BrushConverter().ConvertFromString("White") as SolidColorBrush;     
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
