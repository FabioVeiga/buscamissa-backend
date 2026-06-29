using System.Text;
using Azure.Identity;
using BuscaMissa.Context;
using BuscaMissa.DTOs;
using BuscaMissa.DTOs.SettingsDto;
using BuscaMissa.Middlewares;
using BuscaMissa.Services.v1;
using MailerSendNetCore.Common.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Asp.Versioning;
using BuscaMissa.Services.v2;
using IgrejaService = BuscaMissa.Services.v1.IgrejaService;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

#pragma warning disable CA2208 // Instantiate argument exceptions correctly
string keyVaultUri = Environment.GetEnvironmentVariable("KeyVaultUri") ?? throw new ArgumentNullException("KeyVaultUri must be provided.");
string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? throw new ArgumentNullException("ASPNETCORE_ENVIRONMENT must be provided.");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly

if (env.Equals("Production", StringComparison.OrdinalIgnoreCase))
{
    keyVaultUri = keyVaultUri.Replace("dev", "prod"); // Assign the modified value back to keyVaultUri
}
builder.Configuration.AddAzureKeyVault(
    new Uri(keyVaultUri),
    new DefaultAzureCredential()
);

var secret = builder.Configuration["SecretApp"];
var key = Encoding.ASCII.GetBytes(secret!);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration["MySqlConnection"], new MySqlServerVersion(new Version(8, 0, 33)), mySqlOptions =>
    {
        mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(70),
            errorNumbersToAdd: null
        );
    })
);

builder.Services.AddHttpClient();
builder.Services.AddHttpClient<ViaCepService>();
builder.Services.AddHttpClient<BuscaMissa.Services.GeocodingService>();
builder.Services.AddScoped<CodigoValidacaoService>();
builder.Services.AddScoped<ControleService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<EnderecoService>();
builder.Services.AddScoped<IgrejaService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<ImagemService>();
builder.Services.AddScoped<IgrejaTemporariaService>();
builder.Services.AddScoped<ContatoService>();
builder.Services.AddScoped<IgrejaDenunciaService>();
builder.Services.AddScoped<SolicitacaoService>();
builder.Services.AddScoped<RedeSociaisService>();
builder.Services.AddScoped<ContribuidoresService>();
builder.Services.AddScoped<BuscaMissa.Services.v2.IgrejaService>();
builder.Services.AddScoped<ProximasMissasService>();
builder.Services.AddScoped<ServicoModeracaoComentarios>();
builder.Services.AddScoped<ServicoEngajamentoIgreja>();
builder.Services.AddScoped<ConfiabilidadeService>();
builder.Services.AddScoped<EmailEventoIgrejaService>();


builder.Services.Configure<SettingCodigoValidacao>(builder.Configuration.GetSection("SettingCodigoValidacao"));
builder.Services.AddMailerSendEmailClient(builder.Configuration.GetSection("MailerSend"));
builder.Services.AddMailerSendEmailClient(options =>
{
    options.ApiUrl = builder.Configuration["MailerSend:ApiUrl"];
    options.ApiToken = builder.Configuration["MailerSendApiToken"];
});


builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        // Formata a versão como "'v'major.minor" (ex: v1, v2)
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Busca Missa", Version = "v1" });
    c.SwaggerDoc("v2", new OpenApiInfo { Title = "Busca Missa V2", Version = "v2" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
            "JWT Authorization Header - utilizado com Bearer Authentication.\r\n\r\n" +
            "Digite 'Bearer' [espaço] e então seu token no campo abaixo.\r\n\r\n" +
            "Exemplo (informar sem as aspas): 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// Adicionar autorização
builder.Services.AddAuthorization();

// Adicionar controladores e serviços do Swagger, se necessário
builder.Services.AddControllers();

builder.Services.Configure<SettingCodigoValidacao>(builder.Configuration.GetSection("MailerSendEmailSetting"));
builder.Services.Configure<AzureBlobStorage>(builder.Configuration.GetSection("AzureBlobStorage"));

// Adicione o serviço CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();
if (!env.Equals("Production", StringComparison.OrdinalIgnoreCase))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = string.Empty;
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Busca Missa v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Busca Missa v2");
    });
}

app.UseHttpsRedirection();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseRouting();
app.UseCors("AllowLocalhost");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
    DatabaseSeeder.Seed(context, builder.Configuration["SenhaAdmin"]!);
    BuscaMissa.Services.SlugBackfillService.Executar(context);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
