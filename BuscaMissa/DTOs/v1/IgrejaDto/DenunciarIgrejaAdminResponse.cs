using BuscaMissa.Models;

namespace BuscaMissa.DTOs.IgrejaDto
{
    public class DenunciarIgrejaAdminResponse
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = default!;

          public static explicit operator DenunciarIgrejaAdminResponse(IgrejaDenuncia igrejaDenuncia)
          {
            return new DenunciarIgrejaAdminResponse()
            {
                Descricao = igrejaDenuncia.Descricao,
                Id = igrejaDenuncia.Id
            };
          }
    }
}