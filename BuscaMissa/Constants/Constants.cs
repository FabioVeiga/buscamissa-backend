namespace BuscaMissa.Constants;

public static class Constants
{
    public static string LogoUrl = "https://buscamissa.com.br/assets/logo.png";
    public static string InstagramIconUrl = "https://upload.wikimedia.org/wikipedia/commons/a/a5/Instagram_icon.png";
    public static string FacebookIconUrl = "https://upload.wikimedia.org/wikipedia/commons/5/51/Facebook_f_logo_%282019%29.svg";
    public static string BuscaMissaSiteUrl = "https://buscamissa.com.br/";
    public static string BuscaMissaInstagramUrl = "https://www.instagram.com/buscamissa";
    public static string BuscaMissaFacebookUrl = "https://www.facebook.com/buscamissa";

    // Fallback do FrontendBaseUrl quando a configuração não define (usado por
    // AdminController, IgrejaController v2 e SitemapController).
    public const string FrontendBaseUrlDefault = "https://buscamissa.com.br";
    // E-mail de suporte exibido ao usuário e usado como conta admin.
    public const string EmailSuporte = "suporte@buscamissa.com.br";
    // Nome do secret no Key Vault que guarda a senha canônica do Admin (usado no login
    // via SenhaHelper/DatabaseSeeder e re-sincronizado quando o Admin troca a própria senha).
    public const string SecretNameSenhaAdmin = "SenhaAdmin";

    // Mensagem genérica para erros 500 — nunca retornar ex.Message ao cliente
    // (vaza detalhes internos). O erro completo vai para o log (Serilog).
    public const string MensagemErroInterno = "Ocorreu um erro ao processar a requisição. Tente novamente mais tarde.";
}