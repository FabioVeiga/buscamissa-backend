
using BuscaMissa.Enums;
using BuscaMissa.Models;

namespace BuscaMissa.Context
{
    public class DatabaseSeeder
    {
        private static readonly string emailAdmin = "suporte@buscamissa.com.br";
        private static readonly string nomeAdmin = "Busca Missa - Administrador";

        public static void Seed(ApplicationDbContext context, string senhaAdmin)
        {
            var temUsuarioAdmin = context.Usuarios.Any(u => u.Email == emailAdmin && u.Perfil == PerfilEnum.Admin);
            if (!temUsuarioAdmin)
            {
                var usuario = new Usuario
                {
                    AceitarPromocao = true,
                    AceitarTermo = true,
                    Email = emailAdmin,
                    Nome = nomeAdmin,
                    Perfil = PerfilEnum.Admin,
                    Senha = Helpers.SenhaHelper.Encriptar(senhaAdmin)
                };
                context.Usuarios.Add(usuario);
                context.SaveChanges();
            }
        }
    }
}