
using BuscaMissa.Enums;
using BuscaMissa.Models;

namespace BuscaMissa.Context
{
    public class DatabaseSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        var temUsuarioAdmin = context.Usuarios.Any(u => u.Email == "droidbinho@gmail.com" && u.Perfil == PerfilEnum.Admin);
        if (!temUsuarioAdmin){
            var usuario = new Usuario
            {
                AceitarPromocao = true,
                AceitarTermo = true,
                Email = "droidbinho@gmail.com",
                Nome = "Fabio Veiga - Admin",
                Perfil = PerfilEnum.Admin,
                Senha = Helpers.SenhaHelper.Encriptar("123456")
            };
            context.Usuarios.Add(usuario);
            context.SaveChanges();
        }
    }
}
}