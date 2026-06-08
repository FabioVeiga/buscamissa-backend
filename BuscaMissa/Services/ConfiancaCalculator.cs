using BuscaMissa.DTOs.MissaDto;
using BuscaMissa.Enums;
using BuscaMissa.Models;

namespace BuscaMissa.Services;

/// <summary>
/// Calcula o score de confiabilidade dos horários de missa (0–100).
///
/// FONTES — mede autoridade/origem da informação:
///   SiteOficial         = 50   (máxima autoridade institucional)
///   Diocese             = 45
///   SecretariaParoquial = 40
///   GoogleBusiness      = 30   (perfil oficial verificável pelo Google)
///   Usuario             = 25   (comunidade é fonte legítima de validação)
///   RedeSocial          = 20   (Instagram/Facebook — menos rastreável)
///   Desconhecida        =  0
///
/// RECÊNCIA — mede probabilidade de o horário estar correto hoje:
///   < 30 dias  = 50   (muito provável estar correto)
///   30–90 dias = 40
///   90–180     = 30
///   180–365    = 15
///   > 365 dias =  0
///
/// THRESHOLDS:
///   Score >= 75 → Alta          (ex: Usuário recente = 25+50 = 75 → Alta)
///   Score >= 51 → Média         (ex: Usuário 30–90d  = 25+40 = 65 → Média)
///   Score >= 26 → Baixa
///   Score  < 26 → Desconhecida
///
/// EVOLUÇÃO FUTURA PLANEJADA:
///   Adicionar fator "confirmações da comunidade" (0–10 pontos extras)
///   baseado em confirmações independentes via POST /confirmar.
///   Múltiplos usuários independentes confirmando o mesmo horário
///   aumentam gradualmente o score sem substituir a autoridade da fonte.
/// </summary>
public static class ConfiancaCalculator
{
    // ── Pontos por fonte ──────────────────────────────────────────────────────

    private static int PontosFonte(FontePrincipalEnum fonte) => fonte switch
    {
        FontePrincipalEnum.SiteOficial         => 50,
        FontePrincipalEnum.Diocese             => 45,
        FontePrincipalEnum.SecretariaParoquial => 40,
        FontePrincipalEnum.GoogleBusiness      => 30,
        FontePrincipalEnum.Usuario             => 25,
        FontePrincipalEnum.RedeSocial          => 20,
        _                                      => 0
    };

    // ── Pontos por recência ───────────────────────────────────────────────────

    private static int PontosRecencia(DateTime data)
    {
        var dias = (DateTime.UtcNow - data).TotalDays;
        return dias switch
        {
            <= 30  => 50,
            <= 90  => 40,
            <= 180 => 30,
            <= 365 => 15,
            _      => 0
        };
    }

    // ── Score (0–100) ─────────────────────────────────────────────────────────

    /// <summary>
    /// Calcula score 0–100.
    /// Se a missa não tiver fonte/validação própria (pré-migration),
    /// usa Igreja.Alteracao como fallback com peso de Usuario.
    /// </summary>
    public static int CalcularScore(
        FontePrincipalEnum fonte,
        DateTime? ultimaValidacao,
        DateTime? alteracaoFallback = null)
    {
        if (fonte == FontePrincipalEnum.Desconhecida && ultimaValidacao is null && alteracaoFallback.HasValue)
            return PontosFonte(FontePrincipalEnum.Usuario) + PontosRecencia(alteracaoFallback.Value);

        return PontosFonte(fonte) + (ultimaValidacao.HasValue ? PontosRecencia(ultimaValidacao.Value) : 0);
    }

    // ── Status a partir do score ──────────────────────────────────────────────

    /// <summary>
    /// Converte score numérico em nível de confiança.
    /// Threshold Alta = 75 garante que Usuario + recente (25+50=75) = Alta.
    /// </summary>
    public static StatusConfiancaEnum ScoreParaStatus(int score) => score switch
    {
        >= 75 => StatusConfiancaEnum.Alta,
        >= 51 => StatusConfiancaEnum.Media,
        >= 26 => StatusConfiancaEnum.Baixa,
        _     => StatusConfiancaEnum.Desconhecida
    };

