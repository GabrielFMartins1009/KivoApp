using KivoApp.Models;
using SQLite;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace KivoApp.Services
{
    public static class MetaService
    {
        public static ObservableCollection<Meta> Metas { get; } = new();

        public static async Task LoadFromDatabaseAsync()
        {
            var db = DatabaseService.GetConnection();
            var metas = await db.Table<Meta>().ToListAsync();
           


            Metas.Clear();
            foreach (var meta in metas)
                Metas.Add(meta);
        }

        public static async Task AdicionarMetaAsync(Meta meta)
        {
            var db = DatabaseService.GetConnection();
            await db.InsertAsync(meta);
            Metas.Add(meta);
        }

        public static async Task RemoverMetaAsync(Meta meta)
        {
            var db = DatabaseService.GetConnection();
            await db.DeleteAsync(meta);
            Metas.Remove(meta);
        }

        // Atualiza valores das metas de acordo com o saldo disponível
        public static void AtualizarMetas(decimal saldoDisponivel)
        {
            foreach (var meta in Metas)
            {
                meta.ValorAtual = saldoDisponivel >= meta.ValorAlvo ? meta.ValorAlvo : saldoDisponivel;
            }
        }
    }
}
