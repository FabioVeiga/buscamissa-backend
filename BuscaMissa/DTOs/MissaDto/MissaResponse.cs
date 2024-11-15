
using BuscaMissa.Enums;
using BuscaMissa.Models;

namespace BuscaMissa.DTOs.MissaDto
{
    public class MissaResponse
    {
        public int Id { get; set; }
        public DiaDaSemanaEnum DiaSemana { get; set; }
        public string Horario { get; set; } = default!;

        public static explicit operator MissaResponse(Missa missa)
        {
            return new MissaResponse{
                Id = missa.Id,
                DiaSemana = missa.DiaSemana,
                Horario = missa.Horario.ToString()
            };
        }

        public static explicit operator MissaResponse(MissaTemporaria missa)
        {
            return new MissaResponse{
                Id = missa.Id,
                DiaSemana = missa.DiaSemana,
                Horario = missa.Horario.ToString()
            };
        }
    }
}