using KivoApp.Models;
using SQLite;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace KivoApp.Services
{
    public static class TransacaoService
    {
        public static ObservableCollection<Transacao> Transacoes { get; } = new();

        public static async Task LoadFromDatabaseAsync()
        {
            var db = DatabaseService.GetConnection();
            var transacoes = await db.Table<Transacao>().ToListAsync();


            Transacoes.Clear();
            foreach (var t in transacoes)
                Transacoes.Add(t);
        }

        public static async Task AdicionarTransacaoAsync(Transacao transacao)
        {
            var db = DatabaseService.GetConnection();
            await db.InsertAsync(transacao);
            Transacoes.Add(transacao);
        }

        public static async Task RemoverTransacaoAsync(Transacao transacao)
        {
            var db = DatabaseService.GetConnection();
            await db.DeleteAsync(transacao);
            Transacoes.Remove(transacao);
        }

        public static decimal CalcularSaldo()
        {
            decimal entradas = Transacoes.Where(t => t.Tipo == "Entrada").Sum(t => t.Valor);
            decimal saidas = Transacoes.Where(t => t.Tipo == "Saída").Sum(t => t.Valor);
            return entradas - saidas;
        }
    }
}
