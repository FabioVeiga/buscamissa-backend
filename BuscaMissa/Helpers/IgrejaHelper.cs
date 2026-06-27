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

    // Candidatos de slug para desempatar homônimas por bairro/logradouro (sem sufixo numérico):
    // base -> base-bairro -> base-logradouro -> base-bairro-logradouro (omitindo vazios).
    public static List<string> CandidatosSlug(string baseSlug, string? bairro, string? logradouro)
    {
        var b = NormalizarSlug(bairro ?? string.Empty);
        var l = NormalizarLogradouro(logradouro);
        var lista = new List<string> { baseSlug };
        if (b.Length > 0) lista.Add($"{baseSlug}-{b}");
        if (l.Length > 0) lista.Add($"{baseSlug}-{l}");
        if (b.Length > 0 && l.Length > 0) lista.Add($"{baseSlug}-{b}-{l}");
        return lista;
    }

    // Slug da cidade (ex: "São José dos Campos" -> "sao-jose-dos-campos")
    public static string CriarCidadeSlug(string localidade) => NormalizarSlug(localidade);

    public static string CriarNomeUnico(Igreja model)
    {
        return NormalizarSlug($"{model.Endereco.Uf}-{model.Endereco.Localidade}-{model.Nome}");
    }

    // Prefixos eclesiásticos removidos antes da comparação de duplicidade
    private static readonly Regex _prefixoEclesiastico = new(
        @"^(paroquia|paroquia|igreja|capela|santuario|catedral|basilica|" +
        @"matriz|comunidade|mosteiro|abadia|convento|ermida|oratorio)\s+",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Remove o prefixo eclesiástico do nome para comparação de duplicidade.
    // "Igreja São João" e "Paróquia São João" → "sao-joao" (mesma chave)
    public static string NormalizarNomeDedup(string nome)
    {
        var slug = NormalizarSlug(nome);
        var semPrefixo = _prefixoEclesiastico.Replace(slug.Replace("-", " "), "").Trim();
        return Regex.Replace(semPrefixo, @"\s+", "-").Trim('-');
    }

    // Expansões de tipo de via — unifica abreviações antes de normalizar
    private static readonly Dictionary<string, string> _tiposVia = new()
    {
        ["r"] = "rua", ["av"] = "avenida", ["pc"] = "praca", ["pca"] = "praca",
        ["pça"] = "praca", ["praca"] = "praca", ["tv"] = "travessa", ["trav"] = "travessa",
        ["estr"] = "estrada", ["rod"] = "rodovia", ["al"] = "alameda", ["lgo"] = "largo",
        ["pq"] = "parque", ["jd"] = "jardim",
    };

    // Normaliza logradouro para comparação de duplicidade:
    // expande o tipo de via ("R." -> "rua", "Av" -> "avenida") e então aplica NormalizarSlug.
    // Assim "Rua São João" e "R. São João" geram a MESMA chave.
    public static string NormalizarLogradouro(string? logradouro)
    {
        if (string.IsNullOrWhiteSpace(logradouro))
            return string.Empty;

        var tokens = logradouro.Trim().Split([' '], 2, StringSplitOptions.RemoveEmptyEntries);
        var primeiro = NormalizarSlug(tokens[0]); // remove "." e acentos do tipo
        if (_tiposVia.TryGetValue(primeiro, out var expandido) && tokens.Length == 2)
            logradouro = $"{expandido} {tokens[1]}";

        return NormalizarSlug(logradouro);
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
