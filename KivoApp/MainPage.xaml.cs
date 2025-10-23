using KivoApp.Services;
using KivoApp.Models;
using System.ComponentModel;
using System.Linq;

namespace KivoApp;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    private decimal saldo;
    private decimal totalEntradas;
    private decimal totalSaidas;

    public decimal Saldo
    {
        get => saldo;
        set { saldo = value; OnPropertyChanged(); }
    }

    public decimal TotalEntradas
    {
        get => totalEntradas;
        set { totalEntradas = value; OnPropertyChanged(); }
    }

    public decimal TotalSaidas
    {
        get => totalSaidas;
        set { totalSaidas = value; OnPropertyChanged(); }
    }

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;

        // Escuta mudanças no histórico
        TransacaoService.Transacoes.CollectionChanged += (s, e) => AtualizarTotais();
        
        // Escuta atualizações globais
        MessagingCenter.Subscribe<object>(this, "AtualizarTudo", async (sender) =>
        {
            await TransacaoService.RecarregarDadosAsync();
            AtualizarTotais();
        });
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await TransacaoService.RecarregarDadosAsync();
        AtualizarTotais();
    }

    private void AtualizarTotais()
    {
        TotalEntradas = TransacaoService.Transacoes
            .Where(t => t.Tipo == "Entrada")
            .Sum(t => t.Valor);

        TotalSaidas = TransacaoService.Transacoes
            .Where(t => t.Tipo == "Saída")
            .Sum(t => t.Valor);

        Saldo = TotalEntradas - TotalSaidas;
    }

    private async void OnNovaTransacaoClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new NovaTransacaoPage());
    
    }

    // Não esqueça de fazer unsubscribe quando a página for destruída
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        MessagingCenter.Unsubscribe<object>(this, "AtualizarTudo");
    }
}
