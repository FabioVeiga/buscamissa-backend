using BuscaMissa.DTOs.IgrejaDto;
using BuscaMissa.Enums;
using System.Text.RegularExpressions;

namespace BuscaMissa.Helpers
{
    public static class RedeSocialHelper
    {
        // Extrai o handle de uma URL completa ou limpa um handle puro (remove @ e espaços).
        public static string NormalizarHandle(TipoRedeSocialEnum tipo, string entrada)
        {
            if (string.IsNullOrWhiteSpace(entrada)) return string.Empty;

            entrada = entrada.Trim();

            if (entrada.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                entrada.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
            {
                var url = entrada.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? entrada
                    : "https://" + entrada;

                if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    var path = uri.AbsolutePath.Trim('/');
                    var primeiro = path.Split('/').FirstOrDefault(s => !string.IsNullOrWhiteSpace(s)) ?? string.Empty;
                    return Uri.UnescapeDataString(primeiro).TrimStart('@');
                }
            }

            return entrada.TrimStart('@');
        }

        public static bool ValidarRedesSociais(RedeSolcialIgrejaRequest request)
        {
            var handle = request.NomeDoPerfil;
            if (string.IsNullOrWhiteSpace(handle)) return false;

            return request.TipoRedeSocial switch
            {
                // letras, números, ponto, hífen — mínimo 5 (regra do Facebook)
                TipoRedeSocialEnum.Facebook => Regex.IsMatch(handle, @"^[\w.\-]{5,}$"),
                // letras, números, ponto, underscore — até 30 chars
                TipoRedeSocialEnum.Instagram => Regex.IsMatch(handle, @"^[\w.]{1,30}$"),
                // handles podem conter letras Unicode (ex: acentuadas), números, ponto, hífen, underscore
                TipoRedeSocialEnum.YouTube => Regex.IsMatch(handle, @"^[\p{L}\p{N}._\-]{3,}$"),
                // letras, números, ponto, underscore, hífen — até 24 chars
                TipoRedeSocialEnum.TikTok => Regex.IsMatch(handle, @"^[\w.\-]{2,24}$"),
                // apenas letras, números e underscore — até 15 chars (regra do Twitter/X)
                TipoRedeSocialEnum.Twitter => Regex.IsMatch(handle, @"^[a-zA-Z0-9_]{1,15}$"),
                _ => false,
            };
        }

        public static string ObterURlRedesSociais(TipoRedeSocialEnum tipoRedeSocialEnum, string nomeDoPerfil)
        {
            return tipoRedeSocialEnum switch
            {
                TipoRedeSocialEnum.Facebook => $"https://www.facebook.com/{nomeDoPerfil}",
                TipoRedeSocialEnum.Instagram => $"https://www.instagram.com/{nomeDoPerfil}/",
                TipoRedeSocialEnum.YouTube => $"https://www.youtube.com/@{nomeDoPerfil}",
                TipoRedeSocialEnum.TikTok => $"https://www.tiktok.com/@{nomeDoPerfil}",
                TipoRedeSocialEnum.Twitter => $"https://x.com/{nomeDoPerfil}",
                _ => string.Empty,
            };
        }

        public static string ObterUrlBase(TipoRedeSocialEnum tipoRedeSocialEnum)
        {
            return tipoRedeSocialEnum switch
            {
                TipoRedeSocialEnum.Facebook => "https://www.facebook.com/",
                TipoRedeSocialEnum.Instagram => "https://www.instagram.com/",
                TipoRedeSocialEnum.YouTube => "https://www.youtube.com/@",
                TipoRedeSocialEnum.TikTok => "https://www.tiktok.com/@",
                TipoRedeSocialEnum.Twitter => "https://x.com/",
                _ => string.Empty,
            };
        }
    }
}
