using BuscaMissa.Models;

namespace BuscaMissa.DTOs
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
        public UsuarioDtoResponse Usuario { get; set; } = default!;
        public EnderecoIgrejaResponse Endereco { get; set; } = default!;

        //TODO: ver quais dados deve retornar
        public static explicit operator IgrejaResponse(Igreja igreja)
        {
            return new IgrejaResponse{
                Id = igreja.Id,
                Nome = igreja.Nome,
                Paroco = igreja.Paroco,
                ImagemUrl = igreja.ImagemUrl,
                Ativo = igreja.Ativo,
                Criacao = igreja.Criacao,
                Alteracao = igreja.Alteracao,
                Usuario = (UsuarioDtoResponse)igreja.Usuario,
                Endereco = (EnderecoIgrejaResponse)igreja.Endereco
            };
        }
    }
}