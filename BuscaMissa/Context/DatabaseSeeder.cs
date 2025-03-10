
using BuscaMissa.Enums;
using BuscaMissa.Models;

namespace BuscaMissa.Context
{
    public class DatabaseSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        if (!context.Usuarios.Any(u => u.Email == "droidbinho@gmail.com")){
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