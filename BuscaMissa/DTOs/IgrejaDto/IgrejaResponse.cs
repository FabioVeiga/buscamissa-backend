using BuscaMissa.DTOs.EnderecoDto;
using BuscaMissa.DTOs.MissaDto;
using BuscaMissa.DTOs.UsuarioDto;
using BuscaMissa.Enums;
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

        public IList<IgrejaRedesSociaisResponse> RedesSociais { get; set; } = [];

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
                Contato = igreja.Contato is null ? null : (IgrejaContatoResponse)igreja.Contato,
                RedesSociais = igreja.RedesSociais is null ? Array.Empty<IgrejaRedesSociaisResponse>() : [.. igreja.RedesSociais.Select(rs => (IgrejaRedesSociaisResponse)rs)]
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
            if(contato is null) return new();
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

    public class IgrejaRedesSociaisResponse()
    {
        public int Id { get; set; }
        public TipoRedeSocialEnum TipoRedeSocial { get; set; }
        public string NomeRedeSocial { get; set; } = default!;
        public string Url { get; set; } = default!;
        public string NomeDoPerfil { get; set; } = default!;

        public static explicit operator IgrejaRedesSociaisResponse(RedeSocial redeSocial)
        {
            return new IgrejaRedesSociaisResponse()
            {
                Id = redeSocial.Id,
                TipoRedeSocial = redeSocial.TipoRedeSocial,
                NomeRedeSocial = redeSocial.TipoRedeSocial.ToString(),
                NomeDoPerfil = redeSocial.NomeDoPerfil,
                Url =  Helpers.RedeSocialHelper.ObterURlRedesSociais(redeSocial.TipoRedeSocial, redeSocial.NomeDoPerfil)
            };
        }
    }
}