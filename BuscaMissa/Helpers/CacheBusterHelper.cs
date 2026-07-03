namespace BuscaMissa.Helpers
{
    public static class CacheBusterHelper
    {
        /// <summary>
        /// Adiciona um cache-buster (timestamp) à URL da imagem para forçar refresh em caso de alterações.
        /// Exemplo: https://blob.../image.jpg?v=1609459200
        /// </summary>
        public static string AdicionarCacheBuster(string? imagemUrl, DateTime? dataAlteracao)
        {
            if (string.IsNullOrEmpty(imagemUrl))
                return string.Empty;

            // Se a URL já tem query string, não adiciona novamente
            if (imagemUrl.Contains("?"))
                return imagemUrl;

            // Usa o timestamp de alteração como versão
            if (dataAlteracao.HasValue)
            {
                var timestamp = new DateTimeOffset(dataAlteracao.Value).ToUnixTimeSeconds();
                return $"{imagemUrl}?v={timestamp}";
            }

            return imagemUrl;
        }
    }
}
