using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CodeHive.Shared.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>
 : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{

    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _log;
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> log) => _log = log;
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        _log.LogInformation("---> Handling {Request}", name);
        var response = await next();
        _log.LogInformation("<--- Handled {Request}", name);
        return response;
    }
}
