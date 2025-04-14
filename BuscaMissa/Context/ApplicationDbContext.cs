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
        public DbSet<IgrejaTemporaria> IgrejaTemporarias { get; set; }
        public DbSet<RedeSocial> RedesSociais { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<IgrejaDenuncia> IgrejaDenuncias { get; set; }
        public DbSet<Solicitacao> Solicitacoes { get; set; }
        public DbSet<Contribuidor> Contribuidores { get; set; }
    }

    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Console.WriteLine(env);
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

            var connectionString = configuration.GetConnectionString("AzureSqlConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'AzureSqlConnection' not found.");
            }
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}




