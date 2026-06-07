using BuscaMissa.Enums;
using BuscaMissa.Models;

namespace BuscaMissa.Services;

public static class ConfiancaCalculator
{
    public static StatusConfiancaEnum Calcular(Missa missa)
    {
        if (missa.FontePrincipal == FontePrincipalEnum.Paroquia)
            return StatusConfiancaEnum.Alta;

        if (missa.UltimaValidacao is null)
            return StatusConfiancaEnum.Desconhecida;

        var diasDesdeValidacao = (DateTime.UtcNow - missa.UltimaValidacao.Value).TotalDays;

        return diasDesdeValidacao switch
        {
            <= 30 => StatusConfiancaEnum.Alta,
            <= 90 => StatusConfiancaEnum.Media,
            _ => StatusConfiancaEnum.Baixa
        };
    }

    // Retorna o pior status entre todas as missas de uma igreja
    public static StatusConfiancaEnum CalcularParaIgreja(IEnumerable<Missa> missas)
    {
        var lista = missas.ToList();
        if (lista.Count == 0) return StatusConfiancaEnum.Desconhecida;

        return lista.Select(Calcular).Min();
    }
}
