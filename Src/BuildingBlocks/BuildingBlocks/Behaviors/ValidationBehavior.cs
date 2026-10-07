using BuildingBlocks.CQRS;
using FluentValidation;
using MediatR;

namespace BuildingBlocks.Behaviors
{
    // This class is a pipeline behavior that validates the request before passing it to the next handler in the pipeline.
    // It uses FluentValidation to validate the request and throws a ValidationException if any validation failures occur.

    public class ValidationBehavior<TRequest, TResponse>
        // IEnumerable<IValidator<TRequest>> validators is a collection of validators for the request type TRequest
        (IEnumerable<IValidator<TRequest>> validators)
        : IPipelineBehavior<TRequest, TResponse>
        // TRequest is the type of the request (command) being handled, TResponse is the type of the response returned by the handler
        where TRequest : ICommand<TResponse>
    {
        // request is the command (request), next is the next handler in the pipeline (handle method), cancellationToken is used to cancel the operation

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Create a validation context for the request
            var context = new ValidationContext<TRequest>(request);
            // Run all the validators for the request and collect the validation results
            var validationResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            // Collect all the validation failures from the validation results
            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToList();
            if (failures.Any())
            {
                throw new ValidationException(failures);
            }
            // If there are no validation failures, call the next handler in the pipeline
            return await next();
        }
    }
}
