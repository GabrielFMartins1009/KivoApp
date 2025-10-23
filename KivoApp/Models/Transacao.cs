using System;
using SQLite;

namespace KivoApp.Models
{
    public class Transacao
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Descricao { get; set; } = string.Empty;

        public decimal Valor { get; set; }

        // Exemplos de valores: "Entrada", "Saída"
        public string Tipo { get; set; } = string.Empty;

        public DateTime Data { get; set; } = DateTime.Now;

        // Propriedade auxiliar (opcional): true se for entrada
        public bool IsEntrada { get; set; }
    }
}
