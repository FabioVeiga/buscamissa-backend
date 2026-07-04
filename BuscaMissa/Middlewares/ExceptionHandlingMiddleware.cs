using System.Text.Json;
using BuscaMissa.DTOs;

namespace BuscaMissa.Middlewares;

/// <summary>
/// Captura exceções não tratadas em toda a aplicação, registra o erro completo
/// (com o CorrelationId) e devolve ao cliente uma mensagem genérica — sem vazar
/// detalhes internos (stack trace, SQL, nomes de tabela).
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.Items.TryGetValue(HeaderName, out var id) ? id?.ToString() : null;
            logger.LogError(ex, "Exceção não tratada. CorrelationId: {CorrelationId}", correlationId);

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<dynamic>(new
            {
                mensagemTela = "Ocorreu um erro ao processar a requisição. Tente novamente mais tarde.",
                correlationId
            });
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
