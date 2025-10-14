using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace KivoApp.Converters
{
    public class PorcentagemToProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal porcentagem)
                return (double)Math.Clamp(porcentagem / 100m, 0, 1);
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
