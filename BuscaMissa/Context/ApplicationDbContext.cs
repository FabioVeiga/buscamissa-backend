using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Context
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<CodigoPermissao> CodigoPermissoes { get; set; }
        public DbSet<Contato> Contatos { get; set; }
        public DbSet<Controle> Controles { get; set; }
        public DbSet<Endereco> Enderecos { get; set; }
        public DbSet<Igreja> Igrejas { get; set; }
        public DbSet<Missa> Missas { get; set; }
        public DbSet<MissaTemporaria> MissasTemporarias { get; set; }
        public DbSet<RedeSocial> RedesSociais { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
    }
}




