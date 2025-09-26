namespace KivoApp;

public partial class NovaTransacaoPage : ContentPage
{
    public NovaTransacaoPage()
    {
        InitializeComponent();
    }

    
    private async void OnSalvarTransacaoClicked(object sender, EventArgs e)
    {
        // Aqui você pode adicionar lógica futura para salvar a transação
        await DisplayAlert("Sucesso", "Transação salva com sucesso!", "OK");

        // Volta para a página anterior
        await Navigation.PopAsync();
    }
}
