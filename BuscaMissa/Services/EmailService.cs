using BuscaMissa.DTOs;
using BuscaMissa.Helpers;
using MailerSendNetCore.Common.Interfaces;
using MailerSendNetCore.Emails.Dtos;
using Microsoft.Extensions.Options;

namespace BuscaMissa.Services
{
    public class EmailService(IOptions<MailerSendEmailSetting> options, IMailerSendEmailClient mailerSendEmailClient)
    {
        private readonly IMailerSendEmailClient _mailerSendEmailClient = mailerSendEmailClient;
        private readonly MailerSendEmailSetting _mailerSendEmailSetting = options.Value;

        public async Task<string?> EnviarCodigoValidacao(string[] to, string subject, MailerSendEmailAttachment[] attachments, IDictionary<string, string>? variables, CancellationToken cancellationToken = default)
        {
            var parameters = new MailerSendEmailParameters();
            parameters
                .WithTemplateId(_mailerSendEmailSetting.TemplateIdCodigoValidacao)
                .WithFrom(_mailerSendEmailSetting.RemetenteEmail, _mailerSendEmailSetting.RemetenteNome)
                .WithTo(to)
                .WithAttachment(attachments)
                .WithSubject(subject);

            if (variables is { Count: > 0 })
            {
                foreach (var recipient in to)
                {
                    parameters.WithPersonalization(recipient, variables);
                }
            }

            var response = await _mailerSendEmailClient.SendEmailAsync(parameters, cancellationToken);
            if (response is { Errors.Count: > 0 })
            {
                Console.WriteLine(response);         
            }

            return response.MessageId;
        }

        public static IDictionary<string,string> DicionarioParaEnvioDoCodigo(string nome, int codigo, DateTime validoAte)
        {
            return new Dictionary<string,string>{
                {"nome", nome},
                {"codigo", codigo.ToString()},
                {"validoAte", DataHoraHelper.Formatar(validoAte)}
            };

        }
    }
}



