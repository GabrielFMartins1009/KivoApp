using KivoApp.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using System;
using System.Threading.Tasks;

namespace KivoApp
{
    public partial class App : Application
    {
        const int WindowWidth = 400;
        const int WindowHeight = 750;

        public App()
        {
            InitializeComponent();

            // Inicializa dados em background (não bloquear UI)
            _ = InitializeDataAsync();

#if WINDOWS
            Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
            {
                var mauiWindow = handler.VirtualView;
                var nativeWindow = handler.PlatformView;
                nativeWindow.Activate();

                IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                appWindow.Resize(new Windows.Graphics.SizeInt32(WindowWidth, WindowHeight));
            });
#endif
        }

        private async Task InitializeDataAsync()
        {
            // Carrega transações e metas do armazenamento
            await TransacaoService.LoadFromStorageAsync();
            await MetaService.LoadFromStorageAsync();

            // Atualiza metas com saldo atual (após carregar transações)
            var saldo = TransacaoService.CalcularSaldo();
            MetaService.AtualizarMetas(saldo);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {

            // Retorna a janela com o AppShell como conteúdo
            return new Window(new AppShell());
        }
    }
}   
