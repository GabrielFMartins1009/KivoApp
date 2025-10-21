using KivoApp.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace KivoApp.Services
{
    public static class DatabaseService
    {
        private static SQLiteAsyncConnection? _database;
        private static string? _databasePath;

        // Define o caminho do banco — chamada no App.xaml.cs
        public static void Initialize(string databasePath)
        {
            _databasePath = databasePath;
        }

        // Cria o banco e as tabelas, se ainda não existirem
        public static async Task InitializeAsync()
        {
            if (_database != null)
                return;

            if (string.IsNullOrEmpty(_databasePath))
                throw new Exception("Database path não definido. Chame DatabaseService.Initialize(path) antes.");

            _database = new SQLiteAsyncConnection(_databasePath);

            await _database.CreateTableAsync<Transacao>();
            await _database.CreateTableAsync<Meta>();
        }

        // Retorna a instância do banco
        public static SQLiteAsyncConnection GetConnection()
        {
            if (_database == null)
                throw new Exception("Banco de dados não inicializado. Chame DatabaseService.InitializeAsync() antes.");
            return _database;
        }

        // CRUD de Transações
        public static async Task<List<Transacao>> GetTransacoesAsync()
        {
            if (_database == null)
                throw new Exception("Banco de dados não inicializado.");

            return await _database.Table<Transacao>().ToListAsync();
        }

        public static async Task<int> SaveTransacaoAsync(Transacao transacao)
        {
            if (_database == null)
                throw new Exception("Banco de dados não inicializado.");

            if (transacao.Id != 0)
                return await _database.UpdateAsync(transacao);
            else
                return await _database.InsertAsync(transacao);
        }

        public static async Task<int> DeleteTransacaoAsync(Transacao transacao)
        {
            if (_database == null)
                throw new Exception("Banco de dados não inicializado.");

            return await _database.DeleteAsync(transacao);
        }

        // 🔹 CRUD de Metas
        public static async Task<List<Meta>> GetMetasAsync()
        {
            if (_database == null)
                throw new Exception("Banco de dados não inicializado.");

            return await _database.Table<Meta>().ToListAsync();
        }

        public static async Task<int> SaveMetaAsync(Meta meta)
        {
            if (_database == null)
                throw new Exception("Banco de dados não inicializado.");

            if (meta.Id != 0)
                return await _database.UpdateAsync(meta);
            else
                return await _database.InsertAsync(meta);
        }

        public static async Task<int> DeleteMetaAsync(Meta meta)
        {
            if (_database == null)
                throw new Exception("Banco de dados não inicializado.");

            return await _database.DeleteAsync(meta);
        }
    }
}
