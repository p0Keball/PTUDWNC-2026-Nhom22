using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FoodBlog.Application.Common.Behaviors;

/// <summary>
/// FR-OBS-002 / SRS 6.3 — log mọi command/query MediatR kèm thời gian xử lý.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next(cancellationToken);
            stopwatch.Stop();
            logger.LogInformation(
                "MediatR {RequestName} handled in {ElapsedMilliseconds} ms",
                requestName, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex,
                "MediatR {RequestName} failed after {ElapsedMilliseconds} ms",
                requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
