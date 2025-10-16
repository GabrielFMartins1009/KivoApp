using KivoApp.Models;
using SQLite;
using System;
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

        // Cria o banco e as tabelas, se ainda não existir
        public static async Task InitializeAsync()
        {
            if (_database != null)
                return;

            if (string.IsNullOrEmpty(_databasePath))
                throw new Exception("Database path não definido. Chame DatabaseService.Initialize(path) antes.");

            // Cria conexão
            _database = new SQLiteAsyncConnection(_databasePath);

            // Cria tabelas se não existirem
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
    }
}
