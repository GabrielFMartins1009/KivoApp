namespace KivoApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnNovaTransacaoClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new NovaTransacaoPage());
    }
}
