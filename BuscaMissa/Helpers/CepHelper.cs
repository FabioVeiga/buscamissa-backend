using System.Text.RegularExpressions;

namespace BuscaMissa.Helpers
{
    public static class CepHelper
    {
        public static string FormatarCep(string cepInput)
        {
            return Regex.Replace(cepInput, "[^0-9]", "");
        }
    }
}