using SQLite;

namespace KivoApp.Models
{
    public class Transacao
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Tipo { get; set; } = "Entrada"; 

        public DateTime Data { get; set; } = DateTime.Now;
        public bool IsEntrada { get; set; }
    }
}
