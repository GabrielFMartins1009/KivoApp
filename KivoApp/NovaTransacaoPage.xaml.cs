namespace KivoApp;

public partial class NovaTransacaoPage : ContentPage
{
    public NovaTransacaoPage()
    {
        InitializeComponent();
    }

    
    private async void OnSalvarTransacaoClicked(object sender, EventArgs e)
    {
        // Aqui voc� pode adicionar l�gica futura para salvar a transa��o
        await DisplayAlert("Sucesso", "Transa��o salva com sucesso!", "OK");

        // Volta para a p�gina anterior
        await Navigation.PopAsync();
    }
}
