using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

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

    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
             IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 23)));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}




