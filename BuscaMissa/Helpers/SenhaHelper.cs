namespace BuscaMissa.Helpers
{
    public static class SenhaHelper
    {
        public static string GerarSenhaTemporariaString()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        public static int GerarSenhaTemporariaInt()
        {
            return new Random().Next(100000, 999999);
        }

        public static string Encriptar(string input){
            // Gera um salt aleatório por hash (embutido no resultado). Nunca reutilizar
            // um salt fixo/estático — isso enfraquece o BCrypt.
            return BCrypt.Net.BCrypt.HashPassword(input);
        }
        
        public static bool Validar(string senhaDigitada, string senhaDB){
            return BCrypt.Net.BCrypt.Verify(senhaDigitada, senhaDB);
        }
    }

}