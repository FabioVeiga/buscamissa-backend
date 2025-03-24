using System.ComponentModel.DataAnnotations;
using BuscaMissa.Enums;

namespace BuscaMissa.Models
{
    public class Solicitacao
    {
        [Key]
        public int Id { get; set; }
        public string Numero { get; set; } = $"#{DateTime.Now.Ticks}";
        [Required]
        public TipoSolicitacaoEnum Tipo { get; set; }
        [Required]
        public string Assunto { get; set; } = default!;
        [Required]
        public string Mensagem { get; set; } = default!;
        public DateTime DataSolicitacao { get; set; } = DateTime.Now;
        [Required]
        public string NomeSolicitante { get; set; } = default!;
        [Required]
        public string EmailSolicitante { get; set; } = default!;
        public bool Resolvido { get; set; } = false;
        public string? Solucao { get; set; }
        public string? Resposta { get; set; }
        public bool? EnviarResposta { get; set; }
        public DateTime? DataSolucao { get; set; }
    }
}