using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Azure.Security.KeyVault.Secrets;
using BuscaMissa.Context;
using BuscaMissa.DTOs.PaginacaoDto;
using BuscaMissa.DTOs.UsuarioDto;
using BuscaMissa.Helpers;
using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BuscaMissa.Services.v1
{
    public class UsuarioService(ApplicationDbContext context, ILogger<UsuarioService> logger, IConfiguration configuration, SecretClient secretClient)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<UsuarioService> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly SecretClient _secretClient = secretClient;

        public async Task<Usuario> InserirAsync(CriacaoUsuarioRequest request)
        {
            Usuario usuario = (Usuario)request;
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<Usuario> InserirAsync(UsuarioGerarCodigoRequest request)
        {
            Usuario usuario = (Usuario)request;
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<bool> EditarAsync(Usuario usuario)
        {
           
            _context.Usuarios.Update(usuario);
            var resultado = await _context.SaveChangesAsync();
            return resultado > 0;
        }

        public bool Autenticar(LoginRequest request, Usuario usuario)
        {
            if(!request.Email.Contains(usuario.Email))
                return false;
            if(!SenhaHelper.Validar(request.Senha, usuario.Senha))
                return false;
            return true;
        }

        // Retorna false quando a senha atual informada não confere (uso pelo próprio usuário).
        public async Task<bool> TrocarSenhaAsync(Usuario usuario, string senhaAtual, string novaSenha)
        {
            if (!SenhaHelper.Validar(senhaAtual, usuario.Senha))
                return false;

            await AtualizarSenhaAsync(usuario, novaSenha);
            return true;
        }

        // Reset administrativo: não exige a senha atual (uso restrito ao perfil Admin).
        public async Task ResetarSenhaAsync(Usuario usuario, string novaSenha)
        {
            await AtualizarSenhaAsync(usuario, novaSenha);
        }

        private async Task AtualizarSenhaAsync(Usuario usuario, string novaSenha)
        {
            usuario.Senha = SenhaHelper.Encriptar(novaSenha);
            await EditarAsync(usuario);

            // A senha do Admin também vive como secret no Key Vault (fonte usada pelo
            // DatabaseSeeder na primeira criação do usuário); mantém os dois sincronizados.
            if (usuario.Email.Equals(Constants.Constants.EmailSuporte, StringComparison.OrdinalIgnoreCase))
                await _secretClient.SetSecretAsync(Constants.Constants.SecretNameSenhaAdmin, novaSenha);
        }

        public async Task<Usuario?> BuscarPorCodigo(int id)
        {
            return await _context
            .Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<Usuario?> BuscarPorEmailAsync(string email)
        {
            return await _context
            .Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email.ToUpper() == email.ToUpper());
        }
        public UsuarioResponse GerarTokenAsync(Usuario usuario)
        {
            var expiracao = usuario.Perfil == Enums.PerfilEnum.App ? DateTime.UtcNow.AddYears(10) : DateTime.UtcNow.AddHours(2);
            var secret = _configuration["SecretApp"];
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(secret!);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(
                    [
                        new(ClaimTypes.Email, usuario.Email),
                        new(ClaimTypes.Role, usuario.Perfil.ToString()),
                    ]),
                    Expires = expiracao,
                    Issuer = _configuration["Jwt:Issuer"] ?? "BuscaMissa",
                    Audience = _configuration["Jwt:Audience"] ?? "BuscaMissaApi",
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return new UsuarioResponse(){
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    AcessToken = new AcessToken(tokenHandler.WriteToken(token), expiracao)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{nameof(UsuarioService)}::{nameof(GerarTokenAsync)}] - Exception: {ex}");
                throw;
            }
        }
    
        public async Task<Paginacao<Usuario>> BuscarPorFiltroAsync(UsuarioFiltroRequest filtro)
        {
            var query = _context.Usuarios
            .Select(x => new Usuario{
                Id = x.Id,
                Nome = x.Nome,
                Email = x.Email,
                Perfil = x.Perfil,
                Criacao = x.Criacao,
                AceitarPromocao = x.AceitarPromocao,
                AceitarTermo = x.AceitarTermo,
                Bloqueado = x.Bloqueado,
                MotivoBloqueio = x.MotivoBloqueio,
                TotalIgrejas = x.Igrejas.Count
            })
            .AsNoTracking()
            .AsQueryable();

            if (filtro.Nome != null)
                query = query.Where(x => x.Nome.Contains(filtro.Nome));
            if (filtro.Email != null)
                query = query.Where(x => x.Email.Contains(filtro.Email));
            if(filtro.Bloqueado.HasValue)
                query = query.Where(x => x.Bloqueado == filtro.Bloqueado.Value);
            if(filtro.Perfil.HasValue)
                query = query.Where(x => x.Perfil == filtro.Perfil.Value);
            if(filtro.CriacaoInicio.HasValue)
                query = query.Where(x => x.Criacao >= filtro.CriacaoInicio.Value);
            if(filtro.CriacaoFim.HasValue)
                query = query.Where(x => x.Criacao <= filtro.CriacaoFim.Value);

            var resultado = await query.PaginacaoAsync(filtro.Paginacao.PageIndex, filtro.Paginacao.PageSize);
            return resultado;
        }

        public async Task<List<IgrejaResumoResponse>> BuscarIgrejasDoUsuarioAsync(int usuarioId)
        {
            return await _context.Igrejas
                .Where(x => x.UsuarioId == usuarioId)
                .Select(x => new IgrejaResumoResponse
                {
                    Id = x.Id,
                    Nome = x.Nome,
                    Uf = x.Endereco.Uf,
                    Localidade = x.Endereco.Localidade
                })
                .AsNoTracking()
                .ToListAsync();
        }

    }
}

