namespace BuscaMissa.DTOs.v1.RedeSocialDto
{
    public class RedeSocialTipoResponse(int id, string nome, string urlBase, string icone)
    {
        public int Id { get; set; } = id;
        public string Nome { get; set; } = nome;
        public string UrlBase { get; set; } = urlBase;
        public string Icone { get; set; } = icone;
    }
}
