using System.Collections.ObjectModel;
using KivoApp.Models;
using System.Threading.Tasks;

namespace KivoApp.Services
{
    public static class MetaService
    {
        private static ObservableCollection<Meta> metas = new ObservableCollection<Meta>();
        public static ObservableCollection<Meta> Metas => metas;

        static MetaService()
        {
            metas.CollectionChanged += async (s, e) =>
            {
                await DataStorageService.SaveMetasAsync(metas);
            };
        }

        public static void AdicionarMeta(Meta meta) => metas.Add(meta);
        public static void RemoverMeta(Meta meta) => metas.Remove(meta);

        public static async Task LoadFromStorageAsync()
        {
            var list = await DataStorageService.LoadMetasAsync();
            metas.Clear();
            foreach (var m in list) metas.Add(m);
        }

        public static void AtualizarMetas(decimal saldoDisponivel)
        {
            foreach (var meta in metas)
            {
                meta.ValorAtual = saldoDisponivel >= meta.ValorAlvo ? meta.ValorAlvo : saldoDisponivel;
            }
        }
    }
}
