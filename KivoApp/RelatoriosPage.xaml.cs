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

            // Inicializa opções do Picker
            TipoPicker.ItemsSource = new List<string> { "Todas", "Entrada", "Saída" };
            TipoPicker.SelectedIndex = 0;

            HistoricoCollectionView.ItemsSource = ListaFiltrada;

            // Escuta mensagens de atualização global
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

        // 🔹 Carrega todas as transações
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

        // 🔹 Exclui uma transação (com logs e atualização automática)
        private async void ExcluirTransacao_Clicked(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[ExcluirTransacao_Clicked] sender type: {(sender?.GetType().FullName ?? "null")}");

                object? param = null;

                if (sender is ImageButton ib)
                {
                    param = ib.CommandParameter;
                    System.Diagnostics.Debug.WriteLine("[ExcluirTransacao_Clicked] sender is ImageButton");
                }
                else if (sender is Button btn)
                {
                    param = btn.CommandParameter;
                    System.Diagnostics.Debug.WriteLine("[ExcluirTransacao_Clicked] sender is Button");
                }
                else if (sender is VisualElement ve)
                {
                    param = ve.BindingContext;
                    System.Diagnostics.Debug.WriteLine("[ExcluirTransacao_Clicked] sender is VisualElement (fallback BindingContext)");
                }

                if (param == null)
                {
                    await DisplayAlert("Erro", "Transação inválida para exclusão (parâmetro nulo).", "OK");
                    return;
                }

                Transacao? transacao = param as Transacao ?? (sender as VisualElement)?.BindingContext as Transacao;
                if (transacao == null)
                {
                    await DisplayAlert("Erro", "Não foi possível identificar a transação.", "OK");
                    return;
                }

                bool confirmar = await DisplayAlert("Confirmação", $"Deseja realmente excluir '{transacao.Descricao}'?", "Sim", "Não");
                if (!confirmar) return;

                int linhasAfetadas = await DatabaseService.DeleteTransacaoAsync(transacao);
                bool sucesso = linhasAfetadas > 0;


                if (sucesso)
                {
                    // Remove da lista e atualiza UI
                    TodasTransacoes.Remove(transacao);
                    ListaFiltrada.Remove(transacao);
                    AtualizarTotaisEGrafico();

                    // Atualiza metas automaticamente
                    MessagingCenter.Send<object>(this, "AtualizarMetas");

                    await DisplayAlert("Sucesso", "Transação excluída com sucesso!", "OK");
                }
                else
                {
                    await DisplayAlert("Erro", "Ocorreu um problema ao excluir a transação.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ExcluirTransacao_Clicked] EXCEPTION: {ex}");
                await DisplayAlert("Erro", $"Falha ao excluir: {ex.Message}", "OK");
            }
        }

        private void OnTipoPickerChanged(object sender, EventArgs e) => FiltrarTransacoes();
        private void OnDataFiltroChanged(object sender, DateChangedEventArgs e) => FiltrarTransacoes();

        // 🔹 Filtro por tipo e data
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
                        ListaFiltrada.Add(t);

                    AtualizarTotaisEGrafico();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao filtrar: {ex.Message}");
                }
            });
        }

        // 🔹 Atualiza totais e gráfico
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
                LabelMode = Microcharts.LabelMode.None
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
