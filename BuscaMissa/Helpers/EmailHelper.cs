using System.Text.RegularExpressions;

namespace BuscaMissa.Helpers
{
    public static class EmailHelper
    {
        public static bool ValidarEmail(string email){
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }
    }
}