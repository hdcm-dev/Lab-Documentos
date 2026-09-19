using Microsoft.AspNetCore.Diagnostics;
using MyProject.Domain.Common;

namespace MyProject.WebAPI.ExceptionHandlers;

/// <summary>Traduce una regla de negocio incumplida a una respuesta HTTP 400 con ProblemDetails.</summary>
public class DomainExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;

    public DomainExceptionHandler(IProblemDetailsService problemDetails) => _problemDetails = problemDetails;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        if (exception is not DomainException)
            return false; // otras excepciones siguen su curso (500)

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Regla de negocio incumplida",
                Detail = exception.Message,
            },
        });
    }
}
