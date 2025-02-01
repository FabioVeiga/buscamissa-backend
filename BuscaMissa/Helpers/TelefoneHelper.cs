using System.Text.RegularExpressions;

namespace BuscaMissa.Helpers
{
    public static class TelefoneHelper
    {
        public static bool ValidarCelular(string telefone)
        {
            string pattern = @"^\d{11}$";
            return Regex.IsMatch(telefone, pattern);
        }

        public static string NormalizarTelefone(string telefone)
        {
            return Regex.Replace(telefone, @"[^\d]", "");
        }
    }
}