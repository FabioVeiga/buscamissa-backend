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
        if (request.RedeSociais != null && request.RedeSociais.Any())
            return NormalizarSlug(request.RedeSociais.FirstOrDefault()!.NomeDoPerfil);

        return NormalizarSlug($"{request.Endereco.Uf}-{request.Nome}");
    }

    public static string CriarNomeUnico(Igreja model)
    {
        return NormalizarSlug($"{model.Id}-{model.Nome}");
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
