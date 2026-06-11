using System.Text.Json;
using EAuditoria.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace EAuditoria.API.Middleware;

/// <summary>
/// Middleware global que traduz exceções em respostas ProblemDetails (RFC 7807):
/// ValidationException → 400 (com campo errors), NotFoundException → 404,
/// ConflictException → 409, demais → 500.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment env)
{
    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await WriteProblem(context, new ValidationProblemDetails(
                ex.Errors.ToDictionary(kv => kv.Key, kv => kv.Value))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Erro de validação.",
                Instance = context.Request.Path
            });
        }
        catch (NotFoundException ex)
        {
            await WriteProblem(context, BuildProblem(
                context, StatusCodes.Status404NotFound, "Recurso não encontrado.", ex.Message));
        }
        catch (ConflictException ex)
        {
            await WriteProblem(context, BuildProblem(
                context, StatusCodes.Status409Conflict, "Conflito.", ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro não tratado ao processar {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await WriteProblem(context, BuildProblem(
                context, StatusCodes.Status500InternalServerError,
                "Ocorreu um erro inesperado.",
                env.IsDevelopment() ? ex.ToString() : "Erro interno do servidor."));
        }
    }

    private static ProblemDetails BuildProblem(
        HttpContext context, int status, string title, string detail) => new()
    {
        Status = status,
        Title = title,
        Detail = detail,
        Instance = context.Request.Path
    };

    private static async Task WriteProblem(HttpContext context, ProblemDetails problem)
    {
        context.Response.Clear();
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        // Serializa pelo tipo em runtime para preservar campos de tipos derivados
        // (ex.: o campo "errors" de ValidationProblemDetails).
        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problem, problem.GetType(), JsonOptions));
    }
}
