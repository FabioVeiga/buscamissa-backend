using BuscaMissa.DTOs.v1.EmailHtmlGenerator;

namespace BuscaMissa.Constants
{
    public static class Contant
    {
        public static readonly string EmailReportarProblema = EmailHtmlGenerator.GerarLayout(
            "Ação Tomada Sobre o Problema Reportado",
            @"<p>Olá, {nome}.</p>
              <p>Você reportou um problema com a seguinte descrição:</p>
              <div class=""church-details"">
                  <p>{descricao}</p>
              </div>
              <p>Gostaríamos de informar que tomamos a seguinte ação com base no problema reportado:</p>
              <div class=""church-details"">
                  <p>{solução}</p>
              </div>
              <p>Agradecemos por sua contribuição e por nos ajudar a melhorar continuamente.</p>
              <p>Se tiver mais alguma questão, não hesite em entrar em contato.</p>
              <div class=""button-container"">
                  <a href=""https://buscamissa.com.br/solicitar"" class=""button"">Fale Conosco</a>
              </div>"
        );

        public static readonly string EmailValidacaoToken = EmailHtmlGenerator.GerarLayout(
            "Validação do Código",
            @"<p>Olá, {nome}.</p>
              <p>Para confirmar e ativar a igreja, informe o código de validação abaixo:</p>
              <div class=""church-details"" style=""text-align: center;"">
                  <p style=""font-size: 28px; font-weight: bold; letter-spacing: 6px; color: #bc5d10;"">{token}</p>
              </div>
              <p>Se você não solicitou esta validação, por favor ignore este e-mail.</p>
              <p>Se tiver mais alguma questão, não hesite em entrar em contato.</p>
              <div class=""button-container"">
                  <a href=""https://buscamissa.com.br/solicitar"" class=""button"">Fale Conosco</a>
              </div>"
        );

        public static readonly string EmailSolicitacaoResposta = EmailHtmlGenerator.GerarLayout(
            "Resposta à Sua Solicitação",
            @"<p>Olá, {nomeUsuario}.</p>
              <p>Recebemos sua solicitação com os seguintes detalhes:</p>
              <div class=""church-details"">
                  <p><strong>Número da Solicitação:</strong> {numeroSolicitacao}</p>
                  <p><strong>Assunto:</strong> {assuntoSolicitacao}</p>
                  <p><strong>Mensagem:</strong> {mensagemSolicitacao}</p>
              </div>
              <p>Abaixo está a nossa resposta à sua solicitação:</p>
              <div class=""church-details"">
                  <p>{respostaSolicitacao}</p>
              </div>
              <p>Se precisar de mais ajuda, não hesite em entrar em contato conosco.</p>
              <div class=""button-container"">
                  <a href=""https://buscamissa.com.br/solicitar"" class=""button"">Fale Conosco</a>
              </div>"
        );
    }
}
