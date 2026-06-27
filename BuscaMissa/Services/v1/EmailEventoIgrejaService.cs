using BuscaMissa.Context;
using BuscaMissa.DTOs.PaginacaoDto;
using BuscaMissa.DTOs.v1.EmailEventoIgrejaDto;
using BuscaMissa.Enums;
using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services.v1;

public class EmailEventoIgrejaService(
    ApplicationDbContext context,
    ILogger<EmailEventoIgrejaService> logger)
{
    public async Task<bool> EmailCriacaoJaEnviadoAsync(int igrejaId)
    {
        return await context.EmailEventosIgreja
            .AsNoTracking()
            .AnyAsync(x =>
                x.IgrejaId == igrejaId &&
                x.Tipo == TipoEmailEventoIgrejaEnum.Criacao &&
                x.Enviado);
    }
    
    public async Task<EmailEventoIgreja?> BuscarPorIdAsync(int id)
    {
        return await context.Set<EmailEventoIgreja>()
            .Include(x => x.Igreja)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<EmailEventoIgrejaResponse>> BuscarPorIgrejaIdAsync(int igrejaId)
    {
        return await context.Set<EmailEventoIgreja>()
            .AsNoTracking()
            .Include(x => x.Igreja)
            .Where(x => x.IgrejaId == igrejaId)
            .OrderByDescending(x => x.DataCriacao)
            .Select(x => (EmailEventoIgrejaResponse)x)
            .ToListAsync();
    }

    public async Task<Paginacao<EmailEventoIgrejaResponse>> BuscarPorFiltroAsync(FiltroEmailEventoIgrejaRequest filtro)
    {
        var query = context.Set<EmailEventoIgreja>()
            .AsNoTracking()
            .Include(x => x.Igreja)
            .AsQueryable();

        if (filtro.IgrejaId.HasValue)
            query = query.Where(x => x.IgrejaId == filtro.IgrejaId.Value);

        if (filtro.Tipo.HasValue)
            query = query.Where(x => x.Tipo == filtro.Tipo.Value);

        if (!string.IsNullOrWhiteSpace(filtro.EmailDestino))
            query = query.Where(x => x.EmailDestino.Contains(filtro.EmailDestino));

        if (filtro.Ativo.HasValue)
            query = query.Where(x => x.Ativo == filtro.Ativo.Value);

        if (filtro.Enviado.HasValue)
            query = query.Where(x => x.Enviado == filtro.Enviado.Value);

        if (filtro.DataCriacaoInicio.HasValue)
            query = query.Where(x => x.DataCriacao >= filtro.DataCriacaoInicio.Value);

        if (filtro.DataCriacaoFim.HasValue)
            query = query.Where(x => x.DataCriacao <= filtro.DataCriacaoFim.Value);

        var total = await query.CountAsync();

        var itens = await query
            .OrderByDescending(x => x.DataCriacao)
            .Skip((filtro.PageIndex - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .Select(x => (EmailEventoIgrejaResponse)x)
            .ToListAsync();

        return new Paginacao<EmailEventoIgrejaResponse>(
            filtro.PageIndex,
            filtro.PageSize,
            total,
            itens
        );
    }

    public async Task<EmailEventoIgreja> InserirAsync(CriarEmailEventoIgrejaRequest request)
    {
        try
        {
            var model = new EmailEventoIgreja
            {
                IgrejaId = request.IgrejaId,
                Tipo = request.Tipo,
                Assunto = request.Assunto,
                EmailDestino = request.EmailDestino,
                NomeDestino = request.NomeDestino,
                Html = request.Html,
                Ativo = request.Ativo,
                Enviado = request.Enviado,
                DataEnvio = request.DataEnvio,
                Observacao = request.Observacao,
                DataCriacao = DateTime.UtcNow
            };

            context.Set<EmailEventoIgreja>().Add(model);
            await context.SaveChangesAsync();

            return model;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao inserir evento de e-mail da igreja {IgrejaId}", request.IgrejaId);
            throw;
        }
    }

    public async Task<EmailEventoIgreja> EditarAsync(EmailEventoIgreja model, AtualizarEmailEventoIgrejaRequest request)
    {
        try
        {
            model.IgrejaId = request.IgrejaId;
            model.Tipo = request.Tipo;
            model.Assunto = request.Assunto;
            model.EmailDestino = request.EmailDestino;
            model.NomeDestino = request.NomeDestino;
            model.Html = request.Html;
            model.Ativo = request.Ativo;
            model.Enviado = request.Enviado;
            model.DataEnvio = request.DataEnvio;
            model.Observacao = request.Observacao;
            model.DataAlteracao = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return model;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao atualizar evento de e-mail {EmailEventoIgrejaId}", model.Id);
            throw;
        }
    }
}