using System.Text.RegularExpressions;

namespace BuscaMissa.Helpers
{
    public static class CepHelper
    {
        public static string FormatarCep(string cepInput)
        {
            var formattedCep = Regex.Replace(cepInput, "[^0-9]", "");
            return formattedCep.PadLeft(8, '0'); 
        }
    }
}