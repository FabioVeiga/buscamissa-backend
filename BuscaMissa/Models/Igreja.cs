using System.ComponentModel.DataAnnotations;
using BuscaMissa.DTOs;
using BuscaMissa.DTOs.IgrejaDto;

namespace BuscaMissa.Models
{
    public class Igreja
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nome { get; set; } = null!;
        public string? Paroco { get; set; }
        public string? ImagemUrl { get; set; }
        [Required]        
        public DateTime Criacao { get; set; } = DateTime.Now;
        public DateTime Alteracao { get; set; }
        public bool Ativo { get; set; } = false;
        
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        
        public ICollection<Missa> Missas { get; set; } = [];

        public Endereco Endereco { get; set; } = null!;

        public Contato? Contato {get; set;}
        public IgrejaDenuncia? Denuncia {get; set;}

        public ICollection<RedeSocial>? RedesSociais { get; set; }

        public static explicit operator Igreja(CriacaoIgrejaRequest request)
        {
            var retorno = new Igreja{
                Nome = request.Nome,
                Paroco = request.Paroco,
            };
            foreach (var item in request.Missas)
            {
                Missa missa = (Missa)item;
                retorno.Missas.Add(missa);
            }
            //removendo missas iguais
                retorno.Missas = [.. retorno.Missas
                .GroupBy(m => new { m.DiaSemana, m.Horario })
                .Select(g => g.First())];
            retorno.Endereco = (Endereco)request.Endereco;
            if(request.Contato is not null)
                retorno.Contato = (Contato)request.Contato;

            if(request.RedeSociais is not null)
            {
                retorno.RedesSociais = [];
                foreach (var item in request.RedeSociais)
                {
                    retorno.RedesSociais!.Add((RedeSocial)item);
                }
            }
            return retorno;
        }
    }

}




