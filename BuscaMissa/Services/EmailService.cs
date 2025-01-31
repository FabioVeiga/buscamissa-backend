using BuscaMissa.DTOs;
using BuscaMissa.Helpers;
using MailerSendNetCore.Common.Interfaces;
using MailerSendNetCore.Emails.Dtos;
using Microsoft.Extensions.Options;

namespace BuscaMissa.Services
{
    public class EmailService(IOptions<SettingCodigoValidacao> options, IMailerSendEmailClient mailerSendEmailClient)
    {
        private readonly IMailerSendEmailClient _mailerSendEmailClient = mailerSendEmailClient;
        private readonly SettingCodigoValidacao _mailerSendEmailSetting = options.Value;

        public async Task<string?> EnviarEmail(string[] to, string subject, IDictionary<string, string>? variables, CancellationToken cancellationToken = default)
        {
            var parameters = new MailerSendEmailParameters();
            parameters
                .WithTemplateId(_mailerSendEmailSetting.TemplateIdCodigoValidacao)
                .WithFrom(_mailerSendEmailSetting.RemetenteEmail, _mailerSendEmailSetting.RemetenteNome)
                .WithTo(to)
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

        public async Task<bool> EnviarCodigoValidador(string nome, int codigo, DateTime validoAte, string emailPara)
        {
            try
            {
                var dict = new Dictionary<string,string>{
                    {"nome", nome},
                    {"codigo", codigo.ToString()},
                    {"validoAte", DataHoraHelper.Formatar(validoAte)}
                };
                var enviado = await EnviarEmail([emailPara], "Código para Validação", dict);
                return enviado != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}



