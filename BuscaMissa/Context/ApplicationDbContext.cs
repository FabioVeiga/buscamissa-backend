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
        public DbSet<CurtidaIgreja> CurtidasIgreja { get; set; }

        public DbSet<AvaliacaoIgreja> AvaliacoesIgreja { get; set; }

        public DbSet<ComentarioIgreja> ComentariosIgreja { get; set; }

        public DbSet<VisualizacaoIgreja> VisualizacoesIgreja { get; set; }

        public DbSet<EstatisticasEngajamentoIgreja> EstatisticasEngajamentoIgreja { get; set; }

        public DbSet<ConfirmacaoHorario> ConfirmacoesHorario { get; set; }
        public DbSet<EmailEventoIgreja> EmailEventosIgreja { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigurarIgreja(modelBuilder);
            ConfigurarEndereco(modelBuilder);
            ConfigurarCurtidas(modelBuilder);
            ConfigurarAvaliacoes(modelBuilder);
            ConfigurarComentarios(modelBuilder);
            ConfigurarVisualizacoes(modelBuilder);
            ConfigurarEstatisticas(modelBuilder);
            ConfigurarConfiabilidade(modelBuilder);
            ConfigurarEmailEventosIgreja(modelBuilder);
        }
        
        private void ConfigurarEmailEventosIgreja(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmailEventoIgreja>()
                .HasIndex(x => x.IgrejaId);

            modelBuilder.Entity<EmailEventoIgreja>()
                .HasIndex(x => x.Tipo);

            modelBuilder.Entity<EmailEventoIgreja>()
                .HasIndex(x => x.EmailDestino);

            modelBuilder.Entity<EmailEventoIgreja>()
                .HasIndex(x => x.DataCriacao);

            modelBuilder.Entity<EmailEventoIgreja>()
                .HasIndex(x => new { x.IgrejaId, x.Tipo });
        }

        private void ConfigurarConfiabilidade(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConfirmacaoHorario>()
                .HasIndex(x => new { x.IgrejaId, x.HashFingerprint })
                .IsUnique();

            modelBuilder.Entity<ConfirmacaoHorario>()
                .HasIndex(x => x.DataCriacao);
        }

        private void ConfigurarIgreja(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Igreja>()
                .HasIndex(x => x.NomeUnico)
                .IsUnique()
                .HasFilter("NomeUnico IS NOT NULL");

            // Slug local — usado nas URLs /paroquia/{uf}/{cidade}/{slug}
            modelBuilder.Entity<Igreja>()
                .HasIndex(x => x.Slug);
        }

        private void ConfigurarEndereco(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Endereco>()
                .HasIndex(x => new { x.Latitude, x.Longitude });

            // CidadeSlug — busca por cidade e URLs /missas/{uf}/{cidade}
            modelBuilder.Entity<Endereco>()
                .HasIndex(x => new { x.Uf, x.CidadeSlug });
        }
        
        private void ConfigurarEstatisticas(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EstatisticasEngajamentoIgreja>()
                .HasKey(x => x.IgrejaId);

            modelBuilder.Entity<EstatisticasEngajamentoIgreja>()
                .HasIndex(x => x.TotalCurtidas);

            modelBuilder.Entity<EstatisticasEngajamentoIgreja>()
                .HasIndex(x => x.MediaAvaliacoes);

            modelBuilder.Entity<EstatisticasEngajamentoIgreja>()
                .HasIndex(x => x.TotalVisualizacoes);
        }
        
        private void ConfigurarVisualizacoes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VisualizacaoIgreja>()
                .HasIndex(x => x.IgrejaId);

            modelBuilder.Entity<VisualizacaoIgreja>()
                .HasIndex(x => x.DataCriacao);

            modelBuilder.Entity<VisualizacaoIgreja>()
                .HasIndex(x => x.HashFingerprint);
        }
        
        private void ConfigurarComentarios(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ComentarioIgreja>()
                .HasIndex(x => x.IgrejaId);

            modelBuilder.Entity<ComentarioIgreja>()
                .HasIndex(x => x.Aprovado);

            modelBuilder.Entity<ComentarioIgreja>()
                .HasIndex(x => new { x.IgrejaId, x.Aprovado });
        }
        
        private void ConfigurarAvaliacoes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AvaliacaoIgreja>()
                .HasIndex(x => x.IgrejaId);

            modelBuilder.Entity<AvaliacaoIgreja>()
                .HasIndex(x => new { x.IgrejaId, x.HashFingerprint })
                .IsUnique();
        }
        
        private void ConfigurarCurtidas(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CurtidaIgreja>()
                .HasIndex(x => x.IgrejaId);

            modelBuilder.Entity<CurtidaIgreja>()
                .HasIndex(x => new { x.IgrejaId, x.HashFingerprint })
                .IsUnique();

            modelBuilder.Entity<CurtidaIgreja>()
                .HasIndex(x => x.EnderecoIp);
        }

    }
    

    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Console.WriteLine($"Executando em ambiente: {env}");

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.{env}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("MySqlConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'MySqlConnection' not found.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 33)));

            return new ApplicationDbContext(optionsBuilder.Options);
        }

    }
}




