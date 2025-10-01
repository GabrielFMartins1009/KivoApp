using System.Collections.ObjectModel;
using KivoApp.Models;

namespace KivoApp.Services
{
    public static class TransacaoService
    {
        // ObservableCollection para a UI atualizar automaticamente
        public static ObservableCollection<Transacao> Transacoes { get; } = new ObservableCollection<Transacao>();

        public static void AdicionarTransacao(Transacao transacao) => Transacoes.Add(transacao);
        public static void RemoverTransacao(Transacao transacao) => Transacoes.Remove(transacao);
    }
}
