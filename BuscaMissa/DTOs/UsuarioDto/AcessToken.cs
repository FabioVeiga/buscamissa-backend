namespace BuscaMissa.DTOs.UsuarioDto
{
    public class AcessToken(string token, DateTime expiracao)
    {
        public string Token { get; private set; } = token;
        public DateTime Expira { get; private set; } = expiracao;
    }
}