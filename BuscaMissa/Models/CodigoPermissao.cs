using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.Models
{
    public class CodigoPermissao
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int CodigoToken { get; set; }
        [Required]
        public DateTime ValidoAte { get; set; }
        public int? ControleId { get; set; }
        public Controle? Controle { get; set; }
    }
}