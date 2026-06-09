using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using BuscaMissa.DTOs.IgrejaDto;
using BuscaMissa.Models;

namespace BuscaMissa.Helpers;

public static class IgrejaHelper
{
    public static string CriarNomeUnico(CriacaoIgrejaRequest request)
    {
        return NormalizarSlug($"{request.Endereco.Uf}-{request.Endereco.Localidade}-{request.Nome}");
    }

    // Gera slug com sufixo numérico para evitar colisões: "sp-campinas-nossa-senhora-2"
    public static string CriarNomeUnicoComSufixo(string baseSlug, int sufixo)
    {
        return sufixo <= 1 ? baseSlug : $"{baseSlug}-{sufixo}";
    }

    // Slug local à cidade — apenas o nome da paróquia (ex: "paroquia-sao-joao-bosco")
    public static string CriarSlugLocal(string nome) => NormalizarSlug(nome);

    // Slug da cidade (ex: "São José dos Campos" -> "sao-jose-dos-campos")
    public static string CriarCidadeSlug(string localidade) => NormalizarSlug(localidade);

    public static string CriarNomeUnico(Igreja model)
    {
        return NormalizarSlug($"{model.Endereco.Uf}-{model.Endereco.Localidade}-{model.Nome}");
    }

    public static string NormalizarSlug(string texto)
    {
        var normalizado = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var semAcentos = sb.ToString().Normalize(NormalizationForm.FormC).ToLower();
        return Regex.Replace(semAcentos, @"[^a-z0-9]+", "-").Trim('-');
    }
}
