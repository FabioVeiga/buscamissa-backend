using BuscaMissa.DTOs.IgrejaDto;
using BuscaMissa.Enums;

namespace BuscaMissa.Helpers
{
    public static class RedeSocialHelper
    {
        public static async Task<bool> ValidarRedesSociais(RedeSolcialIgrejaRequest request)
        {
            return request.TipoRedeSocial switch
            {
                TipoRedeSocialEnum.Facebook => await ValidarPerfilFacebook(request.NomeDoPerfil),
                TipoRedeSocialEnum.Instagram => await ValidarPerfilInstagram(request.NomeDoPerfil),
                TipoRedeSocialEnum.TikTok => await ValidarPerfilTikTok(request.NomeDoPerfil),
                TipoRedeSocialEnum.YouTube => await ValidarPerfilYouTubePersonalizado(request.NomeDoPerfil),
                _ => false,
            };
        }

        public static string ObterURlRedesSociais(TipoRedeSocialEnum tipoRedeSocialEnum, string nomeDoPerfil)
        {
            return tipoRedeSocialEnum switch
            {
                TipoRedeSocialEnum.Facebook => $"https://www.facebook.com/{nomeDoPerfil}",
                TipoRedeSocialEnum.Instagram => $"https://www.instagram.com/{nomeDoPerfil}/",
                TipoRedeSocialEnum.YouTube => $"https://www.youtube.com/c/{nomeDoPerfil}",
                TipoRedeSocialEnum.TikTok => $"https://www.tiktok.com/@{nomeDoPerfil}",
                _ => string.Empty,
            };
        }

        static async Task<bool> ValidarPerfilInstagram(string username)
        {
            string url = $"https://www.instagram.com/{username}/";
            using (HttpClient client = new())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (HttpRequestException)
                {
                    return false;
                }
            }
        }

        static async Task<bool> ValidarPerfilFacebook(string username)
        {
            // URL do perfil do Facebook
            string url = $"https://www.facebook.com/{username}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Faz uma requisição GET à URL do perfil
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Verifica se a resposta foi bem-sucedida (código 200)
                    if (response.IsSuccessStatusCode)
                    {
                        // Verifica se a URL foi redirecionada para a página de login
                        if (response.RequestMessage!.RequestUri!.ToString().Contains("login"))
                        {
                            return false; // Redirecionado para a página de login
                        }
                        return true; // Perfil existe
                    }
                    else
                    {
                        return false; // Perfil não encontrado
                    }
                }
                catch (HttpRequestException)
                {
                    // Erro na requisição (por exemplo, problemas de rede)
                    return false;
                }
            }
        }

        static async Task<bool> ValidarPerfilYouTubePersonalizado(string username)
        {
            // URL personalizada do canal do YouTube
            string url = $"https://www.youtube.com/c/{username}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Faz uma requisição GET à URL do canal
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Verifica se a resposta foi bem-sucedida (código 200)
                    if (response.IsSuccessStatusCode)
                    {
                        return true; // Canal existe
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return false; // Canal não encontrado
                    }
                    else
                    {
                        // Outros códigos de status (por exemplo, 403 para acesso proibido)
                        return false;
                    }
                }
                catch (HttpRequestException)
                {
                    // Erro na requisição (por exemplo, problemas de rede)
                    return false;
                }
            }
        }

        static async Task<bool> ValidarPerfilTikTok(string username)
        {
            // URL do perfil do TikTok
            string url = $"https://www.tiktok.com/@{username}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Faz uma requisição GET à URL do perfil
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Verifica se a resposta foi bem-sucedida (código 200)
                    if (response.IsSuccessStatusCode)
                    {
                        return true; // Perfil existe
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return false; // Perfil não encontrado
                    }
                    else
                    {
                        // Outros códigos de status (por exemplo, 403 para acesso proibido)
                        return false;
                    }
                }
                catch (HttpRequestException)
                {
                    // Erro na requisição (por exemplo, problemas de rede)
                    return false;
                }
            }
        }


    }
}