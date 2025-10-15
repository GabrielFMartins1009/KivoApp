using System.Globalization;
using Microsoft.Maui.Controls;
using KivoApp.Models;
using KivoApp.Services;

namespace KivoApp
{
    public partial class MetasPage : ContentPage
    {
        public MetasPage()
        {
            InitializeComponent();
            MetasCollectionView.ItemsSource = MetaService.Metas;

            // Seta a data atual como padrão
            DataMetaPicker.Date = DateTime.Now;
        }

        private void ValorAlvoEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            FormatarCampoMonetario(ValorAlvoEntry, e);
        }

        private void FormatarCampoMonetario(Entry entry, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(entry.Text))
                return;

            string apenasNumeros = new string(entry.Text.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(apenasNumeros))
            {
                entry.Text = string.Empty;
                return;
            }

            decimal valor = decimal.Parse(apenasNumeros) / 100;
            entry.Text = string.Format(CultureInfo.GetCultureInfo("pt-BR"), "{0:N2}", valor);
            entry.CursorPosition = entry.Text.Length;
        }

        private async void SalvarMeta_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DescricaoEntry.Text) ||
                string.IsNullOrWhiteSpace(ValorAlvoEntry.Text))
            {
                await DisplayAlert("Erro", "Preencha todos os campos!", "OK");
                return;
            }

            decimal valorAlvo = decimal.Parse(ValorAlvoEntry.Text, NumberStyles.Currency, CultureInfo.GetCultureInfo("pt-BR"));

            var novaMeta = new Meta
            {
                Descricao = DescricaoEntry.Text,
                ValorAlvo = valorAlvo,
                ValorAtual = 0,
                DataMeta = DataMetaPicker.Date
            };

            MetaService.AdicionarMeta(novaMeta);

            // Atualiza CollectionView
            MetasCollectionView.ItemsSource = null;
            MetasCollectionView.ItemsSource = MetaService.Metas;

            DescricaoEntry.Text = string.Empty;
            ValorAlvoEntry.Text = string.Empty;
        }
    }
}
