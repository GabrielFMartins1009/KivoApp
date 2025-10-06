using System;
using System.Globalization;
using System.Linq;
using System.Collections.Specialized;
using Microsoft.Maui.Controls;
using KivoApp.Services;
using KivoApp.Models;
using System.Collections.ObjectModel;
using Microcharts;
using SkiaSharp;

namespace KivoApp
{
    public partial class RelatoriosPage : ContentPage
    {
        public RelatoriosPage()
        {
            InitializeComponent();

            TipoPicker.SelectedIndex = 0; // "Todas"
            AtualizarTotais();
            AtualizarListaFiltrada();

            TransacaoService.Transacoes.CollectionChanged += Transacoes_CollectionChanged;
        }

        private void Transacoes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                AtualizarTotais();
                AtualizarListaFiltrada();
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            AtualizarTotais();
            AtualizarListaFiltrada();
        }

        protected override void OnDisappearing()
        {
            TransacaoService.Transacoes.CollectionChanged -= Transacoes_CollectionChanged;
            base.OnDisappearing();
        }

        private void AtualizarTotais()
        {
            var transacoes = TransacaoService.Transacoes;

            // Últimos 30 dias
            var corte = DateTime.Now.AddDays(-30);
            var ultimos30 = transacoes.Where(t => t.Data >= corte).ToList();

            var totalEntradas = ultimos30.Where(t => string.Equals(t.Tipo, "Entrada", StringComparison.OrdinalIgnoreCase)).Sum(t => t.Valor);
            var totalSaidas = ultimos30.Where(t => string.Equals(t.Tipo, "Saída", StringComparison.OrdinalIgnoreCase) || string.Equals(t.Tipo, "Saida", StringComparison.OrdinalIgnoreCase)).Sum(t => t.Valor);
            var saldo = totalEntradas - totalSaidas;

            var cult = new CultureInfo("pt-BR");
            EntradasLabel.Text = totalEntradas.ToString("C", cult);
            SaidasLabel.Text = totalSaidas.ToString("C", cult);
            SaldoLabel.Text = saldo.ToString("C", cult);

            // Gráfico
            var entries = new System.Collections.Generic.List<ChartEntry>
            {
                new ChartEntry((float)totalEntradas)
                {
                    Label = "Entradas",
                    ValueLabel = totalEntradas.ToString("N2", cult),
                    Color = SKColor.Parse("#4CAF50")
                },
                new ChartEntry((float)totalSaidas)
                {
                    Label = "Saídas",
                    ValueLabel = totalSaidas.ToString("N2", cult),
                    Color = SKColor.Parse("#F44336")
                },
                new ChartEntry((float)saldo)
                {
                    Label = "Saldo",
                    ValueLabel = saldo.ToString("N2", cult),
                    Color = SKColor.Parse("#2196F3")
                }
            };

            chartView.Chart = new DonutChart
            {
                Entries = entries,
                LabelTextSize = 18,
                HoleRadius = 0.5f
            };
        }

        private void OnTipoPickerChanged(object sender, EventArgs e)
        {
            AtualizarListaFiltrada();
        }

        private void OnDataFiltroChanged(object sender, DateChangedEventArgs e)
        {
            AtualizarListaFiltrada();
        }

        private void AtualizarListaFiltrada()
        {
            var transacoes = TransacaoService.Transacoes.AsEnumerable();

            // --- Filtro por Tipo ---
            string tipoSelecionado = TipoPicker.SelectedItem?.ToString() ?? "Todas";

            if (!string.IsNullOrEmpty(tipoSelecionado) && tipoSelecionado != "Todas")
            {
                string tipoNormalizado = tipoSelecionado.TrimEnd('s'); // "Entradas" ? "Entrada"
                transacoes = transacoes.Where(t =>
                    string.Equals(t.Tipo, tipoNormalizado, StringComparison.OrdinalIgnoreCase));
            }

            // --- Filtro por Data ---
            DateTime dataInicio = DataInicioPicker?.Date ?? DateTime.MinValue;
            DateTime dataFim = DataFimPicker?.Date ?? DateTime.MaxValue;
            dataFim = dataFim.AddDays(1).AddTicks(-1); // inclui o dia inteiro

            transacoes = transacoes.Where(t => t.Data >= dataInicio && t.Data <= dataFim);

            // --- Atualiza Lista ---
            HistoricoCollectionView.ItemsSource = transacoes
                .OrderByDescending(t => t.Data)
                .ToList();
        }
    }
}
