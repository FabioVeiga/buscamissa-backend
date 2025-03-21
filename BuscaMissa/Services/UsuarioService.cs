using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BuscaMissa.Context;
using BuscaMissa.DTOs.PaginacaoDto;
using BuscaMissa.DTOs.UsuarioDto;
using BuscaMissa.Helpers;
using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BuscaMissa.Services
{
    public class UsuarioService(ApplicationDbContext context, ILogger<UsuarioService> logger)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<UsuarioService> _logger = logger;

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

        public bool Autenticar(LoginRequest request, Usuario usuario)
        {
            if(!request.Email.Contains(usuario.Email))
                return false;
            if(!SenhaHelper.Validar(request.Senha, usuario.Senha))
                return false;
            return true;
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
            var secret = Environment.GetEnvironmentVariable("SecretApp");
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
                AceitarTermo = x.AceitarTermo
            })
            .AsNoTracking()
            .AsQueryable();
            if (filtro.Nome != null)
            query = query.Where(x => x.Nome.Contains(filtro.Nome));
            if (filtro.Email != null)
            query = query.Where(x => x.Email.Contains(filtro.Email));

            var resultado = await query.PaginacaoAsync(filtro.Paginacao.PageIndex, filtro.Paginacao.PageSize);
            return resultado;
        }
    
    }
}

