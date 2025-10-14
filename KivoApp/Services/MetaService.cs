using System.Collections.ObjectModel;
using KivoApp.Models;

namespace KivoApp.Services
{
    public static class MetaService
    {
        private static ObservableCollection<Meta> metas = new();

        public static ObservableCollection<Meta> ObterMetas() => metas;

        public static void AdicionarMeta(Meta meta)
        {
            metas.Add(meta);
        }

        // Atualiza progresso das metas de acordo com o saldo
        public static void AtualizarMetas(decimal saldoDisponivel)
        {
            foreach (var meta in metas)
            {
                meta.ValorAtual = saldoDisponivel >= meta.ValorAlvo ? meta.ValorAlvo : saldoDisponivel;
            }
        }
    }
}
