using BuscaMissa.Context;
using BuscaMissa.DTOs.ControleDto;
using BuscaMissa.DTOs.EnderecoDto;
using BuscaMissa.DTOs.IgrejaDto;
using BuscaMissa.DTOs.MissaDto;
using BuscaMissa.DTOs.PaginacaoDto;
using BuscaMissa.Enums;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services.v1;

// Fila de "Aprovações Pendentes" do Admin — reaproveita Controle/IgrejaTemporaria/MissaTemporaria
// que já existem para o fluxo público de criação/alteração validado por e-mail. Não cria
// nenhuma tabela de log/histórico: só dá visibilidade e ações administrativas sobre o que
// já está pendente, chamando os mesmos métodos de serviço que o fluxo por token já usa.
public class AprovacaoService(
    ApplicationDbContext context,
    IgrejaService igrejaService,
    IgrejaTemporariaService igrejaTemporariaService,
    ILogger<AprovacaoService> logger)
{
    // Statuses que representam algo aguardando ação (nem concluído, nem rejeitado).
    private static readonly StatusEnum[] StatusPendentes =
    [
        StatusEnum.Igreja_Criacao,
        StatusEnum.Igreja_Criacao_Aguardando_Codigo_Validador,
        StatusEnum.Igreja_Atualizacao_Temporaria_Inserido,
        StatusEnum.Igreja_Atualizacao_Aguardando_Codigo_Validador,
    ];

    private static string ObterTipo(StatusEnum status) =>
        status is StatusEnum.Igreja_Criacao or StatusEnum.Igreja_Criacao_Aguardando_Codigo_Validador
            ? "Criacao"
            : "Alteracao";

    public async Task<Paginacao<ControlePendenteResponse>> BuscarPendentesAsync(FiltroControleRequest filtro)
    {
        try
        {
            var query = context.Controles
                .Include(x => x.Igreja)
                .ThenInclude(x => x!.Endereco)
                .Include(x => x.Igreja)
                .ThenInclude(x => x!.Usuario)
                .AsNoTracking()
                .AsQueryable();

            query = filtro.Status.HasValue
                ? query.Where(x => x.Status == filtro.Status.Value)
                : query.Where(x => StatusPendentes.Contains(x.Status));

            var total = await query.CountAsync();

            var itens = await query
                .OrderBy(x => x.DataCriacao)
                .Skip((filtro.Paginacao.PageIndex - 1) * filtro.Paginacao.PageSize)
                .Take(filtro.Paginacao.PageSize)
                .Select(x => new ControlePendenteResponse
                {
                    ControleId = x.Id,
                    Status = x.Status,
                    DataCriacao = x.DataCriacao,
                    IgrejaId = x.IgrejaId,
                    NomeIgreja = x.Igreja!.Nome,
                    Cidade = x.Igreja.Endereco.Localidade,
                    Uf = x.Igreja.Endereco.Uf,
                    Tipo = ObterTipo(x.Status),
                    UsuarioNome = x.Igreja.Usuario != null ? x.Igreja.Usuario.Nome : null,
                    UsuarioEmail = x.Igreja.Usuario != null ? x.Igreja.Usuario.Email : null,
                })
                .ToListAsync();

            return new Paginacao<ControlePendenteResponse>(
                filtro.Paginacao.PageIndex, filtro.Paginacao.PageSize, total, itens);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar fila de aprovações pendentes");
            throw;
        }
    }

    public async Task<ControleDetalheResponse?> ObterDetalheAsync(int controleId)
    {
        var controle = await context.Controles
            .Include(x => x.Igreja)
            .ThenInclude(x => x!.Endereco)
            .Include(x => x.Igreja)
            .ThenInclude(x => x!.Missas)
            .Include(x => x.Igreja)
            .ThenInclude(x => x!.Usuario)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == controleId);

        if (controle?.Igreja is null) return null;

        var tipo = ObterTipo(controle.Status);
        var response = new ControleDetalheResponse
        {
            ControleId = controle.Id,
            Status = controle.Status,
            DataCriacao = controle.DataCriacao,
            Tipo = tipo,
            IgrejaId = controle.IgrejaId,
            UsuarioNome = controle.Igreja.Usuario?.Nome,
            UsuarioEmail = controle.Igreja.Usuario?.Email,
        };

        if (tipo == "Criacao")
        {
            // Não há "antes" — a igreja proposta é a própria igreja (ainda inativa).
            response.DadosAtuais = null;
            response.DadosPropostos = new DadosComparacaoResponse
            {
                Nome = controle.Igreja.Nome,
                Paroco = controle.Igreja.Paroco,
                ImagemUrl = controle.Igreja.ImagemUrl,
                Endereco = (EnderecoIgrejaResponse)controle.Igreja.Endereco,
                Missas = controle.Igreja.Missas.Select(m => new MissaComparacaoItem
                {
                    DiaSemana = m.DiaSemana,
                    Horario = m.Horario.ToString(@"hh\:mm"),
                    Observacao = m.Observacao,
                }).ToList(),
            };
            return response;
        }

        // Alteração: "atual" é a igreja como está publicada hoje; "proposto" vem da
        // IgrejaTemporaria/MissaTemporaria (dados enviados pelo usuário, aguardando validação).
        response.DadosAtuais = new DadosComparacaoResponse
        {
            Nome = controle.Igreja.Nome,
            Paroco = controle.Igreja.Paroco,
            ImagemUrl = controle.Igreja.ImagemUrl,
            Missas = controle.Igreja.Missas.Select(m => new MissaComparacaoItem
            {
                DiaSemana = m.DiaSemana,
                Horario = m.Horario.ToString(@"hh\:mm"),
                Observacao = m.Observacao,
            }).ToList(),
        };

        var temporaria = await context.IgrejaTemporarias
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IgrejaId == controle.IgrejaId);

        var missasTemp = await context.MissasTemporarias
            .AsNoTracking()
            .Where(x => x.IgrejaId == controle.IgrejaId)
            .ToListAsync();

        response.DadosPropostos = new DadosComparacaoResponse
        {
            Nome = controle.Igreja.Nome,
            Paroco = temporaria?.Paroco,
            ImagemUrl = temporaria?.ImagemUrl,
            Missas = missasTemp.Select(m => new MissaComparacaoItem
            {
                DiaSemana = m.DiaSemana,
                Horario = m.Horario.ToString(@"hh\:mm"),
                Observacao = m.Observacao,
            }).ToList(),
        };

        return response;
    }

    /// <summary>Aprova como está — mesma finalização que o fluxo de token já faz, sem exigir um Usuario validado.</summary>
    public async Task<bool> AprovarAsync(int controleId)
    {
        var controle = await context.Controles
            .Include(x => x.Igreja)
            .ThenInclude(x => x!.Missas)
            .FirstOrDefaultAsync(x => x.Id == controleId);

        if (controle?.Igreja is null) return false;

        if (ObterTipo(controle.Status) == "Criacao")
        {
            controle.Igreja.Ativo = true;
            controle.Igreja.Alteracao = DateTime.Now;
        }
        else
        {
            var temporaria = await igrejaTemporariaService.BuscarPorIgrejaIdAsync(controle.Igreja.Id);
            if (temporaria is null) return false;

            var aplicado = await igrejaService.EditarPorTemporariaAsync(controle.Igreja, temporaria);
            if (!aplicado) return false;
        }

        controle.Status = StatusEnum.Finalizado;
        await context.SaveChangesAsync();
        return true;
    }

    /// <summary>Alteração: aplica os dados ajustados pelo Admin (em vez dos propostos originalmente) e finaliza.</summary>
    public async Task<bool> AjustarAlteracaoAsync(int controleId, AjustarAlteracaoRequest request)
    {
        var controle = await context.Controles
            .Include(x => x.Igreja)
            .ThenInclude(x => x!.Missas)
            .FirstOrDefaultAsync(x => x.Id == controleId);

        if (controle?.Igreja is null || ObterTipo(controle.Status) != "Alteracao") return false;

        var atualizacaoAjustada = new AtualizacaoIgrejaResponse
        {
            Id = controle.Igreja.Id,
            Nome = controle.Igreja.Nome,
            Paroco = request.Paroco,
            ImagemUrl = request.Imagem,
            MissasTemporaria = request.Missas.Select(m => new MissaResponse
            {
                DiaSemana = m.DiaSemana,
                Horario = m.Horario,
                Observacao = m.Observacao,
            }).ToList(),
        };

        var aplicado = await igrejaService.EditarPorTemporariaAsync(controle.Igreja, atualizacaoAjustada);
        if (!aplicado) return false;

        controle.Status = StatusEnum.Finalizado;
        await context.SaveChangesAsync();
        return true;
    }

    /// <summary>Recusa a pendência. Para alteração, limpa os dados temporários; a igreja publicada não é tocada.</summary>
    public async Task<bool> RejeitarAsync(int controleId)
    {
        var controle = await context.Controles.FirstOrDefaultAsync(x => x.Id == controleId);
        if (controle is null) return false;

        if (ObterTipo(controle.Status) == "Alteracao" && controle.IgrejaId.HasValue)
        {
            await igrejaTemporariaService.DeletaIgrejaAsync(controle.IgrejaId.Value);
            await igrejaTemporariaService.DeletaMissasTemporarias(controle.IgrejaId.Value);
        }

        controle.Status = StatusEnum.Rejeitado;
        await context.SaveChangesAsync();
        return true;
    }
}
