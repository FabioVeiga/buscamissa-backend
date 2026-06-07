using BuscaMissa.Enums;
using BuscaMissa.Models;
using BuscaMissa.Services;

namespace BuscaMissa.DTOs.MissaDto
{
    public class MissaResponse
    {
        public int Id { get; set; }
        public DiaDaSemanaEnum DiaSemana { get; set; }
        public string Horario { get; set; } = default!;
        public string? Observacao { get; set; }

        // Proveniência
        public FontePrincipalEnum FontePrincipal { get; set; }
        public DateTime? UltimaValidacao { get; set; }

        // Confiança calculada (preenchida em memória, nunca via EF)
        public int ScoreConfianca { get; set; }
        public StatusConfiancaEnum StatusConfianca { get; set; }
        public string NivelConfianca { get; set; } = "Horário não confirmado";
        public string FonteLabel { get; set; } = "Fonte não identificada";
        public string DescricaoConfianca { get; set; } = "Esses horários ainda não foram confirmados e podem estar desatualizados.";

        // Cast simples — NÃO calcula confiança aqui (incomível pelo EF Core)
        public static explicit operator MissaResponse(Missa missa)
        {
            return new MissaResponse
            {
                Id             = missa.Id,
                DiaSemana      = missa.DiaSemana,
                Horario        = missa.Horario.ToString(),
                Observacao     = missa.Observacao,
                FontePrincipal = missa.FontePrincipal,
                UltimaValidacao = missa.UltimaValidacao
                // ScoreConfianca e demais campos preenchidos pelo ConfiancaCalculator.PreencherConfianca()
            };
        }

        public static explicit operator MissaResponse(MissaTemporaria missa)
        {
            return new MissaResponse
            {
                Id        = missa.Id,
                DiaSemana = missa.DiaSemana,
                Horario   = missa.Horario.ToString(),
                Observacao = missa.Observacao
            };
        }
    }
}
