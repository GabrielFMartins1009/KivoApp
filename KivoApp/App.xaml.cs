namespace KivoApp
{
    public partial class App : Application
    {
        const int WindowWidth = 400;
        const int WindowHeight = 750;

        public App()
        {
            InitializeComponent();
            
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

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Cria a NavigationPage com a MainPage como root
            //  var navPage = new NavigationPage(new MainPage());

            // Retorna a Window com a NavigationPage como conteúdo
            return new Window(new AppShell());
        }
    }
}
