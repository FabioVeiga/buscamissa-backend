using System.Text.RegularExpressions;

namespace BuscaMissa.Helpers
{
    public static class CepHelper
    {
        public static int FormatarCep(string cepInput)
        {
            var formatado = Regex.Replace(cepInput, "[^0-9]", "");
            if (!int.TryParse(formatado, out int cepInteiro))
            {
                throw new Exception("CEP inválido");
            }
            return cepInteiro;
        }
    }
}