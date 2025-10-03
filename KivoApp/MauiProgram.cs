using SkiaSharp.Views.Maui.Controls.Hosting;
using Microcharts.Maui; // se a API UseMicrocharts() existir na versão instalada
using KivoApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp();           // << obrigatório para Skia/Microcharts

        // se a sua versão de Microcharts oferecer UseMicrocharts extension:
        // builder.UseMicrocharts();

        // restante: fonts, serviços, etc...
        return builder.Build();
    }
}
