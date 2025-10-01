using System;
using System.Globalization;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;

namespace KivoApp.Converters
{
    public class TipoToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var tipo = value?.ToString();

            // Tenta pegar das resources (se existir); senão, usa cores padrão
            var res = Application.Current?.Resources;
            if (tipo == "Entrada")
            {
                if (res != null && res.ContainsKey("PrimaryColor")) return (Color)res["PrimaryColor"];
                return Colors.Green;
            }

            if (tipo == "Saída" || tipo == "Saida" || tipo == "Despesa")
            {
                if (res != null && res.ContainsKey("AccentColor")) return (Color)res["AccentColor"];
                return Colors.Red;
            }

            if (res != null && res.ContainsKey("TextPrimaryColor")) return (Color)res["TextPrimaryColor"];
            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
