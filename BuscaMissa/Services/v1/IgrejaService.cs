using BuscaMissa.Context;
using BuscaMissa.DTOs.EnderecoDto;
using BuscaMissa.DTOs.IgrejaDto;
using BuscaMissa.DTOs.MissaDto;
using BuscaMissa.DTOs.PaginacaoDto;
using BuscaMissa.DTOs.UsuarioDto;
using BuscaMissa.Helpers;
using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services.v1
{
    public class IgrejaService(
        ApplicationDbContext context, 
        ILogger<IgrejaService> logger, 
        IgrejaTemporariaService igrejaTemporariaService, 
        ImagemService imagemService)
    {
        public async Task<Igreja?> BuscarPorIdAsync(int id)
        {
            try
            {
                return await context.Igrejas
                    .Include(igreja => igreja.Endereco)
                    .Include(x => x.Usuario)
                    .Include(x => x.Missas)
                    .Include(igreja => igreja.Contato)
                    .Include(igreja => igreja.RedesSociais)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(igreja => igreja.Id == id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching Igreja with ID {Id}", id);
                throw;
            }
        }

        public async Task<IgrejaResponse?> BuscarPorCepAsync(string cep)
        {
            try
            {
                var model = await context.Igrejas
                    .Include(igreja => igreja.Endereco)
                    .Include(x => x.Usuario)
                    .Include(x => x.Missas)
                    .Include(Igreja => Igreja.Contato)
                    .Include(Igreja => Igreja.RedesSociais)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Endereco.Cep == CepHelper.FormatarCep(cep));

                if (model == null) return null;
                return (IgrejaResponse)model;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching Igreja with CEP {Cep}", cep);
                throw;
            }
        }

        public async Task<Igreja> InserirAsync(CriacaoIgrejaRequest request)
        {
            try
            {
                var model = (Igreja)request;
                model.NomeUnico = await GerarSlugUnicoAsync(IgrejaHelper.CriarNomeUnico(request));
                // Missas cadastradas por usuário nascem validadas
                foreach (var missa in model.Missas)
                {
                    missa.FontePrincipal = Enums.FontePrincipalEnum.Usuario;
                    missa.UltimaValidacao = DateTime.UtcNow;
                }
                context.Igrejas.Add(model);
                await context.SaveChangesAsync();
                return model;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while insering Igreja {IgrejaRequest}", request);
                throw;
            }
        }

        private async Task<string> GerarSlugUnicoAsync(string baseSlug)
        {
            var sufixo = 1;
            var slug = baseSlug;
            while (await context.Igrejas.AnyAsync(x => x.NomeUnico == slug))
            {
                sufixo++;
                slug = IgrejaHelper.CriarNomeUnicoComSufixo(baseSlug, sufixo);
            }
            return slug;
        }

        public async Task<bool> AtivarAsync(Controle model, Usuario usuario)
        {
            try
            {
                model.Igreja!.Ativo = true;
                model.Igreja.Alteracao = DateTime.Now;
                model.Igreja.Usuario = usuario;
                model.Igreja.UsuarioId = usuario.Id;
                model.Status = Enums.StatusEnum.Finalizado;
                context.Controles.Update(model);
                var resultado = await context.SaveChangesAsync();
                return resultado > 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while activate Igreja {Igreja}", model);
                throw;
            }
        }

        public async Task<bool> AtivarAsync(int igrejaId, int usuarioId)
        {
            try
            {
                var model = await context.Controles
                    .Include(x => x.Igreja)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.IgrejaId == igrejaId);

                if (model == null) return false;

                model.Igreja!.Ativo = true;
                model.Igreja.Alteracao = DateTime.Now;
                model.Igreja.UsuarioId = usuarioId;
                model.Status = Enums.StatusEnum.Finalizado;
                context.Controles.Update(model);
                var resultado = await context.SaveChangesAsync();
                return resultado > 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while activate Igreja {Igreja}", igrejaId);
                throw;
            }
        }
        
       
        public async Task<Paginacao<IgrejaResponse>> BuscarPorFiltros(FiltroIgrejaRequest filtro)
        {
            try
            {
                var query = context.Igrejas
                .Include(x => x.Endereco)
                .Include(x => x.Usuario)
                .Include(Igreja => Igreja.Contato)
                .Include(Igreja => Igreja.RedesSociais)
                .Include(x => x.Denuncia)
                .AsNoTracking()
                .Where(x =>
                    x.Endereco.Uf == filtro.Uf.ToUpper()
                    && x.Ativo == filtro.Ativo)
                .AsQueryable();

                if (!string.IsNullOrEmpty(filtro.Localidade))
                    query = query.Where(x => x.Endereco.Localidade == filtro.Localidade);

                if (!string.IsNullOrEmpty(filtro.Bairro))
                    query = query.Where(x => x.Endereco.Bairro == filtro.Bairro);

                if (!string.IsNullOrEmpty(filtro.Nome))
                    query = query.Where(x => x.Nome.ToUpper().Contains(filtro.Nome.ToUpper()));

                if (filtro.DiaDaSemana is not null)
                    query = query.Where(x => x.Missas.Any(y => y.DiaSemana == filtro.DiaDaSemana));

                if (!string.IsNullOrEmpty(filtro.Horario))
                    query = query.Where(x => x.Missas.Any(y => y.Horario == filtro.HorarioMissa));


                var aux = query.Select(x => new IgrejaResponse()
                {
                    Id = x.Id,
                    Nome = x.Nome,
                    NomeUnico = x.NomeUnico,
                    Endereco = (EnderecoIgrejaResponse)x.Endereco,
                    Usuario = x.Usuario == null ? null : (UsuarioDtoResponse)x.Usuario,
                    Alteracao = x.Alteracao,
                    Ativo = x.Ativo,
                    Criacao = x.Criacao,
                    ImagemUrl = x.ImagemUrl == null ? null: imagemService.ObterUrlAzureBlob($"igreja/{x.ImagemUrl!}"),
                    Paroco = x.Paroco,
                    Missas = x.Missas.Select(m => (MissaResponse)m).ToList(),
                    Contato = x.Contato == null ? null : (IgrejaContatoResponse)x.Contato,
                    RedesSociais = x.RedesSociais == null ? Array.Empty<IgrejaRedesSociaisResponse>() : x.RedesSociais.Select(r => (IgrejaRedesSociaisResponse)r).ToList(),
                    Denuncia = x.Denuncia == null ? null : string.IsNullOrEmpty(x.Denuncia.AcaoRealizada) ? (DenunciarIgrejaAdminResponse)x.Denuncia : null
                });

                var resultado = await aux.PaginacaoAsync(filtro.Paginacao.PageIndex, filtro.Paginacao.PageSize);
                return resultado;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Paginacao<IgrejaResponse>> BuscarPorFiltrosAsync(FiltroIgrejaAdminRequest filtro)
        {
            try
            {
                var query = context.Igrejas
                .Include(x => x.Endereco)
                .Include(x => x.Usuario)
                .Include(Igreja => Igreja.Contato)
                .Include(Igreja => Igreja.RedesSociais)
                .Include(x => x.Denuncia)
                .AsNoTracking()
                .Where(x =>
                    x.Ativo == filtro.Ativo)
                .AsQueryable();

                if (!string.IsNullOrEmpty(filtro.Uf))
                    query = query.Where(x => x.Endereco.Uf == filtro.Uf.ToUpper());

                if (!string.IsNullOrEmpty(filtro.Localidade))
                    query = query.Where(x => x.Endereco.Localidade == filtro.Localidade);

                if (!string.IsNullOrEmpty(filtro.Bairro))
                    query = query.Where(x => x.Endereco.Bairro == filtro.Bairro);

                if (!string.IsNullOrEmpty(filtro.Nome))
                    query = query.Where(x => x.Nome.ToUpper().Contains(filtro.Nome.ToUpper()));

                if (filtro.DiaDaSemana is not null)
                    query = query.Where(x => x.Missas.Any(y => y.DiaSemana == filtro.DiaDaSemana));

                if (!string.IsNullOrEmpty(filtro.Horario))
                    query = query.Where(x => x.Missas.Any(y => y.Horario == filtro.HorarioMissa));

                if(filtro.Denuncia)
                    query = query.Where(x => x.Denuncia != null && string.IsNullOrEmpty(x.Denuncia.AcaoRealizada));

                var aux = query.Select(x => new IgrejaResponse()
                {
                    Id = x.Id,
                    Nome = x.Nome,
                    NomeUnico = x.NomeUnico,
                    Endereco = (EnderecoIgrejaResponse)x.Endereco,
                    Usuario = x.Usuario == null ? null : (UsuarioDtoResponse)x.Usuario,
                    Alteracao = x.Alteracao,
                    Ativo = x.Ativo,
                    Criacao = x.Criacao,
                    ImagemUrl = x.ImagemUrl == null ? null: imagemService.ObterUrlAzureBlob($"igreja/{x.ImagemUrl!}"),
                    Paroco = x.Paroco,
                    Missas = x.Missas.Select(m => (MissaResponse)m).ToList(),
                    Contato = x.Contato == null ? null : (IgrejaContatoResponse)x.Contato,
                    RedesSociais = x.RedesSociais == null ? Array.Empty<IgrejaRedesSociaisResponse>() : x.RedesSociais.Select(r => (IgrejaRedesSociaisResponse)r).ToList(),
                    Denuncia = x.Denuncia == null ? null : string.IsNullOrEmpty(x.Denuncia.AcaoRealizada) ? (DenunciarIgrejaAdminResponse)x.Denuncia : null
                });

                var resultado = await aux.PaginacaoAsync(filtro.Paginacao.PageIndex, filtro.Paginacao.PageSize);
                return resultado;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Igreja> EditarAsync(Igreja model)
        {
            try
            {
                model.Alteracao = DateTime.Now;
                context.Igrejas.Update(model);
                await context.SaveChangesAsync();
                return model;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while editing Igreja {Igreja}", model);
                throw;
            }
        }

        public async Task<Igreja> EditarAsync(Igreja model, AtualicaoIgrejaRequest request)
        {
            try
            {
                model.Alteracao = DateTime.Now;
                if(request.Paroco is not null)
                    model.Paroco = request.Paroco;

                var listExcluir = model.Missas.Where(x => !request.Missas.Any(y => y.Id == x.Id)).ToList();
                await RemoverMissaAsync(listExcluir);

                foreach (var item in request.Missas)
                {
                    var temMissa = model.Missas.FirstOrDefault(x => x.Id == item.Id);
                    if(temMissa is not null)
                    {
                        temMissa.DiaSemana = item.DiaSemana;
                        temMissa.Horario = item.HorarioMissa;
                        temMissa.Observacao = item.Observacao;
                        // Edição pelo usuário = horário verificado agora
                        temMissa.FontePrincipal = Enums.FontePrincipalEnum.Usuario;
                        temMissa.UltimaValidacao = DateTime.UtcNow;
                    }
                    else
                    {
                        var nova = (Missa)item!;
                        nova.FontePrincipal = Enums.FontePrincipalEnum.Usuario;
                        nova.UltimaValidacao = DateTime.UtcNow;
                        model.Missas.Add(nova);
                    }
                }

                context.Igrejas.Update(model);
                await context.SaveChangesAsync();
                return model;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while editing Igreja {Igreja}", model);
                throw;
            }
        }

        public async Task<Igreja> EditarAsync(Igreja model, AtualicaoIgrejaAdminRequest request)
        {
            try
            {
                model.Alteracao = DateTime.Now;
                if(request.Nome is not null)
                    model.Nome = request.Nome;

                if(request.Paroco is not null)
                    model.Paroco = request.Paroco;

                var listExcluir = model.Missas.Where(x => !request.Missas.Any(y => y.Id == x.Id)).ToList();
                await RemoverMissaAsync(listExcluir);

                foreach (var item in request.Missas)
                {
                    var temMissa = model.Missas.FirstOrDefault(x => x.Id == item.Id);
                    if(temMissa is not null)
                    {
                        temMissa.DiaSemana = item.DiaSemana;
                        temMissa.Horario = item.HorarioMissa;
                        temMissa.Observacao = item.Observacao;
                        // Edição pelo admin = horário verificado agora
                        temMissa.FontePrincipal = Enums.FontePrincipalEnum.Usuario;
                        temMissa.UltimaValidacao = DateTime.UtcNow;
                    }
                    else
                    {
                        var nova = (Missa)item!;
                        nova.FontePrincipal = Enums.FontePrincipalEnum.Usuario;
                        nova.UltimaValidacao = DateTime.UtcNow;
                        model.Missas.Add(nova);
                    }
                }

                context.Igrejas.Update(model);
                await context.SaveChangesAsync();
                return model;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while editing Igreja {Igreja}", model);
                throw;
            }
        }

        private async Task RemoverMissaAsync(IList<Missa> missas)
        {
            context.Missas.RemoveRange(missas);
            await context.SaveChangesAsync();
        }

        public async Task<bool> EditarPorTemporariaAsync(Igreja igreja, AtualizacaoIgrejaResponse atualizacao)
        {
            var strategy = context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    context.Missas.RemoveRange(igreja.Missas);
                    context.Missas.AddRange(atualizacao.MissasTemporaria.Select(x => new Missa()
                    {
                        DiaSemana = x.DiaSemana,
                        Horario = TimeSpan.Parse(x.Horario),
                        IgrejaId = igreja.Id,
                        Observacao = x.Observacao
                    }));
                    igreja.Paroco = atualizacao.Paroco;
                    igreja.Alteracao = DateTime.Now;

                    if(atualizacao.ImagemUrl != null)
                    {
                        var nome = $"{igreja.Id}{ImageHelper.BuscarExtensao(atualizacao.ImagemUrl)}";
                        imagemService.UploadAzure(atualizacao.ImagemUrl, "igreja", nome);
                        igreja.ImagemUrl = nome;
                    }

                    // Delete IgrejaTemporarias and MissasTemporarias
                    var deleteIgrejaResult = await igrejaTemporariaService.DeletaIgrejaAsync(igreja.Id);
                    var deleteMissasResult = await igrejaTemporariaService.DeletaMissasTemporarias(igreja.Id);

                    if (!deleteIgrejaResult || !deleteMissasResult)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }
                    
                    //inserir nome unico
                    if (string.IsNullOrWhiteSpace(igreja.NomeUnico))
                    {
                        igreja.NomeUnico = IgrejaHelper.CriarNomeUnico(igreja);
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while editing Igreja {Igreja}", igreja);
                    await transaction.RollbackAsync();
                    return false;
                }
            });
        }
    
        public InformacoesGeraisResponse InformacoesGeraisResponse()
        {
            try
            {
                return new InformacoesGeraisResponse{
                    QuantidadesIgrejas = context.Igrejas.AsNoTracking().Count(x => x.Ativo),
                    QuantidadeMissas = context.Missas.Include(x => x.Igreja).AsNoTracking().Count(x => x.Igreja.Ativo),
                    QuantidadeIgrejaDenunciaNaoAtendida = context.IgrejaDenuncias.AsNoTracking().Count(x => string.IsNullOrEmpty(x.AcaoRealizada)),
                    QuantidadeSolicitacoesNaoAtendida = context.Solicitacoes.AsNoTracking().Count(x => !x.Resolvido),
                    QuantidadeDeUsuarios = context.Usuarios.AsNoTracking().Count(x => x.Perfil != Enums.PerfilEnum.Admin)
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while getting InformacoesGeraisResponse");
                throw;
            }
        }

        
        
    
    }
}