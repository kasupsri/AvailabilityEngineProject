using System.Diagnostics;
using Kasupsri.Utilities.Logging.Correlation;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Kasupsri.Utilities.Logging;

public class CorrelationHeaderMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public CorrelationHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICorrelationIdAccessor correlationIdAccessor)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault();

        if (string.IsNullOrEmpty(correlationId))
            correlationId = Guid.NewGuid().ToString();

        correlationIdAccessor.SetCorrelationId(correlationId);
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;

        if (Activity.Current != null)
            Activity.Current.SetTag("correlation.id", correlationId);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
