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
        private ObservableCollection<Transacao> TodasTransacoes { get; set; } = new();
        private ObservableCollection<Transacao> ListaFiltrada { get; set; } = new();

        public RelatoriosPage()
        {
            InitializeComponent();
            BindingContext = this;

            // Inicializa as opções do Picker
            TipoPicker.ItemsSource = new List<string> { "Todas", "Entrada", "Saída" };
            TipoPicker.SelectedIndex = 0;

            HistoricoCollectionView.ItemsSource = ListaFiltrada;

            // Escuta atualizações
            MessagingCenter.Subscribe<object>(this, "AtualizarTudo", async (_) =>
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await CarregarTransacoesAsync();
                    FiltrarTransacoes();
                });
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CarregarTransacoesAsync();
            FiltrarTransacoes();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<object>(this, "AtualizarTudo");
        }

        // 🔹 Carrega todas as transações do banco
        private async Task CarregarTransacoesAsync()
        {
            try
            {
                var lista = await DatabaseService.GetTransacoesAsync();

                TodasTransacoes.Clear();
                ListaFiltrada.Clear();

                foreach (var t in lista.OrderByDescending(x => x.Data))
                {
                    TodasTransacoes.Add(t);
                    ListaFiltrada.Add(t);
                }

                AtualizarTotaisEGrafico();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar transações: {ex.Message}");
            }
        }

        // 🔹 Exclui uma transação
        private async Task ExcluirTransacaoAsync(Transacao transacao)
        {
            if (transacao == null) return;

            try
            {
                bool confirmar = await DisplayAlert("Confirmação", "Deseja excluir esta transação?", "Sim", "Não");
                if (!confirmar) return;

                // Remove do banco primeiro
                await DatabaseService.DeleteTransacaoAsync(transacao);

                // Atualiza a UI imediatamente na thread principal
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    // Remove das coleções locais
                    if (TodasTransacoes.Contains(transacao))
                        TodasTransacoes.Remove(transacao);
                    if (ListaFiltrada.Contains(transacao))
                        ListaFiltrada.Remove(transacao);
                });

                // Remove do serviço e notifica outras páginas
                if (TransacaoService.Transacoes.Contains(transacao))
                    TransacaoService.Transacoes.Remove(transacao);

                // Atualiza visuais
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    AtualizarTotaisEGrafico();
                    HistoricoCollectionView.ItemsSource = null;
                    HistoricoCollectionView.ItemsSource = ListaFiltrada;
                });

                // Notifica outras páginas
                var saldoAtual = TransacaoService.CalcularSaldo();
                await MetaService.AtualizarMetas(saldoAtual);
                MessagingCenter.Send<object>(this, "AtualizarTudo");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao excluir transação: {ex.Message}");
            }
        }

        private async void ExcluirTransacao_Clicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is Transacao transacao)
                await ExcluirTransacaoAsync(transacao);
        }

        private void OnTipoPickerChanged(object sender, EventArgs e) => FiltrarTransacoes();
        private void OnDataFiltroChanged(object sender, DateChangedEventArgs e) => FiltrarTransacoes();

        // 🔹 Filtro de tipo e data
        private void FiltrarTransacoes()
        {
            if (TodasTransacoes.Count == 0)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ListaFiltrada.Clear();
                    AtualizarTotaisEGrafico();
                });
                return;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    string tipoSelecionado = TipoPicker.SelectedItem?.ToString() ?? "Todas";
                    DateTime dataInicio = DataInicioPicker?.Date ?? DateTime.MinValue;
                    DateTime dataFim = DataFimPicker?.Date ?? DateTime.MaxValue;

                    var filtradas = TodasTransacoes
                        .Where(t => (tipoSelecionado == "Todas" || t.Tipo?.Equals(tipoSelecionado, StringComparison.OrdinalIgnoreCase) == true)
                                && t.Data.Date >= dataInicio.Date
                                && t.Data.Date <= dataFim.Date)
                        .OrderByDescending(t => t.Data)
                        .ToList();

                    ListaFiltrada.Clear();
                    
                    foreach (var t in filtradas)
                    {
                        ListaFiltrada.Add(t);
                    }

                    AtualizarTotaisEGrafico();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao filtrar: {ex.Message}");
                }
            });
        }

        private bool TipoCombina(string? tipoRegistro, string tipoSelecionado)
        {
            if (string.IsNullOrWhiteSpace(tipoRegistro)) return false;

            string reg = tipoRegistro.ToLowerInvariant();
            string sel = tipoSelecionado.ToLowerInvariant();

            if (sel.Contains("todas")) return true;
            if (sel.Contains("entrada")) return reg.Contains("entrada");
            if (sel.Contains("saída") || sel.Contains("saida")) return reg.Contains("saída") || reg.Contains("saida");

            return reg == sel;
        }

        // 🔹 Atualiza totais, legenda custom e gráfico
        private void AtualizarTotaisEGrafico()
        {
            if (ListaFiltrada.Count == 0)
            {
                SaldoLabel.Text = "R$ 0,00";
                LegendaEntradasValue.Text = "R$ 0,00";
                LegendaSaidasValue.Text = "R$ 0,00";
                chartView.Chart = null;
                return;
            }

            decimal totalEntradas = ListaFiltrada.Where(t => IsEntrada(t.Tipo)).Sum(t => t.Valor);
            decimal totalSaidas = ListaFiltrada.Where(t => IsSaida(t.Tipo)).Sum(t => t.Valor);
            decimal saldo = totalEntradas - totalSaidas;

            SaldoLabel.Text = $"R$ {saldo:N2}";
            LegendaEntradasValue.Text = $"R$ {totalEntradas:N2}";
            LegendaSaidasValue.Text = $"R$ {totalSaidas:N2}";

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
                BackgroundColor = SKColors.Transparent,
                LabelMode = Microcharts.LabelMode.None // Remove os labels do gráfico
            };
        }

        private bool IsEntrada(string? tipo) =>
            !string.IsNullOrWhiteSpace(tipo) && tipo.ToLowerInvariant().Contains("entrada");

        private bool IsSaida(string? tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo)) return false;
            var t = tipo.ToLowerInvariant();
            return t.Contains("saida") || t.Contains("saída");
        }
    }
}
