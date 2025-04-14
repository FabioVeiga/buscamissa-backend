using System.Text;
using Azure.Identity;
using BuscaMissa.Context;
using BuscaMissa.DTOs;
using BuscaMissa.DTOs.SettingsDto;
using BuscaMissa.Services;
using MailerSendNetCore.Common;
using MailerSendNetCore.Common.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;
System.Console.WriteLine($"ASPNETCORE_ENVIRONMENT: {env}");
string keyVaultUri = Environment.GetEnvironmentVariable("KeyVaultUri") ?? throw new ArgumentNullException("KeyVaultUri must be provided.");
var secret = Environment.GetEnvironmentVariable("SecretApp");
var key = Encoding.ASCII.GetBytes(secret!);

builder.Configuration.AddAzureKeyVault(
    new Uri(keyVaultUri),
    new DefaultAzureCredential()
);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration["AzureSqlConnection"]));

builder.Services.AddScoped<CodigoValidacaoService>();
builder.Services.AddScoped<ControleService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<EnderecoService>();
builder.Services.AddScoped<IgrejaService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<ImagemService>();
builder.Services.AddHttpClient<ViaCepService>();
builder.Services.AddScoped<IgrejaTemporariaService>();
builder.Services.AddScoped<ContatoService>();
builder.Services.AddScoped<IgrejaDenunciaService>();
builder.Services.AddScoped<SolicitacaoService>();
builder.Services.AddScoped<RedeSociaisService>();
builder.Services.AddScoped<ContribuidoresService>();
builder.Services.Configure<SettingCodigoValidacao>(builder.Configuration.GetSection("SettingCodigoValidacao"));
builder.Services.AddMailerSendEmailClient(builder.Configuration.GetSection("MailerSend"));
builder.Services.AddMailerSendEmailClient(options =>
{
    options.ApiUrl = builder.Configuration["MailerSend:ApiUrl"];
    options.ApiToken = builder.Configuration["MailerSendApiToken"];
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Busca Missa", Version = "v1" });

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<SettingCodigoValidacao>(builder.Configuration.GetSection("MailerSendEmailSetting"));
builder.Services.Configure<S3BucketSetting>(builder.Configuration.GetSection("S3BucketSetting"));
builder.Services.Configure<AzureBlobStorage>(builder.Configuration.GetSection("AzureBlobStorage"));
Environment.SetEnvironmentVariable("AzureBlobStorage",builder.Configuration["AzureBlobStorage"]);

// Adicione o serviço CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost4200", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();

        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Aplica migrações automaticamente (opcional)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    DatabaseSeeder.Seed(context);
}

// Use o CORS
app.UseCors("AllowLocalhost4200");

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = string.Empty;
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
});


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
