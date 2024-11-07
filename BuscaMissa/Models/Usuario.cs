using System.ComponentModel.DataAnnotations;
using BuscaMissa.DTOs;
using BuscaMissa.DTOs.UsuarioDto;
using BuscaMissa.Enums;

namespace BuscaMissa.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nome { get; set; } = null!;
        [Required]
        public PerfilEnum Perfil { get; set; } = PerfilEnum.Regular;
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string Senha { get; set; } = null!;
        [Required]
        public bool AceitarTermo { get; set; }
        [Required]
        public DateTime Criacao { get; set; } = DateTime.Now;
        public ICollection<Igreja> Igrejas { get; set; } = [];

        public static explicit operator Usuario(IgrejaCriacaoUsuarioRequest request)
        {
            return new Usuario{
                Nome = request.Nome,
                Email = request.Email,
                AceitarTermo = request.AceitarTermo,
                Perfil = PerfilEnum.Regular,
                Senha = Helpers.SenhaHelper.Encriptar(Helpers.SenhaHelper.GerarSenhaTemporariaString())
            };
        }

        public static explicit operator Usuario(CriacaoUsuarioRequest request)
        {
            return new Usuario{
                Nome = request.Nome,
                Perfil = request.Perfil,
                Email = request.Email,
                Senha = Helpers.SenhaHelper.Encriptar(request.Senha),
                AceitarTermo = request.AceitarTermo,
            };
        }

        public static explicit operator Usuario(IgrejaRequest request)
        {
            return new Usuario{
                Nome = request.NomeUsuario,
                Email = request.EmailUsuario
            };
        }

        public static explicit operator Usuario(ValidarCriacaoIgrejaRequest request)
        {
            return new Usuario{
                Nome = request.Nome,
                Email = request.Email
            };
        }
        
    }

}