    // ── Labels de exibição ────────────────────────────────────────────────────

    public static string ObterNivelLabel(StatusConfiancaEnum status) => status switch
    {
        StatusConfiancaEnum.Alta  => "Confirmado recentemente",
        StatusConfiancaEnum.Media => "Horário validado",
        StatusConfiancaEnum.Baixa => "Aguardando confirmação",
        _                         => "Horário não confirmado"
    };

    public static string ObterDescricao(StatusConfiancaEnum status) => status switch
    {
        StatusConfiancaEnum.Alta  => "Os horários foram confirmados recentemente.",
        StatusConfiancaEnum.Media => "Os horários foram verificados, mas há algum tempo.",
        StatusConfiancaEnum.Baixa => "Os horários estão aguardando confirmação da comunidade.",
        _                         => "Esses horários ainda não foram confirmados e podem estar desatualizados."
    };

    public static string ObterFonteLabel(FontePrincipalEnum fonte, bool usouFallback = false)
    {
        if (usouFallback) return "Informado pela comunidade";
        return fonte switch
        {
            FontePrincipalEnum.SiteOficial         => "Site oficial da paróquia",
            FontePrincipalEnum.Diocese             => "Site da diocese",
            FontePrincipalEnum.SecretariaParoquial => "Secretaria paroquial",
            FontePrincipalEnum.GoogleBusiness      => "Google Business da paróquia",
            FontePrincipalEnum.Usuario             => "Informado pela comunidade",
            FontePrincipalEnum.RedeSocial          => "Redes sociais da paróquia",
            _                                      => "Fonte não identificada"
        };
    }

    // ── Overloads de conveniência ─────────────────────────────────────────────

    public static StatusConfiancaEnum Calcular(Missa missa, DateTime? alteracaoFallback = null)
        => ScoreParaStatus(CalcularScore(missa.FontePrincipal, missa.UltimaValidacao, alteracaoFallback));

    public static StatusConfiancaEnum Calcular(MissaResponse missa)
        => ScoreParaStatus(CalcularScore(missa.FontePrincipal, missa.UltimaValidacao));

    public static StatusConfiancaEnum CalcularParaIgreja(IEnumerable<MissaResponse> missas)
    {
        var lista = missas.ToList();
        if (lista.Count == 0) return StatusConfiancaEnum.Desconhecida;
        return lista.Select(m => ScoreParaStatus(m.ScoreConfianca)).Min();
    }

    public static StatusConfiancaEnum CalcularParaIgreja(IEnumerable<Missa> missas)
    {
        var lista = missas.ToList();
        if (lista.Count == 0) return StatusConfiancaEnum.Desconhecida;
        return lista.Select(m => Calcular(m)).Min();
    }

    /// <summary>
    /// Preenche todos os campos de confiança de um MissaResponse em memória.
    /// Chamar após materialização do EF, nunca dentro de projeção LINQ.
    /// </summary>
    public static void PreencherConfianca(MissaResponse m, DateTime? alteracaoFallback = null)
    {
        bool usouFallback = m.FontePrincipal == FontePrincipalEnum.Desconhecida
                            && m.UltimaValidacao is null
                            && alteracaoFallback.HasValue;

        m.ScoreConfianca     = CalcularScore(m.FontePrincipal, m.UltimaValidacao, alteracaoFallback);
        m.StatusConfianca    = ScoreParaStatus(m.ScoreConfianca);
        m.NivelConfianca     = ObterNivelLabel(m.StatusConfianca);
        m.FonteLabel         = ObterFonteLabel(m.FontePrincipal, usouFallback);
        m.DescricaoConfianca = ObterDescricao(m.StatusConfianca);
    }
}
