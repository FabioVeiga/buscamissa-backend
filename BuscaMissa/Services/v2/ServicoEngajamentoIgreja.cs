using BuscaMissa.Context;
using BuscaMissa.DTOs.PaginacaoDto;
using BuscaMissa.DTOs.v2.IgrejaDto;
using BuscaMissa.Helpers;
using BuscaMissa.Models;
using BuscaMissa.Util;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services.v2;

public class ServicoEngajamentoIgreja(
    ApplicationDbContext context,
    ServicoModeracaoComentarios moderacao)
{
    public async Task Curtir(CurtidaRequest request, string ip)
    {
        var hash = GeradorHash.Gerar(request.Fingerprint);

        var jaCurtiu = await context.CurtidasIgreja
            .AnyAsync(x =>
                x.IgrejaId == request.IgrejaId &&
                (x.HashFingerprint == hash || x.EnderecoIp == ip));

        if (jaCurtiu)
            throw new ApplicationException("Usuário já curtiu esta igreja.");

        var curtida = new CurtidaIgreja
        {
            Id = Guid.NewGuid(),
            IgrejaId = request.IgrejaId,
            HashFingerprint = hash,
            EnderecoIp = ip,
            DataCriacao = DateTime.UtcNow
        };

        context.CurtidasIgreja.Add(curtida);

        await AtualizarCurtidas(request.IgrejaId);

        await context.SaveChangesAsync();
    }

    public async Task AvaliarAsync(AvaliacaoRequest request)
    {
        var hash = GeradorHash.Gerar(request.Fingerprint);

        var jaAvaliou = await context.AvaliacoesIgreja
            .AnyAsync(x =>
                x.IgrejaId == request.IgrejaId &&
                x.HashFingerprint == hash);

        if (jaAvaliou)
            throw new ApplicationException("Usuário já avaliou esta igreja.");

        var avaliacao = new AvaliacaoIgreja
        {
            Id = Guid.NewGuid(),
            IgrejaId = request.IgrejaId,
            Nota = request.Nota,
            HashFingerprint = hash,
            DataCriacao = DateTime.UtcNow
        };

        context.AvaliacoesIgreja.Add(avaliacao);

        await AtualizarAvaliacoes(request.IgrejaId);

        await context.SaveChangesAsync();
    }

    public async Task ComentarAsync(ComentarioRequest request, string ip)
    {
        var hash = GeradorHash.Gerar(request.Fingerprint);
        
        var limiteSpam = DateTime.UtcNow.AddMinutes(-2);

        var spam = await context.ComentariosIgreja
            .AnyAsync(x =>
                (x.HashFingerprint == hash || x.EnderecoIp == ip) &&
                x.DataCriacao >= limiteSpam);

        if (spam)
            throw new ApplicationException("Aguarde um pouco antes de comentar novamente.");

        var limite = DateTime.UtcNow.AddDays(-1);

        var jaComentou = await context.ComentariosIgreja
            .AnyAsync(x =>
                x.IgrejaId == request.IgrejaId &&
                (x.HashFingerprint == hash || x.EnderecoIp == ip) &&
                x.DataCriacao >= limite);

        if (jaComentou)
            throw new ApplicationException("Você já comentou nesta igreja hoje.");

        var validacao = moderacao.Validar(request.Comentario);

        var comentario = new ComentarioIgreja
        {
            Id = Guid.NewGuid(),
            IgrejaId = request.IgrejaId,
            Nome = request.Nome,
            Comentario = request.Comentario,
            HashFingerprint = hash,
            EnderecoIp = ip,
            Aprovado = validacao.permitido,
            MotivoBloqueio = validacao.motivo,
            DataCriacao = DateTime.UtcNow
        };

        context.ComentariosIgreja.Add(comentario);

        if (validacao.permitido)
        {
            await AtualizarComentarios(request.IgrejaId);
        }

        await context.SaveChangesAsync();
    }

    public async Task RegistrarVisualizacao(int igrejaId, string fingerprint, string ip)
    {
        var hash = GeradorHash.Gerar(fingerprint);

        var ultimaVisualizacao = await context.VisualizacoesIgreja
            .Where(x => x.IgrejaId == igrejaId && x.HashFingerprint == hash)
            .OrderByDescending(x => x.DataCriacao)
            .FirstOrDefaultAsync();

        if (ultimaVisualizacao != null &&
            ultimaVisualizacao.DataCriacao > DateTime.UtcNow.AddMinutes(-30))
            return;

        var view = new VisualizacaoIgreja
        {
            Id = Guid.NewGuid(),
            IgrejaId = igrejaId,
            HashFingerprint = hash,
            EnderecoIp = ip,
            DataCriacao = DateTime.UtcNow
        };

        context.VisualizacoesIgreja.Add(view);

        await AtualizarVisualizacoes(igrejaId);

        await context.SaveChangesAsync();
    }

    public async Task<EngajamentoResponse> ObterEstatisticas(int igrejaId)
    {
        var stats = await context.EstatisticasEngajamentoIgreja
            .FirstOrDefaultAsync(x => x.IgrejaId == igrejaId);

        if (stats == null)
        {
            return new EngajamentoResponse();
        }

        return new EngajamentoResponse
        {
            Curtidas = stats.TotalCurtidas,
            Visualizacoes = stats.TotalVisualizacoes,
            MediaAvaliacoes = stats.MediaAvaliacoes,
            TotalAvaliacoes = stats.TotalAvaliacoes,
            TotalComentarios = stats.TotalComentarios
        };
    }

    public async Task<Paginacao<ComentarioResponse>> ObterComentarios(int igrejaId, PaginacaoRequest paginacao)
    {
        var query =  context.ComentariosIgreja
            .Where(x => x.IgrejaId == igrejaId && x.Aprovado)
            .OrderByDescending(x => x.DataCriacao)
            .Select(x => new ComentarioResponse
            {
                Nome = x.Nome,
                Comentario = x.Comentario,
                DataCriacao = x.DataCriacao
            })
            .AsQueryable();
        
        var resultado = await query.PaginacaoAsync(paginacao.PageIndex, paginacao.PageSize);
        return resultado;
    }

    public async Task<List<EstatisticasEngajamentoIgreja>> ObterTrending()
    {
        return await context.EstatisticasEngajamentoIgreja
            .Include(x => x.Igreja)
            .OrderByDescending(x =>
                (x.TotalCurtidas * 2) +
                (x.TotalAvaliacoes * 3) +
                (x.TotalVisualizacoes))
            .Take(10)
            .ToListAsync();
    }

    private async Task AtualizarCurtidas(int igrejaId)
    {
        var stats = await ObterOuCriarEstatisticas(igrejaId);
        stats.TotalCurtidas += 1;
        stats.UltimaAtualizacao = DateTime.UtcNow;
    }

    private async Task AtualizarComentarios(int igrejaId)
    {
        var stats = await ObterOuCriarEstatisticas(igrejaId);
        stats.TotalComentarios += 1;
        stats.UltimaAtualizacao = DateTime.UtcNow;
    }

    private async Task AtualizarVisualizacoes(int igrejaId)
    {
        var stats = await ObterOuCriarEstatisticas(igrejaId);
        stats.TotalVisualizacoes += 1;
        stats.UltimaAtualizacao = DateTime.UtcNow;
    }

    private async Task AtualizarAvaliacoes(int igrejaId)
    {
        var stats = await ObterOuCriarEstatisticas(igrejaId);

        stats.TotalAvaliacoes = await context.AvaliacoesIgreja
            .CountAsync(x => x.IgrejaId == igrejaId);

        stats.MediaAvaliacoes = await context.AvaliacoesIgreja
            .Where(x => x.IgrejaId == igrejaId)
            .Select(x => (double?)x.Nota)
            .AverageAsync() ?? 0;

        stats.UltimaAtualizacao = DateTime.UtcNow;
    }

    private async Task<EstatisticasEngajamentoIgreja> ObterOuCriarEstatisticas(int igrejaId)
    {
        var stats = await context.EstatisticasEngajamentoIgreja
            .FirstOrDefaultAsync(x => x.IgrejaId == igrejaId);

        if (stats != null)
            return stats;

        stats = new EstatisticasEngajamentoIgreja
        {
            IgrejaId = igrejaId,
            TotalCurtidas = 0,
            TotalAvaliacoes = 0,
            MediaAvaliacoes = 0,
            TotalComentarios = 0,
            TotalVisualizacoes = 0,
            UltimaAtualizacao = DateTime.UtcNow
        };

        context.EstatisticasEngajamentoIgreja.Add(stats);

        return stats;
    }
}