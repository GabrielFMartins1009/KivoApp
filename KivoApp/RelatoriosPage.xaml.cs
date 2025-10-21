using Microsoft.Maui.Controls;
using KivoApp.Models;
using KivoApp.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Microcharts;
using SkiaSharp; 

namespace KivoApp
{
    public partial class RelatoriosPage : ContentPage
    {
        public ObservableCollection<Transacao> Transacoes { get; set; }
        private ObservableCollection<Transacao> TodasTransacoes { get; set; }
        public Command<Transacao> ExcluirTransacaoCommand { get; }

        public RelatoriosPage()
        {
            InitializeComponent();
            Transacoes = new ObservableCollection<Transacao>();
            TodasTransacoes = new ObservableCollection<Transacao>();
            ExcluirTransacaoCommand = new Command<Transacao>(async (transacao) => await ExcluirTransacao(transacao));
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CarregarTransacoes();
            AtualizarTotaisEGrafico();
        }

        private async Task CarregarTransacoes()
        {
            var lista = await DatabaseService.GetTransacoesAsync();
            TodasTransacoes.Clear();
            Transacoes.Clear();

            foreach (var t in lista)
            {
                TodasTransacoes.Add(t);
                Transacoes.Add(t);
            }

            HistoricoCollectionView.ItemsSource = Transacoes;
        }

        private async Task ExcluirTransacao(Transacao transacao)
        {
            bool confirmar = await DisplayAlert("Confirmação", "Deseja excluir esta transação?", "Sim", "Não");
            if (confirmar)
            {
                await DatabaseService.DeleteTransacaoAsync(transacao);
                Transacoes.Remove(transacao);
                TodasTransacoes.Remove(transacao);
                AtualizarTotaisEGrafico();
            }
        }

        private async void ExcluirTransacao_Clicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is Transacao transacao)
            {
                await ExcluirTransacao(transacao);
            }
        }


        private void OnTipoPickerChanged(object sender, EventArgs e)
        {
            FiltrarTransacoes();
        }

        private void OnDataFiltroChanged(object sender, DateChangedEventArgs e)
        {
            FiltrarTransacoes();
        }

        private void FiltrarTransacoes()
        {
            if (TodasTransacoes == null || TodasTransacoes.Count == 0)
                return;

            var tipoSelecionado = TipoPicker.SelectedItem?.ToString() ?? "Todas";
            var dataInicio = DataInicioPicker.Date;
            var dataFim = DataFimPicker.Date;

            var filtradas = TodasTransacoes.Where(t =>
                (tipoSelecionado == "Todas" || t.Tipo == tipoSelecionado) &&
                t.Data >= dataInicio &&
                t.Data <= dataFim).ToList();

            Transacoes.Clear();
            foreach (var t in filtradas)
                Transacoes.Add(t);

            HistoricoCollectionView.ItemsSource = Transacoes;
            AtualizarTotaisEGrafico();
        }

        private void AtualizarTotaisEGrafico()
        {
            if (Transacoes.Count == 0)
            {
                EntradasLabel.Text = "R$ 0,00";
                SaidasLabel.Text = "R$ 0,00";
                SaldoLabel.Text = "R$ 0,00";
                chartView.Chart = null;
                return;
            }

            decimal totalEntradas = Transacoes.Where(t => t.Tipo == "Entradas").Sum(t => t.Valor);
            decimal totalSaidas = Transacoes.Where(t => t.Tipo == "Saídas").Sum(t => t.Valor);
            decimal saldo = totalEntradas - totalSaidas;

            EntradasLabel.Text = $"R$ {totalEntradas:N2}";
            SaidasLabel.Text = $"R$ {totalSaidas:N2}";
            SaldoLabel.Text = $"R$ {saldo:N2}";

            // Cria o gráfico de rosca (DonutChart)
            var entries = new[]
            {
                new ChartEntry((float)totalEntradas)
                {
                    Label = "Entradas",
                    ValueLabel = totalEntradas.ToString("C"),
                    Color = SKColor.Parse("#4CAF50")
                },
                new ChartEntry((float)totalSaidas)
                {
                    Label = "Saídas",
                    ValueLabel = totalSaidas.ToString("C"),
                    Color = SKColor.Parse("#F44336")
                }
            };

            chartView.Chart = new DonutChart
            {
                Entries = entries,
                HoleRadius = 0.5f,
                LabelTextSize = 28,
                BackgroundColor = SKColors.Transparent
            };
        }
    }
}

