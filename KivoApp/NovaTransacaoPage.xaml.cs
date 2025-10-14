using System;
using System.Globalization;
using System.Linq;
using KivoApp.Models;
using KivoApp.Services;

namespace KivoApp;

public partial class NovaTransacaoPage : ContentPage
{
    public NovaTransacaoPage()
    {
        InitializeComponent();
        DataPicker.Date = DateTime.Now; // seta hoje como padrão
    }

    private async void OnSalvarTransacaoClicked(object sender, EventArgs e)
    {
        // Validações simples
        if (string.IsNullOrWhiteSpace(DescricaoEntry.Text))
        {
            await DisplayAlert("Atenção", "Preencha a descrição.", "OK");
            return;
        }

        if (!TryParseCurrency(ValorEntry.Text, out decimal valor) || valor <= 0m)
        {
            await DisplayAlert("Atenção", "Informe um valor válido.", "OK");
            return;
        }

        var tipo = TipoPicker.SelectedItem?.ToString() ?? "Entrada";
        var data = DataPicker.Date;

        var t = new Transacao
        {
            Descricao = DescricaoEntry.Text,
            Valor = valor,
            Tipo = tipo,
            Data = data
        };

        // Adiciona a transação
        TransacaoService.AdicionarTransacao(t);

        // Atualiza as metas depois que a transação foi adicionada
        decimal saldoDisponivel = TransacaoService.CalcularSaldo();
        MetaService.AtualizarMetas(saldoDisponivel);


        await DisplayAlert("Sucesso", "Transação adicionada!", "OK");
        await Navigation.PopAsync();
    }

    // Tenta converter textos como "R$ 1.234,56" ou "1234,56" ou "123456" (últimos 2 dígitos = centavos)
    private bool TryParseCurrency(string texto, out decimal valor)
    {
        valor = 0m;
        if (string.IsNullOrWhiteSpace(texto)) return false;

        var culture = new CultureInfo("pt-BR");

        // 1) tenta como currency direto (ex: "R$ 1.234,56")
        if (decimal.TryParse(texto, NumberStyles.Currency | NumberStyles.Number, culture, out valor))
            return true;

        // 2) remove caracteres inválidos e tenta como número com separador
        var filtrado = new string(texto.Where(c => char.IsDigit(c) || c == ',' || c == '.').ToArray());
        if (decimal.TryParse(filtrado, NumberStyles.Number, culture, out valor))
            return true;

        // 3) se sobrar só dígitos (ex: "123456"), considera últimos 2 dígitos como centavos
        var apenasDigitos = new string(texto.Where(char.IsDigit).ToArray());
        if (decimal.TryParse(apenasDigitos, out decimal n))
        {
            valor = n / 100m;
            return true;
        }

        return false;
    }

    private void OnValorEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        var entry = (Entry)sender;

        if (string.IsNullOrWhiteSpace(e.NewTextValue))
            return;

        // Remove tudo que não é número
        string apenasNumeros = new string(e.NewTextValue.Where(char.IsDigit).ToArray());

        if (string.IsNullOrEmpty(apenasNumeros))
        {
            entry.Text = string.Empty;
            return;
        }

        // Converte para decimal (em centavos)
        if (decimal.TryParse(apenasNumeros, out decimal valor))
        {
            valor /= 100; // Últimos 2 dígitos como centavos
            entry.Text = string.Format(new CultureInfo("pt-BR"), "{0:C}", valor);

            // Move o cursor para o fim
            entry.CursorPosition = entry.Text.Length;
        }
    }
}
