using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.Models
{
    public class Contato
    {
        [Key]
        public int Id { get; set; }
        public string? EmailContato { get; set; }
        public bool? EmailContatoValidado { get; set; }
        public string? DDD { get; set; }
        public string? Telefone { get; set; }
        public bool? TelefoneValidado { get; set; }
        public string? DDDWhatsApp { get; set; }
        public string? TelefoneWhatsApp { get; set; }
        public bool? TelefoneWhatsAppValidado { get; set; }

        public int IgrejaId { get; set; }
        public Igreja Igreja { get; set; } = null!;

    }
}