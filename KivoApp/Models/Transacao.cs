using System;

namespace KivoApp.Models
{
    public class Transacao
    {
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public string Tipo { get; set; } // "Entrada" ou "Saída"
        public DateTime Data { get; set; }
    }
}
