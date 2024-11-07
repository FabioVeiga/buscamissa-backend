using System.ComponentModel.DataAnnotations;
using BuscaMissa.Enums;

namespace BuscaMissa.Models
{
    public class RedeSocial
    {
        [Key]
        public int Id { get; set; }
        public TipoRedeSocialEnum? TipoRedeSocial { get; set; }
        public string? Url { get; set; }
        public string? Site { get; set; }
        public int IgrejaId { get; set; }
        public Igreja Igreja { get; set; } = null!;
    }
}