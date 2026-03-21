using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace FlowCore.Shared.Middleware;

public class CorrelationIdDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString();
        if (!string.IsNullOrEmpty(correlationId))
            request.Headers.Add("X-Correlation-ID", correlationId);

        return await base.SendAsync(request, cancellationToken);
    }
}
