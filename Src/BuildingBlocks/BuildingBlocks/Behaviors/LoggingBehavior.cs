using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BuildingBlocks.Behaviors
{
    public class LoggingBehavior<TRequest, TRespone>
        (ILogger<LoggingBehavior<TRequest, TRespone>> logger)
        : IPipelineBehavior<TRequest, TRespone>
        where TRequest : notnull, IRequest<TRespone>
        where TRespone : notnull
    {
        public async Task<TRespone> Handle(TRequest request, RequestHandlerDelegate<TRespone> next, CancellationToken cancellationToken)
        {
            logger.LogInformation("[START] Handle request={Request} - Response={Response} - RequestData={RequestData}",
                typeof(TRequest).Name, typeof(TRespone).Name, request);
            var timer = new Stopwatch();
            timer.Start();

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
