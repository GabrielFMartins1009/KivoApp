using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using KivoApp.Models;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Storage;

namespace KivoApp.Services
{
    public static class DataStorageService
    {
        static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            //conversores customizados, aqui
        };

        const string TransacoesFileName = "transacoes.json";
        const string MetasFileName = "metas.json";

        static string GetPath(string fileName) =>
            Path.Combine(FileSystem.AppDataDirectory, fileName);

        #region Transacoes
        public static async Task SaveTransacoesAsync(IEnumerable<Transacao> transacoes)
        {
            try
            {
                var path = GetPath(TransacoesFileName);
                using var stream = File.Create(path);
                await JsonSerializer.SerializeAsync(stream, transacoes.ToList(), _jsonOptions).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao salvar transações: {ex}");
            }
        }

        public static async Task<List<Transacao>> LoadTransacoesAsync()
        {
            try
            {
                var path = GetPath(TransacoesFileName);
                if (!File.Exists(path)) return new List<Transacao>();
                using var stream = File.OpenRead(path);
                var list = await JsonSerializer.DeserializeAsync<List<Transacao>>(stream, _jsonOptions).ConfigureAwait(false);
                return list ?? new List<Transacao>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar transações: {ex}");
                return new List<Transacao>();
            }
        }
        #endregion

        #region Metas
        public static async Task SaveMetasAsync(IEnumerable<Meta> metas)
        {
            try
            {
                var path = GetPath(MetasFileName);
                using var stream = File.Create(path);
                await JsonSerializer.SerializeAsync(stream, metas.ToList(), _jsonOptions).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao salvar metas: {ex}");
            }
        }

        public static async Task<List<Meta>> LoadMetasAsync()
        {
            try
            {
                var path = GetPath(MetasFileName);
                if (!File.Exists(path)) return new List<Meta>();
                using var stream = File.OpenRead(path);
                var list = await JsonSerializer.DeserializeAsync<List<Meta>>(stream, _jsonOptions).ConfigureAwait(false);
                return list ?? new List<Meta>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar metas: {ex}");
                return new List<Meta>();
            }
        }
        #endregion

        // Conveniência: carregar ambos
        public static async Task<(List<Transacao> transacoes, List<Meta> metas)> LoadAllAsync()
        {
            var t = await LoadTransacoesAsync().ConfigureAwait(false);
            var m = await LoadMetasAsync().ConfigureAwait(false);
            return (t, m);
        }

        // Conveniência: salvar ambos
        public static async Task SaveAllAsync(IEnumerable<Transacao> transacoes, IEnumerable<Meta> metas)
        {
            await SaveTransacoesAsync(transacoes).ConfigureAwait(false);
            await SaveMetasAsync(metas).ConfigureAwait(false);
        }
    }
}
