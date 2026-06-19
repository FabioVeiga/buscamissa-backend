using BuscaMissa.Enums;

namespace BuscaMissa.Helpers;

public static class PeriodoHelper
{
    private static readonly TimeSpan InicioManha = new(5, 0, 0);
    private static readonly TimeSpan FimManha    = new(11, 59, 59);
    private static readonly TimeSpan InicioTarde = new(12, 0, 0);
    private static readonly TimeSpan FimTarde    = new(17, 59, 59);
    private static readonly TimeSpan InicioNoite = new(18, 0, 0);
    private static readonly TimeSpan FimNoite    = new(23, 59, 59);

    public static (TimeSpan De, TimeSpan Ate) ObterFaixa(PeriodoEnum periodo) => periodo switch
    {
        PeriodoEnum.Manha => (InicioManha, FimManha),
        PeriodoEnum.Tarde => (InicioTarde, FimTarde),
        PeriodoEnum.Noite => (InicioNoite, FimNoite),
        _ => throw new ArgumentOutOfRangeException(nameof(periodo))
    };

    /// <summary>
    /// Resolve o nome do período a partir de uma querystring SEO ("manha", "tarde", "noite").
    /// Retorna null se não reconhecer.
    /// </summary>
    public static PeriodoEnum? ParseSlug(string? slug) => slug?.ToLowerInvariant() switch
    {
        "manha"  or "manhã"  => PeriodoEnum.Manha,
        "tarde"              => PeriodoEnum.Tarde,
        "noite"              => PeriodoEnum.Noite,
        _                    => null
    };
}
