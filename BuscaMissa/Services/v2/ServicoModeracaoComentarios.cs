namespace BuscaMissa.Services.v2;

public class ServicoModeracaoComentarios
{
    private static readonly List<string> PalavrasProibidas = new()
    {
        "idiota",
        "lixo",
        "ódio"
    };

    public (bool permitido, string motivo) Validar(string comentario)
    {
        var texto = comentario.ToLower();

        if (texto.Contains("http") || texto.Contains("www"))
            return (false, "Links não são permitidos");

        foreach (var palavra in PalavrasProibidas)
        {
            if (texto.Contains(palavra))
                return (false, "Linguagem ofensiva detectada");
        }

        return (true, "Liberado");
    }
}