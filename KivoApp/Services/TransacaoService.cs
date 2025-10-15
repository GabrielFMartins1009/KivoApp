using System.Collections.ObjectModel;
using KivoApp.Models;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace KivoApp.Services
{
    public static class TransacaoService
    {
        public static ObservableCollection<Transacao> Transacoes { get; } = new ObservableCollection<Transacao>();

        static TransacaoService()
        {
            // sempre que a coleção mudar, salva automaticamente (fire-and-forget)
            Transacoes.CollectionChanged += async (s, e) =>
            {
                await DataStorageService.SaveTransacoesAsync(Transacoes);
            };
        }

        public static void AdicionarTransacao(Transacao transacao) => Transacoes.Add(transacao);
        public static void RemoverTransacao(Transacao transacao) => Transacoes.Remove(transacao);

        // utilitário para calcular saldo (já discutido)
        public static decimal CalcularSaldo()
        {
            decimal saldo = 0;
            foreach (var t in Transacoes)
            {
                if (string.Equals(t.Tipo, "Entrada", System.StringComparison.OrdinalIgnoreCase))
                    saldo += t.Valor;
                else
                    saldo -= t.Valor;
            }
            return saldo;
        }

        // Carrega transacoes na coleção (substitui conteúdo atual)
        public static async Task LoadFromStorageAsync()
        {
            var list = await DataStorageService.LoadTransacoesAsync();
            Transacoes.Clear();
            foreach (var t in list) Transacoes.Add(t);
        }
    }
}
