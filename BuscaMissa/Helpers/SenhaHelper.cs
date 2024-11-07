namespace BuscaMissa.Helpers
{
    public static class SenhaHelper
    {
        static readonly string salt = BCrypt.Net.BCrypt.GenerateSalt();

        public static string GerarSenhaTemporariaString()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        public static int GerarSenhaTemporariaInt()
        {
            return new Random().Next(100000, 999999);
        }

        public static string Encriptar(string input){
            return BCrypt.Net.BCrypt.HashPassword(input, salt);
        }
        
        public static bool Validar(string senhaDigitada, string senhaDB){
            return BCrypt.Net.BCrypt.Verify(senhaDigitada, senhaDB);
        }
    }

}