using System.ComponentModel.DataAnnotations;
using BuscaMissa.DTOs.IgrejaDto;

namespace BuscaMissa.Models
{
    public class Contato
    {
        [Key]
        public int Id { get; set; }
        public string? EmailContato { get; set; }
        public bool? EmailContatoValidado { get; set; } = false;
        public string? DDD { get; set; }
        public string? Telefone { get; set; }
        public bool? TelefoneValidado { get; set; } = false;
        public string? DDDWhatsApp { get; set; }
        public string? TelefoneWhatsApp { get; set; }
        public bool? TelefoneWhatsAppValidado { get; set; } = false;

        public int IgrejaId { get; set; }
        public Igreja Igreja { get; set; } = null!;

        public static explicit operator Contato(CriacaoIgrejaContatoRequest request)
        {
            return new Contato(){
                DDD = request.DDD,
                Telefone = request.Telefone,
                EmailContato = request.EmailContato,
                DDDWhatsApp = request.DDDWhatsApp,
                TelefoneWhatsApp = request.TelefoneWhatsApp

            };
        }
    }
}