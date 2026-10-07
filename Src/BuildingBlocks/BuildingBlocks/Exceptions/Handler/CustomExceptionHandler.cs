using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Exceptions.Handler
{
    // This class is a custom exception handler that implements the IExceptionHandler interface.
    // It handles exceptions thrown during the processing of HTTP requests and returns appropriate HTTP responses with problem details.
    public class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger) : IExceptionHandler
    {
        // This method handles the exception and writes the problem details to the HTTP response.
        //context is the HttpContext for the current request,
        //exception is the exception that was thrown,
        //and cancellationToken is used to cancel the operation.
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(
                "Error Message :{exceptionMessage}, Time of occurrence {time}",
                exception.Message, DateTime.UtcNow);

            // Determine the appropriate HTTP status code and problem details based on the type of exception

            (string Detail, string Title, int StatusCode) details = exception switch
            {
                InternalServerException =>
               (
                    exception.Message,
                    exception.GetType().Name,
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError
                ),
                ValidationException =>
                 (
                    exception.Message,
                    exception.GetType().Name,
                    context.Response.StatusCode = StatusCodes.Status400BadRequest
                ),
                BadRequestException =>
                 (
                    exception.Message,
                    exception.GetType().Name,
                    context.Response.StatusCode = StatusCodes.Status400BadRequest
                ),
                NotFoundException =>
                 (
                    exception.Message,
                    exception.GetType().Name,
                    context.Response.StatusCode = StatusCodes.Status404NotFound
                ),
                _ =>
                 (
                    exception.Message,
                    exception.GetType().Name,
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError
                ),
            };
            // Set the response content type to "application/problem+json" to indicate that the response contains problem details
            var problemsDetails = new ProblemDetails
            {
                Title = details.Title,
                Detail = details.Detail,
                Status = details.StatusCode,
                Instance = context.Request.Path
            };
            // Add additional information to the problem details, such as the trace identifier and validation errors (if applicable)
            problemsDetails.Extensions.Add("traceId", context.TraceIdentifier);
            // If the exception is a ValidationException, add the validation errors to the problem details
            if (exception is ValidationException validationException)
            {
                problemsDetails.Extensions.Add("ValidationErrors", validationException.Errors);
            }
            // Set the response status code and write the problem details as JSON to the HTTP response
            await context.Response.WriteAsJsonAsync(problemsDetails, cancellationToken);
            return true;
        }
    }
}
