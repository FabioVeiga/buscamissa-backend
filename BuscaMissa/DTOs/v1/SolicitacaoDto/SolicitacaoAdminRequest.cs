using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs.SolicitacaoDto
{
    public class SolicitacaoAdminRequest
    {
        [Required]
        public bool Resolvido { get; set; } = false;
        [Required]
        public string Solucao { get; set; } = default!;
        [Required]
        public string Resposta { get; set; } = default!;
        [Required]
        public bool EnviarResposta { get; set; } = false;
    }
}