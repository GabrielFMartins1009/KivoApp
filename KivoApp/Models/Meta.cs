using System;
using System.ComponentModel;

namespace KivoApp.Models
{
    public class Meta : INotifyPropertyChanged
    {
        private string descricao = string.Empty;
        private decimal valorAlvo;
        private decimal valorAtual;
        private DateTime dataMeta = DateTime.Now;

        public string Descricao
        {
            get => descricao;
            set
            {
                if (descricao == value) return;
                descricao = value;
                OnPropertyChanged(nameof(Descricao));
            }
        }

        public decimal ValorAlvo
        {
            get => valorAlvo;
            set
            {
                if (valorAlvo == value) return;
                valorAlvo = value;
                OnPropertyChanged(nameof(ValorAlvo));
                OnPropertyChanged(nameof(Porcentagem));
                OnPropertyChanged(nameof(Progresso));
            }
        }

        public decimal ValorAtual
        {
            get => valorAtual;
            set
            {
                if (valorAtual == value) return;
                valorAtual = value;
                OnPropertyChanged(nameof(ValorAtual));
                OnPropertyChanged(nameof(Porcentagem));
                OnPropertyChanged(nameof(Progresso));
            }
        }

        public DateTime DataMeta
        {
            get => dataMeta;
            set
            {
                if (dataMeta == value) return;
                dataMeta = value;
                OnPropertyChanged(nameof(DataMeta));
            }
        }

        // Porcentagem entre 0.0 e 1.0 para ProgressBar binding direto
        public double Porcentagem => ValorAlvo == 0 ? 0.0 : Math.Min((double)ValorAtual / (double)ValorAlvo, 1.0);

        public string Progresso => $"{Porcentagem * 100:N1}% concluído";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
