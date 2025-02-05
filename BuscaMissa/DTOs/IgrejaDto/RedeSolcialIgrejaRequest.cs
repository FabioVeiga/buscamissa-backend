using System.ComponentModel.DataAnnotations;
using BuscaMissa.Enums;
using BuscaMissa.Filters;
using BuscaMissa.Helpers;

namespace BuscaMissa.DTOs.IgrejaDto
{
    public class RedeSolcialIgrejaRequest : IValidatableObject
    {
        public TipoRedeSocialEnum TipoRedeSocial { get; set; }
        [NoProfanity]
        public string NomeDoPerfil { get; set; } = null!;
        internal bool isValido = false;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            if(!EnumHelper.ValidarEnum<TipoRedeSocialEnum>((int)TipoRedeSocial))
            {
                results.Add(new ValidationResult($"Tipo de rede social inválido: {TipoRedeSocial}", [nameof(TipoRedeSocial)]));
            }else{
                isValido = RedeSocialHelper.ValidarRedesSociais(this).Result;
                if(!isValido)
                {
                    results.Add(new ValidationResult($"Rede social inválida: {NomeDoPerfil}", [nameof(NomeDoPerfil)]));
                }
            }
            return results;
        }
    }
}