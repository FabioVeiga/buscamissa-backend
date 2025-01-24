using System.Text;
using BuscaMissa.Context;
using BuscaMissa.DTOs;
using BuscaMissa.Services;
using MailerSendNetCore.Common;
using MailerSendNetCore.Common.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var stringConnection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(stringConnection, 
        new MySqlServerVersion(new Version(8, 0, 23))));

builder.Services.AddScoped<CodigoValidacaoService>();
builder.Services.AddScoped<ControleService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<EnderecoService>();
builder.Services.AddScoped<IgrejaService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddHttpClient<ViaCepService>();
builder.Services.AddScoped<IgrejaTemporariaService>();
builder.Services.Configure<SettingCodigoValidacao>(builder.Configuration.GetSection("SettingCodigoValidacao"));
builder.Services.AddMailerSendEmailClient(builder.Configuration.GetSection("MailerSend"));
builder.Services.AddMailerSendEmailClient(options =>
{
    options.ApiUrl = builder.Configuration["MailerSend:ApiUrl"];
    options.ApiToken = builder.Configuration["MailerSend:ApiToken"];
});
builder.Services.AddMailerSendEmailClient(new MailerSendEmailClientOptions
{
    ApiUrl = builder.Configuration["MailerSend:ApiUrl"],
    ApiToken = builder.Configuration["MailerSend:ApiToken"]
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
var secret = Environment.GetEnvironmentVariable("SecretApp");
#pragma warning disable CS8604 // Possible null reference argument.
var key = Encoding.ASCII.GetBytes(secret);
#pragma warning restore CS8604 // Possible null reference argument.

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

/* builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["AzureStorage:ConnectionString"]);
}); */

builder.Services.Configure<SettingCodigoValidacao>(builder.Configuration.GetSection("MailerSendEmailSetting"));

// Adicione o serviço CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost4200", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

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
