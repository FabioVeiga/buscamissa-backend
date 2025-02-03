using System.ComponentModel.DataAnnotations;
using BuscaMissa.DTOs.IgrejaDto;
using BuscaMissa.Enums;
using BuscaMissa.Helpers;

namespace BuscaMissa.Models
{
    public class RedeSocial
    {
        [Key]
        public int Id { get; set; }
        public TipoRedeSocialEnum TipoRedeSocial { get; set; }
        public string NomeDoPerfil { get; set; } = null!;
        public bool Verificado { get; set; } = false;
        public int IgrejaId { get; set; }
        public Igreja? Igreja { get; set; }


        public static explicit operator RedeSocial(RedeSolcialIgrejaRequest request)
        {
            return new RedeSocial()
            {
                NomeDoPerfil = request.NomeDoPerfil,
                TipoRedeSocial = request.TipoRedeSocial,
                Verificado = request.isValido
            };
        }
    }
}