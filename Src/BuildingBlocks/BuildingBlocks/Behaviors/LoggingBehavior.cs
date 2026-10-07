using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BuildingBlocks.Behaviors
{
    // This class is a pipeline behavior that logs the request and response for each command handled by MediatR.
    public class LoggingBehavior<TRequest, TRespone>
        //ILogger<LoggingBehavior<TRequest, TRespone>> logger is used to log information about the request and response
        (ILogger<LoggingBehavior<TRequest, TRespone>> logger)
        : IPipelineBehavior<TRequest, TRespone>
        where TRequest : notnull, IRequest<TRespone>
        where TRespone : notnull
    {
        // request is the command (request), next is the next handler in the pipeline (handle method), cancellationToken is used to cancel the operation
        public async Task<TRespone> Handle(TRequest request, RequestHandlerDelegate<TRespone> next, CancellationToken cancellationToken)
        {
            logger.LogInformation("[START] Handle request={Request} - Response={Response} - RequestData={RequestData}",
                typeof(TRequest).Name, typeof(TRespone).Name, request);
            // Start a stopwatch to measure the time taken to handle the request
            var timer = new Stopwatch();
            timer.Start();

            // Call the next handler in the pipeline and get the response
            var response = await next();
            timer.Stop();

            var timeTaken = timer.Elapsed;
            if (timeTaken.Seconds > 3)
                logger.LogWarning("[PERFORMANCE] The request {Request} took {TimeTaken} seconds",
                    typeof(TRequest).Name, timeTaken.Seconds);

            logger.LogInformation("[END] Handled {Request} with {Response}",
                typeof(TRequest).Name, typeof(TRespone).Name);

            return response;

        }
    }
}
