using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Travel.Application.Exceptions;

namespace Travel.Api.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            await Results.ValidationProblem(errors).ExecuteAsync(httpContext);
            return true;
        }

        if (exception is TravelPlanUpdateException notFoundException)
        {
            await Results.Problem(
                detail: notFoundException.Message,
                statusCode: StatusCodes.Status404NotFound).ExecuteAsync(httpContext);
            return true;
        }

        return false;
    }
}
