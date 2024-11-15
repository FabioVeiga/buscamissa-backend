using System.ComponentModel.DataAnnotations;
using BuscaMissa.DTOs.IgrejaDto;

namespace BuscaMissa.Models
{
    public class IgrejaTemporaria
    {
        [Key]
        public int Id { get; set; }
        public string? Paroco { get; set; }
        public string? ImagemUrl { get; set; }
        [Required]
        public int IgrejaId { get; set; }

        public static explicit operator IgrejaTemporaria(AtualicaoIgrejaRequest request)
        {
            return new IgrejaTemporaria{
                Paroco = request.Paroco,
                ImagemUrl = request.ImagemUrl,
                IgrejaId = request.Id
            };
        }

    }
}