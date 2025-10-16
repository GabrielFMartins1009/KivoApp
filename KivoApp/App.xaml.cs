using KivoApp.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using System;
using System.IO;
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

            // Inicializa o banco de dados SQLite (configura o caminho do arquivo)
            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "KivoApp.db3"
            );

            // Inicializa o banco de dados estático
            DatabaseService.Initialize(dbPath);

            // Inicializa dados sem travar a UI
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
            try
            {
                // Cria tabelas se não existirem
                await DatabaseService.InitializeAsync();

                // Carrega transações e metas do banco
                await TransacaoService.LoadFromDatabaseAsync();
                await MetaService.LoadFromDatabaseAsync();

                // Atualiza metas com saldo atual
                var saldo = TransacaoService.CalcularSaldo();
                MetaService.AtualizarMetas(saldo);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao inicializar dados: {ex.Message}");
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Retorna a janela principal com AppShell
            return new Window(new AppShell());
        }
    }
}
