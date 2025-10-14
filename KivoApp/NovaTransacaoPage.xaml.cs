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
        DataPicker.Date = DateTime.Now; // seta hoje como padr�o
    }

    private async void OnSalvarTransacaoClicked(object sender, EventArgs e)
    {
        // Valida��es simples
        if (string.IsNullOrWhiteSpace(DescricaoEntry.Text))
        {
            await DisplayAlert("Aten��o", "Preencha a descri��o.", "OK");
            return;
        }

        if (!TryParseCurrency(ValorEntry.Text, out decimal valor) || valor <= 0m)
        {
            await DisplayAlert("Aten��o", "Informe um valor v�lido.", "OK");
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

        // Adiciona a transa��o
        TransacaoService.AdicionarTransacao(t);

        // Atualiza as metas depois que a transa��o foi adicionada
        decimal saldoDisponivel = TransacaoService.CalcularSaldo();
        MetaService.AtualizarMetas(saldoDisponivel);


        await DisplayAlert("Sucesso", "Transa��o adicionada!", "OK");
        await Navigation.PopAsync();
    }

    // Tenta converter textos como "R$ 1.234,56" ou "1234,56" ou "123456" (�ltimos 2 d�gitos = centavos)
    private bool TryParseCurrency(string texto, out decimal valor)
    {
        valor = 0m;
        if (string.IsNullOrWhiteSpace(texto)) return false;

        var culture = new CultureInfo("pt-BR");

        // 1) tenta como currency direto (ex: "R$ 1.234,56")
        if (decimal.TryParse(texto, NumberStyles.Currency | NumberStyles.Number, culture, out valor))
            return true;

        // 2) remove caracteres inv�lidos e tenta como n�mero com separador
        var filtrado = new string(texto.Where(c => char.IsDigit(c) || c == ',' || c == '.').ToArray());
        if (decimal.TryParse(filtrado, NumberStyles.Number, culture, out valor))
            return true;

        // 3) se sobrar s� d�gitos (ex: "123456"), considera �ltimos 2 d�gitos como centavos
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

        // Remove tudo que n�o � n�mero
        string apenasNumeros = new string(e.NewTextValue.Where(char.IsDigit).ToArray());

        if (string.IsNullOrEmpty(apenasNumeros))
        {
            entry.Text = string.Empty;
            return;
        }

        // Converte para decimal (em centavos)
        if (decimal.TryParse(apenasNumeros, out decimal valor))
        {
            valor /= 100; // �ltimos 2 d�gitos como centavos
            entry.Text = string.Format(new CultureInfo("pt-BR"), "{0:C}", valor);

            // Move o cursor para o fim
            entry.CursorPosition = entry.Text.Length;
        }
    }
}
