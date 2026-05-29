namespace BuscaMissa.DTOs.SettingsDto
{
    public class MailerSendEmailSetting
    {
        public string RemetenteNome { get; set; } = default!;
        public string RemetenteEmail { get; set; } = default!;
        public string TemplateIdCodigoValidacao { get; set; } = default!;
    }
}