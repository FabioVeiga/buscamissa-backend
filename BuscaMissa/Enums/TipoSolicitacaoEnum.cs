using System.ComponentModel;

namespace BuscaMissa.Enums
{
    public enum TipoSolicitacaoEnum
    {
        [Description("Elogio")]
        Elogio = 1,
        [Description("Sugestão")]
        Sugestao = 2,
        [Description("Reclamação")]
        Reclamacao = 3,
        [Description("Atualização de dados")]
        Atualizacao_dados = 4,
        [Description("Email bloqueado")]
        Email_bloqueado = 5,
        [Description("Outro")]
        Outro = 1000
    }
}