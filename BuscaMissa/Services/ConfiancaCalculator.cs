using BuscaMissa.DTOs.MissaDto;
using BuscaMissa.Enums;
using BuscaMissa.Models;

namespace BuscaMissa.Services;

public static class ConfiancaCalculator
{
    // ── Score (0-100) ──────────────────────────────────────────────────────────

    private static int PontosFonte(FontePrincipalEnum fonte) => fonte switch
    {
        FontePrincipalEnum.SiteOficial         => 50,
        FontePrincipalEnum.Diocese             => 45,
        FontePrincipalEnum.SecretariaParoquial => 40,
        FontePrincipalEnum.RedeSocial          => 25,
        FontePrincipalEnum.Usuario             => 15,
        _                                      => 0
    };

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

    /// <summary>
    /// Calcula score 0-100.
    /// Se a missa não tiver fonte/validação própria, usa os dados da igreja como fallback.
    /// </summary>
    public static int CalcularScore(
        FontePrincipalEnum fonte,
        DateTime? ultimaValidacao,
        DateTime? alteracaoFallback = null)
    {
        // Fallback: usa data de alteração da igreja como se fosse validação de usuário
        if (fonte == FontePrincipalEnum.Desconhecida && ultimaValidacao is null && alteracaoFallback.HasValue)
            return PontosFonte(FontePrincipalEnum.Usuario) + PontosRecencia(alteracaoFallback.Value);

        var dataRef = ultimaValidacao;
        return PontosFonte(fonte) + (dataRef.HasValue ? PontosRecencia(dataRef.Value) : 0);
    }

    // ── Status enum ────────────────────────────────────────────────────────────

    public static StatusConfiancaEnum ScoreParaStatus(int score) => score switch
    {
        >= 76 => StatusConfiancaEnum.Alta,
        >= 51 => StatusConfiancaEnum.Media,
        >= 26 => StatusConfiancaEnum.Baixa,
        _     => StatusConfiancaEnum.Desconhecida
    };

    // ── Labels de exibição ────────────────────────────────────────────────────

    public static string ObterNivelLabel(StatusConfiancaEnum status) => status switch
    {
        StatusConfiancaEnum.Alta         => "Confirmado recentemente",
        StatusConfiancaEnum.Media        => "Horário validado",
        StatusConfiancaEnum.Baixa        => "Obtido de redes sociais",
        _                               => "Horário não confirmado"
    };

    public static string ObterDescricao(StatusConfiancaEnum status) => status switch
    {
        StatusConfiancaEnum.Alta  => "Os horários foram confirmados a partir de fonte oficial.",
        StatusConfiancaEnum.Media => "Os horários foram revisados recentemente.",
        StatusConfiancaEnum.Baixa => "Os horários foram obtidos de redes sociais.",
        _                        => "Esses horários ainda não foram confirmados e podem estar desatualizados."
    };

    public static string ObterFonteLabel(FontePrincipalEnum fonte, bool usouFallback = false)
    {
        if (usouFallback) return "Informado por usuário";
        return fonte switch
        {
            FontePrincipalEnum.SiteOficial         => "Site oficial da paróquia",
            FontePrincipalEnum.Diocese             => "Site da diocese",
            FontePrincipalEnum.SecretariaParoquial => "Secretaria paroquial",
            FontePrincipalEnum.RedeSocial          => "Redes sociais da paróquia",
            FontePrincipalEnum.Usuario             => "Informado por usuário",
            _                                      => "Fonte não identificada"
        };
    }

    // ── Overloads de conveniência ─────────────────────────────────────────────

    public static StatusConfiancaEnum Calcular(Missa missa, DateTime? alteracaoFallback = null)
    {
        var score = CalcularScore(missa.FontePrincipal, missa.UltimaValidacao, alteracaoFallback);
        return ScoreParaStatus(score);
    }

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
