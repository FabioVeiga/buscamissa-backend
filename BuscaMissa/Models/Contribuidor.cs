using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.Models
{
    public class Contribuidor
    {
        [Key]
        public int Id { get; set; }
        public string Nome { get; set; } = default!;
        public DateTime DataContribuicao { get; set; } = DateTime.Now;
    }
}