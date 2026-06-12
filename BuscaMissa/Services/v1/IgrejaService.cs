using BuscaMissa.Context;
using BuscaMissa.DTOs.EnderecoDto;
using BuscaMissa.DTOs.IgrejaDto;
using BuscaMissa.DTOs.MissaDto;
using BuscaMissa.DTOs.PaginacaoDto;
using BuscaMissa.DTOs.UsuarioDto;
using BuscaMissa.DTOs.v1.IgrejaDto;
using BuscaMissa.Enums;
using BuscaMissa.Helpers;
using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services.v1
{
    public class IgrejaService(
        ApplicationDbContext context,
        ILogger<IgrejaService> logger,
        IgrejaTemporariaService igrejaTemporariaService,
        ImagemService imagemService,
        ViaCepService viaCepService)
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

                // CidadeSlug (desnormalizado para URLs e busca por cidade)
                model.Endereco.CidadeSlug = IgrejaHelper.CriarCidadeSlug(model.Endereco.Localidade);

                // Slug local à cidade, único dentro de (Uf, CidadeSlug)
                model.Slug = await GerarSlugLocalUnicoAsync(
                    IgrejaHelper.CriarSlugLocal(model.Nome),
                    model.Endereco.Uf,
                    model.Endereco.CidadeSlug);

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

        public async Task<ImportacaoIgrejaLoteResponse> ImportarLoteAsync(ImportacaoIgrejaLoteRequest request)
        {
            var response = new ImportacaoIgrejaLoteResponse();
            var cepCache = new Dictionary<string, EnderecoViaCepResponse?>();

            for (var i = 0; i < request.Igrejas.Count; i++)
            {
                var item = request.Igrejas[i];
                var linha = i + 1;

                try
                {
                    var cepFormatado = CepHelper.FormatarCep(item.Cep);
                    string logradouro, bairro, localidade, uf, estado, regiao;

                    // Usa endereço do payload quando há cidade + UF — evita chamada ao ViaCEP
                    // (Logradouro pode vir vazio quando o ViaCEP não resolveu o CEP no cliente)
                    if (!string.IsNullOrWhiteSpace(item.Localidade) &&
                        !string.IsNullOrWhiteSpace(item.Uf))
                    {
                        logradouro = item.Logradouro ?? string.Empty;
                        bairro     = item.Bairro ?? string.Empty;
                        localidade = item.Localidade;
                        uf         = NormalizarUf(item.Uf);
                        estado     = item.Estado ?? string.Empty;
                        regiao     = item.Regiao ?? string.Empty;
                    }
                    else
                    {
                        if (!cepCache.TryGetValue(cepFormatado, out var viaCep))
                        {
                            viaCep = await viaCepService.ConsultarCepAsync(cepFormatado);
                            cepCache[cepFormatado] = viaCep;
                        }

                        if (viaCep == null)
                        {
                            response.Erros.Add(new ImportacaoErroItem
                            {
                                Linha = linha,
                                Nome = item.Nome,
                                Motivo = $"CEP {item.Cep} não encontrado"
                            });
                            continue;
                        }

                        logradouro = viaCep.Logradouro;
                        bairro     = viaCep.Bairro;
                        localidade = viaCep.Localidade;
                        uf         = NormalizarUf(viaCep.Uf);
                        estado     = viaCep.Estado;
                        regiao     = viaCep.Regiao;
                    }

                    var cidadeSlug = IgrejaHelper.CriarCidadeSlug(localidade);
                    var slugLocal = IgrejaHelper.CriarSlugLocal(item.Nome);
                    uf = NormalizarUf(uf);

                    var duplicata = await context.Igrejas
                        .AnyAsync(x =>
                            x.Slug == slugLocal &&
                            x.Endereco.CidadeSlug == cidadeSlug &&
                            x.Endereco.Uf == uf);

                    if (duplicata)
                    {
                        response.Puladas++;
                        continue;
                    }

                    var missas = item.Missas
                        .Select(m => new { Dia = MapearDiaSemana(m.DiaSemana), m.Horario, m.Observacao })
                        .Where(m => m.Dia.HasValue && TimeSpan.TryParse(m.Horario, out _))
                        .GroupBy(m => (m.Dia, m.Horario))
                        .Select(g => g.First())
                        .Select(m => new Missa
                        {
                            DiaSemana = m.Dia!.Value,
                            Horario = TimeSpan.Parse(m.Horario),
                            Observacao = string.IsNullOrWhiteSpace(m.Observacao) ? null : m.Observacao.Trim(),
                            FontePrincipal = FontePrincipalEnum.Usuario,
                            UltimaValidacao = DateTime.UtcNow
                        })
                        .ToList();

                    var igreja = new Igreja
                    {
                        Nome = item.Nome,
                        Paroco = item.Paroco,
                        Ativo = true,
                        Criacao = DateTime.Now,
                        Alteracao = DateTime.Now,
                        Missas = missas,
                        Endereco = new Endereco
                        {
                            Cep = cepFormatado,
                            Logradouro = logradouro,
                            Bairro = bairro,
                            Localidade = localidade,
                            CidadeSlug = cidadeSlug,
                            Uf = uf,
                            Estado = estado,
                            Regiao = regiao,
                            Numero = item.Numero
                        }
                    };

                    var baseNomeUnico = IgrejaHelper.CriarNomeUnico(igreja);
                    igreja.NomeUnico = await GerarSlugUnicoAsync(baseNomeUnico);
                    igreja.Slug = await GerarSlugLocalUnicoAsync(slugLocal, uf, cidadeSlug);

                    var contato = CriarContato(item);
                    if (contato != null) igreja.Contato = contato;

                    context.Igrejas.Add(igreja);
                    await context.SaveChangesAsync();

                    // Foto: baixa da URL informada e sobe para o blob (falha não invalida a igreja)
                    if (!string.IsNullOrWhiteSpace(item.ImagemUrl))
                    {
                        try
                        {
                            var nomeArquivo = await BaixarESalvarImagemAsync(item.ImagemUrl, igreja.Id);
                            if (nomeArquivo != null)
                            {
                                igreja.ImagemUrl = nomeArquivo;
                                await context.SaveChangesAsync();
                            }
                        }
                        catch (Exception exImg)
                        {
                            logger.LogWarning(exImg, "Falha ao baixar imagem da igreja {Nome}: {Url}", item.Nome, item.ImagemUrl);
                        }
                    }

                    response.Inseridas++;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Erro ao importar igreja linha {Linha}: {Nome}", linha, item.Nome);
                    response.Erros.Add(new ImportacaoErroItem
                    {
                        Linha = linha,
                        Nome = item.Nome,
                        Motivo = "Erro interno ao processar"
                    });
                }
            }

            return response;
        }

        private static readonly HttpClient _httpImagem = new() { Timeout = TimeSpan.FromSeconds(30) };

        // Baixa a imagem da URL e sobe para o blob na pasta "igreja". Retorna o nome do arquivo salvo.
        private async Task<string?> BaixarESalvarImagemAsync(string url, int igrejaId)
        {
            using var resposta = await _httpImagem.GetAsync(url);
            if (!resposta.IsSuccessStatusCode) return null;

            var bytes = await resposta.Content.ReadAsByteArrayAsync();
            if (bytes.Length == 0) return null;

            var contentType = resposta.Content.Headers.ContentType?.MediaType ?? string.Empty;
            var extensao = contentType switch
            {
                "image/png"  => ".png",
                "image/gif"  => ".gif",
                "image/bmp"  => ".bmp",
                "image/webp" => ".webp",
                _            => ".jpg"
            };

            var nomeArquivo = $"{igrejaId}{extensao}";
            imagemService.UploadAzure(Convert.ToBase64String(bytes), "igreja", nomeArquivo);
            return nomeArquivo;
        }

        // UF sempre com 2 dígitos maiúsculos
        private static string NormalizarUf(string? uf)
        {
            var limpo = (uf ?? string.Empty).Trim().ToUpper();
            return limpo.Length > 2 ? limpo[..2] : limpo;
        }

        private static Contato? CriarContato(ImportacaoIgrejaItemRequest item)
        {
            if (string.IsNullOrWhiteSpace(item.Email) &&
                string.IsNullOrWhiteSpace(item.Telefone) &&
                string.IsNullOrWhiteSpace(item.WhatsApp) &&
                string.IsNullOrWhiteSpace(item.Site))
                return null;

            var (ddd, tel) = ParseFone(item.Telefone);
            var (dddWa, wa) = ParseFone(item.WhatsApp);

            return new Contato
            {
                EmailContato = item.Email,
                DDD = ddd,
                Telefone = tel,
                DDDWhatsApp = dddWa,
                TelefoneWhatsApp = wa,
                Website = item.Site
            };
        }

        // Converte "(19) 3234-8269" → ("19", "32348269")
        private static (string? ddd, string? numero) ParseFone(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return (null, null);
            var digits = new string(raw.Where(char.IsDigit).ToArray());
            if (digits.Length < 10) return (null, null);
            var ddd = digits[..2];
            var numero = digits[2..];
            return (ddd, numero);
        }

        private static DiaDaSemanaEnum? MapearDiaSemana(string dia) => dia.Trim().ToLower() switch
        {
            "domingo" => DiaDaSemanaEnum.Domingo,
            "segunda" or "segunda-feira" => DiaDaSemanaEnum.SegundaFeira,
            "terça" or "terca" or "terça-feira" or "terca-feira" => DiaDaSemanaEnum.TercaFeira,
            "quarta" or "quarta-feira" => DiaDaSemanaEnum.QuartaFeira,
            "quinta" or "quinta-feira" => DiaDaSemanaEnum.QuintaFeira,
            "sexta" or "sexta-feira" => DiaDaSemanaEnum.SextaFeira,
            "sábado" or "sabado" => DiaDaSemanaEnum.Sabado,
            _ => null
        };

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

        // Garante unicidade do slug local dentro da cidade (Uf + CidadeSlug)
        private async Task<string> GerarSlugLocalUnicoAsync(string baseSlug, string uf, string cidadeSlug)
        {
            var sufixo = 1;
            var slug = baseSlug;
            while (await context.Igrejas.AnyAsync(x =>
                x.Slug == slug &&
                x.Endereco.Uf == uf &&
                x.Endereco.CidadeSlug == cidadeSlug))
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
                .Include(x => x.Missas)
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
                    Slug = x.Slug,
                    Endereco = (EnderecoIgrejaResponse)x.Endereco,
                    Usuario = x.Usuario == null ? null : (UsuarioDtoResponse)x.Usuario,
                    Alteracao = x.Alteracao,
                    Ativo = x.Ativo,
                    Criacao = x.Criacao,
                    ImagemUrl = x.ImagemUrl == null ? null : imagemService.ObterUrlAzureBlob($"igreja/{x.ImagemUrl!}"),
                    Paroco = x.Paroco,
                    Missas = x.Missas.Select(m => new MissaResponse
                    {
                        Id = m.Id,
                        DiaSemana = m.DiaSemana,
                        Horario = m.Horario.ToString(),
                        Observacao = m.Observacao,
                        FontePrincipal = m.FontePrincipal,
                        UltimaValidacao = m.UltimaValidacao
                    }).ToList(),
                    Contato = x.Contato == null ? null : (IgrejaContatoResponse)x.Contato,
                    RedesSociais = x.RedesSociais == null ? Array.Empty<IgrejaRedesSociaisResponse>() : x.RedesSociais.Select(r => (IgrejaRedesSociaisResponse)r).ToList(),
                    Denuncia = x.Denuncia == null ? null : string.IsNullOrEmpty(x.Denuncia.AcaoRealizada) ? (DenunciarIgrejaAdminResponse)x.Denuncia : null
                });

                var resultado = await aux.PaginacaoAsync(filtro.Paginacao.PageIndex, filtro.Paginacao.PageSize);

                // Preencher confiança em memória após materialização
                foreach (var ig in resultado.Items)
                {
                    DateTime? fallback = ig.Usuario != null ? ig.Alteracao : null;
                    foreach (var m in ig.Missas)
                        Services.ConfiancaCalculator.PreencherConfianca(m, fallback);
                    ig.StatusConfianca = Services.ConfiancaCalculator.CalcularParaIgreja(ig.Missas);
                }

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
                .Include(x => x.Missas)
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

                if (filtro.Denuncia)
                    query = query.Where(x => x.Denuncia != null && string.IsNullOrEmpty(x.Denuncia.AcaoRealizada));

                var aux = query.Select(x => new IgrejaResponse()
                {
                    Id = x.Id,
                    Nome = x.Nome,
                    NomeUnico = x.NomeUnico,
                    Slug = x.Slug,
                    Endereco = (EnderecoIgrejaResponse)x.Endereco,
                    Usuario = x.Usuario == null ? null : (UsuarioDtoResponse)x.Usuario,
                    Alteracao = x.Alteracao,
                    Ativo = x.Ativo,
                    Criacao = x.Criacao,
                    ImagemUrl = x.ImagemUrl == null ? null : imagemService.ObterUrlAzureBlob($"igreja/{x.ImagemUrl!}"),
                    Paroco = x.Paroco,
                    Missas = x.Missas.Select(m => new MissaResponse
                    {
                        Id = m.Id,
                        DiaSemana = m.DiaSemana,
                        Horario = m.Horario.ToString(),
                        Observacao = m.Observacao,
                        FontePrincipal = m.FontePrincipal,
                        UltimaValidacao = m.UltimaValidacao
                    }).ToList(),
                    Contato = x.Contato == null ? null : (IgrejaContatoResponse)x.Contato,
                    RedesSociais = x.RedesSociais == null ? Array.Empty<IgrejaRedesSociaisResponse>() : x.RedesSociais.Select(r => (IgrejaRedesSociaisResponse)r).ToList(),
                    Denuncia = x.Denuncia == null ? null : string.IsNullOrEmpty(x.Denuncia.AcaoRealizada) ? (DenunciarIgrejaAdminResponse)x.Denuncia : null
                });

                var resultado = await aux.PaginacaoAsync(filtro.Paginacao.PageIndex, filtro.Paginacao.PageSize);

                // Preencher confiança em memória após materialização
                foreach (var ig in resultado.Items)
                {
                    DateTime? fallback = ig.Usuario != null ? ig.Alteracao : null;
                    foreach (var m in ig.Missas)
                        Services.ConfiancaCalculator.PreencherConfianca(m, fallback);
                    ig.StatusConfianca = Services.ConfiancaCalculator.CalcularParaIgreja(ig.Missas);
                }

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