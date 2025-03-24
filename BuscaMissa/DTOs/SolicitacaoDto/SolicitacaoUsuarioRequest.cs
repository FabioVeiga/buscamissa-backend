using System.ComponentModel.DataAnnotations;
using BuscaMissa.Enums;

namespace BuscaMissa.DTOs.SolicitacaoDto
{
    public class SolicitacaoUsuarioRequest
    {
        [Required]
        public TipoSolicitacaoEnum Tipo { get; set; }
        [Required]
        public string Assunto { get; set; } = default!;
        [Required]
        public string Mensagem { get; set; } = default!;
        [Required]
        public string NomeSolicitante { get; set; } = default!;
        [Required]
        [EmailAddress]
        public string EmailSolicitante { get; set; } = default!;
    }
}