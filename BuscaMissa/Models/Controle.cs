using System.ComponentModel.DataAnnotations;
using BuscaMissa.Enums;

namespace BuscaMissa.Models
{
    public class Controle
    {
        [Key]
        public int Id { get; set; }
        public StatusEnum Status { get; set; }
        public int? IgrejaId { get; set; }
        public Igreja? Igreja { get; set; }
    }
}