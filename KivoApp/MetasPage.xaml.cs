using Microsoft.Maui.Controls;
using KivoApp.Models;
using KivoApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace KivoApp
{
    public partial class MetasPage : ContentPage
    {
        public ObservableCollection<Meta> ListaMetas => MetaService.Metas;

        public MetasPage()
        {
            InitializeComponent();
            BindingContext = this;

            // Escuta atualizações das metas
            MessagingCenter.Subscribe<object>(this, "MetasAtualizadas", async (_) =>
            {
                await MainThread.InvokeOnMainThreadAsync(async () => await CarregarMetasAsync());
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CarregarMetasAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<object>(this, "MetasAtualizadas");
            MessagingCenter.Unsubscribe<object>(this, "AtualizarTudo");

        }

        private async Task CarregarMetasAsync()
        {
            try
            {
                await MetaService.LoadFromDatabaseAsync();

                // Atualiza metas com base no saldo atual
                var saldoAtual = TransacaoService.CalcularSaldo();
                await MetaService.AtualizarMetas(saldoAtual);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar metas: {ex.Message}");
            }
        }

        private async void SalvarMeta_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DescricaoEntry.Text))
            {
                await DisplayAlert("Erro", "Digite uma descrição para a meta.", "OK");
                return;
            }

            if (!decimal.TryParse(ValorAlvoEntry.Text?.Replace("R$", "").Replace(" ", ""), out decimal valorAlvo))
            {
                await DisplayAlert("Erro", "Digite um valor válido.", "OK");
                return;
            }

            var meta = new Meta
            {
                Descricao = DescricaoEntry.Text.Trim(),
                ValorAlvo = valorAlvo,
                DataMeta = DataMetaPicker.Date
            };

            await MetaService.AdicionarMetaAsync(meta);

            // Atualiza metas considerando o saldo atual
            var saldoAtual = TransacaoService.CalcularSaldo();
            await MetaService.AtualizarMetas(saldoAtual);

            // Limpa os campos após salvar
            DescricaoEntry.Text = string.Empty;
            ValorAlvoEntry.Text = string.Empty;
            DataMetaPicker.Date = DateTime.Now;
        }

        private void ValorAlvoEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                string texto = entry.Text?.Replace("R$", "").Replace(" ", "").Replace(",", "").Replace(".", "") ?? "0";

                if (decimal.TryParse(texto, out decimal valor))
                {
                    // formata enquanto digita
                    entry.Text = $"R$ {valor / 100:N2}";
                    entry.CursorPosition = entry.Text.Length;
                }
            }
        }
        private async void ExcluirMeta_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Meta metaSelecionada)
            {
                bool confirmar = await DisplayAlert(
                    "Excluir Meta",
                    $"Deseja realmente excluir a meta \"{metaSelecionada.Descricao}\"?",
                    "Sim", "Cancelar");

                if (confirmar)
                {
                    try
                    {
                        await MetaService.RemoverMetaAsync(metaSelecionada);

                        // Atualiza a lista após exclusão
                        await CarregarMetasAsync();

                        await DisplayAlert("Sucesso", "Meta excluída com sucesso!", "OK");
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Erro", $"Falha ao excluir meta: {ex.Message}", "OK");
                    }
                }
            }
        }

    }

}
