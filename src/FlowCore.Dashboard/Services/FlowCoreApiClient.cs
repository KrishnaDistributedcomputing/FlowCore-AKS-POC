using System.Text;
using System.Text.Json;

namespace FlowCore.Dashboard.Services;

public class FlowCoreApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<FlowCoreApiClient> _logger;

    public FlowCoreApiClient(HttpClient http, ILogger<FlowCoreApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<bool> CheckHealthAsync(string path)
    {
        try
        {
            var resp = await _http.GetAsync(path);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check failed for {Path}", path);
            return false;
        }
    }

    public async Task<(bool ok, string detail)> GetAsync(string path)
    {
        try
        {
            var resp = await _http.GetAsync(path);
            var body = await resp.Content.ReadAsStringAsync();
            var snippet = body.Length > 200 ? body[..200] + "..." : body;
            return (resp.IsSuccessStatusCode, $"{(int)resp.StatusCode} — {snippet}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool ok, string detail)> PostAsync(string path, object payload)
    {
        try
        {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync(path, content);
            var body = await resp.Content.ReadAsStringAsync();
            var snippet = body.Length > 200 ? body[..200] + "..." : body;
            return (resp.IsSuccessStatusCode, $"{(int)resp.StatusCode} — {snippet}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
