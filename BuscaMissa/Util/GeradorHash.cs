namespace BuscaMissa.Util;

using System.Security.Cryptography;
using System.Text;

public static class GeradorHash
{
    public static string Gerar(string texto)
    {
        using var sha = SHA256.Create();

        var bytes = Encoding.UTF8.GetBytes(texto);

        var hash = sha.ComputeHash(bytes);

        return Convert.ToBase64String(hash);
    }
}