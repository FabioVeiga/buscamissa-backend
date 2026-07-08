using BuscaMissa.Enums;

namespace BuscaMissa.DTOs.ControleDto
{
    // Item da fila de Aprovações Pendentes — lista o que já existe em Controle/Igreja,
    // sem nenhuma tabela de log nova.
    public class ControlePendenteResponse
    {
        public int ControleId { get; set; }
        public StatusEnum Status { get; set; }
        public DateTime DataCriacao { get; set; }
        public int? IgrejaId { get; set; }
        public string? NomeIgreja { get; set; }
        public string? Cidade { get; set; }
        public string? Uf { get; set; }

        // Nulo em criações ainda não aprovadas — o contribuidor só é vinculado à
        // igreja quando o Admin aprova (ver AprovacaoService.AprovarAsync).
        public string? UsuarioNome { get; set; }
        public string? UsuarioEmail { get; set; }

        // "Criacao" ou "Alteracao" — derivado do Status.
        public string Tipo { get; set; } = default!;
    }
}
