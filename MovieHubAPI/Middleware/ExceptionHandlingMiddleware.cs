using System.Net;
using System.Text.Json;

namespace MovieHubAPI.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            ArgumentException => (int)HttpStatusCode.BadRequest,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            _ => (int)HttpStatusCode.InternalServerError
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var problem = new
        {
            status = statusCode,
            title = GetTitleForStatus(statusCode),
            detail = _env.IsDevelopment()
                ? exception.Message
                : "Ha ocurrido un error inesperado. Inténtalo de nuevo más tarde.",
            traceId = context.TraceIdentifier
        };

        if (_env.IsDevelopment() && exception.StackTrace is not null)
        {
            problem = problem with { detail = $"{exception.Message}\n{exception.StackTrace}" };
        }

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static string GetTitleForStatus(int statusCode) => statusCode switch
    {
        400 => "Solicitud inválida",
        404 => "Recurso no encontrado",
        500 => "Error interno del servidor",
        _ => "Error"
    };
}
