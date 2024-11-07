using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.Models
{
    public class MissaTemporaria
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string DiaSemana { get; set; } = null!;
        [Required]
        public TimeSpan Horario { get; set; }
        public int IgrejaId { get; set; }
        public Igreja Igreja { get; set; } = null!;
    }

}