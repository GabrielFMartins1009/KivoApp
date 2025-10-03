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

        // escuta mudanças no histórico
        TransacaoService.Transacoes.CollectionChanged += (s, e) => AtualizarTotais();
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
}
