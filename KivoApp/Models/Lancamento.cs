namespace KivoApp.Models
{
    public class Lancamento
    {
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public bool EhEntrada { get; set; } // true = entrada, false = saída
    }
}
