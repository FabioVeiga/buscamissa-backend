using System.Text;
using BuscaMissa.DTOs.EnderecoDto;

namespace BuscaMissa.DTOs.v1.EmailHtmlGenerator
{
    public static class EmailHtmlGenerator
    {
        // Método para gerar o HTML para criação de igreja
        public static string GerarHtmlEmailCriacao(string nomeIgreja, string logradouro, int? numero, string bairro, string localidade, string estado, string? paroco, string linkPaginaIgreja)
        {
            var enderecoFormatado = FormatarEndereco( logradouro,  numero, bairro, localidade, estado);
            var parocoHtml = !string.IsNullOrWhiteSpace(paroco) ? $"<p><strong>Pároco:</strong> {paroco}</p>" : string.Empty;

            var htmlBuilder = new StringBuilder();
            htmlBuilder.Append(GetHtmlHeader());
            htmlBuilder.Append($@"
                    <tr>
                        <td class=""content"">
                            <h2>Olá!</h2>
                            <p>A {nomeIgreja} foi cadastrada no Busca Missa, plataforma gratuita que ajuda os fiéis a encontrarem igrejas e horários de missas.</p>
                            <p>Pedimos apenas um minuto da sua atenção para verificar se as informações estão corretas.</p>

                            <div class=""church-details"">
                                <p><strong>Nome:</strong> {nomeIgreja}</p>
                                <p><strong>Endereço:</strong> {enderecoFormatado}</p>
                                {parocoHtml}
                            </div>

                            <div class=""button-container"">
                                <a href=""{linkPaginaIgreja}"" class=""button"">👉 Conferir informações</a>
                            </div>
                            <p>Caso seja necessário, você poderá solicitar qualquer alteração de forma simples e gratuita.</p>
                            <p>Muito obrigado por nos ajudar a manter os dados sempre atualizados!</p>
                            <p>Deus abençoe.</p>
                            <p>Equipe Busca Missa</p>
                        </td>
                    </tr>");
            htmlBuilder.Append(GetHtmlFooter());

            return htmlBuilder.ToString();
        }

        // Método para gerar o HTML para alteração de igreja
        public static string GerarHtmlEmailAlteracao(string nomeIgreja, string logradouro, int? numero, string bairro, string localidade, string estado, string? paroco, string linkPaginaIgreja)
        {
            var enderecoFormatado = FormatarEndereco(logradouro, numero, bairro, localidade, estado);
            var parocoHtml = !string.IsNullOrWhiteSpace(paroco) ? $"<p><strong>Pároco:</strong> {paroco}</p>" : string.Empty;

            var htmlBuilder = new StringBuilder();
            htmlBuilder.Append(GetHtmlHeader());
            htmlBuilder.Append($@"
                    <tr>
                        <td class=""content"">
                            <h2>Olá!</h2>
                            <p>As informações da {nomeIgreja} foram atualizadas no Busca Missa.</p>
                            <p>Para garantir que os fiéis encontrem sempre os horários corretos, pedimos que confira os dados da sua igreja.</p>

                            <div class=""church-details"">
                                <p><strong>Nome:</strong> {nomeIgreja}</p>
                                <p><strong>Endereço:</strong> {enderecoFormatado}</p>
                                {parocoHtml}
                            </div>

                            <div class=""button-container"">
                                <a href=""{linkPaginaIgreja}"" class=""button"">👉 Revisar informações</a>
                            </div>
                            <p>Se encontrar qualquer informação incorreta, você poderá solicitar a alteração rapidamente.</p>
                            <p>Obrigado pela colaboração!</p>
                            <p>Equipe Busca Missa</p>
                        </td>
                    </tr>");
            htmlBuilder.Append(GetHtmlFooter());

            return htmlBuilder.ToString();
        }

        // Mesmo e-mail de contato cadastrado em várias igrejas (ex: diocese, secretaria
        // paroquial) — um único e-mail com a lista de igrejas, em vez de um por igreja.
        public static string GerarHtmlEmailMultiplasIgrejas(
            IList<(string NomeIgreja, string LinkPaginaIgreja)> igrejas, bool criacao)
        {
            var introducao = criacao
                ? "As igrejas abaixo foram cadastradas no Busca Missa, plataforma gratuita que ajuda os fiéis a encontrarem igrejas e horários de missas."
                : "As informações das igrejas abaixo foram atualizadas no Busca Missa.";

            var chamada = criacao
                ? "Pedimos apenas um minuto da sua atenção para verificar se as informações de cada uma estão corretas:"
                : "Para garantir que os fiéis encontrem sempre os horários corretos, pedimos que confira os dados de cada uma:";

            var itensHtml = new StringBuilder();
            foreach (var igreja in igrejas)
            {
                itensHtml.Append($@"
                                <div class=""church-details"">
                                    <p><strong>{igreja.NomeIgreja}</strong></p>
                                    <div class=""button-container"" style=""margin: 10px 0;"">
                                        <a href=""{igreja.LinkPaginaIgreja}"" class=""button"">👉 {(criacao ? "Conferir informações" : "Revisar informações")}</a>
                                    </div>
                                </div>");
            }

            var htmlBuilder = new StringBuilder();
            htmlBuilder.Append(GetHtmlHeader());
            htmlBuilder.Append($@"
                    <tr>
                        <td class=""content"">
                            <h2>Olá!</h2>
                            <p>{introducao}</p>
                            <p>{chamada}</p>
                            {itensHtml}
                            <p>Caso seja necessário, você poderá solicitar qualquer alteração de forma simples e gratuita.</p>
                            <p>Muito obrigado por nos ajudar a manter os dados sempre atualizados!</p>
                            <p>Deus abençoe.</p>
                            <p>Equipe Busca Missa</p>
                        </td>
                    </tr>");
            htmlBuilder.Append(GetHtmlFooter());

            return htmlBuilder.ToString();
        }

        // Método auxiliar para formatar o endereço
        private static string FormatarEndereco(string logradouro, int? numero, string bairro, string localidade, string estado)
        {
            var sb = new StringBuilder();
            sb.Append(logradouro);

            // A correção aqui é para verificar se Numero é uma string "0" ou um valor numérico 0
            // Como EnderecoIgrejaRequest.Numero é string, a comparação deve ser com string "0"
            if (numero != 0 )
            {
                sb.Append($", {numero}");
            }

            if (!string.IsNullOrWhiteSpace(bairro))
            {
                sb.Append($" - {bairro}");
            }

            // O DTO EnderecoIgrejaRequest tem 'Localidade', não 'Cidade'. Ajustando para 'Localidade'.
            if (!string.IsNullOrWhiteSpace(localidade))
            {
                sb.Append($", {localidade}");
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                sb.Append($" - {estado}");
            }
            
            return sb.ToString();
        }

        public static string GerarLayout(string titulo, string corpo)
        {
            var sb = new StringBuilder();
            sb.Append(GetHtmlHeader());
            sb.Append($@"
                    <tr>
                        <td style=""background-color: #bc5d10; color: #ffffff; padding: 16px 30px; text-align: center; font-size: 20px; font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif;"">
                            {titulo}
                        </td>
                    </tr>
                    <tr>
                        <td class=""content"">
                            {corpo}
                        </td>
                    </tr>");
            sb.Append(GetHtmlFooter());
            return sb.ToString();
        }

        // Parte inicial do HTML (cabeçalho, estilos, logo)
        private static string GetHtmlHeader()
        {
            return $@"
            <!DOCTYPE html>
            <html lang=""pt-BR"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Busca Missa</title>
                <style>
                    body {{
                        font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                        background-color: #f4f4f4;
                        color: #333333;
                    }}
                    table {{
                        width: 100%;
                        border-collapse: collapse;
                    }}
                    td {{
                        padding: 0;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: 20px auto;
                        background-color: #ffffff;
                        border-radius: 8px;
                        overflow: hidden;
                        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                    }}
                    .header {{
                        background-color: #FFF;
                        padding: 20px;
                        text-align: center;
                    }}
                    .header img {{
                        max-width: 180px;
                        height: auto;
                    }}
                    .content {{
                        padding: 30px;
                        line-height: 1.6;
                        color: #555555;
                    }}
                    .content h2 {{
                        color: #bc5d10;
                        font-size: 24px;
                        margin-top: 0;
                        margin-bottom: 15px;
                    }}
                    .content p {{
                        margin-bottom: 15px;
                    }}
                    .button-container {{
                        text-align: center;
                        margin: 25px 0;
                    }}
                    .button {{
                        display: inline-block;
                        background-color: #bc5d10;
                        color: #ffffff;
                        padding: 12px 25px;
                        border-radius: 5px;
                        text-decoration: none;
                        font-weight: bold;
                        font-size: 16px;
                    }}
                    .church-details {{
                        background-color: #f9f9f9;
                        border-left: 4px solid #f3d3b3;
                        padding: 20px;
                        margin: 20px 0;
                        border-radius: 4px;
                    }}
                    .church-details p {{
                        margin: 5px 0;
                    }}
                    .church-details strong {{
                        color: #bc5d10;
                    }}
                    .footer {{
                        background-color: #333333;
                        color: #ffffff;
                        padding: 20px;
                        text-align: center;
                        font-size: 12px;
                    }}
                    .footer a {{
                        color: #ffffff;
                        text-decoration: none;
                        margin: 0 5px;
                    }}
                    .footer a:hover {{
                        text-decoration: underline;
                    }}
                    .social-icons img {{
                        width: 24px;
                        height: 24px;
                        margin: 0 5px;
                        vertical-align: middle;
                    }}
                    .highlight-text {{
                        font-weight: bold;
                        color: #8a4a12;
                    }}
                </style>
            </head>
            <body>
                <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"">
                    <tr>
                        <td align=""center"">
                            <table class=""container"" role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                                <!-- Header -->
                                <tr>
                                    <td class=""header"">
                                        <img src=""{Constants.Constants.LogoUrl}"" alt=""Busca Missa Logo"">
                                    </td>
                                </tr>
            ";
        }

        // Parte final do HTML (rodapé)
        private static string GetHtmlFooter()
        {
            return $@"
                                <!-- Footer -->
                                <tr>
                                    <td class=""footer"">
                                        <p>Visite nosso site: <a href=""{Constants.Constants.BuscaMissaSiteUrl}"" target=""_blank"" style=""text-decoration: underline;"">{Constants.Constants.BuscaMissaSiteUrl.Replace("https://", "")}</a></p>
                                        <p class=""social-icons"">
                                            Siga-nos:
                                            <a href=""{Constants.Constants.BuscaMissaInstagramUrl}"" target=""_blank"" style=""text-decoration: none;"">
                                                <img src=""{Constants.Constants.InstagramIconUrl}"" alt=""Instagram"">
                                            </a>
                                            <a href=""{Constants.Constants.BuscaMissaFacebookUrl}"" target=""_blank"" style=""text-decoration: none;"">
                                                <img src=""{Constants.Constants.FacebookIconUrl}"" alt=""Facebook"">
                                            </a>
                                        </p>
                                        <p>&copy; {DateTime.Now.Year} Busca Missa. Todos os direitos reservados.</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
        }
    }
}