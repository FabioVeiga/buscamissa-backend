using BuscaMissa.DTOs.EnderecoDto;
using BuscaMissa.DTOs.MissaDto;
using BuscaMissa.DTOs.UsuarioDto;
using BuscaMissa.Models;

namespace BuscaMissa.DTOs.IgrejaDto
{
    public class IgrejaResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; } = null!;
        public string? Paroco { get; set; }
        public string? ImagemUrl { get; set; }
        public bool Ativo { get; set; }
        public DateTime Criacao { get; set; }
        public DateTime Alteracao { get; set; }
        public UsuarioDtoResponse? Usuario { get; set; } = default!;
        public EnderecoIgrejaResponse Endereco { get; set; } = default!;
        public IgrejaContatoResponse? Contato { get; set; }
        public IList<MissaResponse> Missas { get; set; } = [];

        //TODO: ver quais dados deve retornar
        public static explicit operator IgrejaResponse(Igreja igreja)
        {
            return new IgrejaResponse{
                Id = igreja.Id,
                Nome = igreja.Nome,
                Paroco = igreja.Paroco,
                ImagemUrl = igreja.ImagemUrl ?? string.Empty,
                Ativo = igreja.Ativo,
                Criacao = igreja.Criacao,
                Alteracao = igreja.Alteracao,
                Usuario = igreja.Usuario is null ? null : (UsuarioDtoResponse)igreja.Usuario,
                Endereco = (EnderecoIgrejaResponse)igreja.Endereco,
                Missas = [.. igreja.Missas.Select(m => (MissaResponse)m)],
                Contato = igreja.Contato is null ? null : (IgrejaContatoResponse)igreja.Contato
            };
        }
    }

    public class IgrejaContatoResponse{
        public string? EmailContato { get; set; }
        public bool? EmailContatoValidado { get; set; }
        public string? DDD { get; set; }
        public string? Telefone { get; set; }
        public bool? TelefoneValidado { get; set; }
        public string? DDDWhatsApp { get; set; }
        public string? TelefoneWhatsApp { get; set; }
        public bool? TelefoneWhatsAppValidado { get; set; }

        public static explicit operator IgrejaContatoResponse(Contato contato)
        {
            var modelo = new IgrejaContatoResponse
            {
                DDD = contato.DDD,
                Telefone = contato.Telefone,
                EmailContato = contato.EmailContato,
                EmailContatoValidado = contato.EmailContatoValidado,
                TelefoneValidado = contato.TelefoneValidado,
                DDDWhatsApp = contato.DDDWhatsApp,
                TelefoneWhatsApp = contato.TelefoneWhatsApp,
                TelefoneWhatsAppValidado = contato.TelefoneWhatsAppValidado
            };
            return modelo;
        }
    }
}